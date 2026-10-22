module Playbook.Samples.Soundings

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

// snippet:producer
// Ordinary .NET 10 task-based enumerator; calls must be serial.
let survey (ready: Task<int>) (createLease: unit -> IAsyncDisposable) =
    { new IAsyncEnumerable<int> with
        member _.GetAsyncEnumerator(token) =
            let mutable position = 0
            let mutable current = 0
            let mutable owned: IAsyncDisposable option = None
            let close () = task {
                match owned with
                | Some lease -> owned <- None; do! lease.DisposeAsync()
                | None -> ()
            }
            { new IAsyncEnumerator<int> with
                member _.Current = current
                member _.MoveNextAsync() = ValueTask<bool>(task {
                    let mutable failure: exn option = None
                    let mutable available = false
                    try
                        token.ThrowIfCancellationRequested()
                        match position with
                        | 0 ->
                            owned <- Some (createLease ())
                            let! depth = ready
                            token.ThrowIfCancellationRequested()
                            current <- depth
                            position <- 1
                            available <- true
                        | 1 -> current <- current + 1; position <- 2; available <- true
                        | _ -> ()
                    with error -> failure <- Some error
                    if not available then
                        position <- 3
                        do! close ()
                    match failure with
                    | Some error -> System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw()
                    | None -> ()
                    return available
                })
                member _.DisposeAsync() =
                    position <- 3
                    ValueTask(close ()) } }
// endsnippet:producer

[<EntryPoint>]
let main _ =
    let gate = TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)
    let release = TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
    let mutable closing = false
    let mutable count = 0
    let lease () =
        { new IAsyncDisposable with
            member _.DisposeAsync() =
                closing <- true
                ValueTask(task {
                    do! release.Task
                    count <- count + 1
                }) }
    let iterator = (survey gate.Task lease).GetAsyncEnumerator()
    let first = iterator.MoveNextAsync().AsTask()
    if first.IsCompleted then failwith "Move must suspend"
    gate.SetResult 17
    if not (first.WaitAsync(TimeSpan.FromSeconds 10.).GetAwaiter().GetResult()) || iterator.Current <> 17 then failwith "Wrong first result"
    if not (iterator.MoveNextAsync().AsTask().GetAwaiter().GetResult()) || iterator.Current <> 18 then failwith "Wrong second result"
    let disposal = iterator.DisposeAsync().AsTask()
    if disposal.IsCompleted || not closing then failwith "Disposal must suspend"
    release.SetResult()
    disposal.WaitAsync(TimeSpan.FromSeconds 10.).GetAwaiter().GetResult()
    iterator.DisposeAsync().AsTask().GetAwaiter().GetResult()
    if count <> 1 then failwith "Cleanup count changed"
    printfn "PASS: previous task-based asynchronous enumerator; pending pull; incremental results; pending early disposal exactly once"
    0
