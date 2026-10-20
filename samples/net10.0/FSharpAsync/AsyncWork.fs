module Playbook.Samples.AsyncWork

open System
open System.Threading
open System.Threading.Tasks

// snippet:start
let inspectOnCaller () =
    let caller = Environment.CurrentManagedThreadId
    // Make a required synchronous prefix explicit before entering the runner.
    let started = Environment.CurrentManagedThreadId
    let resumed =
        async {
            do! Async.Sleep 15
            return Environment.CurrentManagedThreadId
        } |> Async.RunSynchronously
    caller, started, resumed
// endsnippet:start

// snippet:bounded
let loadAsync limit jobs = Async.Parallel(jobs, maxDegreeOfParallelism = limit)

let loadTasks limit (cancellationToken: CancellationToken) factories = task {
    use gate = new SemaphoreSlim(limit)
    let pending =
        factories
        |> Seq.map (fun start -> task {
            do! gate.WaitAsync(cancellationToken)
            try return! start cancellationToken
            finally gate.Release() |> ignore
        })
        |> Seq.toArray
    return! Task.WhenAll pending
}

let inspectBatch () =
    [ for berth in 1..4 -> async { return berth * 10 } ] |> loadAsync 2

let inspectTaskBatch token =
    [ for berth in 1..4 -> fun cancellationToken -> task {
        do! Task.Delay(5, cancellationToken)
        return berth * 10
    } ] |> loadTasks 2 token
// endsnippet:bounded

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
                finally lock sync (fun () -> active <- active - 1)
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

[<EntryPoint>]
let main _ =
    let caller, started, _ = inspectOnCaller ()
    if caller <> started then failwith "Explicit prefix moved threads"
    printfn "PASS: explicit synchronous prefix before the async runner"
    checkBounded ()
    if (inspectBatch () |> Async.RunSynchronously) <> [| 10; 20; 30; 40 |] ||
       inspectTaskBatch(CancellationToken.None).GetAwaiter().GetResult() <> [| 10; 20; 30; 40 |] then
        failwith "Worked batch result changed"
    0
