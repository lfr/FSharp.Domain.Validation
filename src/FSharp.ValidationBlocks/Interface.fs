namespace FSharp.ValidationBlocks

#if FABLE_COMPILER
[<System.Obsolete("For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks.")>]
#endif
type IBlock = interface end

#if FABLE_COMPILER
[<System.Obsolete("For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks.")>]
#endif
type IBlockOf<'baseType> = inherit IBlock

#if FABLE_COMPILER
[<System.Obsolete("For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks.")>]
#endif
type IBlock<'baseType, 'error> =
    inherit IBlockOf<'baseType>
    abstract member Validate: ('baseType -> 'error list)

#if FABLE_COMPILER
[<System.Obsolete("For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks.")>]
#endif
type internal NoValidation<'baseType, 'error> = private NewBlock of 'baseType with
    static member Block (src:'baseType) = NewBlock src
    interface IBlock<'baseType, 'error> with
        member _.Validate = fun _ -> []