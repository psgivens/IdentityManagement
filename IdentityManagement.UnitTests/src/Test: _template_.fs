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

type ``__describe what should  happen__`` (testResources:Composition.TestSystemResources) =
    (*********************************************
     *** Create some sample data for the test  ***
     *********************************************)
    let userStreamId = StreamId.create ()
    let groupStreamId = StreamId.create ()
    let roleStreamId = StreamId.create ()

    // TODO: create some sample data

    (******************************* 
     *** Create the Actor system *** 
     *******************************)      

    // TODO: create request-reply actors for processing
    // let userCommandRequestReplyCanceled = 
    //   RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
    //     testResources.System "user_management_command" testResources.ActorGroups.UserManagementActors


    (******************************* 
     *** Utility functions       *** 
     *******************************)      
    // TODO: create utility methods
    // let processCommand (rra:IActorRef) streamId = 
    //   Tests.envelop streamId
    //   >> rra.Ask 
    //   >> runWaitAndIgnore 


    member this.Gherkin () = 
      (*********************************************
       *** Describe the expectations in Gherkin  ***
       *********************************************)
      // TODO: Describe the events in Gherkin

      // RoleGherkin.Given (State None)
      // |> RoleGherkin.When (Events [ 
      //   RoleManagementEvent.Created (roleName, externalRoleId) 
      //   PrincipalAdded groupId
      //   ])
      // |> RoleGherkin.Then (expectState (Some (expectedState)))
      ()

    member this.Preconditions () = 
      (*********************************
       *** Initialize pre-conditions ***
       *********************************)
      // TODO: Do any preprocessing

      // existingEvents
      // |> List.map (Tests.envelop streamId)
      // |> List.iter testResources.Persistence.roleManagementStore.AppendEvent

      // let processUserCommand = 
      //   processCommand userCommandRequestReplyCanceled userStreamId
      // [ UserManagementCommand.Create userDetails ]
      // |> List.iter processUserCommand
      ()

    member this.ActionUnderTest () = 
      (**************************
       *** Perform the action ***
       **************************)    
      // TODO: Perform the actions to test
      // let processGroupCommand = 
      //   processCommand groupCommandRequestReplyCanceled groupStreamId
 
      // [ GroupManagementCommand
      //     .AddUser userId ]
      // |> List.iter processGroupCommand
      ()

    member this.VerifyState () =
      (*************************
       *** Evolve the events ***
       *************************)
      // TODO: Verify the state via events
      // let events = 
      //   testResources.Persistence.roleManagementStore.GetEvents roleStreamId
      //   |> List.map (fun env -> env.Item) 

      // let state = 
      //   events 
      //   |> List.fold IdentityManagement.Domain.RoleManagement.evolve None

      // Assert.Equal (Some expectedState, state)

      (*********************************
       *** Verify the Query DB state ***
       *********************************)
      // TODO: Verify any database state

      // let entityId = StreamId.unbox roleStreamId

      // // Terminate the actors makes us wait until all active messages are complete.
      // testResources.TermateActors ()

      // use context = new IdentityManagementDbContext (testResources.ConnectionOptions)

      // let mapping = query {
      //   for m in context.RolePrincipalMaps do
      //   where (m.PrincipalId = userIdGuid && m.RoleId = entityId)
      //   select m
      //   headOrDefault
      // }

      // Assert.Equal (true, not (isNull mapping))
      ()

type TestTemplate () =

    [<Fact>]
    member this.``__describe what should  happen__`` () =
      use testResources = new Composition.TestSystemResources ()
      let harness = ``__describe what should  happen__`` testResources
      harness.Gherkin ()
      harness.Preconditions ()
      harness.ActionUnderTest ()
      harness.VerifyState ()
