module Playbook.Samples.TideGrid

open System

// snippet:grid
// This remains a valid JIT representation on the older release.
let empty north east rows columns : int[,] = Array2D.zeroCreateBased north east rows columns
let filled north east rows columns value = Array2D.createBased north east rows columns value
let surveyed north east rows columns readDepth = Array2D.initBased north east rows columns readDepth
let depthAt north east (chart: int[,]) = chart[north,east]
let relabel chart = Array2D.mapi (fun north east value -> sprintf "%d,%d:%d" north east value) chart
// endsnippet:grid

let require condition message = if not condition then failwith message

[<EntryPoint>]
let main _ =
    for rows, columns in [2, 3; 0, 3; 2, 0; 0, 0] do
        let blank = empty -4 7 rows columns
        let fixedDepth = filled -4 7 rows columns 9
        let chart = surveyed -4 7 rows columns (fun north east -> north * 100 + east)
        let mapped = Array2D.map ((+) 1) chart
        let copied = Array2D.copy chart
        let rebased = Array2D.rebase chart
        let labelled = relabel chart
        for cells in [blank; fixedDepth; chart; mapped; copied] do
            require (Array2D.base1 cells = -4 && Array2D.base2 cells = 7) "Original lower bounds changed"
            require (Array2D.length1 cells = rows && Array2D.length2 cells = columns) "Dimensions changed"
        require (Array2D.base1 rebased = 0 && Array2D.base2 rebased = 0) "rebase must translate storage"
        for row in 0 .. rows - 1 do
            for column in 0 .. columns - 1 do
                let north, east = -4 + row, 7 + column
                let expected = north * 100 + east
                require (depthAt north east blank = 0 && depthAt north east fixedDepth = 9) "Allocation values changed"
                require (depthAt north east chart = expected) "Original coordinate changed"
                require (mapped[north,east] = expected + 1 && copied[north,east] = expected && rebased[row,column] = expected) "Helper lost values"
                require (labelled[north,east] = sprintf "%d,%d:%d" north east expected) "mapi lost coordinates"
    printfn "PASS: original based JIT storage; create/init/zeroCreate/map/mapi/copy/rebase; preserved coordinates and values; empty dimensions; runtime %O" Environment.Version
    0
