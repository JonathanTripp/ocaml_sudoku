﻿module core.hints

open System
open System.Text

open sudoku
open puzzlemap

exception CellStateInvalid

let first (set : Set<'a>) = Set.toList set |> List.head

let rec doSetSubsets (list : List<'a>) (size : int) (prefix : List<'a>) : List<List<'a>> = 
    match list with
    | x :: xs when size > 0 -> 
        if size = 1 then (x :: prefix) :: doSetSubsets xs 1 prefix
        else 
            let inc = doSetSubsets xs (size - 1) (x :: prefix)
            let dec = doSetSubsets xs size prefix

            List.append inc dec
    | _ -> []

let rec setSubsets (list : List<'a>) (size : int) : List<List<'a>> = doSetSubsets list size []

(*
    let s0 = []
    let p00 = setSubsets s0 0
    let p01 = setSubsets s0 1
    let p02 = setSubsets s0 2

    let s1 = [ 1 ]
    let p10 = setSubsets s1 0
    let p11 = setSubsets s1 1
    let p12 = setSubsets s1 2
    let p13 = setSubsets s1 3

    let s2 = [ 1; 2 ]
    let p20 = setSubsets s2 0
    let p21 = setSubsets s2 1
    let p22 = setSubsets s2 2
    let p23 = setSubsets s2 3
    let p24 = setSubsets s2 4

    let s3 = [ 1; 2; 3 ]
    let p30 = setSubsets s3 0
    let p31 = setSubsets s3 1
    let p32 = setSubsets s3 2
    let p33 = setSubsets s3 3
    let p34 = setSubsets s3 4
    let p35 = setSubsets s3 5
*)

let firstOpt (set : Set<'a>) = 
    let l = Set.toList set
    match l with
    | s :: _ -> Some s
    | _ -> None

type CandidateReduction = 
    { cell : Cell
      candidates : Set<Digit> }
    override this.ToString() = 
        String.Format("Cell {0}, Candidates {1}", this.cell, String.Join(",", Set.toArray this.candidates))

type HintDescription = 
    { primaryHouses : Set<House>
      secondaryHouses : Set<House>
      candidateReductions : Set<CandidateReduction>
      setCellValueAction : Value option
      pointers : Set<CandidateReduction>
      focus : Set<Digit> }
    override this.ToString() = 
        let sb = StringBuilder()

        sb.AppendLine(String.Format("Primary Houses {0}", String.Join(",", Set.toArray this.primaryHouses))) |> ignore
        sb.AppendLine(String.Format("Secondary Houses {0}", String.Join(",", Set.toArray this.secondaryHouses))) 
        |> ignore
        sb.AppendLine(String.Format("Pointers {0}", String.Join(",", Set.toArray this.pointers))) |> ignore

        Set.iter (fun (cr : CandidateReduction) -> sb.AppendLine(String.Format("  {0}", cr)) |> ignore) 
            this.candidateReductions

        sb.ToString()

// To draw a cell we may want to display extra information...
type CellAnnotation = 
    { setValue : Digit option
      primaryHintHouse : bool
      secondaryHintHouse : bool
      setValueReduction : Digit option
      reductions : Set<Digit>
      pointers : Set<Digit>
      focus : Set<Digit> }

type CellAnnotations = Map<Cell, CellAnnotation>

type HintDescription2 = 
    { primaryHouses : Set<House>
      secondaryHouses : Set<House>
      candidateReductions : Set<CandidateReduction>
      setCellValueAction : Value option
      annotations : CellAnnotations }
    override this.ToString() = 
        let sb = StringBuilder()

        sb.AppendLine(String.Format("Primary Houses {0}", String.Join(",", Set.toArray this.primaryHouses))) |> ignore
        sb.AppendLine(String.Format("Secondary Houses {0}", String.Join(",", Set.toArray this.secondaryHouses))) 
        |> ignore

        Set.iter (fun (cr : CandidateReduction) -> sb.AppendLine(String.Format("  {0}", cr)) |> ignore) 
            this.candidateReductions

        sb.AppendLine(String.Format("Set Cell {0}", this.setCellValueAction)) |> ignore

        sb.ToString()

let mhas (allCells : Set<Cell>) (cellHouseCells : CellHouseCells) (puzzleHouseCells : HouseCells) (hd : HintDescription) : HintDescription2 = 

    let annotationLookup (cell : Cell) : CellAnnotation = 

        let setValue, setValueReduction = 
            match hd.setCellValueAction with
            | Some setCellValueAction -> 
                
                let r1 = 
                    if setCellValueAction.cell = cell then Some setCellValueAction.digit
                    else None
                
                let r3 = 
                    let cells = cellHouseCells.Get setCellValueAction.cell

                    if Set.contains cell cells then Some setCellValueAction.digit
                    else None
                
                r1, r3
            | None -> None, None
        
        let cellCandidateReductions =
            hd.candidateReductions
            |> Set.filter (fun pointer -> cell = pointer.cell) 
        
        let reductions = 
            match firstOpt cellCandidateReductions with
            | Some cr -> cr.candidates
            | _ -> set []
        
        let cellPointers =
            hd.pointers
            |> Set.filter (fun pointer -> cell = pointer.cell)
        
        let pointers = 
            match firstOpt cellPointers with
            | Some cr -> cr.candidates
            | _ -> set []
        
        let primaryHouseCells =
            hd.primaryHouses
            |> Set.map puzzleHouseCells.Get
            |> Set.unionMany

        let secondaryHouseCells =
            hd.secondaryHouses
            |> Set.map puzzleHouseCells.Get
            |> Set.unionMany

        { CellAnnotation.setValue = setValue
          primaryHintHouse = Set.contains cell primaryHouseCells
          secondaryHintHouse = Set.contains cell secondaryHouseCells
          setValueReduction = setValueReduction
          reductions = reductions
          pointers = pointers
          focus = hd.focus }

    let annotations =
        allCells
        |> Set.map (fun cell -> (cell, annotationLookup cell))
        |> Map.ofSeq

    { HintDescription2.primaryHouses = hd.primaryHouses
      secondaryHouses = hd.secondaryHouses
      candidateReductions = hd.candidateReductions
      setCellValueAction = hd.setCellValueAction
      annotations = annotations }