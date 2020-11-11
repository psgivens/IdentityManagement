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

type ``Delete role`` (testResources:Composition.TestSystemResources) =
    (*********************************************
     *** Create some sample data for the test  ***
     *********************************************)
    let streamId = StreamId.create ()

    let roleName = "SystemAdmins"
    let externalId = Guid.NewGuid ()
    let userId1 = Guid.NewGuid ()
    let userId2 = Guid.NewGuid ()

    let existingEvents = [
      Created (roleName, externalId)
      PrincipalAdded userId1
      PrincipalAdded userId2
    ]

    let existingEventStore = 
      Map.empty
      |> Map.add streamId (existingEvents |> List.map (Tests.envelop streamId))

    let expectedState = 
      { RoleManagementState.Name = ""
        ExternalId = externalId
        Principals = []
        Deleted = true }

    (******************************* 
     *** Create the Actor system *** 
     *******************************)      

    let roleCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
        testResources.System "role_management_command" testResources.ActorGroups.RoleManagementActors

    (******************************* 
     *** Utility functions       *** 
     *******************************)      
    let processCommand = 
      Tests.envelop streamId
      >> roleCommandRequestReplyCanceled.Ask 
      >> runWaitAndIgnore 


    member this.Gherkin () = 
      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (Preconditions.Events existingEvents)
      |> RoleGherkin.When (Events [ Deleted ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))


    member this.Preconditions () = 
      (*********************************
       *** Initialize pre-conditions ***
       *********************************)
      existingEvents
      |> List.mapi (Tests.envelopi streamId)
      |> List.iter testResources.Persistence.roleManagementStore.AppendEvent


    member this.ActionUnderTest () = 
      (**************************
       *** Perform the action ***
       **************************)
      [ Delete ]
      |> List.iter processCommand


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

