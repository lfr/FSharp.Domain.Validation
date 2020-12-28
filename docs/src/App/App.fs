module App

open FSharp.ValidationBlocks
open FSharp.ValidationBlocks.Example
open Browser.Dom
open Microsoft.FSharp.Reflection

// Mutable variable to count the number of times we clicked the button
let mutable count = 0

// Get a reference to our button and cast the Element to an HTMLButtonElement
let myButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement
let myTxtBox = document.querySelector("#textbox") :?> Browser.Types.HTMLInputElement

// Register our listener
myButton.onclick <- fun _ ->

    //let tweet = Unchecked.blockof<Text> "my text"
    let uci = FSharpType.GetUnionCases(typeof<FreeText>, true) |> Array.head
    let text =
        FSharpValue.MakeUnion(uci, [|box "my text"|], true)
        :?> FreeText

    count <- count + 1
    myButton.innerText <- sprintf "You clicked: %i time(s)" count
    myTxtBox.value <- Block.value text