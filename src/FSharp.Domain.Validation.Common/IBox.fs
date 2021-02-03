namespace FSharp.Domain.Validation

type IBox = interface end
type IBoxOf<'baseType> = inherit IBox

type IBox<'baseType, 'error> =
    inherit IBoxOf<'baseType>
    abstract member Validate: ('baseType -> 'error list)

type internal NoValidation<'baseType, 'error> = private NewBox of 'baseType with
    static member Box (src:'baseType) = NewBox src
    interface IBox<'baseType, 'error> with
        member _.Validate = fun _ -> []