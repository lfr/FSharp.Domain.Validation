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
/// and function signatures a lot more readable
type IText = inherit IBlock<string, TextError>

/// This is a good place to define IText-specific functions
module Text =

    /// Validates the given string treating null/blank as a valid result of None
    let ofOptional<'text when 'text :> IText> s : Result<IText option, TextError list> =
        if System.String.IsNullOrWhiteSpace s then None |> Ok
        else Block.validate (s.Trim()) |> Result.map Some


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



// Alternative type definition using operators (for single condition)
type FreeText' = private FreeText' of string with
    interface IText with
        member _.Validate =
            System.String.IsNullOrWhiteSpace ==> IsMissingOrBlank

// Alternative type definition using operators (for multiple conditions)
type Tweet' = private Tweet' of FreeText with
    interface IText with
        member _.Validate =
            fun s -> !? [
                s.Contains('\t')  => ContainsTabs
                s.Length > 280    => ExceedsMaximumLength 280
            ]
