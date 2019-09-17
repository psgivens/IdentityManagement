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

open IdentityManagement.Data.Models
open Microsoft.Data.Sqlite
open Microsoft.EntityFrameworkCore

type ``Remove user from a role`` (testResources:Composition.TestSystemResources) =
    (*********************************************
     *** Create some sample data for the test  ***
     *********************************************)
    let streamId:StreamId = StreamId.create ()

    let roleName = "SystemAdmins"
    let externalId = Guid.NewGuid ()
    let userId1 = Guid.NewGuid ()
    let userId2 = Guid.NewGuid ()

    let existingEvents = [
      Created (roleName, externalId)
      PrincipalAdded userId1
      PrincipalAdded userId2
    ]

    let userDetails1 =
      { 
          FirstName="Phillip"
          LastName="Givens"
          Email="one@three.com"
      }

    let userDetails2 =
      { 
          FirstName="Irene"
          LastName="Vidyanti"
          Email="two@four.com"
      }

    let expectedMembers = [userId2]
    let expectedState = 
      { RoleManagementState.Name = roleName
        ExternalId = externalId
        Principals = expectedMembers
        Deleted = false }

    (******************************* 
     *** Create the Actor system *** 
     *******************************)
    let userCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
        testResources.System "user_management_command" testResources.ActorGroups.UserManagementActors

    let roleCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
        testResources.System "role_management_command" testResources.ActorGroups.RoleManagementActors

    (******************************* 
     *** Utility functions       *** 
     *******************************)      
    let processCommand (rra:IActorRef) streamId = 
      Tests.envelop streamId
      >> rra.Ask 
      >> runWaitAndIgnore 

    member this.Gherkin () = 
      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (Preconditions.Events existingEvents)
      |> RoleGherkin.When (Events [ PrincipalRemoved userId1 ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))


    member this.Preconditions () = 
      (*********************************
       *** Initialize pre-conditions ***
       *********************************)
      // existingEvents
      // |> List.mapi (Tests.envelopi streamId)
      // |> List.iter testResources.Persistence.roleManagementStore.AppendEvent

      let processUserCommand = 
        processCommand userCommandRequestReplyCanceled (StreamId.box userId1)
      [ UserManagementCommand.Create userDetails1 ]
      |> List.iter processUserCommand

      let processUserCommand' = 
        processCommand userCommandRequestReplyCanceled (StreamId.box userId2)
      [ UserManagementCommand.Create userDetails2 ]
      |> List.iter processUserCommand'

      let processRoleCommand = 
        processCommand roleCommandRequestReplyCanceled streamId

      [ RoleManagementCommand 
          .Create (roleName, externalId)
        AddPrincipal userId1
        AddPrincipal userId2 ]
      |> List.iter processRoleCommand


    member this.ActionUnderTest () = 
      (**************************
       *** Perform the action ***
       **************************)
      let processRoleCommand = 
        processCommand roleCommandRequestReplyCanceled streamId

      [ RemovePrincipal userId1 ]
      |> List.iter processRoleCommand


    member this.VerifyState () =
      (*************************
       *** Evolve the events ***
       *************************)
      let events = 
        testResources.Persistence.roleManagementStore.GetEvents streamId
        |> List.map (fun env -> env.Item) 

      let state = 
        events 
        |> List.fold IdentityManagement.Domain.RoleManagement.evolve None


      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal (Some expectedState, state)
      Assert.Equal (1, state.Value.Principals |> List.length)
      Assert.Equal (expectedMembers |> List.head, state.Value.Principals |> List.head)

      // TODO: Verify that the user is no longer in the role

