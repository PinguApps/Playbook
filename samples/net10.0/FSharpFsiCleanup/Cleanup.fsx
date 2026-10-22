#load "../../net11.0/FSharpFsiCleanup/FsiCleanupSupport.fsx"
open System
open System.Runtime.ExceptionServices
open System.Threading.Tasks
open FsiCleanupSupport

// snippet:workflow
// Explicitly await cleanup when the older FSI asset lacks asynchronous Using.
let inspectBeacon (lease: IAsyncDisposable) (bodyError: exn option) = task {
    let outcome =
        try
            match bodyError with Some error -> raise error | None -> ()
            Choice1Of2 23
        with error -> Choice2Of2(ExceptionDispatchInfo.Capture error)
    do! lease.DisposeAsync()
    match outcome with
    | Choice1Of2 depth -> return depth
    | Choice2Of2 error -> error.Throw(); return Unchecked.defaultof<int>
}
// endsnippet:workflow

showCore ()
Task.Run(fun () -> checkCases true inspectBeacon).GetAwaiter().GetResult()
