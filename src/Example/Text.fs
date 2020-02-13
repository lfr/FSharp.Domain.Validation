namespace FSharp.ValidationBlocks.Example

open FSharp.ValidationBlocks
open System.Text.RegularExpressions
open FSharp.ValidationBlocks.Operators

/// Define all the possible errors that the blocks can yield
type TextError =
    | ContainsControlCharacters
    | ContainsTabs
    | ExceedsMaximumLength of int
    | IsMissingOrBlank

/// This interface in not strictly necessary but it makes type declarations
/// and function signatures a lot lighter and more readable
type IText = inherit IBlock<string, TextError>

/// While not strictly necessary, a module such as this one provides a better API
/// than using the Block module's functions directly
module Text =

    /// Validates the given string into a Text block option result
    let checkOptional<'text when 'text :> IText> s =
        if System.String.IsNullOrWhiteSpace s then None |> Ok
        else Block.validate<'text, TextError> (s.Trim()) |> Result.map Some

    /// Validates the given string into a Text block result
    let check<'text when 'text :> IText> s =
        match checkOptional<'text> s with
        | Ok None -> Error [IsMissingOrBlank]
        | Ok (Some s) -> Ok s | Error e -> Error e

    /// Validates the given string into a Text block, raising an exception when validation fails
    let ofUnchecked s =
        match check s with
        | Ok x -> x
        | Error e -> sprintf "Attempt to access error Result: %A." e |> failwith


/// Single or multi-line text without any validation
type FreeText = private FreeText of string with
    interface IText with
        member _.Validate =
            // System.String.IsNullOrWhiteSpace => IsMissingOrBlank
            fun s ->
                [if s |> System.String.IsNullOrWhiteSpace then IsMissingOrBlank]

                            
/// Single line of text (no control characters)
type Text = private Text of FreeText with
    interface IText with
        member _.Validate =
            fun s ->
                [if s |> Regex("\p{C}").IsMatch then ContainsControlCharacters]


/// Text restricted to 280 characters at most when trimmed
/// (the current maximum length of a tweeet), without tabs
type Tweet = private Tweet of FreeText with
    interface IText with
        member _.Validate =
            fun s ->
                [
                    if s.Contains('\t') then ContainsTabs
                    if s.Length > 280 then ExceedsMaximumLength 280
                ]



// Alternative type definition using operators for single validation condition
type FreeText' = private FreeText' of string with
    interface IText with
        member _.Validate =
            System.String.IsNullOrWhiteSpace => IsMissingOrBlank

// Alternative type definition using operators for multiple validation conditions
type Tweet' = private Tweet' of FreeText with
interface IText with
    member _.Validate = !? [
        (=) '\t' |> String.exists => ContainsTabs
        String.length >> (>) 280 => ExceedsMaximumLength 280
    ]