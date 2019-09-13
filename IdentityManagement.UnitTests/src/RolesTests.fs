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

type RolesTests ()  =

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
      use system = Configuration.defaultConfig () |> System.create "sample-system"

      use connection = Composition.getDbConnection ()
      let persistence = Composition.createPersistenceLayer connection

      let actorGroups = composeActors system persistence

      let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent> 
          system "role_management_command" actorGroups.RoleManagementActors


      (*********************************
       *** Initialize pre-conditions ***
       *********************************)
      let connectionOptions = Composition.getDbContextOptions connection
      use context = new IdentityManagementDbContext (connectionOptions)
      context.Users.Add (
        User (
          Id = newUserId,
          FirstName = "Sample",
          LastName = "Sarah",
          Email = "Sample@Sarah.com"
        )
      ) |> ignore
      context.SaveChanges () |> ignore


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

      (*********************************
       *** Verify the Query DB state ***
       *********************************)
      let entityId = StreamId.unbox streamId

      let mapping = query {
        for m in context.RolePrincipalMaps do
        where (m.PrincipalId = newUserId && m.RoleId = entityId)
        select m
        exactlyOneOrDefault
      }

      // // TODO: Replace this query with just the role-prin-map that we want. 
      // let entity = query {
      //   for r in context.Roles.Include "Members" do
      //   where (r.Id = entityId)
      //   select r
      //   exactlyOneOrDefault
      // }

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
      let system = Configuration.defaultConfig () |> System.create "sample-system"
      
      use connection = Composition.getDbConnection ()
      let persistence' = Composition.createPersistenceLayer connection
      let persistence = { 
        persistence' with 
          roleManagementStore = InMemoryEventStore<RoleManagementEvent> (existingEventStore) }

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
      let system = Configuration.defaultConfig () |> System.create "sample-system"

      use connection = Composition.getDbConnection ()
      let persistence' = Composition.createPersistenceLayer connection
      let persistence = { 
        persistence' with 
          roleManagementStore = InMemoryEventStore<RoleManagementEvent> (existingEventStore) }

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




