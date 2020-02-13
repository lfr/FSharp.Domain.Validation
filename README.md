![nuget](https://img.shields.io/nuget/v/FSharp.ValidationBlocks.svg?style=badge&logo=appveyor&color=brightgreen)

# <img style="border-radius: 8%;" width="64" height="64" src="https://api.nuget.org/v3-flatcontainer/fsharp.validationblocks/0.9.3/icon"> <big>FSharp.ValidationBlocks</big>


A tiny library with huge potential to simplify and streamline your domain design, as you can see from the examples below.

| <center>Without ValidationBlocks</center> | <center>With ValidationBlocks</center> |
|---|---|
|<pre>// Single-case union style<br>type Tweet = private Tweet of string<br>module Tweet =<br>  let validate = function<br>  &#124; s when String.IsNullOrWhitespace s →<br>     IsMissingOrBlank &#124;&gt; Error<br>  &#124; s when s.Length > 280 →<br>     IsTooLong 280 &#124;&gt; Error<br>  &#124; s → Tweet s &#124;&gt; Ok<br>  let value (x:Tweet) =<br>     let (Tweet s) = x in s<br><br>// Object-oriented style<br>type Tweet private (s) =<br>   inherit Text (s)<br>   static member Validate s = function<br>   &#124; s when String.IsNullOrWhitespace s →<br>      IsMissingOrBlank &#124;&gt; Error<br>   &#124; s when s.Length > 280 →<br>      IsTooLong 280 &#124;&gt; Error<br>   &#124; s → Tweet s &#124;&gt; Ok</pre>|<pre>type Tweet = private Tweet of Text with<br>   interface IText with<br>      member _.Validate =<br>         fun s → [if s.Length > 280 then IsTooLong 280]</pre>

The examples without validation blocks have an additional validation case, with validation blocks this validation is implicit when stating that a `Tweet` is a `Tweet of Text`. Since validation blocks are built on top of each other, <u>only the validation that is specific to the block itself has to be explicitly declared</u>. One could imagine a similar behavior with OO style types, but there's no simple way to achieve that while keeping constructors private.

</center>

## Interface? Really?

While the visceral wish to leave everything OO behind is real, F# is a multi-paradigm language, and with the right discipline certain OO concepts can be extremely useful and expressive. For instance here an interface is a concise way **using a single statement** to both identify a type as a ValidationBlock **and** enforce the definition of validation rules. And you only reference interfaces in the block type definition, there's no references to interfaces anywhere in your code that creates or uses validation blocks.

## How it works

There's three straightforward components to using this library. First you declare your error types, then you declare your actual domain types like the `Tweet` above, and finally you use them with the provided functions.

### Define your errors

Before declaring types like the one above, you do need define your error type. This can be a brand new validation-specific discriminated union or part of your existing domain error sum type.

```fsharp
type TextError =
    | ContainsControlCharacters
    | ContainsTabs
    | IsTooLong of int
    | IsMissingOrBlank
```

While not strictly necessary, the following single line of code further improves the readability of your type declarations simply by abbreviating the `IBlock<_,_>` interface for a specific primitive type, for instance here's an abbreviation for blocks of strings.

```fsharp
type IText = inherit IBlock<string, TextError>
```

### Type declaration
Type declaration is reduced to the absolute minimum, a type is given a name and a private constructor, and the interface above that essentially makes them *validation blocks*. In addition to identifying the type as a ValidationBlock, the interface also ensures that you define the validation rule, which consists of a function of the primitive type that returns a list of one or more errors depending on one or more conditions respectively.

```fsharp
/// Single or multi-line non-null non-blank text without any additional validation
type FreeText = private FreeText of string with
    interface IText with
        member _.Validate =
            // error condition
            fun s ->
                [if s |> String.IsNullOrWhiteSpace then IsMissingOrBlank]
```

### Further simplifying type declarations
The type declaration above can be simplified further using the provided `=>` operator that combines a predicate of `string` (in this case) with an error when the predicate is true.

```fsharp
/// Alternative type declaration using the => operator
type FreeText = private FreeText of string with
    interface IText with
        member _.Validate =
            // error condition using validation operators
            String.IsNullOrWhiteSpace => IsMissingOrBlank
```
Make sure to open `FSharp.ValidationBlocks.Operators` in the file(s) where you declare your domain types.

### Creating and using blocks in your code

Using blocks is very easy, let's say you have a block binding called `email`, you can simply access the string value of it using the following:

```fsharp
// get the primitive value from the block
Block.value email // → string
```

There's also an experimental operator `%` that essentially does the same thing:

```fsharp
// experimental — same as Block.value
%email // → string
```

Creating a block from a string is equally simple:

```fsharp
Block.validate "cartermp@fsharp.org" // creates a block from a string by inferring the type
```

If type inference is not available at a specific code location, then you must specify the block type:

```fsharp
Block.validate<Email> "cartermp@fsharp.org" // creates an email block from a string
```
Note that the `validate` method returns a `Result`, which may not always be necessary, for instance when de-serializing values that are guaranteed to be valid, you can just use:
```fsharp
Block.ofUnchecked "this better be valid" // throws an exception if not valid
```

## Serialization

There's a `System.Text.Json.Serialization.JsonConverter` included, if you add it to your serialization options all blocks are serialized to (and de-serialized from) their primitive type. It is good practice to keep your serialized content unaware & independent from implementation concerns (such as `ValidationBlocks`).

## Not just strings

Strings are the perfect example as it's usually the first type for which developers stitch together validation logic, but this library works with anything, you can create a PositiveInt that's guaranteed to be greater than zero, or a FutureDate that's guaranteed to not be in the past. I haven't given other uses of these blocks much thought myself, but they're 100% generic so the sky is the limit.

## Conclusion

Using validation blocks you'll be able to create airtight domains guaranteed to never have invalid content with very little code. Depending on how much you like Lego&trade;, it may even make designing your domain *fun*. Not only you're writing less code, but your domain code files are much cleaner and nicer to work with. You'll also get [ROP](https://fsharpforfunandprofit.com/rop/) almost for free, and while there is a case to be made [against ROP](https://fsharpforfunandprofit.com/posts/against-railway-oriented-programming/), it's definitely a perfect match for content validation, especially content that may be entered by a user.

Tweet [@fishyrock](https://twitter.com/fishyrock) to contribute or give feedback!

### Full working example
You can find a full working example in the file [Text.fs](/lfr/FSharp.ValidationBlocks/blob/master/src/Example/Text.fs)