namespace IdentityManagement.UnitTests

open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain.RoleManagement

open IdentityManagement.Api.Composition
open System
open Xunit
open Common.FSharp
open Common.FSharp.Actors
open Common.FSharp.Envelopes

open Akka.Actor
open Akka.FSharp

module infr =
  let doNotPersist<'a> (uid:UserId) (sid:StreamId) (state:'a option) = ()

  let runWaitAndIgnore = 
    Async.AwaitTask
    >> Async.Ignore
    >> Async.RunSynchronously

  let userId = UserId.create ()
  let envelop = envelopWithDefaults userId (TransId.create ()) 


type InMemoryEventStore<'a> =
  val mutable events : Map<StreamId, Envelope<'a> list> 
  new () = { events = Map.empty }
  interface IEventStore<'a> with
    member this.GetEvents (streamId:StreamId) =
      match this.events |> Map.tryFind streamId with
      | Some (events') -> 
        events'
        |> Seq.toList 
        |> List.sortBy(fun x -> x.Version)
      | None -> []
    member this.AppendEvent (envelope:Envelope<'a>) =
      let store = this :> IEventStore<'a>
      let events' = store.GetEvents envelope.StreamId
      this.events <- this.events |> Map.add envelope.StreamId (envelope::events')

type SampleTest1 ()  =

    (************************* 
     *** Create the system *** 
     *************************)
    let system = Configuration.defaultConfig () |> System.create "sample-system"

    let persistence = {
      userManagementStore = InMemoryEventStore<UserManagementEvent> ()
      groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
      roleManagementStore = InMemoryEventStore<RoleManagementEvent> ()
      persistUserState = infr.doNotPersist
      persistGroupState = infr.doNotPersist
      persistRoleState = infr.doNotPersist
    }

    let actorGroups = composeActors system persistence

    let userCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
        system "user_management_command" actorGroups.UserManagementActors


    [<Fact>]
    member this.``My test`` () =

      let streamId = StreamId.create ()
      (**************************
       *** Perform the action ***
       **************************)
      printfn "Creating user..."
      let details =
        { 
            FirstName="Phillip"
            LastName="Givens"
            Email="one@three.com"
        }

      details
      |> UserManagementCommand.Create
      |> infr.envelop streamId
      |> userCommandRequestReplyCanceled.Ask 
      |> infr.runWaitAndIgnore

      (*************************
       *** Evolve the events ***
       *************************)
      let evolve evts = 
        let evolve' = IdentityManagement.Domain.UserManagement.evolve
        evts |> Seq.fold evolve' None

      let events = persistence.userManagementStore.GetEvents streamId
      let state = 
        events 
        |> Seq.map (fun env -> env.Item) 
        |> evolve

      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal ({ 
        UserManagementState.Details = details
        UserManagementState.State = Active }
        |> Some, state)

