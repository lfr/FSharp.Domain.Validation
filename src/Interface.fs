namespace FSharp.ValidationBlocks

type IBlock = interface end

type IBlock<'baseType, 'error> =
    inherit IBlock
    abstract member Validate: ('baseType -> 'error list)

type internal NoValidation<'baseType, 'error> = private NewBlock of 'baseType with
    static member Block (src:'baseType) = NewBlock src
    interface IBlock<'baseType, 'error> with
        member _.Validate = fun _ -> []