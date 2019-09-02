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



type UsersTests ()  =

    [<Fact>]
    member this.``Create and update user`` () =
      (*********************************************
       *** Create some sample data for the test  ***
       *********************************************)
      let streamId = StreamId.create ()

      let userDetails =
        { 
            FirstName="Phillip"
            LastName="Givens"
            Email="one@three.com"
        }

      let changedDetails = { 
        userDetails with
          Email="one@four.com" }

      let expectedUserState = 
        { UserManagementState.Details = changedDetails
          UserManagementState.State = Active }


      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      UserGherkin.Given (State None)
      |> UserGherkin.When (Events [ 
        UserManagementEvent.Created userDetails
        Deactivated
        Updated changedDetails
        Activated
         ])
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
      [ UserManagementCommand
          .Create userDetails
        Deactivate
        Update changedDetails
        Activate ]
      |> List.iter (fun command ->
        command
        |> Tests.envelop streamId
        |> userCommandRequestReplyCanceled.Ask 
        |> runWaitAndIgnore )

      (*************************
       *** Evolve the events ***
       *************************)
      let events = 
        persistence.userManagementStore.GetEvents streamId
        |> List.map (fun env -> env.Item) 

      let state = 
        events 
        |> List.fold IdentityManagement.Domain.UserManagement.evolve None

      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal (Some expectedUserState, state)


