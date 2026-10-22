#load "FsiCleanupSupport.fsx"
open System
open System.Threading.Tasks
open FsiCleanupSupport

// snippet:workflow
let inspectBeacon (lease: IAsyncDisposable) (bodyError: exn option) = task {
    use owned = lease
    match bodyError with Some error -> return raise error | None -> return 23
}
// endsnippet:workflow

showCore ()
Task.Run(fun () -> checkCases false inspectBeacon).GetAwaiter().GetResult()
