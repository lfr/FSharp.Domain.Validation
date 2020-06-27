open System
open FSharp.ValidationBlocks
open FSharp.ValidationBlocks.Example
open FSharp.ValidationBlocks.Serialization
open System.Text.Json

[<EntryPoint>]
let main argv =

    // TODO make actual tests out of these
    let msg = "If all goes well these should be equal"
    let s = "te\0st\t"

    // check that Block.value = initial value
    let sb =
        Block.validate<FreeText> s
        |> function Ok b -> b | _ -> failwith "error"
    printfn "%s: %A = %A (canonicalized)" msg s (%sb)

    let options = System.Text.Json.JsonSerializerOptions()
    options.Converters.Add(ValidationBlockJsonConverterFactory())

    // check serialized
    let serialized = JsonSerializer.Serialize(sb, options)
    printfn "%s: %A = %A" msg serialized (s |> Block.Utils.canonicalize |> JsonSerializer.Serialize)
    
    // check deserialized
    let deserialized =
        JsonSerializer.Deserialize(serialized, typeof<FreeText>, options)
    printfn "%s: %A = %A" msg sb deserialized
    
    // check non-valid text
    let test = Block.validate<Text> "This is \not valid text."
    printfn "%s: %A" msg test
    
    0 // exit
