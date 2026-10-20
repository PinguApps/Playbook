module Playbook.Samples.Diagnostics

open System
open System.Globalization

// snippet:culture
let display (culture: CultureInfo) (mass: decimal) (date: DateTime) =
    // Format values explicitly before interpolation: plain holes are invariant in F# 11.
    $"""Inspection: {mass.ToString("F2", culture)} kg on {date.ToString("d", culture)}"""
// endsnippet:culture

// snippet:reflectionfree
// Compile this assembly with OtherFlags=--reflectionfree.
type Reading = { Dock: string; Count: int }
type Inspection = Ready of Reading | Paused
let reading = { Dock = "MOSS"; Count = 7 }
let diagnostic = $"{Ready reading}"
let anonymousDiagnostic = string {| Dock = "MOSS"; Count = 7 |}
// endsnippet:reflectionfree

[<EntryPoint>]
let main _ =
    let previous = CultureInfo.CurrentCulture
    try
        CultureInfo.CurrentCulture <- CultureInfo.GetCultureInfo("de-DE")
        let mass = 4.25M
        if $"{mass}" <> "4.25" || $"{mass:F2}" <> "4.25" then failwith "Expected invariant holes"
        let date = DateTime(2026, 10, 20)
        if display (CultureInfo.GetCultureInfo("en-GB")) mass date <> "Inspection: 4.25 kg on 20/10/2026" then failwith "UK culture lost"
        if display (CultureInfo.GetCultureInfo("de-DE")) mass date <> "Inspection: 4,25 kg on 20.10.2026" then failwith "German culture lost"
        if diagnostic <> "Ready({ Dock = MOSS; Count = 7 })" || string Paused <> "Paused" then failwith "Generated diagnostic changed"
        if anonymousDiagnostic <> "{| Count = 7; Dock = MOSS |}" then failwith anonymousDiagnostic
        Console.WriteLine("PASS: explicit UK/German culture, invariant holes and generated reflection-free record/union diagnostics")
        0
    finally CultureInfo.CurrentCulture <- previous

