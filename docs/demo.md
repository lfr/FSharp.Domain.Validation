---
layout: default
title: Demo
permalink: /demo/
---

## Fable validation demo 💙

Try typing something below and vaguely guessing what the `validate` function does, knowing that the helper function  `Result.text` simply converts `validate`'s `Result<_,_>` to readable text.
<div class="object-container">
    <object type="text/html" data="https://validation-blocks-fable.herokuapp.com/"></object>
</div>

You may be thinking that validate looks something like this:
```
match 'type with
| Text     -> check that it's single line and not null
| FreeText -> check that it's not null
| Integer  -> check that it can be parsed to int
```
Spoiler alert: validate does nothing of the sort, it's not even defined anywhere in this demo's code. It's a generic function from `FSharp.ValidationBlocks`, one that has no awareness of what the types `Text` or `FreeText` are, since these are only defined in the demo code!

## 100% Object-free ✔

You may be thinking "*ok, so `FreeText` is an object with a private constructor that validates a string*", but it's in fact much simpler than that, it's just a combination of a validation rule with a type name, with the interface below only being used to identify it as a ValidationBlock and enforce the definition of a validation rule:

```fsharp
type FreeText = private FreeText of string with
  interface TextBlock with
    member _.Validate =
      String.IsNullOrWhiteSpace ==> IsMissingOrBlank
      // 🤯
```

This simplicity is not just a nicety, remember that you're supposed to replace **ᴀʟʟ** your strings with similar types, it's crucial that these can be defined with minimal code.

## DRY™ certified ✔

While our other demo type `Text` also rejects empty strings, its definition doesn't even explicitly declare that rule:

```fsharp
type Text = private Text of FreeText with
  interface TextBlock with
    member _.Validate =
      containsControlChars ==> ContainsCtrlChars
      // 🤯🤯
```

## KISS™ certified ✔

So declaring types requires very little code, but validating does too, in fact the validation function from above doesn't even need to specify the type when it can be inferred:

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

* With Fable you'll have to use the package <u>and namespace</u> `FSharp.ValidationBlocks.Fable` **instead of** `FSharp.ValidationBlocks`
* The function `Unchecked.blockof` won't be available until [Fable#2321](https://github.com/fable-compiler/Fable/issues/2321) is closed, so for now the only way to quickly skip `Result<_,_>` is with with something like:<br>
  ```fsharp
  |> function Ok x -> x | _ -> failwith "invalid!"
  ```

### Share the love 💙

Like what you see? Twitter share

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