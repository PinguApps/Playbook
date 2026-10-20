module Playbook.Samples.Models

open System
open System.Text.Json

// snippet:compose
type Parcel = { Code: string; Mass: decimal }
type Dispatch = { ...Parcel; Dock: int }
let parcel = { Code = "MOSS-17"; Mass = 4.25M }
let dispatch: Dispatch = { ...parcel; Dock = 6 }
let redirected: Dispatch = { ...dispatch; Dock = 8 }
let summary = {| ...parcel; Dock = 6 |}
// endsnippet:compose


// snippet:json
type Shipment = Waiting | Loaded of code: string * kilograms: decimal
// Built-in reflection-based support; no custom converter or JsonUnion attribute.
let encode (value: Shipment) = JsonSerializer.Serialize value
let decode (json: string) = JsonSerializer.Deserialize<Shipment> json
let restored = decode (encode (Loaded("MOSS-17", 4.25M)))
// endsnippet:json

[<EntryPoint>]
let main _ =
    if dispatch.Code <> parcel.Code || dispatch.Mass <> parcel.Mass || redirected.Dock <> 8 || dispatch.Dock <> 6 then
        failwith "Record fields changed"
    if summary.Code <> parcel.Code || summary.Mass <> parcel.Mass || summary.Dock <> 6 then failwith "Anonymous spread changed"
    for value in [ Waiting; Loaded("MOSS-17", 4.25M) ] do
        if decode (encode value) <> value then failwith "Union round trip changed"
        Console.WriteLine(encode value)
    if restored <> Loaded("MOSS-17", 4.25M) then failwith "Union payload changed"
    Console.WriteLine("PASS: record composition, construction and built-in union round trips")
    0
