module core.puzzlemap

open smap
open sudoku

val makeColumn : int -> column

val makeRow : int -> row

val makeCell : column -> row -> cell

val makeStack : int -> stack

val makeBand : int -> band

val makeBox : stack -> band -> box

val makeValue : cell -> digit -> value

val makeCandidate : cell -> digit -> candidate

val makeCandidateReduction : cell -> digits -> candidateReduction

[<NoComparisonAttribute;NoEqualityAttribute>]
type puzzleMap =
    {
        columns : column list
        rows : row list
        cells : cell list
        stacks : stack list
        bands : band list
        boxes : box list
        houses : house list
        (* for a column, return the cells in it *)
        columnCells : SMap<column, cell list>
        (* for a row, return the cells in it *)
        rowCells : SMap<row, cell list>
        (* for a column, which stack is it in? *)
        columnStack : SMap<column, stack>
        (* for a stack, return the columns in it *)
        stackColumns : SMap<stack, column list>
        (* for a row, which band is it in? *)
        rowBand : SMap<row, band>
        (* for a band, return the rows in it *)
        bandRows : SMap<band, row list>
        (* for a cell, which box is it in? *)
        cellBox : SMap<cell, box>
        (* for a box, return the cells in it *)
        boxCells : SMap<box, cell list>
        (* for a house, return the cells in it *)
        houseCells : SMap<house, cells>
        cellHouseCells : SMap<cell, cells>
        housesCells : houses -> cells
        houseCellCandidateReductions : house -> cellCandidates -> candidateReductions

        //abstract member houseCellCandidates : SMap<house, cellCandidates>
    }

val tPuzzleMap : puzzleShape -> puzzleMap
