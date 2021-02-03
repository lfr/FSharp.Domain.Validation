namespace FSharp.Domain.Validation

type BoxInfo =
    {
        BaseType : System.Type
        ErrorType : System.Type
        ValidateMethod : System.Reflection.MethodInfo
    }

module Reflection =

    type Flags = System.Reflection.BindingFlags
    
    let private noBox = Unchecked.defaultof<IBox<obj,obj>>
    let private typeError (t:System.Type) =
        sprintf "'%s' is not a Validation box." t.Name
    let internal isBox (t:System.Type) =
        t.GetInterfaces() |> Array.exists ((=) typeof<IBox>)
    
    let mutable private biCache : Map<System.Guid, BoxInfo> = Map.empty
    /// Gets reflection information about the specified box type
    let boxinfo (boxType:System.Type) =
        biCache
        |> Map.tryFind boxType.GUID
        |> Option.defaultWith
            (fun () ->
                if isBox boxType then
                    let validateMi =
                        boxType.GetMethods(Flags.NonPublic ||| Flags.Instance)
                        |> Array.find
                            (fun mi -> nameof noBox.Validate |> mi.Name.EndsWith)
                    let bi =
                        boxType.GetInterfaces()
                        |> Array.find
                            (fun i ->
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() = typedefof<IBox<_,_>>)
                        |> fun i -> i.GetGenericArguments()
                        |> fun x ->
                            {
                                BaseType = x.[0]
                                ErrorType = x.[1]
                                ValidateMethod = validateMi
                            
                            }
                    biCache <- biCache |> Map.add boxType.GUID bi
                    bi
                else typeError boxType |> invalidArg (nameof boxType))