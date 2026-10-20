module Playbook.Samples.AsyncWork

open System
open System.Threading
open System.Threading.Tasks

// snippet:start
let inspectOnCaller () =
    if not (isNull SynchronizationContext.Current) then
        invalidOp "Run this blocking operation outside a SynchronizationContext."
    let caller = Environment.CurrentManagedThreadId
    async {
        let started = Environment.CurrentManagedThreadId
        do! Async.Sleep 15
        return caller, started, Environment.CurrentManagedThreadId
    } |> Async.RunSynchronouslyImmediate
// endsnippet:start

// snippet:bounded
let loadAsync limit jobs = jobs |> Async.parallelLimit limit

// Pass factories, not already-running tasks: admission happens before invocation.
let loadTasks limit cancellationToken factories =
    factories |> Task.parallelLimit limit cancellationToken

let inspectBatch () =
    [ for berth in 1..4 -> async { return berth * 10 } ] |> loadAsync 2

let inspectTaskBatch token =
    [ for berth in 1..4 -> fun cancellationToken -> task {
        do! Task.Delay(5, cancellationToken)
        return berth * 10
    } ] |> loadTasks 2 token
// endsnippet:bounded

type RecordingContext() =
    inherit SynchronizationContext()
    let mutable posts = 0
    member _.Posts = posts
    override _.Post(callback, state) =
        Interlocked.Increment(&posts) |> ignore
        ThreadPool.QueueUserWorkItem(fun _ -> callback.Invoke(state)) |> ignore

let checkBounded () =
    let limit = 2
    for taskMode in [ false; true ] do
        let mutable active = 0
        let mutable peak = 0
        let mutable started = 0
        let sync = obj()
        let entered = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)
        let release = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)
        let startWork id =
            lock sync (fun () ->
                active <- active + 1
                started <- started + 1
                peak <- max peak active
                if started = limit then entered.TrySetResult(()) |> ignore)
            task {
                try
                    do! release.Task
                    do! Task.Delay((5 - id) * 8)
                    return id * 10
                finally
                    lock sync (fun () -> active <- active - 1)
            }
        let pending =
            if taskMode then
                [ for id in 1..4 -> fun (_: CancellationToken) -> startWork id ]
                |> loadTasks limit CancellationToken.None
            else
                [ for id in 1..4 -> async { return! startWork id |> Async.AwaitTask } ]
                |> loadAsync limit |> Async.StartAsTask
        if not (entered.Task.Wait(TimeSpan.FromSeconds 5.0)) then failwith "Work was not admitted"
        lock sync (fun () -> if started <> limit then failwith "Work started before admission")
        release.SetResult(())
        let results = pending.GetAwaiter().GetResult()
        if results <> [| 10; 20; 30; 40 |] || peak <> limit || active <> 0 then failwith "Concurrency or result order changed"
        printfn "PASS: %s factories started=%d peak=%d results=%A" (if taskMode then "task" else "async") started peak results

let checkSiblingCancellation () =
    let siblingStarted = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)
    let siblingCanceled = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)
    let failing (_: CancellationToken) = task {
        do! siblingStarted.Task
        return raise (InvalidOperationException("Dock load failed"))
    }
    let waiting (token: CancellationToken) = task {
        use _registration = token.Register(fun () -> siblingCanceled.TrySetResult(()) |> ignore)
        siblingStarted.SetResult(())
        do! Task.Delay(Timeout.Infinite, token)
        return 0
    }
    try
        loadTasks 2 CancellationToken.None [ failing; waiting ] |> fun work -> work.GetAwaiter().GetResult() |> ignore
        failwith "Expected the batch failure"
    with :? InvalidOperationException as error ->
        if error.Message <> "Dock load failed" || not (siblingCanceled.Task.Wait(TimeSpan.FromSeconds 5.0)) then
            failwith "Sibling failure cancellation changed"
    printfn "PASS: task factory sibling token canceled after original failure"

[<EntryPoint>]
let main _ =
    let original = SynchronizationContext.Current
    let context = RecordingContext()
    try
        SynchronizationContext.SetSynchronizationContext(context)
        // RC2's actual behavior differs from the release-note claim that the
        // ambient context is disregarded: this suspension posts to it.
        let result = async { do! Async.Sleep 15
                             return 7 } |> Async.RunSynchronouslyImmediate
        if result <> 7 || context.Posts = 0 then failwith "Measured RC2 context behavior changed; reconcile guidance"
        try
            inspectOnCaller () |> ignore
            failwith "Expected the sample's context guard"
        with :? InvalidOperationException -> ()
        printfn "PASS: actual RC2 ambient context posts=%d; guarded wrapper rejects context" context.Posts
    finally SynchronizationContext.SetSynchronizationContext(original)
    let caller, started, resumed = inspectOnCaller ()
    if caller <> started || caller = resumed then failwith "Caller-start semantics changed"
    printfn "PASS: immediate prefix on caller; asynchronous suspension resumed elsewhere"
    checkBounded ()
    checkSiblingCancellation ()
    if (inspectBatch () |> Async.RunSynchronously) <> [| 10; 20; 30; 40 |] ||
       inspectTaskBatch(CancellationToken.None).GetAwaiter().GetResult() <> [| 10; 20; 30; 40 |] then
        failwith "Worked batch result changed"
    0
