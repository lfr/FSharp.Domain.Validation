namespace FSharp.ValidationBlocks

module Utils =

    /// All control characters
    let ctrlChars =
        [int System.Char.MinValue .. int System.Char.MaxValue]
        |> List.map char
        |> List.filter
            (fun c -> System.Char.IsControl c)
        |> List.distinct
        |> List.toArray

    /// All control or whitespace characters
    let ctrlWhitespaceChars =
        [int System.Char.MinValue .. int System.Char.MaxValue]
        |> List.map char
        |> List.filter
            (fun c -> System.Char.IsWhiteSpace c)
        |> List.append (List.ofArray ctrlChars)
        |> List.distinct
        |> List.toArray

    /// Removes leading and trailing whitespace/control characters as well as
    /// any occurrences of the null (0x0000) character
    let inline canonicalize (inp:'a) (inpType:System.Type) : 'a =
        if inpType <> typeof<string> then inp else
            (box inp :?> string).Trim(ctrlWhitespaceChars).Replace("\0","") |> unbox


    /// Returnes true if string contains control characters
    let containsControlCharacters (s:string) =
        ctrlChars |> Array.exists s.Contains
