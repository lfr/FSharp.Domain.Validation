[![nuget](https://img.shields.io/nuget/v/FSharp.ValidationBlocks.svg?style=badge&logo=nuget&color=brightgreen&cacheSeconds=21600&label=FSharp.ValidationBlocks)](https://www.nuget.org/packages/FSharp.ValidationBlocks/)
[![nuget](https://img.shields.io/nuget/v/FSharp.ValidationBlocks.svg?style=badge&logo=nuget&color=brightgreen&cacheSeconds=21600&label=FSharp.ValidationBlocks.Fable)](https://www.nuget.org/packages/FSharp.ValidationBlocks.Fable/)
<!-- [![twitter](https://img.shields.io/twitter/follow/LuisLikeIewis?label=Follow%20%40IuisIikeIewis&style=social)](http://twitter.com/intent/user?screen_name=LuisLikeIewis)  -->

|<sup><sub><sup>⚠&nbsp;</sup></sub></sup>ꜰᴀʙʟᴇ<sup><sub><sup>&nbsp;<sup>⚠</sup></sup></sub></sup>|For Fable projects only reference the package & namespace [FSharp.ValidationBlocks.Fable](https://www.nuget.org/packages/FSharp.ValidationBlocks.Fable/)|
:---: | :---

<br>

<p>
    <img width="100%" src="https://raw.githubusercontent.com/lfr/FSharp.ValidationBlocks/master/logo/hd.png">
</p>

A tiny F# library with huge potential to simplify your domain design, as you can see from the examples below:
<a name="anchor">
| <center>Without ValidationBlocks</center> | <center>With ValidationBlocks<a name="anchor2" /></center> |
|---|---|
|<pre><a href="#anchor"><img src="assets/style-single-case.svg" alt="// Single-case union style" width="100%" /></a><br>type Tweet = private Tweet of string<br>module Tweet =<br>  let validate = function<br>  &#124; s when String.IsNullOrWhitespace s →<br>     IsMissingOrBlank &#124;&gt; Error<br>  &#124; s when s.Length > 280 →<br>     IsTooLong 280 &#124;&gt; Error<br>  &#124; s → Tweet s &#124;&gt; Ok<br>  let value (Tweet s) = x in s<br><br><a href="#anchor"><img src="assets/style-oo.svg" alt="// Object-oriented style" width="100%" /></a><br>type Tweet private (s) = class end with<br>   static member Validate = function<br>   &#124; s when String.IsNullOrWhitespace s →<br>      IsMissingOrBlank &#124;&gt; Error<br>   &#124; s when s.Length > 280 →<br>      IsTooLong 280 &#124;&gt; Error<br>   &#124; s → Tweet s &#124;&gt; Ok<br>   interface IConstrained&lt;string&gt; with<br>      member x.Value = s</pre><a href="#anchor2"><img src="assets/scroll.svg" width="80%" /></a>|<pre>type Tweet = private Tweet of Text with<br>   interface TextBlock with<br>      member _.Validate =<br>         fun s -> s.Length > 280 => IsTooLong 280</pre><center><small>➡&nbsp;</small>[See the live demo!](https://impure.fun/FSharp.ValidationBlocks/demo/)</center>

You may have noticed that the examples on the left have an additional validation case. On the right this validation is implicit in the statement that a `Tweet` is a `Tweet of Text`. Since validation blocks are built on top of each other, the only rules that need to be explicitly declared are the rules specific to the block itself!




## Interface? Really?

F# is a multi-paradigm language. Whether you think it's a good thing or a bad thing (it's both), with the right discipline certain OO concepts can be harnessed for their expressiveness without any of the baggage. For instance here we use `interface` as an elegant way to both:

* Identify a type as a ValidationBlock
* Enforce the definition of validation rules

There's no other mentions of interfaces in the code that uses or creates validation blocks, only when you declare the block in your domain definition file.

## How it works

First you declare your error types, then you declare your actual domain types (i.e. `Tweet`), and finally you use them with the provided `Block.value` and `Block.validate` functions. These 3 simple steps are enough to ensure at compilation time that all your domain is always **always** valid!

<p align="center">
    <a href="https://impure.fun/FSharp.ValidationBlocks/demo/">
        <img src="assets/demo.gif" alt="demo" />
    </a>
</p>

### Declaring your errors

Before declaring types like the one above, you do need define your error type. This can be a brand new validation-specific discriminated union or part of an existing one.

```fsharp
type TextError =
    | ContainsControlCharacters
    | ContainsTabs
    | IsTooLong of int
    | IsMissingOrBlank
```

While not strictly necessary, the next single line of code greatly improves the readability of your type declarations by abbreviating the `IBlock<_,_>` interface for a specific primitive type.

```fsharp
// all string blocks can now interface TextBlock instead of IBlock<string, TextError>
type TextBlock = inherit IBlock<string, TextError>
```

### Declaring ValidationBlocks
Type declaration is reduced to the absolute minimum. A type is given a name, a private constructor, and the interface above that essentially makes it a **ValidationBlock** and ensures that you define the validation rule.

The  validation rule is a function of the primitive type (`string` here) that returns a list of one or more errors depending on the stated conditions.

```fsharp
/// Single or multi-line non-null non-blank text without any additional validation
type FreeText = private FreeText of string with
    interface TextBlock with
        member _.Validate =
            // validation ruleᵴ (only one)
            fun s ->
                [if s |> String.IsNullOrWhiteSpace then IsMissingOrBlank]
```

### Simpler validation rules with validation operators
The type declaration above can be simplified further using the provided `=>` and `==>` operators that here combine a predicate of `string` with the appropriate error.

```fsharp
/// Alternative type declaration using the ==> operator
type FreeText = private FreeText of string with
    interface TextBlock with
        member _.Validate =
            // same validation rule using validation operators
            String.IsNullOrWhiteSpace ==> IsMissingOrBlank
```
To use validation operators make sure to open `FSharp.ValidationBlocks.Operators` in the file(s) where you declare your ValidationBlocks. See [Text.fs](/src/Example/Text.fs#L57) for more examples of validation operators.

### Creating and using blocks in your code

Using validation blocks is easy, let's say you have a block binding called `email`, you can simply access its value using the following:

```fsharp
// get the primitive value from the block
Block.value email // → string
```

There's also an experimental operator `%` that essentially does the same thing. Note that this operator is *opened* automatically along with the namespace `FSharp.ValidationBlocks`. To avoid operator pollution this is advertised as experimental until the final operator characters are decided.

```fsharp
// experimental — same as Block.value
%email // → string
```

Creating a block is just as simple:

```fsharp
// create a block, canonicalizing (i.e. trimming) the input if it's a string
Block.validate s // → Ok 'block | Error e
```

`Block.validate` canonicalization consists of trimming both whitespace and control charcters, as well as removing ocurrences of the null character. While this should be the preferred way of creating blocks, it's possible to skip canonicalization by using `Block.verbatim` instead.

When type inference isn't possible, specify block type using the generic parameter:

```fsharp
// create a block when block type can be inferred
Block.validate<Tweet> s // → Ok Tweet | Error e
```

Do **not** force type inference using type annotations as it's unnecessarily verbose:

```fsharp
// incorrect example, do not copy/paste
let email : Result<Email, TextError list> =      // :(
    Block.validate "incorrect@dont.do"

// correct alternative
let email =
    Block.validate<Email> "cartermp@fsharp.lang" // :)
```

In both cases the resulting `email` is of type `Result<Email, TextError list>`.

## Exceptions instead of Error
The `Block.validate` method returns a `Result`, which may not always be necessary, for instance when de-serializing values that are guaranteed to be valid, you can just use:

```fsharp
// throws an exception if not valid
Unchecked.blockof "this better be valid"         // → 'block (inferred)
// same as above without type inference
Unchecked.blockof<Text> "this better be valid 2" // → Text
```

## Serialization

There's a `System.Text.Json.Serialization.JsonConverter` included, if you add it to your serialization options all blocks are serialized to (and de-serialized from) their primitive type. It is good practice to keep your serialized content independent from implementation considerations such as ValidationBlocks.

## Not just strings

Strings are the perfect example as it's usually the first type for which developers stitch together validation logic, but this library works with anything, you can create a `PositiveInt` that's guaranteed to be greater than zero, or a `FutureDate` that's guaranteed to not be in the past. Lists, vectors, any type of object really, if you can write a predicate against it, you can validate it. They're 100% generic so the sky is the limit.

## Ok looks good, but I'm still not sure

I've created a checklist to help you decide whether this library is a good match for your project:

- [x] My project contains domain objects/records

If your project satisfies all of the above this library is for you!

It dramatically reduces the amount of code necessary to make illegal states unrepresentable while being tiny and built only with `FSharp.Core`. It uses F# concepts in the way they're meant to be used, so if one day you decide to no longer use it, you can simply get rid of it and still keep all the single-case unions that you've defined. All you'll need to do is create your own implementation of `Block.validate` and `Block.value` or just make the single case constructors public.

In addition to the above, if you use the provided **JsonConverter**, your blocks will be serialized as their primitive type (i.e. string) and not as ValidationBlocks, so you're not adding any indirect dependency between this library and whatever is on the other side of your serialization.

## Conclusion

Using validation blocks you can create airtight domain objects guaranteed to never have invalid content. Not only you're writing less code, but your domain code files are much smaller and nicer to work with. You'll also get [ROP](https://fsharpforfunandprofit.com/rop/) almost for free, and while there is a case to be made [against ROP](https://fsharpforfunandprofit.com/posts/against-railway-oriented-programming/), it's definitely a perfect match for content validation, especially content that may be entered by a user.

### Full working example
You can find a full working example in the file [Text.fs](/src/Example/Text.fs)
