[<AutoOpen>]
module Helper

open FSharp.ValidationBlocks.Fable

// Helper function to handle errors and valid results
let displayResult = function
| Ok block ->
    Block.value block
    |> fun (s:string) -> s.Replace("\n", "◄┘") // display new lines as '◄┘'
    |> sprintf "👍 \"%s\""
| Error e -> sprintf "😢 %A" e |> fun s -> s.ToLower()

