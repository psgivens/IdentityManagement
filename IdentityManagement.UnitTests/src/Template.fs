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
open Common.FSharp.EventSourceGherkin

open Akka.Actor
open Akka.FSharp



type TestTemplate () =

    [<Fact>]
    member this.``Copy this and modify to meet your needs`` () =
      ()

    // [<Fact>]
    // member this.``Copy this and modify to meet your needs`` () =
    //   (*********************************************
    //    *** Create some sample data for the test  ***
    //    *********************************************)


    //   (*********************************************
    //    *** Describe the expectations in Gherkin  ***
    //    *********************************************)
    //   UserGherkin.Given (State None)
    //   |> UserGherkin.When (Events [])
    //   |> UserGherkin.Then (expectState (None))


    //   (******************************* 
    //    *** Create the Actor system *** 
    //    *******************************)      
    //   let system = Configuration.defaultConfig () |> System.create "sample-system"

    //   let persistence = {
    //     userManagementStore = InMemoryEventStore<UserManagementEvent> ()
    //     groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
    //     roleManagementStore = InMemoryEventStore<RoleManagementEvent> ()
    //     persistUserState = doNotPersist
    //     persistGroupState = doNotPersist
    //     persistRoleState = doNotPersist
    //   }

    //   let actorGroups = composeActors persistence system

    //   let userCommandRequestReplyCanceled = 
    //     RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
    //       system "user_management_command" actorGroups.UserManagementActors


    //   (**************************
    //    *** Perform the action ***
    //    **************************)
    //   let streamId = StreamId.create ()

    //   [ (* UserManagementCommand.Activate *) ]
    //   |> List.iter (fun command ->
    //     command
    //     |> Tests.envelop streamId
    //     |> userCommandRequestReplyCanceled.Ask 
    //     |> runWaitAndIgnore )

    //   (*************************
    //    *** Evolve the events ***
    //    *************************)
    //   let events = 
    //     persistence.userManagementStore.GetEvents streamId
    //     |> List.map (fun env -> env.Item) 

    //   let state = 
    //     events 
    //     |> List.fold IdentityManagement.Domain.UserManagement.evolve None

    //   (************************
    //    *** Verify the state ***
    //    ************************)
    //   Assert.Equal (None, state)


