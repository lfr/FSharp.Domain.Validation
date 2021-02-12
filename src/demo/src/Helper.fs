module Result

open FSharp.Domain.Validation
open FSharp.Domain.Validation.Utils

// Helper function to translate Result`2 objects into readable text
let toText = function
| Ok block ->
    Box.value block
    |> sprintf "👍 \"%A\""
    |> fun s -> s.Replace("\n", "◄┘") // display new lines as '◄┘'
| Error e -> (sprintf "😢 %A" e |> depascalize).Replace("[", "[input ")

