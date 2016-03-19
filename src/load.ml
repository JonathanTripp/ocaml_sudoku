module load

open smap
open sudoku
open puzzlemap

(* Load a sudoku given as a single line of gridSize*gridSize characters *)
let loadPuzzle (cells : cell list) (alphabetisedLine : digit option list) : SMap<cell, digit option> = 
    List.zip cells alphabetisedLine
    |> SMap.ofList

let load (puzzleShape : puzzleShape) (sudoku : string) : solution = 

    let charToDigit (trialDigit : char) : digit option = 
        let compareAlpha (Digit charDigit) = trialDigit = charDigit in
        List.tryFind compareAlpha puzzleShape.alphabet
        in

    let alphabetisedLine =
        sudoku
        |> List.ofSeq
        |> List.map charToDigit
        in

    let p = tPuzzleMap puzzleShape in

    let given = loadPuzzle p.cells alphabetisedLine in

    let current = givenToCurrent p.cells given (Digits.ofList puzzleShape.alphabet) in

    { solution.given = given;
      current = current;
      steps = [ Load sudoku ] }