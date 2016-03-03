module hints.wing

open core.sudoku
open core.puzzlemap
open core.hints

let makeHints (p : puzzleMap) (cellCandidates : cellCandidates) pointerCells primaryHouses secondaryHouses candidate = 
    let pointers =
        pointerCells
        |> Cells.map (fun cell -> 
            { candidateReduction.cell = cell
              candidates = Digits.singleton candidate }) 
        |> CandidateReductions.ofSet

    let colCells =
        secondaryHouses
        |> Houses.map p.houseCells.Get
        |> Cells.unionMany

    let candidatesReductions = 
        Cells.difference colCells pointerCells
        |> Cells.map (fun cell -> 
            { candidateReduction.cell = cell
              candidates = cellCandidates.Get cell })
        |> CandidateReductions.ofSet
        |> CandidateReductions.filter (fun cr -> Digits.contains candidate cr.candidates)
        |> CandidateReductions.map (fun cr -> 
            { candidateReduction.cell = cr.cell
              candidates = Digits.singleton candidate }) 
        |> CandidateReductions.ofSet

    if CandidateReductions.count candidatesReductions > 0 then 
        Some { hintDescription.candidateReductions = candidatesReductions
               primaryHouses = primaryHouses
               secondaryHouses = secondaryHouses
               pointers = pointers
               setCellValueAction = None
               focus = Digits.empty }
    else None

let xWingsPerHouseCandidate (p : puzzleMap) (cellCandidates : cellCandidates) (house1 : house) (house2 : house) (candidate : digit) = 

    let houseCandidateCells1 =
        house1
        |> p.houseCells.Get
        |> Cells.map (fun cell -> { candidateReduction.cell = cell; candidates = cellCandidates.Get cell }) 
        |> CandidateReductions.ofSet

    let houseCandidateCells2 =
        house2
        |> p.houseCells.Get
        |> Cells.map (fun cell -> { candidateReduction.cell = cell; candidates = cellCandidates.Get cell }) 
        |> CandidateReductions.ofSet

    let hht1 = CandidateReductions.filter (fun cr -> Digits.contains candidate cr.candidates) houseCandidateCells1
    let hht2 = CandidateReductions.filter (fun cr -> Digits.contains candidate cr.candidates) houseCandidateCells2

    match house1, house2 with
    | HRow row1, HRow row2 -> 
        let cols1 = CandidateReductions.map (fun cr -> cr.cell.col) hht1 |> Columns.ofSet
        let cols2 = CandidateReductions.map (fun cr -> cr.cell.col) hht2 |> Columns.ofSet

        let cols = Columns.union cols1 cols2

        if Columns.count cols1 = 2 && Columns.count cols2 = 2 && Columns.count cols = 2 then 
            let row1Cells =
                cols
                |> Columns.map (fun col -> { cell.col = col; row = row1 })
                |> Cells.ofSet

            let row2Cells = 
                cols
                |> Columns.map (fun col -> { cell.col = col; row = row2 }) 
                |> Cells.ofSet

            let pointerCells =
                [| row1Cells; row2Cells |]
                |> Cells.unionMany

            let primaryHouses = Houses.ofArray [| house1; house2 |]
            let secondaryHouses =
                cols
                |> Columns.map HColumn
                |> Houses.ofSet

            makeHints p cellCandidates pointerCells primaryHouses secondaryHouses candidate

        else None

    | HColumn col1, HColumn col2 -> 
        let rows1 = CandidateReductions.map (fun cr -> cr.cell.row) hht1 |> Rows.ofSet
        let rows2 = CandidateReductions.map (fun cr -> cr.cell.row) hht2 |> Rows.ofSet

        let rows =
            Rows.union rows1 rows2

        if Rows.count rows1 = 2 && Rows.count rows2 = 2 && Rows.count rows = 2 then 
            let col1Cells =
                rows
                |> Rows.map (fun row -> { cell.col = col1; row = row })
                |> Cells.ofSet

            let col2Cells =
                rows
                |> Rows.map (fun row -> { cell.col = col2; row = row })
                |> Cells.ofSet

            let pointerCells =
                [| col1Cells; col2Cells |]
                |> Cells.unionMany

            let primaryHouses = Houses.ofArray [| house1; house2 |]
            let secondaryHouses =
                rows
                |> Rows.map HRow
                |> Houses.ofSet

            makeHints p cellCandidates pointerCells primaryHouses secondaryHouses candidate

        else None
    | _ -> None

