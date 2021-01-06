[<AutoOpen>]
module Types

open FSharp.ValidationBlocks.Fable
open FSharp.ValidationBlocks.Utils
open System

/// Define all the possible errors that the blocks can yield
type TextError =
    | ``Contains ctrl chars``
    | ``Is missing or blank``
    | ``Is not a valid integer``

/// This interface in not strictly necessary but it makes type declarations
/// and function signatures a lot more readable
type TextBlock = inherit IBlock<string, TextError>

/// Single or multi-line text without any validation
type FreeText = private FreeText of string with
    interface TextBlock with
        member _.Validate =
            fun s ->
                [if s |> String.IsNullOrWhiteSpace then ``Is missing or blank``]
                            
/// Single line of text (no control characters)
type Text = private Text of FreeText with
    interface TextBlock with
        member _.Validate =
            fun s ->
                [if containsControlCharacters s then ``Contains ctrl chars``]

/// String representation of an integer
type Integer = private Integer of FreeText with
    interface TextBlock with
        member _.Validate =
            fun s ->
                if Int32.TryParse(s) |> fst then [] else
                    [``Is not a valid integer``]




/// This type can be used to test that FSharp.ValidationBlocks.Fable throws
/// exceptions on blocks of types that are discriminated unions,
/// test with: Block.validate<TestDu> (Ok "some text")
[<System.Obsolete>]
type TestDu = private TestDu of Result<string, string> with
    interface IBlock<Result<string, string>, TextError> with
        member _.Validate = fun _ -> []

