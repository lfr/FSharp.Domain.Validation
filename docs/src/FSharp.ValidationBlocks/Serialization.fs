namespace FSharp.ValidationBlocks


open System.Text.Json
open System.Text.Json.Serialization
open FSharp.ValidationBlocks.Reflection

module Serialization =


    type private ValidationBlockJsonConverter<'block> () =
        inherit JsonConverter<'block>()
            
        override _.CanConvert t = typeof<'block>.IsAssignableFrom(t)
    
        override _.Read(reader, t, options) =            
            let bi = t |> blockInfo
            let value = JsonSerializer.Deserialize(&reader, bi.BaseType, options)
            let result = Runtime.verbatim t value
            result.Unbox()
            
        override _.Write (writer, value, options) =
            let bi = value.GetType() |> blockInfo
            JsonSerializer.Serialize(writer, value |> Block.unwrap, bi.BaseType, options)


    type ValidationBlockJsonConverterFactory() =
        inherit JsonConverterFactory()

        let typedef = typedefof<ValidationBlockJsonConverter<_>>

        let implementsIBlock (t:System.Type) =
            t.GetInterfaces() |> Array.exists (fun i -> i.IsGenericType &&
                i.GetGenericTypeDefinition() = typedefof<IBlock<_,_>>)    
                
        override _.CanConvert t =
            t |> typeof<IBlock>.IsAssignableFrom && t |> implementsIBlock
                
        override _.CreateConverter (t, _) =
            let converterType = typedef.MakeGenericType([|t|])
            System.Activator.CreateInstance converterType :?> JsonConverter