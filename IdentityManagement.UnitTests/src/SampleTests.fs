namespace IdentityManagement.UnitTests

open IdentityManagement.Api.Composition
open System
open Xunit
open Common.FSharp

open Akka.FSharp
open FSharp.Data

open Common.FSharp.Envelopes

module infr =
  let doNotPersist<'a> (uid:UserId) (sid:StreamId) (state:'a option) = ()

type SampleTest1 ()  =

    let system = Configuration.defaultConfig () |> System.create "sample-system"

    let persistence = {
      persistUserState = infr.doNotPersist
      persistGroupState = infr.doNotPersist
      persistRoleState = infr.doNotPersist
    }

    // printfn "Composing the actors..."
    let actorGroups = composeActors system persistence

    [<Fact>]
    member this.``My test`` () =
        Http.RequestString("http://tomasp.net") |> fun x -> printfn "%d" x.Length

        // Download web site asynchronously
        async { let! html = Http.AsyncRequestString("http://tomasp.net")
            printfn "%d" html.Length }
        |> Async.Start
        Assert.True(true)
