---
layout: default
title: Demo
permalink: /demo/
---

## Fable validation demo 💙

Try typing something below and guessing what `validate` does, knowing that the following `Result.toText` simply converts a `Result<_,_>` to text.
<div class="object-container">
    <object type="text/html" data="https://validation-blocks-fable.herokuapp.com/"></object>
</div>

Perhaps you're thinking that `validate` looks like this:

```
match 'type with
| Text     -> check that it's 1 line & not blank
| FreeText -> check that it's not null
| Integer  -> check that it can be parsed to int
```

Turns out `validate` does nothing of the sort because it's not defined anywhere in the code! It's a generic function from `FSharp.ValidationBlocks`, one that has no awareness of our very own custom types `Text`, `FreeText`, and `Integer`.

## 100% Object-free ✔

You may be thinking "*ok, so `FreeText` is an object with a private constructor that validates a string*", but it's in fact much simpler than that, it's just a combination of a **validation rule** with a **type name**, the interface below only being used to identify it as a validation block and also conveniently enforce the definition of a validation rule with the appropriate signature:

```fsharp
type FreeText = private FreeText of string with
  interface TextBlock with
    member _.Validate =
      String.IsNullOrWhiteSpace ==> IsMissingOrBlank
      // 🤯
```

This simplicity is not just a nicety, if you're going to replace **ᴀʟʟ** your strings with similar types, which you should, it's crucial that these can be defined with minimal code.

## DRY™ certified ✔

While our other custom type `Text` also rejects empty strings, its definition <u>doesn't even declare that rule</u>:

```fsharp
type Text = private Text of FreeText with
  interface TextBlock with
    member _.Validate =
      containsControlChars ==> ContainsCtrlChars
      // 🤯🤯
```

## KISS™ certified ✔

So declaring types requires very little code, but validating does too, in fact the validation function from wouldn't even have to specify a type if it could be inferred like in the code below:

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

### 🚨🚨🚨 Fable users: please note 👇

* With Fable you'll have to use the package and namespace `FSharp.ValidationBlocks.Fable` **instead of** <s>`FSharp.ValidationBlocks`</s>
  ```fsharp
  open FSharp.ValidationBlocks.Fable
  ```
* Records like `MyDomain` above are worthless in javascript unless they can be used, to properly serialize them with [Thoth.Json](https://thoth-org.github.io/Thoth.Json/) use extra encoders <u>for each block type</u>:
  ```fsharp
  let myExtraCoders =
    Extra.empty
    |> Extra.withCustom Codec.Encode<FreeText> Codec.Decode<FreeText>
  ```
* The function `Unchecked.blockof` won't be available in Fable until [Fable#2321](https://github.com/fable-compiler/Fable/issues/2321) is closed, so for now the only way to quickly skip `Result<_,_>` is with with something like:
  ```fsharp
  |> function Ok x -> x | _ -> failwith "💣"
  ```

### Share the love 💙

Like what you see?
<a class="twitter-share-button"
  href="https://twitter.com/intent/tweet?text=Hello%20world">
Tweet</a>{{ permalink }}

## Serialization

.NET

Fable

## Still not convinced?

There's a few alternatives to using `FSharp.ValidationBlocks`:

1. Defining your domain with strings & creating validation functions that are called when necessary
2. Creating objects with validation in the constructor
3. Creating single-case unions with corresponding validation modules
4. Having a validation module that uses reflection to spawn ready-made valid objects or errors 
   
* **#1** is the traditional method of doing validation, but it's unsafe as you need to remember to validate content and you can easily apply the wrong rule, since everything returns strings the errors can only be spotted at runtime 😱
* **#2** and **#3** are equivalent and both require a lot more code than above to declare each type which can potentially make your code reeealy repetitive 😴
* **#4** may sound perfect, but as your domain grows so will that monolithic module and since the validation rules don't live with the types but in the validation module, you can't re-use them across independent projects unless you bring the validation module along with them 😩

Don't agree? I'd love to [hear from you](https://twitter.com/luislikeIewis)!