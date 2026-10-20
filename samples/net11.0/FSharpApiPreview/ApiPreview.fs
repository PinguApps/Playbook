module Playbook.Samples.ApiPreview

open System
open System.Diagnostics.CodeAnalysis

// snippet:generic
module HarbourExtensions =
    type Uri with
        member address.ManifestWidth() = address.Host.Length

open HarbourExtensions

module Manifest =
    let inline width (value: ^T) = ((^T) : (member ManifestWidth: unit -> int) value)

let addressWidth = Manifest.width (Uri("https://moss.example/docks"))
// endsnippet:generic

// snippet:named
type DockBooking() =
    [<RequireNamedArguments>]
    member _.Reserve(arrivals: int, departures: int) = arrivals - departures

let balance = DockBooking().Reserve(departures = 3, arrivals = 12)
// endsnippet:named

[<EntryPoint>]
let main _ =
    if addressWidth <> 12 || balance <> 9 then failwith "Preview API result changed"
    if typeof<RequireNamedArgumentsAttribute>.Assembly <> typeof<DockBooking>.Assembly then
        failwith "The supplied attribute unexpectedly came from another assembly"
    if not (isNull (typeof<obj>.Assembly.GetType("System.Diagnostics.CodeAnalysis.RequireNamedArgumentsAttribute"))) then
        failwith "RC2 BCL attribute availability changed; reconcile the supplied definition"
    printfn "PASS: public SRTP extension; reordered named call; locally supplied attribute"
    0
