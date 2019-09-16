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
































type RolesTests2 ()  =

    [<Fact>]
    member this.``Access database`` () =

      use connection = new SqliteConnection("DataSource=:memory:")
      connection.Open()
      let options = (new DbContextOptionsBuilder<IdentityManagementDbContext> ()).UseSqlite(connection).Options;

      use context = new IdentityManagementDbContext (options)
      context.Database.EnsureCreated() |> ignore

      let id = Guid.NewGuid()
      use context' = new IdentityManagementDbContext (options)
      context'.UserEvents.Add (
          UserEventEnvelopeEntity (  Id = id,
                                  StreamId = Guid.NewGuid (),
                                  UserId = Guid.NewGuid (),
                                  TransactionId = Guid.NewGuid (),
                                  Version = 0s,
                                  TimeStamp = DateTimeOffset.Now ,
                                  Event = "{'key':'value'}"
                                  )) |> ignore         
      context'.SaveChanges () |> ignore

      use context'' = new IdentityManagementDbContext (options)
      let x = context''.UserEvents |> Seq.head
      Assert.Equal (id, x.Id)


    [<Fact>]
    member this.``Create role_ add user_ update title`` () =
      use testResources = new Composition.TestSystemResources ()
      let harness = ``Test: Create role_ add user_ update title`` testResources
      harness.Gherkin ()


    [<Fact>]
    member this.``Create role_ add user to group_ add group to role`` () =
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


      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (State None)
      |> RoleGherkin.When (Events [ 
        RoleManagementEvent.Created (roleName, externalRoleId) 
        PrincipalAdded groupId
        ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))


      (******************************* 
       *** Create the Actor system *** 
       *******************************)      
      use testResources = new Composition.TestSystemResources ()
      
      let userCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
          testResources.System "user_management_command" testResources.ActorGroups.UserManagementActors

      let groupCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<GroupManagementCommand, GroupManagementEvent> 
          testResources.System "group_management_command" testResources.ActorGroups.GroupManagementActors

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          testResources.System "role_management_command" testResources.ActorGroups.RoleManagementActors

      use context = new IdentityManagementDbContext (testResources.ConnectionOptions)

      (*********************************
       *** Initialize pre-conditions ***
       *********************************)
      let processCommand (rra:IActorRef) streamId = 
        Tests.envelop streamId
        >> rra.Ask 
        >> runWaitAndIgnore 

      let processUserCommand = 
        processCommand userCommandRequestReplyCanceled userStreamId
      [ UserManagementCommand.Create userDetails ]
      |> List.iter processUserCommand

      let processGroupCommand = 
        processCommand groupCommandRequestReplyCanceled groupStreamId
      [ GroupManagementCommand.Create groupName
        GroupManagementCommand.AddUser userId ]
      |> List.iter processGroupCommand


      (**************************
       *** Perform the action ***
       **************************)
      let processRoleCommand = processCommand roleCommandRequestReplyCanceled roleStreamId
        
      [ RoleManagementCommand
          .Create (roleName, externalRoleId)
        AddPrincipal groupId ]
      |> List.iter processRoleCommand

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

      let mapping = query {
        for m in context.RolePrincipalMaps do
        where (m.PrincipalId = userIdGuid && m.RoleId = entityId)
        select m
        headOrDefault
      }

      Assert.Equal (true, not (isNull mapping))


    [<Fact>]
    member this.``Create role_ add group to role_ add user to group`` () =
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


      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      RoleGherkin.Given (State None)
      |> RoleGherkin.When (Events [ 
        RoleManagementEvent.Created (roleName, externalRoleId) 
        PrincipalAdded groupId
        ])
      |> RoleGherkin.Then (expectState (Some (expectedState)))


      (******************************* 
       *** Create the Actor system *** 
       *******************************)      
      use testResources = new Composition.TestSystemResources ()

      let userCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
          testResources.System "user_management_command" testResources.ActorGroups.UserManagementActors

      let groupCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<GroupManagementCommand, GroupManagementEvent> 
          testResources.System "group_management_command" testResources.ActorGroups.GroupManagementActors

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          testResources.System "role_management_command" testResources.ActorGroups.RoleManagementActors

      use context = new IdentityManagementDbContext (testResources.ConnectionOptions)

      (*********************************
       *** Initialize pre-conditions ***
       *********************************)
      let processCommand (rra:IActorRef) streamId = 
        Tests.envelop streamId
        >> rra.Ask 
        >> runWaitAndIgnore 

      let processUserCommand = 
        processCommand userCommandRequestReplyCanceled userStreamId
      [ UserManagementCommand.Create userDetails ]
      |> List.iter processUserCommand

      let processGroupCommand = 
        processCommand groupCommandRequestReplyCanceled groupStreamId
      [ GroupManagementCommand
          .Create groupName ]
      |> List.iter processGroupCommand

      let processRoleCommand = processCommand roleCommandRequestReplyCanceled roleStreamId        
      [ RoleManagementCommand
          .Create (roleName, externalRoleId)
        AddPrincipal groupId ]
      |> List.iter processRoleCommand


      (**************************
       *** Perform the action ***
       **************************)    
      [ GroupManagementCommand
          .AddUser userId ]
      |> List.iter processGroupCommand
    
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

      let mapping = query {
        for m in context.RolePrincipalMaps do
        where (m.PrincipalId = userIdGuid && m.RoleId = entityId)
        select m
        headOrDefault
      }

      Assert.Equal (true, not (isNull mapping))



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
      use testResources = new Composition.TestSystemResources ()
      let persistence = { 
        testResources.Persistence with 
          roleManagementStore = InMemoryEventStore<RoleManagementEvent> (existingEventStore) }

      let actorGroups = composeActors persistence testResources.System

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          testResources.System "role_management_command" actorGroups.RoleManagementActors

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
      let events = 
        persistence.roleManagementStore.GetEvents streamId
        |> List.map (fun env -> env.Item) 

      let state = 
        events 
        |> List.fold IdentityManagement.Domain.RoleManagement.evolve None


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
      use testResources = new Composition.TestSystemResources ()

      let persistence = { 
        testResources.Persistence with 
          roleManagementStore = InMemoryEventStore<RoleManagementEvent> (existingEventStore) }

      let actorGroups = composeActors persistence testResources.System

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          testResources.System "role_management_command" actorGroups.RoleManagementActors


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
      let events = 
        persistence.roleManagementStore.GetEvents streamId
        |> List.map (fun env -> env.Item) 

      let state = 
        events 
        |> List.fold IdentityManagement.Domain.RoleManagement.evolve None

      (************************
       *** Verify the state ***
       ************************)
      Assert.Equal (Some expectedState, state)




