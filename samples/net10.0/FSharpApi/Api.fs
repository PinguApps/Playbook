module Playbook.Samples.Api

open System
open System.IO
open System.Xml.Linq

// snippet:delegates
type DockMath =
    static member Remaining (capacity: int) (occupied: int) = capacity - occupied

let remaining = Func<int, int, int>(fun capacity occupied -> DockMath.Remaining capacity occupied)
let freeSlots = remaining.Invoke(18, 7)
// endsnippet:delegates

// snippet:documentation
[<AbstractClass>]
type DockPolicy() =
    /// <summary>Returns the permitted loading mass in kilograms.</summary>
    abstract Limit: unit -> int

type HarbourPolicy() =
    inherit DockPolicy()
    /// <summary>Returns the permitted loading mass in kilograms.</summary>
    override _.Limit() = 240

/// <summary>Formats a numbered harbour berth.</summary>
let dockLabel (number: int) = $"Berth {number}"
// endsnippet:documentation

[<EntryPoint>]
let main _ =
    if freeSlots <> 11 || HarbourPolicy().Limit() <> 240 || dockLabel 6 <> "Berth 6" then failwith "API result changed"
    let xml = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "FSharpApi.xml"))
    let summaries = xml.Descendants(XName.Get "summary") |> Seq.map (fun s -> s.Value.Trim()) |> Seq.toArray
    if summaries |> Array.filter ((=) "Returns the permitted loading mass in kilograms.") |> Array.length <> 2 then
        failwith "Explicit summaries were not emitted"
    if not (Array.contains "Formats a numbered harbour berth." summaries) then failwith "Label summary missing"
    printfn "PASS: explicit delegate adapter and emitted repeated documentation"
    0
