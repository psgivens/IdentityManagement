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




type ``Test: Create role_ add user_ update title`` (testResources:Composition.TestSystemResources) =
    (*********************************************
     *** Create some sample data for the test  ***
     *********************************************)
    let streamId = StreamId.create ()

    let roleName = "SystemAdmins"
    let externalId = Guid.NewGuid ()
    let newRoleName = "SystemAdministrators"
    let newUserId = Guid.NewGuid ()

    let expectedState = 
      { RoleManagementState.Name = newRoleName
        ExternalId = externalId
        Principals = [newUserId]
        Deleted = false }

    (******************************* 
     *** Create the Actor system *** 
     *******************************)     
    let roleCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
        testResources.System "role_management_command" testResources.ActorGroups.RoleManagementActors

    member this.Gherkin () = 
      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (State None)
      |> RoleGherkin.When (Events [ 
        RoleManagementEvent.Created (roleName, externalId) 
        PrincipalAdded newUserId
        NameUpdated newRoleName
        ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))


    member this.Preconditions () = 
      (*********************************
       *** Initialize pre-conditions ***
       *********************************)
      use context = new IdentityManagementDbContext (testResources.ConnectionOptions)       
      context.Users.Add (
        User (
          Id = newUserId,
          FirstName = "Sample",
          LastName = "Sarah",
          Email = "Sample@Sarah.com"
        )
      ) |> ignore
      context.SaveChanges () |> ignore

    member this.ActionUnderTest () = 
      (**************************
       *** Perform the action ***
       **************************)
      let processCommand = 
        Tests.envelop streamId
        >> roleCommandRequestReplyCanceled.Ask 
        >> runWaitAndIgnore 

      [ RoleManagementCommand
          .Create (roleName, externalId)
        AddPrincipal newUserId
        UpdateName newRoleName ]
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

      (*********************************
       *** Verify the Query DB state ***
       *********************************)
      let entityId = StreamId.unbox streamId

      use context = new IdentityManagementDbContext (testResources.ConnectionOptions)       
      let mapping = query {
        for m in context.RolePrincipalMaps do
        where (m.PrincipalId = newUserId && m.RoleId = entityId)
        select m
        exactlyOneOrDefault
      }

      Assert.Equal (true, not (isNull mapping))































