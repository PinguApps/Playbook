module Playbook.Samples.Flags

open System

// snippet:flags
type LegacyAccess =
    | Unload = 'A'
    | Refuel = 'B'

// These expressions compile on the old compiler, but cannot implement flags.
let rejectedOperations = [ (fun () -> LegacyAccess.Unload ||| LegacyAccess.Refuel)
                           (fun () -> LegacyAccess.Unload &&& LegacyAccess.Refuel)
                           (fun () -> LegacyAccess.Unload ^^^ LegacyAccess.Refuel) ]
// endsnippet:flags

[<EntryPoint>]
let main _ =
    for operation in rejectedOperations do
        try
            operation () |> ignore
            failwith "Expected unsupported char-backed flags"
        with :? NotSupportedException -> printfn "PASS: legacy char enum operation throws NotSupportedException"
    0
