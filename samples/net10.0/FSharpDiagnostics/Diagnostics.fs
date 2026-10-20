module Playbook.Samples.Diagnostics

open System
open System.Globalization

// snippet:culture
let display (culture: CultureInfo) (mass: decimal) (date: DateTime) =
    let amount = mass.ToString("F2", culture)
    let day = date.ToString("d", culture)
    $"Inspection: {amount} kg on {day}"
// endsnippet:culture

// snippet:reflectionfree
// Explicit formatting avoids relying on reflection-based generated ToString.
type Reading =
    { Dock: string; Count: int }
    override x.ToString() =
        "{ Dock = " + x.Dock + "; Count = " + x.Count.ToString(CultureInfo.InvariantCulture) + " }"
type Inspection =
    | Ready of Reading
    | Paused
    override x.ToString() =
        match x with
        | Ready reading -> "Ready(" + reading.ToString() + ")"
        | Paused -> "Paused"
let reading = { Dock = "MOSS"; Count = 7 }
let diagnostic = (Ready reading).ToString()
// endsnippet:reflectionfree

[<EntryPoint>]
let main _ =
    let previous = CultureInfo.CurrentCulture
    try
        CultureInfo.CurrentCulture <- CultureInfo.GetCultureInfo("en-US")
        let date = DateTime(2026, 10, 20)
        if display (CultureInfo.GetCultureInfo("en-GB")) 4.25M date <> "Inspection: 4.25 kg on 20/10/2026" then failwith "UK culture lost"
        if display (CultureInfo.GetCultureInfo("de-DE")) 4.25M date <> "Inspection: 4,25 kg on 20.10.2026" then failwith "German culture lost"
        if diagnostic <> "Ready({ Dock = MOSS; Count = 7 })" || Paused.ToString() <> "Paused" then failwith "Diagnostic changed"
        Console.WriteLine("PASS: explicit UK/German culture and record/union diagnostics")
        0
    finally CultureInfo.CurrentCulture <- previous
