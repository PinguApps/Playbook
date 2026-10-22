module Playbook.Samples.TideGrid

open System
open System.Runtime.CompilerServices

// snippet:grid
// Harbour chart coordinates stay external; storage always starts at zero.
type Chart<'T>(north: int, east: int, cells: 'T[,]) =
    member _.North = north
    member _.East = east
    member _.Cells = cells

let empty north east rows columns =
    Chart(north, east, Array2D.zeroCreate rows columns)

let filled north east rows columns value =
    Chart(north, east, Array2D.create rows columns value)

let surveyed north east rows columns readDepth =
    Chart(north, east, Array2D.init rows columns (fun row column -> readDepth (north + row) (east + column)))

let depthAt north east (chart: Chart<'T>) = chart.Cells[north - chart.North, east - chart.East]

let label (north: int) (east: int) (value: int) =
    String.Concat(north.ToString(), ",", east.ToString(), ":", value.ToString())

let relabel (chart: Chart<int>) =
    let labels = Array2D.mapi (fun row column value ->
        label (chart.North + row) (chart.East + column) value) chart.Cells
    Chart(chart.North, chart.East, labels)
// endsnippet:grid

let require condition message = if not condition then failwith message

[<EntryPoint>]
let main arguments =
    if Array.contains "--require-native" arguments then
        require (not RuntimeFeature.IsDynamicCodeSupported) "Expected the actual Native AOT executable"
    for rows, columns in [2, 3; 0, 3; 2, 0; 0, 0] do
        let blank: Chart<int> = empty -4 7 rows columns
        let fixedDepth = filled -4 7 rows columns 9
        let chart = surveyed -4 7 rows columns (fun north east -> north * 100 + east)
        let mapped = Array2D.map ((+) 1) chart.Cells
        let copied = Array2D.copy chart.Cells
        let rebased = Array2D.rebase chart.Cells
        let labelled = relabel chart
        for cells in [blank.Cells; fixedDepth.Cells; chart.Cells; mapped; copied; rebased] do
            require (Array2D.base1 cells = 0 && Array2D.base2 cells = 0) "Storage acquired a lower bound"
            require (Array2D.length1 cells = rows && Array2D.length2 cells = columns) "Dimensions changed"
        for row in 0 .. rows - 1 do
            for column in 0 .. columns - 1 do
                let north, east = -4 + row, 7 + column
                let expected = north * 100 + east
                require (depthAt north east blank = 0 && depthAt north east fixedDepth = 9) "Allocation values changed"
                require (depthAt north east chart = expected) "Coordinate translation changed"
                require (mapped[row,column] = expected + 1 && copied[row,column] = expected && rebased[row,column] = expected) "Helper lost values"
                require (labelled.Cells[row,column] = label north east expected) "mapi lost chart coordinates"
        if rows > 0 && columns > 0 then
            copied[0,0] <- 42
            require (chart.Cells[0,0] <> 42) "copy aliases storage"
    Console.WriteLine("PASS: zeroCreate/create/init/map/mapi/copy/rebase; translated chart coordinates; preserved values; empty dimensions; dynamic code=" + RuntimeFeature.IsDynamicCodeSupported.ToString())
    0
