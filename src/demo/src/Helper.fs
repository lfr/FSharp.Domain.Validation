module Result

open FSharp.ValidationBlocks.Fable
open FSharp.ValidationBlocks.Utils

// Helper function to translate Result`2 objects into readable text
let toText = function
| Ok block ->
    Block.value block
    |> sprintf "👍 \"%A\""
    |> fun s -> s.Replace("\n", "◄┘") // display new lines as '◄┘'
| Error e -> (sprintf "😢 %A" e |> depascalize).Replace("[", "[input ")

