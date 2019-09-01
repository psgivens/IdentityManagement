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

type RolesTests ()  =

    [<Fact>]
    member this.``Create user`` () =
      (*********************************************
       *** Create some sample data for the test  ***
       *********************************************)
      let userDetails =
        { 
            FirstName="Phillip"
            LastName="Givens"
            Email="one@three.com"
        }

      let expectedUserState = 
        { UserManagementState.Details = userDetails
          UserManagementState.State = Active }


      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      UserGherkin.Given (State None)
      |> UserGherkin.When (Events [ UserManagementEvent.Created userDetails ])
      |> UserGherkin.Then (expectState (Some (expectedUserState)))


      (******************************* 
       *** Create the Actor system *** 
       *******************************)      
      let system = Configuration.defaultConfig () |> System.create "sample-system"

      let persistence = {
        userManagementStore = InMemoryEventStore<UserManagementEvent> ()
        groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
        roleManagementStore = InMemoryEventStore<RoleManagementEvent> ()
        persistUserState = doNotPersist
        persistGroupState = doNotPersist
        persistRoleState = doNotPersist
      }

      let actorGroups = composeActors system persistence

      let userCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
          system "user_management_command" actorGroups.UserManagementActors


      (**************************
       *** Perform the action ***
       **************************)
      let streamId = StreamId.create ()
      printfn "Creating user..."

      userDetails
      |> UserManagementCommand.Create
      |> Tests.envelop streamId
      |> userCommandRequestReplyCanceled.Ask 
      |> runWaitAndIgnore

      (*************************
       *** Evolve the events ***
       *************************)
      let evolve evts = 
        let evolve' = IdentityManagement.Domain.UserManagement.evolve
        evts |> Seq.fold evolve' None

      let events = 
        persistence.userManagementStore.GetEvents streamId
        |> Seq.map (fun env -> env.Item) 

      let state = events |> evolve

      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal (Some expectedUserState, state)


