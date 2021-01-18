---
layout: default
title: Fable validation demo 💙
permalink: /demo/
description: >-
  Do not go gentle into that impure night
image: /assets/2021/fable-validation-blocks.png
---

# Fable validation demo 💙

Type something below and try to guess what `validate` does, knowing that the helper `Result.toText` just converts `Result<_,_>` using `%A` and different emojis for success and error.
<div class="object-container">
    <object type="text/html" data="https://validation-blocks-fable.herokuapp.com/"></object>
</div>

Perhaps you have something like this in mind:

```
match 'type with
| Text     -> check that it's 1 line & not null
| FreeText -> check that it's not null
| Integer  -> check that it can be parsed to int
```

Turns out it does nothing of the sort, in fact `validate` is not even defined anywhere in the code! It's a generic function from `FSharp.ValidationBlocks`, one that has no awareness of our own custom types `Text`, `FreeText`, and `Integer`.

## 100% Object-free ✔

You may be thinking "*ok then `FreeText` is an object with a constructor that validates a string*", but it's in fact much simpler than that, it's just a combination of a **validation rule** with a **type name**. The interface below is only used to identify it as a validation block, and conveniently enforce the definition of a validation rule with the appropriate signature:

```fsharp
type FreeText = private FreeText of string with
  interface TextBlock with
    member _.Validate =
      String.IsNullOrWhiteSpace ==> IsMissingOrBlank
      // 🤯
```

This simplicity is not just a nicety, if you're going to replace all your validated strings with similar types, [and you definitely should](https://impure.fun/fun/2020/03/04/these-arent-the-types/), it's important that these can be defined with minimal code.

## Certified DRY™ ✔

Our other custom type `Text` also rejects empty strings, but its definition <u>doesn't even declare that rule</u>:

```fsharp
type Text = private Text of FreeText with
  interface TextBlock with
    member _.Validate =
      containsControlChars ==> ContainsCtrlChars
      // 🤯🤯
```

## Certified KISS™ ✔

So declaring types requires very little code, but so does validating! Most of the time the function `validate` doesn't even have to specify a type like in the code below:

```fsharp
open type FSharp.ValidationBlocks.Block<str, TxtErr>
open FSToolkit.ErrorHandling

// this creates a validated record:
result {

  let! text = validate inp1
  and! freeText = validate inp2
  //🤯🤯🤯

  return
  {
    TextProp = text
    FreeTextProp = freeText
  }
}
```

## May contain traces of RTFM 📖

I know it all sounds super easy but do me (and yourself) a favor, [read the project's README](https://github.com/lfr/FSharp.ValidationBlocks) before trying this at home. Not only that document is more up-to-date than this demo, but it also uses examples that don't make the use of `FSharp.ValidationBlocks.Operators` making them easier to understand. For instance here it's not immediately obvious that the `_.Validate` function returns a list of errors.

## Share the love 💙

Excited about this? Spread the word to your fellow dev!&nbsp;
<a class="twitter-share-button"
  href="https://twitter.com/intent/tweet"
  data-url="https://impure.fun/FSharp.ValidationBlocks/demo/"
  data-related="luislikeIewis"
  data-size="large">
  Share this
</a>

## 🚨 Fable users: read this 👇

* With Fable you'll have to use the package and namespace `FSharp.ValidationBlocks.Fable`, <u>not</u> `FSharp.ValidationBlocks`
  ```fsharp
  open FSharp.ValidationBlocks.Fable
  ```
* Records like `MyDomain` above are worthless unless they can be used in javascript, in order to properly serialize them with [Thoth.Json](https://thoth-org.github.io/Thoth.Json/) use extra encoders <u>for each block type</u>:
  ```fsharp
  open FSharp.ValidationBlocks.Fable.Thoth

  let myExtraCoders =
    Extra.empty
    |> Extra.withCustom
        Codec.Encode<Text>
        Codec.Decode<Text>
    |> Extra.withCustom
        Codec.Encode<FreeText>
        Codec.Decode<FreeText>
    // etc…
  ```

<a id="Unchecked.blockof" />

* The function `Unchecked.blockof` won't be available in Fable until [Fable#2321](https://github.com/fable-compiler/Fable/issues/2321) is closed, so for now the only way to quickly skip `Result<_,_>` is with something like:
  ```fsharp
  |> function Ok x -> x | _ -> failwith "💣"
  ```

<small>This demo code first appeared in [this article]().</small>