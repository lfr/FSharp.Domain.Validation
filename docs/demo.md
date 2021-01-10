---
layout: default
title: Fable validation demo 💙
permalink: /demo/
description: >-
  Do not go gentle into that impure night
image: /assets/2021/fable-validation-blocks.png
---

## Fable validation demo 💙

Try typing something below and guessing what `validate` does, knowing that the following `Result.toText` simply converts a `Result<_,_>` to text.
<div class="object-container">
    <object type="text/html" data="https://validation-blocks-fable.herokuapp.com/"></object>
</div>

Perhaps you have something like this in mind:

```
match 'type with
| Text     -> check that it's 1 line & not blank
| FreeText -> check that it's not null
| Integer  -> check that it can be parsed to int
```

Turns out `validate` does nothing of the sort because it's not defined anywhere in the code! It's a generic function from `FSharp.ValidationBlocks`, one that has no awareness of our own custom types `Text`, `FreeText`, and `Integer`.

## 100% Object-free ✔

You may be thinking "*ok, so `FreeText` is an object with a private constructor that validates a string*", but it's in fact much simpler than that, it's just a combination of a **validation rule** with a **type name**, the interface below only being used to identify it as a validation block and also conveniently enforce the definition of a validation rule with the appropriate signature:

```fsharp
type FreeText = private FreeText of string with
  interface TextBlock with
    member _.Validate =
      String.IsNullOrWhiteSpace ==> IsMissingOrBlank
      // 🤯
```

This simplicity is not just a nicety, if you're going to replace **ᴀʟʟ** your validated strings with similar types, [and you should](https://impure.fun/fun/2020/03/04/these-arent-the-types/), it's crucial that these can be defined with minimal code.

## Certified DRY™ ✔

While our other custom type `Text` also rejects empty strings, its definition <u>doesn't even declare that rule</u>:

```fsharp
type Text = private Text of FreeText with
  interface TextBlock with
    member _.Validate =
      containsControlChars ==> ContainsCtrlChars
      // 🤯🤯
```

## Certified KISS™ ✔

So declaring types requires very little code, but validating does too! In fact the validation function wouldn't even have to specify a type if it could be inferred like in the code below:

```fsharp
open type FSharp.ValidationBlocks.Block<str, TxtErr>

// dummy domain (dumdom?)
type MyDomain =
  {
    TextProp: Text
    FreeTextProp: FreeText
  }

// this creates a validated MyDomain record:
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

I know it all sounds super easy but do me and yourself a favor, [read the project's README](https://github.com/lfr/FSharp.ValidationBlocks) before trying this at home.

## 🚨 Fable users: read this 👇

* With Fable you'll have to use the package and namespace `FSharp.ValidationBlocks.Fable` <u>instead of</u> <s>`FSharp.ValidationBlocks`</s>
  ```fsharp
  open FSharp.ValidationBlocks.Fable
  ```
* Records like `MyDomain` above are worthless unless they can be used in javascript, to properly serialize them with [Thoth.Json](https://thoth-org.github.io/Thoth.Json/) use extra encoders <u>for each block type</u>:
  ```fsharp
  open FSharp.ValidationBlocks.Fable.Thoth

  let myExtraCoders =
    Extra.empty
    |> Extra.withCustom Codec.Encode<FreeText>
       Codec.Decode<FreeText>
  ```

* The function `Unchecked.blockof` won't be available in Fable until [Fable#2321](https://github.com/fable-compiler/Fable/issues/2321) is closed, so for now the only way to quickly skip `Result<_,_>` is with with something like:
  ```fsharp
  |> function Ok x -> x | _ -> failwith "💣"
  ```

## Share the love 💙

Like what you see?
<p>
  <a class="twitter-share-button"
    href="https://twitter.com/intent/tweet"
    data-hashtags="fsharp"
    data-via="luislikeiewis"
    data-url="https://impure.fun/FSharp.ValidationBlocks/demo/"
    data-related="luislikeiewis"
    data-size="large">
    Tweet
  </a>
</p>