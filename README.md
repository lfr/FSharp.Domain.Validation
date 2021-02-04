⚠ You can test the **new library & namespace** by referencing 🔗&nbsp;[0.9.78-rc2](https://www.nuget.org/packages?packagetype=&sortby=relevance&q=FSharp.Domain.Validation&prerel=True) from your project 🎉


<sup>Older API available here:</sup>&nbsp;&nbsp;[![nuget](https://img.shields.io/nuget/v/FSharp.Domain.Validation.svg?style=badge&logo=nuget&color=brightgreen&cacheSeconds=21600&label=FSharp.Domain.Validation)](https://www.nuget.org/packages/FSharp.Domain.Validation/)
[![nuget](https://img.shields.io/nuget/v/FSharp.Domain.Validation.Fable.svg?style=badge&logo=nuget&color=brightgreen&cacheSeconds=21600&label=FSharp.Domain.Validation.Fable)](https://www.nuget.org/packages/FSharp.Domain.Validation.Fable/)
<!-- [![twitter](https://img.shields.io/twitter/follow/LuisLikeIewis?label=Follow%20%40IuisIikeIewis&style=social)](http://twitter.com/intent/user?screen_name=LuisLikeIewis)  -->

|ꜰᴀʙʟᴇ|For Fable projects *only* reference the package `FSharp.Domain.Validation.Fable`|
:---: | :---

<br>

<p>
    <img width="100%" src="https://raw.githubusercontent.com/lfr/FSharp.Domain.Validation/master/logo/hd.png">
</p>

A tiny F# library with huge potential to simplify your domain design, as you can see from the examples below:
<a name="anchor">
| <center>Without this package 👎</center> | <center>Using this package 👍<a name="anchor2" /></center> |
|---|---|
|<pre><a href="#anchor"><img src="assets/style-single-case.svg" alt="// Single-case union style" width="100%" /></a><br>type Tweet = private Tweet of string<br>module Tweet =<br>  let validate = function<br>  &#124; s when String.IsNullOrWhitespace s →<br>     IsMissingOrBlank &#124;&gt; Error<br>  &#124; s when s.Length > 280 →<br>     IsTooLong 280 &#124;&gt; Error<br>  &#124; s → Tweet s &#124;&gt; Ok<br>  let value (Tweet s) = x in s<br><br><a href="#anchor"><img src="assets/style-oo.svg" alt="// Object-oriented style" width="100%" /></a><br>type Tweet private (s) = class end with<br>   static member Validate = function<br>   &#124; s when String.IsNullOrWhitespace s →<br>      IsMissingOrBlank &#124;&gt; Error<br>   &#124; s when s.Length > 280 →<br>      IsTooLong 280 &#124;&gt; Error<br>   &#124; s → Tweet s &#124;&gt; Ok<br>   interface IConstrained&lt;string&gt; with<br>      member x.Value = s</pre><a href="#anchor2"><img src="assets/scroll.svg" width="80%" /></a>|<pre>type Tweet = private Tweet of Text with<br>   interface TextBox with<br>      member _.Validate =<br>         fun s -> s.Length > 280 => IsTooLong 280</pre>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<small>➡&nbsp;</small>[See the live demo](https://impure.fun/FSharp.Domain.Validation/demo/)

You may have noticed that the examples on the left have an additional *not null or empty* validation case. On the right this validation is implicit in the statement that a `Tweet` is a `Tweet of Text`. Since Validation boxes can have inner boxes, the only rules that need to be explicitly declared are the rules specific to the box being defined!




## Interface? Really?

F# is a multi-paradigm language, so there's nothing preventing us from harnessing (hijacking?) OO concepts for their expressiveness without any of the baggage. For instance here we use `interface` as an elegant way to both:

* Identify a type as a Validation box
* Enforce the definition of validation rules

There's no other mentions of interfaces in the code that creates or uses Validation boxes, only when defining new types.

## How it works

First you declare your error types, then you declare your actual domain types (i.e. `Tweet`), and finally you use them with the provided `Box.value` and `Box.validate` functions. These 3 simple steps are enough to ensure at compilation time that your entire domain is **always** valid!

<p align="center">
    <a href="https://impure.fun/FSharp.Domain.Validation/demo/">
        <img src="assets/demo.gif" alt="demo" />
    </a>
    <small><br>Older version of the live demo for future DDD paleontologists</small>
</p>

### Declaring your errors

Before declaring types like the one above, you do need define your error type. This can be a brand new validation-specific discriminated union or part of an existing one.

```fsharp
// These are just an example, create whatever errors
// you need to return from your own validation rules
type TextError =
    | ContainsControlCharacters
    | ContainsTabs
    | IsTooLong of int
    | IsMissingOrBlank
    // ...
```

While not strictly necessary, the next single line of code greatly improves the readability of your type declarations by abbreviating the `IBox<_,_>` interface for a specific primitive type.

```fsharp
// all string-based types can now interface TextBox instead of IBox<string, TextError>
type TextBox = inherit IBox<string, TextError>
```

### Declaring your types
Type declaration is reduced to the absolute minimum. A type is given a name, a private constructor, and the interface above that essentially makes it a **Validation box** and ensures that you define the validation rule.

The  validation rule is a function of the primitive type (`string` here) that returns a list of one or more errors depending on the stated conditions.

```fsharp
/// Single or multi-line non-null non-blank text without any additional validation
type FreeText = private FreeText of string with
    interface TextBox with
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
    interface TextBox with
        member _.Validate =
            // same validation rule using validation operators
            String.IsNullOrWhiteSpace ==> IsMissingOrBlank
```
To use validation operators make sure to open `FSharp.Domain.Validation.Operators` in the file(s) where you declare your Validation types. See [Text.fs](/src/Example/Text.fs#L57) for more examples of validation operators.

### Creating and using boxes in your code

Using Validation boxes is easy, let's say you have a box called `email`, you can simply access its value using the following:

```fsharp
// get the primitive value from the box
Box.value email // → string
```

There's also an experimental operator `%` that essentially does the same thing. Note that this operator is *opened* automatically along with the namespace `FSharp.Domain.Validation`. To avoid operator pollution this is advertised as experimental until the final operator characters are decided.

```fsharp
// experimental — same as Box.value
%email // → string
```

Creating a box is just as simple:

```fsharp
// create a box, canonicalizing (i.e. trimming) the input if it's a string
Box.validate s // → Ok 'box | Error e
```

`Box.validate` canonicalization consists of trimming both whitespace and control characters, as well as removing occurrences of the null character. While this should be the preferred way of creating boxes, it's possible to skip canonicalization by using `Box.verbatim` instead.

When type inference isn't possible, specify the box type using the generic parameter:

```fsharp
// create a box when its type can't be inferred
Box.validate<Tweet> s // → Ok Tweet | Error e
```

⚠ Do **not** force type inference using type annotations as it's unnecessarily verbose:

```fsharp
// incorrect example, do *not* copy/paste
let result : Result<Email, TextError list> = // :(
    Box.validate "incorrect@dont.do"

// correct alternative when type inference isn't available
let result =
    Box.validate<Email> "dev@fsharp.lang"    // :)
```

In both cases the resulting `email` is of type `Result<Email, TextError list>`.

## Exceptions instead of Error
The `Box.validate` method returns a `Result`, which may not always be necessary, for instance when de-serializing values that are guaranteed to be valid, you can just use:

```fsharp
// throws an exception if not valid
Unchecked.boxof "this better be valid"         // → 'box (inferred)

// same as above, when type inference is not available
Unchecked.boxof<Text> "this better be valid 2" // → Text
```

## Serialization

There's a `System.Text.Json.Serialization.JsonConverter` included, if you add it to your serialization options all boxes are serialized to (and de-serialized from) their primitive type. It is good practice to keep your serialized content independent from implementation considerations such as Validation boxes.

## Not just strings

Strings are the perfect example as it's usually the first type for which developers stitch together validation logic, but this library works with anything, you can create a `PositiveInt` that's guaranteed to be greater than zero, or a `FutureDate` that's guaranteed to not be in the past. Lists, vectors, any type of object really, if you can write a predicate against it, you can validate it. It's 100% generic so the sky is the limit.

## Ok looks good, but I'm still not sure

I've created a checklist to help you decide whether this library is a good match for your project:

- [x] My project contains domain objects/records

If your project satisfies all of the above this library is for you!

It dramatically reduces the amount of code necessary to make illegal states unrepresentable while being tiny and built only with `FSharp.Core`. It uses F# concepts in the way they're meant to be used, so if one day you decide to no longer use it, you can simply get rid of it and still keep all the single-case unions that you've defined. All you'll need to do is create your own implementation of `Box.validate` and `Box.value` or just make the single case constructors public.

## Conclusion

Using this library you can create airtight domain objects guaranteed to never have invalid content. Not only you're writing less code, but your domain definition files are much smaller and nicer to work with. You'll also get [ROP](https://fsharpforfunandprofit.com/rop/) almost for free, and while there is a case to be made [against ROP](https://fsharpforfunandprofit.com/posts/against-railway-oriented-programming/), it's definitely a perfect match for content validation, especially content that may be entered by a user.

### Full working example
You can find a full working example in the file [Text.fs](/src/Example/Text.fs)
