// Learn more about F# at http://fsharp.org

open System
open FSharp.ValidationBlocks
open FSharp.ValidationBlocks.Example
open FSharp.ValidationBlocks.Serialization
open System.Text.Json

[<EntryPoint>]
let main argv =

    // TODO make actual tests out of these
    let msg = "If all goes well these should be equal"
    let s = "test"

    // check that Block.value = initial value
    let sb = Unchecked.blockof<Text> s
    printfn "%s: %A = %A" msg s (%sb)

    let options = System.Text.Json.JsonSerializerOptions()
    options.Converters.Add(ValidationBlockJsonConverterFactory())

    // check serialized
    let serialized = JsonSerializer.Serialize(sb, options)
    printfn "%s: %A = %A" msg serialized (s |> JsonSerializer.Serialize)
    
    // check deserialized
    let deserialized =
        JsonSerializer.Deserialize(serialized, typeof<Text>, options)
    printfn "%s: %A = %A" msg sb deserialized
    
    // check non-valid text
    let test = Block.validate<Text> "This is \not valid text."
    printfn "If all goes well this should be an error: %A" test
    
    0 // exit
