module SeedConsumer

open SeedLaboratory
open System.Runtime.CompilerServices

// snippet:consumer
let mutable factoryCalls = 0

[<MethodImpl(MethodImplOptions.NoInlining)>]
let makeFolder (effects: ResizeArray<int>) failAt bias : int -> int -> int =
    factoryCalls <- factoryCalls + 1
    fun total reading ->
        effects.Add reading
        if reading = failAt then invalidOp "damaged seed tray"
        total + reading + bias

[<MethodImpl(MethodImplOptions.NoInlining)>]
let opaque readings effects failAt bias =
    foldSeeds (makeFolder effects failAt bias) 0 readings

[<MethodImpl(MethodImplOptions.NoInlining)>]
let control readings effects failAt bias =
    foldControl (makeFolder effects failAt bias) 0 readings

[<MethodImpl(MethodImplOptions.NoInlining)>]
let literal readings bias = foldSeeds (fun total reading -> total + reading + bias) 0 readings

[<EntryPoint>]
let main _ =
    let readings = [| 2; 5; 9 |]
    let effects = ResizeArray<int>()
    let expected = 28 // three measured readings, plus a captured bias of four each time
    if opaque readings effects -1 4 <> expected || Seq.toList effects <> [2;5;9] then failwith "opaque result/order"
    effects.Clear()
    if control readings effects -1 4 <> expected || Seq.toList effects <> [2;5;9] then failwith "control result/order"
    if literal readings 4 <> expected then failwith "literal result"
    if factoryCalls <> 2 then failwith "one factory call per opaque/control invocation"
    for run in [opaque; control] do
        effects.Clear()
        let mutable rejected = false
        try run readings effects 5 4 |> ignore
        with :? System.InvalidOperationException as error when error.Message = "damaged seed tray" -> rejected <- true
        if not rejected || Seq.toList effects <> [2;5] then failwith "exception propagation/order"
    if opaque [||] effects -1 4 <> 0 then failwith "empty batch"
    printfn "PASS: separate consumer, literal/opaque/control=28, effects=2,5,9; exception stops at 5; empty=0; runtime=%O" System.Environment.Version
    0
// endsnippet:consumer
