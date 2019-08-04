namespace IdentityManagement.UnitTests

open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain.RoleManagement

open IdentityManagement.Api.Composition
open System
open Xunit
open Common.FSharp

open Akka.FSharp
open FSharp.Data

open Common.FSharp.Envelopes

module infr =
  let doNotPersist<'a> (uid:UserId) (sid:StreamId) (state:'a option) = ()

type InMemoryEventStore<'a> () =
  [<DefaultValue>] val mutable events : Map<StreamId, Envelope<'a> list>
  interface IEventStore<'a> with
    member this.GetEvents (streamId:StreamId) =
      this.events.[streamId]
      |> Seq.toList 
      |> List.sortBy(fun x -> x.Version)
    member this.AppendEvent (envelope:Envelope<'a>) =
      this.events <- this.events |> Map.add envelope.StreamId (envelope::this.events.[envelope.StreamId])

type SampleTest1 ()  =

    let system = Configuration.defaultConfig () |> System.create "sample-system"

    let persistence = {
      userManagementStore = InMemoryEventStore<UserManagementEvent> ()
      groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
      roleManagementStore = InMemoryEventStore<RoleManagementEvent> ()
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
