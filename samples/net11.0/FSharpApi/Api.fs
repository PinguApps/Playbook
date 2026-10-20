module Playbook.Samples.Api

open System
open System.IO
open System.Xml.Linq

// snippet:delegates
type DockMath =
    static member Remaining (capacity: int) (occupied: int) = capacity - occupied

let remaining = Func<int, int, int>(DockMath.Remaining)
let freeSlots = remaining.Invoke(18, 7)
// endsnippet:delegates

// snippet:documentation
[<AbstractClass>]
type DockPolicy() =
    /// <summary>Returns the permitted loading mass in kilograms.</summary>
    abstract Limit: unit -> int

type HarbourPolicy() =
    inherit DockPolicy()
    /// <inheritdoc/>
    override _.Limit() = 240

/// <include file='ApiDocs.xml' path='/docs/member[@name="dock-label"]/*'/>
let dockLabel (number: int) = $"Berth {number}"
// endsnippet:documentation

[<EntryPoint>]
let main _ =
    if freeSlots <> 11 || HarbourPolicy().Limit() <> 240 || dockLabel 6 <> "Berth 6" then
        failwith "API result changed"
    let xml = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "FSharpApi.xml"))
    let members = xml.Descendants(XName.Get "member") |> Seq.toArray
    let inherited = members |> Array.find (fun m -> (m.Attribute(XName.Get "name") |> Option.ofObj |> Option.map (fun attribute -> attribute.Value) |> Option.defaultValue "") = "M:Playbook.Samples.Api.HarbourPolicy.Limit")
    if isNull (inherited.Element(XName.Get "inheritdoc")) then failwith "inheritdoc tag was lost"
    let included = members |> Array.find (fun m -> (m.Attribute(XName.Get "name") |> Option.ofObj |> Option.map (fun attribute -> attribute.Value) |> Option.defaultValue "") = "M:Playbook.Samples.Api.dockLabel(System.Int32)")
    if included.Value.Trim() <> "Formats a numbered harbour berth." || not (isNull (included.Element(XName.Get "include"))) then
        failwith "External XML was not expanded"
    printfn "PASS: delegate result; emitted inheritdoc retained; include expanded at compile time"
    0
