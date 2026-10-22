module Playbook.Samples.CompilerHost

open FSharp.Compiler.CodeAnalysis
open Playbook.Samples.CompilerHostChecks

// snippet:configuration
let workspace = FSharpChecker.Create()
// This previous release has no shareImportedAssemblies parameter.
// Reuse one checker for its workspace and let the existing caches do their work.
// endsnippet:configuration

[<EntryPoint>]
let main _ =
    run (fun () -> [| "previous", workspace |])
    0
