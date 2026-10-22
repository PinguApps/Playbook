module Playbook.Samples.RuntimeWorkflow

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers

// snippet:builder
// Inline fragments are specialized into Run's marked method by the compiler.
type SurveySteps() =
    member inline _.Delay([<InlineIfLambda>] work: unit -> 'T) = work
    member inline _.Return(value: 'T) = value
    member inline _.Zero() = ()
    member inline _.Combine((), [<InlineIfLambda>] next: unit -> 'T) = next ()
    member inline _.Bind(work: Task<'T>, [<InlineIfLambda>] next: 'T -> 'U) = next (AsyncHelpers.Await work)
    member inline _.Bind(work: Task, [<InlineIfLambda>] next: unit -> 'T) = AsyncHelpers.Await work; next ()
    member inline _.Bind(work: ValueTask<'T>, [<InlineIfLambda>] next: 'T -> 'U) = next (AsyncHelpers.Await work)
    member inline _.Bind(work: ValueTask, [<InlineIfLambda>] next: unit -> 'T) = AsyncHelpers.Await work; next ()
    member inline _.ReturnFrom(work: Task<'T>) = AsyncHelpers.Await work
    member inline _.ReturnFrom(work: ValueTask<'T>) = AsyncHelpers.Await work
    member inline _.TryFinally([<InlineIfLambda>] work: unit -> 'T, [<InlineIfLambda>] cleanup: unit -> unit) =
        try work () finally cleanup ()
    member inline _.Using(resource: 'R, [<InlineIfLambda>] work: 'R -> 'T) : 'T when 'R :> IAsyncDisposable =
        try work resource
        finally
            if not (isNull (box resource)) then AsyncHelpers.Await(resource.DisposeAsync())

type SurveyTask() =
    inherit SurveySteps()
    member inline _.Run([<InlineIfLambda>] work: unit -> 'T) : Task<'T> = __runtimeAsyncReturn (work ())

type SurveyValueTask() =
    inherit SurveySteps()
    member inline _.Run([<InlineIfLambda>] work: unit -> 'T) : ValueTask<'T> = __runtimeAsyncReturnValueTask (work ())

type SurveyTaskUnit() =
    inherit SurveySteps()
    member inline _.Run([<InlineIfLambda>] work: unit -> unit) : Task = __runtimeAsyncReturnUnit (work ())

type SurveyValueTaskUnit() =
    inherit SurveySteps()
    member inline _.Run([<InlineIfLambda>] work: unit -> unit) : ValueTask = __runtimeAsyncReturnValueTaskUnit (work ())

let surveyTask = SurveyTask()
let surveyValueTask = SurveyValueTask()
let surveyTaskUnit = SurveyTaskUnit()
let surveyValueTaskUnit = SurveyValueTaskUnit()
// endsnippet:builder

// snippet:workflow
let adjustedDepth (first: Task<int>) (second: Task<int>) (lease: IAsyncDisposable) = surveyTask {
    use owned = lease
    let! (depth: int) = first
    let! (correction: int) = second
    return! ValueTask<int>(depth + correction)
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

let require condition message = if not condition then failwith message
let gate () = TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)

let awaitTask (work: Task) = surveyTaskUnit { do! work }
let awaitValueTask (work: ValueTask) = surveyValueTaskUnit { do! work }

[<EntryPoint>]
let main _ =
    let first, second, release = gate (), gate (), gate ()
    let events = ResizeArray<string>()
    let work = adjustedDepth first.Task second.Task (SurveyLease(events, release.Task))
    require (not work.IsCompleted) "First wait must suspend"
    first.SetResult 14
    require (not work.IsCompleted) "Second wait must suspend"
    second.SetResult 3
    require (not work.IsCompleted) "Asynchronous disposal must hold completion"
    release.SetResult 0
    require (work.GetAwaiter().GetResult() = 17) "Depth changed"
    require (Seq.toList events = ["disposing"; "disposed"]) "Disposal order/count changed"
    let pending = gate ()
    let value = surveyValueTask { let! number = ValueTask<int>(pending.Task)
                                 return number + 2 }
    require (not value.IsCompleted) "ValueTask wait must suspend"
    pending.SetResult 5
    require (value.AsTask().GetAwaiter().GetResult() = 7) "ValueTask result changed"
    let unitGate = TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
    let unitTask = awaitTask unitGate.Task
    let unitValue = awaitValueTask (ValueTask(unitGate.Task))
    require (not unitTask.IsCompleted && not unitValue.IsCompleted) "Unit waits must suspend"
    unitGate.SetResult()
    unitTask.GetAwaiter().GetResult()
    unitValue.AsTask().GetAwaiter().GetResult()
    let failGate = gate ()
    let failure = InvalidOperationException("Survey gauge failed")
    let failing = surveyTask { let! _ = failGate.Task
                              return raise failure }
    failGate.SetResult 0
    try failing.GetAwaiter().GetResult() |> ignore; failwith "Failure lost"
    with :? InvalidOperationException as actual -> require (obj.ReferenceEquals(actual, failure)) "Exception replaced"
    let bodyGate, cleanupGate = gate (), gate ()
    let failedEvents = ResizeArray<string>()
    let failedCleanup = adjustedDepth bodyGate.Task (Task.FromResult 3) (SurveyLease(failedEvents, cleanupGate.Task))
    bodyGate.SetException failure
    require (not failedCleanup.IsCompleted) "Failure escaped before pending cleanup"
    cleanupGate.SetResult 0
    try failedCleanup.GetAwaiter().GetResult() |> ignore; failwith "Failure lost after cleanup"
    with :? InvalidOperationException as actual -> require (obj.ReferenceEquals(actual, failure)) "Cleanup replaced body failure"
    require (Seq.toList failedEvents = ["disposing"; "disposed"]) "Exceptional cleanup count changed"
    let marked = typeof<SurveyTask>.Assembly.GetTypes() |> Array.collect (fun t -> t.GetMethods(System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Static ||| System.Reflection.BindingFlags.Instance)) |> Array.filter (fun m -> (int (m.GetMethodImplementationFlags()) &&& 0x2000) <> 0)
    require (marked.Length >= 4) "Missing runtime async carriers"
    printfn "PASS: Task<T>, Task, ValueTask<T>, ValueTask; pending sequential waits; return!; failure identity; asynchronous disposal; %d runtime-async methods; runtime %O" marked.Length Environment.Version
    0
