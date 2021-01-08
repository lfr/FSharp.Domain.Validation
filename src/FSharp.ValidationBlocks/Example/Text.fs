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

#if !FABLE_COMPILER

/// This interface in not strictly necessary but it makes type declarations
/// and function signatures a lot more readable
type TextBlock = inherit IBlock<string, TextError>

/// This is a good place to define IText-specific functions
module Text =

    /// Validates the given trimmed string treating null/blank as a valid result of None
    /// Use: Text.optional<Tweet> "hello!" or Text.optional "hello!"
    let optional<'block when 'block :> TextBlock> s : Result<'block option, TextError list> =
        if System.String.IsNullOrWhiteSpace s then Ok None
        else Block.validate<'block> s |> Result.map Some

    /// Creates a block option from the given string if valid, otherwise throws an exception
    /// Use: Text.uncheckedOptional<Tweet> "hello!" or Text.uncheckedOptional "hello!"
    let uncheckedOptional<'block when 'block :> TextBlock> s : 'block option =
        if System.String.IsNullOrWhiteSpace s then None else
            Unchecked.blockof s |> Some


/// Single or multi-line text without any validation
type FreeText = private FreeText of string with
    interface TextBlock with
        member _.Validate =
            // System.String.IsNullOrWhiteSpace => IsMissingOrBlank
            fun s ->
                [if s |> System.String.IsNullOrWhiteSpace then IsMissingOrBlank]

                            
/// Single line of text (no control characters)
type Text = private Text of FreeText with
    interface TextBlock with
        member _.Validate =
            fun s ->
                [if s |> Regex("\p{C}").IsMatch then ContainsControlCharacters]


/// Text restricted to 280 characters at most when trimmed
/// (the current maximum length of a tweeet), without tabs
type Tweet = private Tweet of FreeText with
    interface TextBlock with
        member _.Validate =
            fun s ->
                [
                    if s.Contains("\t") then ContainsTabs
                    if s.Length > 280 then ExceedsMaximumLength 280
                ]



// Alternative type definition using composing operator and a single condition
type FreeText' = private FreeText' of string with
    interface TextBlock with
        member _.Validate =
            System.String.IsNullOrWhiteSpace ==> IsMissingOrBlank

// Alternative type definition using non-composing operators and multiple conditions
type Tweet' = private Tweet' of FreeText with
    interface TextBlock with
        member _.Validate =
            fun s -> !? [
                s.Contains("\t")  => ContainsTabs
                s.Length > 280    => ExceedsMaximumLength 280
            ]

#endif