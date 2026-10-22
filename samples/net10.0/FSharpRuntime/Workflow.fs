module Playbook.Samples.RuntimeWorkflow

open System
open System.Threading.Tasks

// snippet:workflow
// Ordinary task expressions remain a valid, supported alternative.
let adjustedDepth (first: Task<int>) (second: Task<int>) (lease: IAsyncDisposable) = task {
    use owned = lease
    let! depth = first
    let! correction = second
    return depth + correction
}
// endsnippet:workflow

type SurveyLease(events: ResizeArray<string>, release: Task) =
    interface IAsyncDisposable with
        member _.DisposeAsync() =
            events.Add "disposing"
            ValueTask(task {
                do! release
                events.Add "disposed"
            })

[<EntryPoint>]
let main _ =
    let gate () = TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)
    let first, second, release = gate (), gate (), gate ()
    let events = ResizeArray<string>()
    let work = adjustedDepth first.Task second.Task (SurveyLease(events, release.Task))
    if work.IsCompleted then failwith "Expected pending work"
    first.SetResult 14
    second.SetResult 3
    if work.IsCompleted then failwith "Expected pending disposal"
    release.SetResult 0
    if work.GetAwaiter().GetResult() <> 17 || Seq.toList events <> ["disposing"; "disposed"] then failwith "Result/lifetime changed"
    printfn "PASS: ordinary .NET 10 task; sequential pending work; asynchronous cleanup; runtime %O" Environment.Version
    0
