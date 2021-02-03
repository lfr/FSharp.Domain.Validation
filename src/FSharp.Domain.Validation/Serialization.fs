namespace FSharp.Domain.Validation

open System.Text.Json
open System.Text.Json.Serialization
open FSharp.Domain.Validation.Reflection

module Serialization =

    type private ValidationJsonConverter<'box> () =
        inherit JsonConverter<'box>()
            
        override _.CanConvert t = typeof<'box>.IsAssignableFrom(t)
    
        override _.Read(reader, t, options) =            
            let bi = t |> boxinfo
            let value = JsonSerializer.Deserialize(&reader, bi.BaseType, options)
            let result = Runtime.verbatim t value
            result.Unbox()
            
        override _.Write (writer, value, options) =
            let bi = value.GetType() |> boxinfo
            JsonSerializer.Serialize(writer, value |> Box.unwrap, bi.BaseType, options)


    type ValidationJsonConverterFactory() =
        inherit JsonConverterFactory()

        let typedef = typedefof<ValidationJsonConverter<_>>

        let implementsIBox (t:System.Type) =
            t.GetInterfaces() |> Array.exists (fun i -> i.IsGenericType &&
                i.GetGenericTypeDefinition() = typedefof<IBox<_,_>>)    
                
        override _.CanConvert t =
            t |> typeof<IBox>.IsAssignableFrom && t |> implementsIBox
                
        override _.CreateConverter (t, _) =
            let converterType = typedef.MakeGenericType([|t|])
            System.Activator.CreateInstance converterType :?> JsonConverter