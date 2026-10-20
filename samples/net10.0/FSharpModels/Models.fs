module Playbook.Samples.Models

open System
open System.Text.Json

// snippet:compose
type Parcel = { Code: string; Mass: decimal }
type Dispatch = { Code: string; Mass: decimal; Dock: int }
let parcel = { Code = "MOSS-17"; Mass = 4.25M }
let dispatch = { Code = parcel.Code; Mass = parcel.Mass; Dock = 6 }
let redirected = { dispatch with Dock = 8 }
// endsnippet:compose


// snippet:json
type Shipment = Waiting | Loaded of code: string * kilograms: decimal
// An explicit wire contract is a valid .NET 10 alternative to a union converter.
type WireShipment = { Kind: string; Code: string; Kilograms: decimal }
let encode value =
    let wire =
        match value with
        | Waiting -> { Kind = "waiting"; Code = ""; Kilograms = 0M }
        | Loaded(code, mass) -> { Kind = "loaded"; Code = code; Kilograms = mass }
    JsonSerializer.Serialize wire
let decode json =
    let wire = JsonSerializer.Deserialize<WireShipment>(json: string) |> Option.ofObj |> Option.defaultWith (fun () -> raise (JsonException("Null shipment")))
    match wire.Kind with
    | "waiting" -> Waiting
    | "loaded" -> Loaded(wire.Code, wire.Kilograms)
    | _ -> raise (JsonException("Unknown shipment kind"))
// endsnippet:json

[<EntryPoint>]
let main _ =
    if dispatch.Code <> parcel.Code || dispatch.Mass <> parcel.Mass || redirected.Dock <> 8 || dispatch.Dock <> 6 then
        failwith "Record fields changed"
    for value in [ Waiting; Loaded("MOSS-17", 4.25M) ] do
        if decode (encode value) <> value then failwith "Union round trip changed"
    try
        JsonSerializer.Serialize(Loaded("MOSS-17", 4.25M)) |> ignore
        failwith "Expected unsupported built-in union serialization on .NET 10"
    with :? NotSupportedException -> ()
    Console.WriteLine("PASS: record composition, construction and explicit-wire union round trips; built-in .NET 10 union rejected")
    0
