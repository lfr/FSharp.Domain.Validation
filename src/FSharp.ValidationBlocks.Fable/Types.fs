namespace FSharp.ValidationBlocks

open FSharp

type IBlock = ValidationBlocks.IBlock
type IBlock<'baseType, 'error> = ValidationBlocks.IBlock<'baseType, 'error> 
type IBlockOf<'baseType> = ValidationBlocks.IBlockOf<'baseType>

