# How does Giraffe's Compose work?

The Giraffe web framework provides a mechanism (in F#) to compose httpHandlers and it looks like this

```
   let compose (handler1 : HttpHandler) (handler2 : HttpHandler) : HttpHandler =
        fun (final : HttpFunc) ->
            let func = final |> handler2 |> handler1
            fun (ctx : HttpContext) ->
                match ctx.Response.HasStarted with
                | true  -> final ctx
                | false -> func ctx
```
where 
```
type HttpFuncResult = Task<HttpContext option>
type HttpFunc = HttpContext -> HttpFuncResult
type HttpHandler = HttpFunc -> HttpContext -> HttpFuncResult
```

Generalising, we have a series of functions that accept a document of some sort (here HttpContext) and which pass that document down the linked list of similar functions, amending it if need be.  The final link in the chain wraps the document in an Option and a Task.  

The bit I don't get is the purpose of the match statement in the inner function 

My reading of it is that, as it goes down the line, the document can be validated and the chain short circuited if that fails.  I accept that this reading may be inaccurate, and that these checks are only intended between pipelines and not between handlers inside a single pipeline.  If that is the case then what follows can be ignored.

In other words, given a pipeline of
handler1 -> handler2 -> handler3 -> handler4 -> finalDocFunc

what I was expecting to see when I ran a simplified version of this was:
handler1 -> checker -> handler2 -> checker -> handler3 -> checker -> handler4 -> finalDocFunc

or at least 
handler1 -> handler2 -> checker -> handler3 -> checker -> handler4 -> finalDocFunc

But what I get is
checker -> checker -> checker -> handler1 -> handler2 -> handler3 -> handler4 -> finalDocFunc

I appreciate that in simplifying the compose function I may have introduced my own error, and I will outline my simplifications below.  I don't think that is the case.

A quick observation:  The result of composing 2 handlers is a checker handler, which when handed a document processing function will pass the document higher up the chain for processing, until it reaches the top and works its way down as the document processing functions call next(doc).  The handlers being composed are what I refer to as worker handlers.

...which is to say that handler1 (in the compose function) will always be a checker handler (except for the initial H1 handler), and it is handler1 that is retrieved by let func = final |> handler2 |> handler1.  So all checker handlers pass their incoming document to another checker handler, until the initial H1 handler (a worker handler) is reached. But then the chain of nexts never passes through a checker.  My question is "Should it?"

As for my simplification, a sample console app is at https://github.com/Tweega/GiraffeCompose

My document
```
type SomeDoc = {
    DocStatus: bool
}
```
The handler types
```
type SomeDocResult = SomeDoc
type DocProcessorFunc = SomeDoc -> SomeDocResult
type Handler = DocProcessorFunc -> SomeDoc -> SomeDocResult
```

The compose function (unchanged except for some tracing)
```
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
```
