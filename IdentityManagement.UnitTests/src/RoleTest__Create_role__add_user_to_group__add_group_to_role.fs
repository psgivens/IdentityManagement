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

type ``Test: Create role, add user to group, add group to role`` (testResources:Composition.TestSystemResources) =
    (*********************************************
     *** Create some sample data for the test  ***
     *********************************************)
    let userStreamId = StreamId.create ()
    let groupStreamId = StreamId.create ()
    let roleStreamId = StreamId.create ()

    let userDetails =
      { 
          FirstName="Phillip"
          LastName="Givens"
          Email="one@three.com"
      }
    let userIdGuid = StreamId.unbox userStreamId
    let userId = UserId.box userIdGuid

    let groupName = "GreatGuys"
    let groupId = StreamId.unbox groupStreamId

    let roleName = "SystemAdmins"
    let externalRoleId = Guid.NewGuid ()

    let expectedState = 
      { RoleManagementState.Name = roleName
        ExternalId = externalRoleId
        Principals = [groupId]
        Deleted = false }

    (******************************* 
     *** Create the Actor system *** 
     *******************************)     
    
    let userCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
        testResources.System "user_management_command" testResources.ActorGroups.UserManagementActors

    let groupCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<GroupManagementCommand, GroupManagementEvent> 
        testResources.System "group_management_command" testResources.ActorGroups.GroupManagementActors

    let roleCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
        testResources.System "role_management_command" testResources.ActorGroups.RoleManagementActors

    let processCommand (rra:IActorRef) streamId = 
      Tests.envelop streamId
      >> rra.Ask 
      >> runWaitAndIgnore 

    member this.Gherkin () = 
      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (State None)
      |> RoleGherkin.When (Events [ 
        RoleManagementEvent.Created (roleName, externalRoleId) 
        PrincipalAdded groupId
        ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))



    member this.Preconditions () = 
      (*********************************
       *** Initialize pre-conditions ***
       *********************************)

      let processUserCommand = 
        processCommand userCommandRequestReplyCanceled userStreamId
      [ UserManagementCommand.Create userDetails ]
      |> List.iter processUserCommand

      let processGroupCommand = 
        processCommand groupCommandRequestReplyCanceled groupStreamId
      [ GroupManagementCommand.Create groupName
        GroupManagementCommand.AddUser userId ]
      |> List.iter processGroupCommand



    member this.ActionUnderTest () = 
      (**************************
       *** Perform the action ***
       **************************)
      let processRoleCommand = processCommand roleCommandRequestReplyCanceled roleStreamId
        
      [ RoleManagementCommand
          .Create (roleName, externalRoleId)
        AddPrincipal groupId ]
      |> List.iter processRoleCommand


    member this.VerifyState () =
      (*************************
       *** Evolve the events ***
       *************************)
      let events = 
        testResources.Persistence.roleManagementStore.GetEvents roleStreamId
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
      let entityId = StreamId.unbox roleStreamId

      // Terminate the actors makes us wait until all active messages are complete.
      testResources.TermateActors ()

      use context = new IdentityManagementDbContext (testResources.ConnectionOptions)
      let mapping = query {
        for m in context.RolePrincipalMaps do
        where (m.PrincipalId = userIdGuid && m.RoleId = entityId)
        select m
        headOrDefault
      }

      Assert.Equal (true, not (isNull mapping))



