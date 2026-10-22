module Playbook.Samples.CompilerHost

open FSharp.Compiler.CodeAnalysis
open Playbook.Samples.CompilerHostChecks

// snippet:configuration
let sharedWorkspace = FSharpChecker.Create()
let separateWorkspace = FSharpChecker.Create(shareImportedAssemblies = false)
// The default is already true. False opts this checker out of import sharing.
// endsnippet:configuration

[<EntryPoint>]
let main _ =
    run (fun () -> [| "default", sharedWorkspace; "disabled", separateWorkspace |])
    0