let xWingsPerHouse (p : puzzleMap) (cellCandidates : cellCandidates) (house1 : house) 
    (house2 : house) : hintDescription array = 


    let houseCandidates1 =
        house1
        |> p.houseCells.Get
        |> Cells.map cellCandidates.Get
        |> Digits.unionMany

    let houseCandidates2 =
        house2
        |> p.houseCells.Get
        |> Cells.map cellCandidates.Get
        |> Digits.unionMany

    Digits.intersect houseCandidates1 houseCandidates2
    |> Digits.toArray
    |> Array.map (xWingsPerHouseCandidate p cellCandidates house1 house2)
    |> Array.choose id

let xWings (p : puzzleMap) (cellCandidates : cellCandidates) : hintDescription array =
    let rows =
        p.rows
        |> Array.map HRow

    let cols =
        p.columns
        |> Array.map HColumn

    let rowHints1 = 
        rows
        |> Array.mapi 
            (fun i row1 -> 
                Array.sub rows (i + 1) (rows.Length - i - 1)
                |> Array.mapi
                    (fun j row2 -> xWingsPerHouse p cellCandidates row1 row2)) 

    let rowHints = 
        rowHints1
        |> Array.concat
        |> Array.concat

    let colHints1 = 
        cols
        |> Array.mapi 
            (fun i col1 -> 
                Array.sub cols (i + 1) (cols.Length - i - 1)
                |> Array.mapi
                    (fun j col2 -> xWingsPerHouse p cellCandidates col1 col2)) 
    
    let colHints = 
        colHints1
        |> Array.concat
        |> Array.concat

    [| rowHints; colHints |]
    |> Array.concat

let yWingsPerHouseCandidate (p : puzzleMap) (cellCandidates : cellCandidates)
    (house1 : house) (house2 : house) houseCandidateCells1 houseCandidateCells2 (candidate : digit) = 
    let hht1 = CandidateReductions.filter (fun cr -> Digits.contains candidate cr.candidates) houseCandidateCells1
    let hht2 = CandidateReductions.filter (fun cr -> Digits.contains candidate cr.candidates) houseCandidateCells2

    match house1, house2 with
    | HRow row1, HRow row2 -> 
        let cols1 = CandidateReductions.map (fun cr -> cr.cell.col) hht1 |> Columns.ofSet
        let cols2 = CandidateReductions.map (fun cr -> cr.cell.col) hht2 |> Columns.ofSet

        let cols = Columns.union cols1 cols2

        if Columns.count cols1 = 2 && Columns.count cols2 = 2 && Columns.count cols = 2 then 
            let row1Cells =
                cols
                |> Columns.map (fun col -> { cell.col = col; row = row1 })
                |> Cells.ofSet

            let row2Cells =
                cols
                |> Columns.map (fun col -> { cell.col = col; row = row2 })
                |> Cells.ofSet

            let pointerCells =
                Cells.union row1Cells row2Cells

            let primaryHouses = Houses.ofArray [| house1; house2 |]
            let secondaryHouses =
                cols
                |> Columns.map HColumn
                |> Houses.ofSet

            makeHints p cellCandidates pointerCells primaryHouses secondaryHouses candidate

        else None

    | HColumn col1, HColumn col2 -> 
        let rows1 = CandidateReductions.map (fun cr -> cr.cell.row) hht1 |> Rows.ofSet
        let rows2 = CandidateReductions.map (fun cr -> cr.cell.row) hht2 |> Rows.ofSet

        let rows =
            Rows.union rows1 rows2

        if Rows.count rows1 = 2 && Rows.count rows2 = 2 && Rows.count rows = 2 then 
            let col1Cells = 
                rows
                |> Rows.map (fun row -> { cell.col = col1; row = row })
                |> Cells.ofSet

            let col2Cells =
                rows
                |> Rows.map (fun row -> { cell.col = col2; row = row }) 
                |> Cells.ofSet

            let pointerCells = Cells.union col1Cells col2Cells

            let primaryHouses = Houses.ofArray [| house1; house2 |]
            let secondaryHouses =
                rows
                |> Rows.map HRow
                |> Houses.ofSet

            makeHints p cellCandidates pointerCells primaryHouses secondaryHouses candidate

        else None
    | _ -> None

