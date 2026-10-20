module Playbook.Samples.Rethrow

// snippet:rethrow
open System
open System.Runtime.CompilerServices
open System.Runtime.ExceptionServices
open System.Threading.Tasks

[<MethodImpl(MethodImplOptions.NoInlining)>]
let failAtDock () : int = raise (InvalidOperationException("Berth gate refused entry"))

let loadAsync () = async {
    try
        do! Async.Sleep 1
        return failAtDock ()
    with error ->
        ExceptionDispatchInfo.Capture(error).Throw()
        return Unchecked.defaultof<int> // unreachable; satisfies the expression's result type
}

let loadTask () = task {
    try
        do! Task.Delay 1
        return failAtDock ()
    with error ->
        ExceptionDispatchInfo.Capture(error).Throw()
        return Unchecked.defaultof<int>
}
// endsnippet:rethrow

[<EntryPoint>]
let main _ =
    for run in [ (fun () -> loadAsync () |> Async.RunSynchronously |> ignore)
                 (fun () -> loadTask().GetAwaiter().GetResult() |> ignore) ] do
        try
            run ()
            failwith "Expected the original failure"
        with :? InvalidOperationException as error ->
            if error.Message <> "Berth gate refused entry" || not ((error.StackTrace |> Option.ofObj |> Option.defaultValue "").Contains "failAtDock") then
                failwith "Original exception stack was lost"
            printfn "PASS: dispatch-info preserves original failAtDock frame: %s" error.Message
    0
