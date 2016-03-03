module core.eliminateCandidate

open sudoku
open puzzlemap
open hints

let eliminateCandidateApply (p : puzzleMap) (candidate : candidate) (current : current) : current = 

    let update (cell : cell) : cellContents = 
        let cellContents = current.Get cell
        match cellContents with
        | BigNumber _ -> cellContents
        | PencilMarks candidates -> 
            if candidate.cell = cell then PencilMarks(Digits.remove candidate.digit candidates)
            else cellContents

    makeMapLookup<cell, cellContents> p.cells update
    :> current

let eliminateCandidateHintDescription (p: puzzleMap) (candidate : candidate) : hintDescription =
    let cr = 
        { candidateReduction.cell = candidate.cell
          candidates = Digits.singleton candidate.digit }

    { hintDescription.primaryHouses = Houses.empty
      secondaryHouses = Houses.empty
      candidateReductions = CandidateReductions.singleton cr
      setCellValueAction = None
      pointers = CandidateReductions.empty
      focus = Digits.empty }

let eliminateCandidateStep (p : puzzleMap) (candidate : candidate) (solution : solution) : solution =
    { solution with current = eliminateCandidateApply p candidate solution.current
                    steps = (Eliminate candidate) :: solution.steps }