let yWingsPerHouse (p : puzzleMap) (cellCandidates : cellCandidates) (row1 : row) 
    (row2 : row) (col1 : column) (col2 : column)  : hintDescription array = 

    let cell11 = { cell.col = col1; row = row1 }
    let cell12 = { cell.col = col2; row = row1 }
    let cell21 = { cell.col = col1; row = row2 }
    let cell22 = { cell.col = col2; row = row2 }
    
    let cells = [| cell11; cell12; cell21; cell22 |]

    let candidateCells =
        cells
        |> Array.map (fun cell -> { candidateReduction.cell = cell; candidates = cellCandidates.Get cell })

    let ccell11 = { candidateReduction.cell = cell11; candidates = cellCandidates.Get cell11 }
    let ccell12 = { candidateReduction.cell = cell12; candidates = cellCandidates.Get cell12 }
    let ccell21 = { candidateReduction.cell = cell21; candidates = cellCandidates.Get cell21 }
    let ccell22 = { candidateReduction.cell = cell22; candidates = cellCandidates.Get cell22 }

    let allNonEmpty =
        candidateCells
        |> Array.forall (fun cr -> Digits.count cr.candidates > 0)

    if allNonEmpty then 
        let triples = 
            [| (cell12, [| ccell11; ccell12; ccell22 |], cell21)
               (cell22, [| ccell12; ccell22; ccell21 |], cell11)
               (cell21, [| ccell22; ccell21; ccell11 |], cell12)
               (cell11, [| ccell21; ccell11; ccell12 |], cell22) |]

        triples
        |> Array.map
          (fun (pivot1, triple, other) -> 
            let ccs = Array.map (fun cr -> cr.candidates) triple

            let allPairs =
                ccs
                |> Array.forall (fun c -> Digits.count c = 2)

            if allPairs then 
                let allCandidates =
                    ccs
                    |> Digits.unionMany
                    |> Digits.toArray

                if Array.length allCandidates = 3 then 
                    match triple with
                    | [| left; pivot; right; _ |] -> 
                        let removee = Digits.difference (Digits.ofArray allCandidates) pivot.candidates

                        if Digits.count removee = 1 && (left.candidates <> right.candidates) && 
                            Digits.isSubset removee (cellCandidates.Get other) then

                            let candidateReductions = { candidateReduction.cell = other; candidates = removee }

                            let pointers =
                                triple
                                |> CandidateReductions.ofArray

                            let primaryHouses = 
                                [| HRow row1
                                   HRow row2
                                   HColumn col1
                                   HColumn col2 |]
                                |> Houses.ofArray

                            let primaryHouseCells = p.housesCells primaryHouses

                            Some { hintDescription.candidateReductions = CandidateReductions.singleton candidateReductions
                                   primaryHouses = primaryHouses
                                   secondaryHouses = Houses.empty
                                   pointers = pointers
                                   setCellValueAction = None
                                   focus = Digits.empty }
                        else None
                    | _ -> None
                else None
            else None)
        |> Array.choose id
    else Array.empty

let yWings (p : puzzleMap) (cellCandidates : cellCandidates) : hintDescription array =
    let colsa = p.columns

    let hints =
        p.rows
        |> Array.mapi 
            (fun i row1 ->
                Array.sub p.rows (i + 1) (p.rows.Length - i - 1)
                |> Array.mapi 
                    (fun j row2 -> 
                        colsa
                        |> Array.mapi 
                            (fun k col1 -> 
                                Array.sub colsa (k + 1) (colsa.Length - k - 1)
                                |> Array.mapi
                                    (fun l col2 -> yWingsPerHouse p cellCandidates row1 row2 col1 col2)))) 

    hints
    |> Array.concat
    |> Array.concat
    |> Array.concat
    |> Array.concat
