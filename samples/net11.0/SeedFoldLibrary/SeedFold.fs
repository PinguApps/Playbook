// snippet:library
module SeedLaboratory

let inline foldSeeds ([<InlineIfLambda; OptimizeClosureIfNotInlined>] folder: int -> int -> int) initial (readings: int[]) =
    let mutable total = initial
    for reading in readings do
        total <- folder total reading
    total

// Keep a fair unannotated control with the same API and loop.
let inline foldControl ([<InlineIfLambda>] folder: int -> int -> int) initial (readings: int[]) =
    let mutable total = initial
    for reading in readings do
        total <- folder total reading
    total
// endsnippet:library
