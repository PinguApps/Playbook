module Playbook.Samples.ApiPreview

open System

// snippet:generic
// A projection supplies the operation without changing or wrapping Uri.
let width project value = project value
let addressWidth = width (fun (address: Uri) -> address.Host.Length) (Uri("https://moss.example/docks"))
// endsnippet:generic

// snippet:named
type DockBooking() =
    member _.Reserve(arrivals: int, departures: int) = arrivals - departures

// Names are available, but this compiler does not require them.
let balance = DockBooking().Reserve(departures = 3, arrivals = 12)
let positionalBalance = DockBooking().Reserve(12, 3)
// endsnippet:named

[<EntryPoint>]
let main _ =
    if addressWidth <> 12 || balance <> 9 || positionalBalance <> 9 then failwith "API comparison changed"
    printfn "PASS: projected operation; optional named and positional calls"
    0
