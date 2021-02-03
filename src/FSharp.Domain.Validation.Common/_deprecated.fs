namespace FSharp

[<System.Obsolete("Use FSharp.Domain.Validation instead.")>]
module ValidationBlocks =

    [<System.Obsolete("Use FSharp.Domain.Validation.IBox instead.")>]
    type IBlock = FSharp.Domain.Validation.IBox
    [<System.Obsolete("Use FSharp.Domain.Validation.IBoxOf instead.")>]
    type IBlockOf<'baseType> = FSharp.Domain.Validation.IBoxOf<'baseType>
    [<System.Obsolete("Use FSharp.Domain.Validation.IBox instead.")>]
    type IBlock<'baseType, 'error> = FSharp.Domain.Validation.IBox<'baseType, 'error>

    open FSharp.Domain.Validation.Utils

    [<System.Obsolete("Use FSharp.Domain.Validation.Utils instead.")>]
    module Utils =

        /// Removes leading and trailing whitespace/control characters as well as
        /// any occurrences of the null (0x0000) character
        [<System.Obsolete("Use FSharp.Domain.Validation.Utils.containsControlCharacters instead.")>]
        let canonicalize = canonicalize

        /// Converts 'PascalCase' to 'lower case'.
        /// This can be useful to convert error union cases to error messages.
        [<System.Obsolete("Use FSharp.Domain.Validation.Utils.containsControlCharacters instead.")>]
        let depascalize = depascalize

        /// Returnes true if string contains control characters
        [<System.Obsolete("Use FSharp.Domain.Validation.Utils.containsControlCharacters instead.")>]
        let containsControlCharacters = containsControlCharacters
