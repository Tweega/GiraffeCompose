module Compose


type SomeDoc = {
    DocStatus: bool
}

type SomeDocResult = SomeDoc
type DocProcessorFunc = SomeDoc -> SomeDocResult
type Handler = DocProcessorFunc -> SomeDoc -> SomeDocResult

let handler1 (next: DocProcessorFunc) = 
    printfn "Setting next of handler1"
    fun (someDoc: SomeDoc) ->
        printfn "In handler1: doc status: %b" someDoc.DocStatus
        next {someDoc with DocStatus = false}

let handler2 (next: DocProcessorFunc) = 
    printfn "Setting next of handler2"
    fun (someDoc: SomeDoc) ->
        printfn "In handler2: doc status: %b" someDoc.DocStatus
        next {someDoc with DocStatus = false}

let handler3 (next: DocProcessorFunc) = 
    printfn "Setting next of handler3"
    fun (someDoc: SomeDoc) ->
        printfn "In handler3: doc status: %b" someDoc.DocStatus
        next someDoc

let handler4 (next: DocProcessorFunc) = 
    printfn "Setting next of handler4"
    fun (someDoc: SomeDoc) ->
        printfn "In handler4: doc status: %b" someDoc.DocStatus
        next someDoc

let finalFunc (someDoc: SomeDoc) =
    printfn "In final func: doc status: %b" someDoc.DocStatus
    someDoc

let compose (handler1: Handler) (handler2:Handler) =
    fun (final: DocProcessorFunc) ->
        let func = final |> handler2 |> handler1
        fun (doc: SomeDoc) ->
            printfn "Doc status is : %b" doc.DocStatus
            if doc.DocStatus then
                printfn "Calling  func"
                func doc
            else
                // we never get here
                printfn "Calling  final"
                final doc

let (>=>) = compose

let test() =

    let composed = handler1 >=> handler2 >=> handler3 >=> handler4
    let docProcessor = composed finalFunc

    let doc = {
        DocStatus = true
    }

    let processedDoc = docProcessor doc
    printfn "processed doc: %A" processedDoc