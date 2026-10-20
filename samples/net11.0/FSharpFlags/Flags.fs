module Playbook.Samples.Flags

open System

// snippet:flags
[<Flags>]
type DockAccess =
    | None = 0
    | Unload = 1
    | Refuel = 2
    | Repair = 4

let granted = DockAccess.Unload ||| DockAccess.Refuel
let mayUnload = granted &&& DockAccess.Unload = DockAccess.Unload
let toggled = granted ^^^ DockAccess.Refuel
// endsnippet:flags

[<EntryPoint>]
let main _ =
    if int granted <> 3 || not mayUnload || toggled <> DockAccess.Unload then failwith "Flag semantics changed"
    for left in 0..7 do
        for right in 0..7 do
            let a, b = enum<DockAccess> left, enum<DockAccess> right
            if int (a ||| b) <> (left ||| right) || int (a &&& b) <> (left &&& right) || int (a ^^^ b) <> (left ^^^ right) then
                failwith "Flag truth table changed"
    printfn "PASS: integer flags preserve union/intersection/toggle across all three-bit combinations"
    0
