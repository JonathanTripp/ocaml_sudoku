module core.force

open System
open System.Diagnostics

open smap
open sudoku
open puzzlemap
open setCell

let isPencilMarksCellContents (cellContents : cellContents) : bool =
    match cellContents with
    | BigNumber _ -> false
    | PencilMarks _ -> true

let isValidCellContents (cellContents : cellContents) : bool =
    match cellContents with
    | BigNumber _ -> true
    | PencilMarks candidates -> Digits.count candidates > 0

let isValid (solution : solution) (cells : cell list) : bool =
    cells
    |> List.map (SMap.get solution.current)
    |> List.forall isValidCellContents

let rec searchr (p : puzzleMap) (solution : solution) (existing : solution list) : solution list = 
    let emptyCell : cell option =
        p.cells
        |> List.tryFind (SMap.get solution.current >> isPencilMarksCellContents)

    match emptyCell with
    | Some cell ->
        let candidates =
            let cellContents = SMap.get solution.current cell
            match cellContents with
            | BigNumber _ -> []
            | PencilMarks candidates -> candidates |> Digits.toList
        
        candidates
        |> List.map
            (fun digit ->
                let setCellValue = makeValue cell digit
                
                let current = setCellDigitApply p setCellValue solution.current

                let newSolution =
                    { solution with
                        current = current
                        steps = (Placement setCellValue) :: solution.steps }

                (*Console.WriteLine ("Trying {0}", setCellValue) *)

                if isValid newSolution p.cells then
                    (*Console.WriteLine(">")*)
                    searchr p newSolution existing
                else
                    (*
                    let cell =
                        List.find
                            (fun cell -> 
                                let cellContents = newSolution.current cell
                                match cellContents with
                                | BigNumber _ -> false
                                | PencilMarks candidates -> Digits.count candidates = 0)
                            cells

                    Console.WriteLine(String.Format("< {0}", cell))
                    *)
                    [])
            |> List.concat
    | None -> solution :: existing

let solve (p : puzzleMap) (solution : solution) : solution list =
    let stopwatch = new Stopwatch()
    stopwatch.Start()

    let results = searchr p solution []

    stopwatch.Stop()
    Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed)

    results
