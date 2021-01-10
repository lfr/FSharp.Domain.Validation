namespace FSharp.ValidationBlocks

open System

module Utils =

    /// All control or whitespace characters
    let ctrlWhitespaceChars =
        [int System.Char.MinValue .. int System.Char.MaxValue]
        |> List.map char
        |> List.filter
            (fun c -> System.Char.IsControl c || System.Char.IsWhiteSpace c)
        |> List.toArray
        
    /// All control characters
    let ctrlChars =
        ctrlWhitespaceChars
        |> Array.filter
            (fun c -> System.Char.IsControl c)

    /// Removes leading and trailing whitespace/control characters as well as
    /// any occurrences of the null (0x0000) character
    let inline canonicalize (inp:'a) (inpType:System.Type) : 'a =
        match box inp with
        | null -> null
        | x when inpType <> typeof<string> -> x
        | x -> (box x :?> string).Trim(ctrlWhitespaceChars).Replace("\0","") |> box
        |> unbox

    /// Converts 'PascalCase' to 'lower case'.
    /// This can be useful to convert error union cases to error messages.
    let inline depascalize (s:string) =        
        s.ToCharArray ()
        |> Array.mapi
            (fun i c ->
                match c with
                | c when i = 0 -> [|Char.ToLowerInvariant c|]
                | c when Char.IsUpper c -> [|' '; Char.ToLowerInvariant c|]
                | c -> [|c|])
        |> Array.concat
        |> String

    /// Returnes true if string contains control characters
    let containsControlCharacters (s:string) =
        ctrlChars |> Array.exists s.Contains
