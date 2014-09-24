﻿module console.console

open System

open core.sudoku

open format

// Things we may want to write
type ConsoleChar = 
    | CChar of char
    | CStr of string
    | ColouredChar of char * ConsoleColor
    | ColouredString of string * ConsoleColor
    | NL

val defaultGridChars : gridChars<seq<ConsoleChar>>

val sNL : seq<ConsoleChar>

val defaultSolutionChars : solutionChars<seq<ConsoleChar>>

val drawAnnotatedSymbol : CellContents -> CellContents -> ConsoleChar

val ConsoleWriteChar : ConsoleChar -> Unit

val drawFLFE : Candidate -> Candidate -> CellContents -> CellContents -> ConsoleChar

val drawFL2 : Candidate -> Candidate -> CellContents -> CellContents -> CellContents -> CellAnnotation -> ConsoleChar
