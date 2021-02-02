namespace FSharp.ValidationBlocks

type IBlock = interface end
type IBlockOf<'baseType> = inherit IBlock

type IBlock<'baseType, 'error> =
    inherit IBlockOf<'baseType>
    abstract member Validate: ('baseType -> 'error list)

type internal NoValidation<'baseType, 'error> = private NewBlock of 'baseType with
    static member Block (src:'baseType) = NewBlock src
    interface IBlock<'baseType, 'error> with
        member _.Validate = fun _ -> []