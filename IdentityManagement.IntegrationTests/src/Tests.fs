namespace IdentityManagement.IntegrationTests

open System
open Xunit
open Common.FSharp

open FSharp.Data

type Test1 ()  =

    [<Fact>]
    member this.``My test`` () =
        Http.RequestString("http://tomasp.net") |> fun x -> printfn "%d" x.Length

        // Download web site asynchronously
        async { let! html = Http.AsyncRequestString("http://tomasp.net")
            printfn "%d" html.Length }
        |> Async.Start
        Assert.True(true)


    [<Fact>]
    member this.``My second test`` () =
        Http.RequestString("http://tomasp.net") |> fun x -> printfn "%d" x.Length

        // Download web site asynchronously
        async { let! html = Http.AsyncRequestString("http://tomasp.net")
            printfn "%d" html.Length }
        |> Async.Start
        Assert.True(true)
