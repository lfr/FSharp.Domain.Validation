namespace FSharp.ValidationBlocks


open System.Text.Json
open System.Text.Json.Serialization

module Serialization =


    type private ValidationBlockJsonConverter<'block> () =
        inherit JsonConverter<'block>()

        let genericParameters (t:System.Type) =
            t.GetInterfaces()
            |> Array.find (fun i -> i.IsGenericType && i.GetGenericTypeDefinition() = typedefof<IBlock<_,_>>)
            |> fun i -> i.GetGenericArguments() |> fun x -> x.[0], x.[1]
            
        override _.CanConvert t = typeof<'block>.IsAssignableFrom(t)
    
        override _.Read(reader, t, options) =            
            let baseType, errorType = t |> genericParameters
            let value = JsonSerializer.Deserialize(&reader, baseType, options)
            let result = Block.runtimeWrap t errorType value
            result.Unbox()
            
        override _.Write (writer, value, options) =
            let baseType, _ = value.GetType() |> genericParameters
            JsonSerializer.Serialize(writer, value |> Block.unwrap, baseType, options)


    type ValidationBlockJsonConverterFactory() =
        inherit JsonConverterFactory()

        let typedef = typedefof<ValidationBlockJsonConverter<_>>

        let implementsIBlock (t:System.Type) =
            t.GetInterfaces() |> Array.exists (fun i -> i.IsGenericType &&
                i.GetGenericTypeDefinition() = typedefof<IBlock<_,_>>)    
                
        override _.CanConvert t =
            t |> typeof<IBlock>.IsAssignableFrom && t |> implementsIBlock
                
        override x.CreateConverter (t, _) =
            let converterType = typedef.MakeGenericType([|t|])
            System.Activator.CreateInstance converterType :?> JsonConverter