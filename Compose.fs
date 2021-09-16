module Compose


type EFDoc = {
    IsNew: bool
    DocStatus: bool
    ErrMsg: string
}

type EFDocResult = EFDoc
type EFFunc = EFDoc -> EFDocResult
type EFHandler = EFFunc -> EFDoc -> EFDocResult

let handler1 (efFunc: EFFunc) = 
    printfn "Setting next of handler1"
    fun (efDoc: EFDoc) ->
        printfn "In handler1: doc status: %b" efDoc.DocStatus
        efFunc {efDoc with DocStatus = false}

let handler2 (efFunc: EFFunc) = 
    printfn "Setting next of handler2"
    fun (efDoc: EFDoc) ->
        printfn "In handler2: doc status: %b" efDoc.DocStatus
        efFunc {efDoc with DocStatus = false}

let handler3 (efFunc: EFFunc) = 
    printfn "Setting next of handler3"
    fun (efDoc: EFDoc) ->
        printfn "In handler3: doc status: %b" efDoc.DocStatus
        efFunc efDoc

let handler4 (efFunc: EFFunc) = 
    printfn "Setting next of handler4"
    fun (efDoc: EFDoc) ->
        printfn "In handler4: doc status: %b" efDoc.DocStatus
        efFunc efDoc

let finalFunc (efDoc: EFDoc) =
    printfn "In final func: doc status: %b" efDoc.DocStatus
    efDoc

let compose (efh1: EFHandler) (efh2:EFHandler) =
    fun (final: EFFunc) ->
        printfn "Setting next of combiner"
        let func = final |> efh2 |> efh1
        fun (doc: EFDoc) ->
            printfn "Doc status is : %b" doc.DocStatus
            if doc.DocStatus then
                printfn "Calling  func"
                func doc
            else
                printfn "Calling  final"
                final doc

let (>=>) = compose

let test() =

    let jj = handler1 >=> handler2 >=> handler3 >=> handler4
    let gg = jj finalFunc

    printfn "gg: %A" gg
    let doc = {
        IsNew = true
        DocStatus = true
        ErrMsg = ""
    }

    let yy = gg doc
    printfn "final doc: %A" yy