namespace FSharp.Domain.Validation.Example


open FSharp.Domain.Validation
open FSharp.Domain.Validation.Operators
open FSharp.Domain.Validation.Utils

/// Define all the possible errors that the boxes can yield
type TextError =
    | ContainsControlCharacters
    | ContainsTabs
    | ExceedsMaximumLength of int
    | IsMissingOrBlank

#if !FABLE_COMPILER

/// This interface in not strictly necessary but it makes type declarations
/// and function signatures a lot more readable
type TextBox = inherit IBox<string, TextError>

/// This is a good place to define IText-specific functions
module Text =

    /// Validates the given trimmed string treating null/blank as a valid result of None
    /// Use: Text.optional<Tweet> "hello!" or Text.optional "hello!"
    let optional<'box when 'box :> TextBox> s : Result<'box option, TextError list> =
        if System.String.IsNullOrWhiteSpace s then Ok None
        else Box.validate<'box> s |> Result.map Some

    /// Creates a box option from the given string if valid, otherwise throws an exception
    /// Use: Text.uncheckedOptional<Tweet> "hello!" or Text.uncheckedOptional "hello!"
    let uncheckedOptional<'box when 'box :> TextBox> s : 'box option =
        if System.String.IsNullOrWhiteSpace s then None else
            Unchecked.boxof s |> Some


/// Single or multi-line text without any validation
type FreeText = private FreeText of string with
    interface TextBox with
        member _.Validate =
            // System.String.IsNullOrWhiteSpace => IsMissingOrBlank
            fun s ->
                [if s |> System.String.IsNullOrWhiteSpace then IsMissingOrBlank]

                            
/// Single line of text (no control characters)
type Text = private Text of FreeText with
    interface TextBox with
        member _.Validate =
            fun s ->
                [if containsControlCharacters s then ContainsControlCharacters]


/// Text restricted to 280 characters at most when trimmed
/// (the current maximum length of a tweeet), without tabs
type Tweet = private Tweet of FreeText with
    interface TextBox with
        member _.Validate =
            fun s ->
                [
                    if s.Contains("\t") then ContainsTabs
                    if s.Length > 280 then ExceedsMaximumLength 280
                ]



// Alternative type definition using composing operator and a single condition
type FreeText' = private FreeText' of string with
    interface TextBox with
        member _.Validate =
            System.String.IsNullOrWhiteSpace ==> IsMissingOrBlank

// Alternative type definition using non-composing operators and multiple conditions
type Tweet' = private Tweet' of FreeText with
    interface TextBox with
        member _.Validate =
            fun s -> !? [
                s.Contains("\t")  => ContainsTabs
                s.Length > 280    => ExceedsMaximumLength 280
            ]

#endif