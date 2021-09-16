module Compose


type HandlerDoc = {
    DocStatus: bool
}

type HandlerDocResult = HandlerDoc
type HandlerFunc = HandlerDoc -> HandlerDocResult
type HandlerHandler = HandlerFunc -> HandlerDoc -> HandlerDocResult

let handler1 (next: HandlerFunc) = 
    printfn "Setting next of handler1"
    fun (handlerDoc: HandlerDoc) ->
        printfn "In handler1: doc status: %b" handlerDoc.DocStatus
        next {handlerDoc with DocStatus = false}

let handler2 (next: HandlerFunc) = 
    printfn "Setting next of handler2"
    fun (handlerDoc: HandlerDoc) ->
        printfn "In handler2: doc status: %b" handlerDoc.DocStatus
        next {handlerDoc with DocStatus = false}

let handler3 (next: HandlerFunc) = 
    printfn "Setting next of handler3"
    fun (handlerDoc: HandlerDoc) ->
        printfn "In handler3: doc status: %b" handlerDoc.DocStatus
        next handlerDoc

let handler4 (next: HandlerFunc) = 
    printfn "Setting next of handler4"
    fun (handlerDoc: HandlerDoc) ->
        printfn "In handler4: doc status: %b" handlerDoc.DocStatus
        next handlerDoc

let finalFunc (handlerDoc: HandlerDoc) =
    printfn "In final func: doc status: %b" handlerDoc.DocStatus
    handlerDoc

let compose (handlerh1: HandlerHandler) (handlerh2:HandlerHandler) =
    fun (final: HandlerFunc) ->
        printfn "Setting next of combiner"
        let func = final |> handlerh2 |> handlerh1
        fun (doc: HandlerDoc) ->
            printfn "Doc status is : %b" doc.DocStatus
            if doc.DocStatus then
                printfn "Calling  func"
                func doc
            else
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