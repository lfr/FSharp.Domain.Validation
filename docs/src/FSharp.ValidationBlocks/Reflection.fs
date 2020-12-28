namespace FSharp.ValidationBlocks

type BlockInfo =
    {
        BaseType : System.Type
        ErrorType : System.Type
        ValidateMethod : System.Reflection.MethodInfo
    }

module Reflection =

    type Flags = System.Reflection.BindingFlags

    // Fable does not support nameof
    let private ``nameof noBlock.Validate`` = "Validate"
    let private ``nameof blockType`` = "blockType"
    
    let private typeError (t:System.Type) =
        sprintf "'%s' is not a ValidationBlock." t.Name
    let internal isBlock (t:System.Type) =
        t.GetInterfaces() |> Array.exists ((=) typeof<IBlock>)
    
    let mutable private biCache : Map<System.Guid, BlockInfo> = Map.empty
    /// Gets reflection information about the specified block type
    let blockInfo (blockType:System.Type) =
        biCache
        |> Map.tryFind blockType.GUID
        |> Option.defaultWith
            (fun () ->
                if isBlock blockType then
                    let validateMi =
                        blockType.GetMethods(Flags.NonPublic ||| Flags.Instance)
                        |> Array.find
                            (fun mi -> ``nameof noBlock.Validate`` |> mi.Name.EndsWith)
                    let bi =
                        blockType.GetInterfaces()
                        |> Array.find
                            (fun i ->
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() = typedefof<IBlock<_,_>>)
                        |> fun i -> i.GetGenericArguments()
                        |> fun x ->
                            {
                                BaseType = x.[0]
                                ErrorType = x.[1]
                                ValidateMethod = validateMi
                            
                            }
                    biCache <- biCache |> Map.add blockType.GUID bi
                    bi
                else typeError blockType |> invalidArg (``nameof blockType``))