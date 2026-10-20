module Playbook.Samples.Constructors

// snippet:construct
type Crate = { Label: string; Capacity: int }
let compact: Crate = { Label = "FERN"; Capacity = 12 }
let spacious: Crate = { Capacity = 24; Label = "REED" }
// endsnippet:construct

[<EntryPoint>]
let main _ =
    if compact <> { Label = "FERN"; Capacity = 12 } || spacious <> { Label = "REED"; Capacity = 24 } then failwith "Construction changed"
    System.Console.WriteLine("PASS: positional and reordered named record construction preserve all fields")
    0
