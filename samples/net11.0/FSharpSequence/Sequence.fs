module Playbook.Samples.Soundings

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers

// snippet:producer
// Application support over real intrinsics; no shipped async-sequence builder.
let inline soundings ([<InlineIfLambda>] recipe: unit -> seq<'T>) : IAsyncEnumerable<'T> =
    __runtimeAsyncSequence recipe

let survey (ready: Task<int>) (createLease: unit -> IAsyncDisposable) (seenToken: CancellationToken -> unit) =
    soundings (fun () -> seq {
        let lease = createLease ()
        try
            let token = __runtimeAsyncSequenceCancellationToken ()
            seenToken token
            let depth = AsyncHelpers.Await ready
            token.ThrowIfCancellationRequested()
            yield depth
            yield depth + 1
        finally AsyncHelpers.Await(lease.DisposeAsync())
    })

let relay (source: IAsyncEnumerable<int>) (createLease: unit -> IAsyncDisposable) =
    soundings (fun () -> seq {
        let lease = createLease ()
        try
            let token = __runtimeAsyncSequenceCancellationToken ()
            let input = source.GetAsyncEnumerator token
            try
                while AsyncHelpers.Await(input.MoveNextAsync()) do
                    yield input.Current + 100
            finally AsyncHelpers.Await(input.DisposeAsync())
        finally AsyncHelpers.Await(lease.DisposeAsync())
    })
// endsnippet:producer

// snippet:serial
// Reject overlaps before entering the raw iterator's unsupported state.
let serially (source: IAsyncEnumerable<'T>) =
    { new IAsyncEnumerable<'T> with
        member _.GetAsyncEnumerator(token) =
            let input = source.GetAsyncEnumerator token
            let mutable busy = 0
            let mutable disposed = false
            let enter () =
                if Interlocked.CompareExchange(&busy, 1, 0) <> 0 then
                    invalidOp "Await the previous move or disposal before another operation"
            { new IAsyncEnumerator<'T> with
                member _.Current = input.Current
                member _.MoveNextAsync() =
                    enter ()
                    ValueTask<bool>(task {
                        try
                            if disposed then return false
                            else return! input.MoveNextAsync()
                        finally Volatile.Write(&busy, 0)
                    })
                member _.DisposeAsync() =
                    enter ()
                    ValueTask(task {
                        try
                            if not disposed then
                                disposed <- true
                                do! input.DisposeAsync()
                        finally Volatile.Write(&busy, 0)
                    }) } }
// endsnippet:serial

type ChartLease(events: ResizeArray<string>, name: string, release: Task, error: exn option) =
    interface IAsyncDisposable with
        member _.DisposeAsync() =
            events.Add(name + " closing")
            ValueTask(task {
                do! release
                events.Add(name + " closed")
                match error with Some failure -> raise failure | None -> ()
            })

let require condition message = if not condition then failwith message
let gate () = TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)
let wait (work: Task<'T>) = work.WaitAsync(TimeSpan.FromSeconds 10.).GetAwaiter().GetResult()
let closed (work: ValueTask) = work.AsTask().WaitAsync(TimeSpan.FromSeconds 10.).GetAwaiter().GetResult()
let next (iterator: IAsyncEnumerator<'T>) = iterator.MoveNextAsync().AsTask() |> wait
let reject action =
    try action (); failwith "Overlap was accepted"
    with :? InvalidOperationException -> ()

[<EntryPoint>]
let main _ =
    let ready, release = gate (), gate ()
    let events = ResizeArray<string>()
    let mutable observed = false
    let source = survey ready.Task (fun () -> ChartLease(events, "chart", release.Task, None)) (fun _ -> observed <- true) |> serially
    require (not observed) "Factory eagerly ran recipe"
    let iterator = source.GetAsyncEnumerator()
    require (not observed) "Acquisition eagerly ran recipe"
    let first = iterator.MoveNextAsync().AsTask()
    require (not first.IsCompleted && observed) "Move must genuinely suspend"
    reject (fun () -> iterator.MoveNextAsync() |> ignore)
    reject (fun () -> iterator.DisposeAsync() |> ignore)
    ready.SetResult 17
    require (wait first && iterator.Current = 17 && events.Count = 0) "First item or ownership changed"
    require (next iterator && iterator.Current = 18 && events.Count = 0) "Incremental result changed"
    let disposal = iterator.DisposeAsync().AsTask()
    require (not disposal.IsCompleted) "Early disposal must await cleanup"
    reject (fun () -> iterator.MoveNextAsync() |> ignore)
    release.SetResult 0
    disposal.WaitAsync(TimeSpan.FromSeconds 10.).GetAwaiter().GetResult()
    require (Seq.toList events = ["chart closing"; "chart closed"]) "Cleanup count/order changed"
    closed (iterator.DisposeAsync())
    require (events.Count = 2) "Repeated guarded disposal repeated cleanup"
    printfn "PASS: pending MoveNextAsync; incremental values; guarded overlapping move/dispose; pending early disposal exactly once"

    use cancellation = new CancellationTokenSource()
    let cancellationEvents = ResizeArray<string>()
    let mutable received = CancellationToken.None
    let cancellable = soundings (fun () -> seq {
        let token = __runtimeAsyncSequenceCancellationToken ()
        received <- token
        AsyncHelpers.Await(Task.Delay(Timeout.Infinite, token))
        yield 1
    })
    let nested = relay cancellable (fun () -> ChartLease(cancellationEvents, "outer", Task.CompletedTask, None)) |> serially
    let cancelled = nested.GetAsyncEnumerator(cancellation.Token)
    let moving = cancelled.MoveNextAsync().AsTask()
    require (not moving.IsCompleted && received = cancellation.Token) "Nested token was not forwarded"
    cancellation.Cancel()
    try wait moving |> ignore; failwith "Cancellation lost"
    with :? OperationCanceledException as error -> require (error.CancellationToken = cancellation.Token) "Cancellation token changed"
    closed (cancelled.DisposeAsync())
    require (Seq.toList cancellationEvents = ["outer closing"; "outer closed"]) "Cancelled outer resource leaked"
    printfn "PASS: nested cancellation token and cancellable pending wait"

    use limitedCancellation = new CancellationTokenSource()
    let nonCancellable = gate ()
    let limitedEvents = ResizeArray<string>()
    let limited = (survey nonCancellable.Task (fun () -> ChartLease(limitedEvents, "limited", Task.CompletedTask, None)) ignore |> serially).GetAsyncEnumerator(limitedCancellation.Token)
    let limitedMove = limited.MoveNextAsync().AsTask()
    limitedCancellation.Cancel()
    require (not limitedMove.IsCompleted) "Cancellation must not complete a non-cancellable wait"
    nonCancellable.SetResult 8
    try wait limitedMove |> ignore; failwith "Post-wait cancellation lost"
    with :? OperationCanceledException -> ()
    closed (limited.DisposeAsync())
    require (limitedEvents.Count = 2) "Non-cancellable wait cleanup leaked"
    printfn "PASS: cancellation waits for non-cancellable work; explicit post-wait check"

    for bodyFails, innerFails, outerFails in [false,false,false; true,false,false; true,true,false; true,true,true; false,true,true] do
        let trace = ResizeArray<string>()
        let innerRelease, outerRelease = gate (), gate ()
        let bodyError = InvalidOperationException("Sounding failed")
        let innerError = ApplicationException("Inner close failed")
        let outerError = ApplicationException("Outer close failed")
        let body = if bodyFails then Task.FromException<int> bodyError else Task.FromResult 4
        let inner = survey body (fun () -> ChartLease(trace, "inner", innerRelease.Task, if innerFails then Some innerError else None)) ignore
        let outer = relay inner (fun () -> ChartLease(trace, "outer", outerRelease.Task, if outerFails then Some outerError else None)) |> serially
        let owned = outer.GetAsyncEnumerator()
        let firstMove = owned.MoveNextAsync().AsTask()
        let completion =
            if bodyFails then firstMove :> Task
            else
                require (wait firstMove && owned.Current = 104) "Nested value changed"
                owned.DisposeAsync().AsTask()
        require (not completion.IsCompleted) "Nested cleanup must genuinely suspend"
        innerRelease.SetResult 0
        outerRelease.SetResult 0
        let result =
            try
                completion.WaitAsync(TimeSpan.FromSeconds 10.).GetAwaiter().GetResult()
                None
            with error -> Some error
        let expected = if outerFails then Some (outerError :> exn) elif innerFails then Some (innerError :> exn) elif bodyFails then Some (bodyError :> exn) else None
        match result, expected with
        | None, None -> ()
        | Some actual, Some wanted -> require (obj.ReferenceEquals(actual, wanted)) "Cleanup/body exception precedence changed"
        | _ -> failwith "Unexpected nested outcome"
        require (Seq.toList trace = ["inner closing"; "inner closed"; "outer closing"; "outer closed"]) "Nested cleanup order/count changed"
    printfn "PASS: nested early and exceptional cleanup; outer cleanup error wins; body error retained when cleanup succeeds"
    0
