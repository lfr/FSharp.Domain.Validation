module Result

open FSharp.ValidationBlocks.Fable

// Helper function to translate Result`2 objects into readable text
let text = function
| Ok block ->
    Block.value block
    |> sprintf "👍 \"%A\""
    |> fun s -> s.Replace("\n", "◄┘") // display new lines as '◄┘'
| Error e -> (sprintf "😢 %A" e).ToLower().Replace("[", "[input ")

