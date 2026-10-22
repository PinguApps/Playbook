// snippet:support
open System
open System.Collections.Concurrent
open System.Reflection
open System.Runtime.Versioning
open System.Security.Cryptography
open System.Threading.Tasks

let require condition message = if not condition then failwith message

type BeaconLease(events: ConcurrentQueue<string>, release: Task, cleanupError: exn option) =
    let entered = TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
    member _.Entered = entered.Task
    interface IAsyncDisposable with
        member _.DisposeAsync() =
            events.Enqueue "closing"
            entered.SetResult()
            ValueTask(task {
                do! release
                events.Enqueue "closed"
                match cleanupError with Some error -> raise error | None -> ()
            })

let showCore () =
    let core = typeof<unit>.Assembly
    printfn "FSharp.Core: %s\nTargetFramework: %s\nSHA256: %s" core.Location
        (core.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName)
        (Convert.ToHexString(SHA256.HashData(IO.File.ReadAllBytes core.Location)).ToLowerInvariant())

let checkCases cleanupWins (run: IAsyncDisposable -> exn option -> Task<int>) =
    for bodyFails, cleanupFails in [false,false; true,false; false,true; true,true] do
        let events = ConcurrentQueue<string>()
        let release = TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        let bodyError = InvalidOperationException("Beacon survey failed")
        let cleanupError = ApplicationException("Beacon close failed")
        let lease = BeaconLease(events, release.Task, if cleanupFails then Some cleanupError else None)
        let work = run lease (if bodyFails then Some bodyError else None)
        require (lease.Entered.Wait(TimeSpan.FromSeconds 10.)) "Disposal never started"
        require (not work.IsCompleted) "Script work completed before asynchronous cleanup"
        require (events.ToArray() = [|"closing"|]) "Cleanup order/count changed before release"
        release.SetResult()
        let expected = if cleanupFails && cleanupWins then Some (cleanupError :> exn) elif bodyFails then Some (bodyError :> exn) else None
        let outcome = try Choice1Of2(work.GetAwaiter().GetResult()) with error -> Choice2Of2 error
        match outcome, expected with
        | Choice1Of2 23, None -> ()
        | Choice2Of2 actual, Some error -> require (obj.ReferenceEquals(actual, error)) "Exception precedence changed"
        | _ -> failwithf "Unexpected script outcome: %A (bodyFails=%b cleanupFails=%b)" outcome bodyFails cleanupFails
        require (events.ToArray() = [|"closing"; "closed"|]) "Cleanup must finish exactly once"
        printfn "PASS: bodyFails=%b cleanupFails=%b; closing,closed; cleanupWins=%b; outcome=%A" bodyFails cleanupFails cleanupWins outcome
    printfn "PASS: script completion follows cleanup"
// endsnippet:support
