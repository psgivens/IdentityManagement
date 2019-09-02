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
    member this.``Create role, add user, update title`` () =
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

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          system "role_management_command" actorGroups.RoleManagementActors


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

      (*************************
       *** Evolve the events ***
       *************************)
      let evolve evts = 
        let evolve' = IdentityManagement.Domain.RoleManagement.evolve
        evts |> Seq.fold evolve' None

      let events = 
        persistence.roleManagementStore.GetEvents streamId
        |> Seq.map (fun env -> env.Item) 

      let state = events |> evolve

      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal (Some expectedState, state)



    [<Fact>]
    member this.``Remove user from a role`` () =
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

      let existingEventStore = 
        Map.empty
        |> Map.add streamId (existingEvents |> List.map (Tests.envelop streamId))

      let expectedState = 
        { RoleManagementState.Name = roleName
          ExternalId = externalId
          Principals = [userId2]
          Deleted = false }


      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (Preconditions.Events existingEvents)
      |> RoleGherkin.When (Events [ PrincipalRemoved userId1 ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))


      (******************************* 
       *** Create the Actor system *** 
       *******************************)      
      let system = Configuration.defaultConfig () |> System.create "sample-system"

      let persistence = {
        userManagementStore = InMemoryEventStore<UserManagementEvent> ()
        groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
        roleManagementStore = InMemoryEventStore<RoleManagementEvent> (existingEventStore)
        persistUserState = doNotPersist
        persistGroupState = doNotPersist
        persistRoleState = doNotPersist
      }

      let actorGroups = composeActors system persistence

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          system "role_management_command" actorGroups.RoleManagementActors

      (**************************
       *** Perform the action ***
       **************************)

      let processCommand = 
        Tests.envelop streamId
        >> roleCommandRequestReplyCanceled.Ask 
        >> runWaitAndIgnore 

      [ RemovePrincipal userId1 ]
      |> List.iter processCommand

      (*************************
       *** Evolve the events ***
       *************************)
      let evolve evts = 
        let evolve' = IdentityManagement.Domain.RoleManagement.evolve
        evts |> Seq.fold evolve' None

      let events = 
        persistence.roleManagementStore.GetEvents streamId
        |> Seq.map (fun env -> env.Item) 

      let state = events |> evolve

      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal (Some expectedState, state)


    [<Fact>]
    member this.``Delete role`` () =
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


      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (Preconditions.Events existingEvents)
      |> RoleGherkin.When (Events [ Deleted ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))


      (******************************* 
       *** Create the Actor system *** 
       *******************************)      
      let system = Configuration.defaultConfig () |> System.create "sample-system"

      let persistence = {
        userManagementStore = InMemoryEventStore<UserManagementEvent> ()
        groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
        roleManagementStore = InMemoryEventStore<RoleManagementEvent> (existingEventStore)
        persistUserState = doNotPersist
        persistGroupState = doNotPersist
        persistRoleState = doNotPersist
      }

      let actorGroups = composeActors system persistence

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          system "role_management_command" actorGroups.RoleManagementActors


      (**************************
       *** Perform the action ***
       **************************)
      let processCommand = 
        Tests.envelop streamId
        >> roleCommandRequestReplyCanceled.Ask 
        >> runWaitAndIgnore 

      [ Delete ]
      |> List.iter processCommand

      (*************************
       *** Evolve the events ***
       *************************)
      let evolve evts = 
        let evolve' = IdentityManagement.Domain.RoleManagement.evolve
        evts |> Seq.fold evolve' None

      let events = 
        persistence.roleManagementStore.GetEvents streamId
        |> Seq.map (fun env -> env.Item) 

      let state = events |> evolve

      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal (Some expectedState, state)




