module IdentityManagement.ProcessingSystem

open System
open Akka.Actor
open Akka.FSharp

open IdentityManagement.Domain
open Common.FSharp.Envelopes
open IdentityManagement.Domain.DomainTypes
open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain
open Common.FSharp.Actors

open IdentityManagement.Domain.DAL.IdentityManagementEventStore
open Common.FSharp.Actors.Infrastructure

open IdentityManagement.Domain.DAL.Database
open Akka.Dispatch.SysMsg
open IdentityManagement.Domain.RoleManagement

type ActorGroups = {
    UserManagementActors:ActorIO<UserManagementCommand>
    GroupManagementActors:ActorIO<GroupManagementCommand>
    RoleManagementActors:ActorIO<RoleManagementCommand>
    }

let composeActors system =
    // Create member management actors
    let userManagementActors = 
        EventSourcingActors.spawn 
            (system,
             "userManagement", 
             UserManagementEventStore (),
             buildState UserManagement.evolve,
             UserManagement.handle,
             DAL.UserManagement.persist)    

    let groupManagementActors = 
        EventSourcingActors.spawn
            (system,
             "groupManagement",
             GroupManagementEventStore (),
             buildState GroupManagement.evolve,
             GroupManagement.handle,
             DAL.GroupManagement.persist
             )

    let roleManagementActors =
        EventSourcingActors.spawn   
            (system,
             "roleManagement",
             RoleManagementEventStore (),
             buildState RoleManagement.evolve,
             RoleManagement.handle,
             DAL.RoleManagement.persist)
             
    { UserManagementActors=userManagementActors
      GroupManagementActors=groupManagementActors
      RoleManagementActors=roleManagementActors }


let initialize () = 
    printfn "Resolve newtonsoft..."

    // System set up
    NewtonsoftHack.resolveNewtonsoft ()  

    printfn "Creating a new database..."
    initializeDatabase ()
    
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    printfn "Composing the actors..."
    let actorGroups = composeActors system

    let userCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
            system "user_management_command" actorGroups.UserManagementActors

    let groupCommandRequestReplyCanceled =
        RequestReplyActor.spawnRequestReplyActor<GroupManagementCommand, GroupManagementEvent>
            system "group_management_command" actorGroups.GroupManagementActors

    let roleCommandRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent>
            system "role_management_command" actorGroups.RoleManagementActors

    let userId = UserId.create ()

    printfn "Creating user..."
    { 
        FirstName="Phillip"
        LastName="Givens"
        Email="one@three.com"
    }
    |> UserManagementCommand.Create
    |> envelopWithDefaults
        userId
        (TransId.create ())
        (StreamId.create ())
        (Version.box 0s)
    |> userCommandRequestReplyCanceled.Ask
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore

    let user = IdentityManagement.Domain.DAL.UserManagement.findUserByEmail "one@three.com"
    printfn "Created User %s with userId %A" user.Email user.Id

    let groupStreamId = StreamId.create ()
    printfn "Using group stream id: %A" groupStreamId

    GroupManagementCommand.Create "masters"
    |> envelopWithDefaults
        userId
        (TransId.create ())
        groupStreamId
        (Version.box 0s)
    |> groupCommandRequestReplyCanceled.Ask
    |> Async.AwaitTask
    |> Async.Ignore
    |> Async.RunSynchronously

    let group = IdentityManagement.Domain.DAL.GroupManagement.findMemberByName "masters"
    printfn "Using group %s with groupId %A" group.Name group.Id

    GroupManagementCommand.AddUser (user.Id |> UserId.box)
    |> envelopWithDefaults
        userId
        (TransId.create ())
        groupStreamId
        (Version.box 0s)
    |> groupCommandRequestReplyCanceled.Ask
    |> Async.AwaitTask
    |> Async.Ignore
    |> Async.RunSynchronously

    let roleName = "super users"
    let roleStreamId = StreamId.create ()
    let externalRoleId = Guid.NewGuid ()

    printfn "Creating role %s with external id %A" roleName externalRoleId

    (roleName, externalRoleId)
    |> RoleManagement.Create
    |> envelopWithDefaults
        userId
        (TransId.create ())
        roleStreamId
        (Version.box 0s)
    |> roleCommandRequestReplyCanceled.Ask
    |> Async.AwaitTask
    |> Async.Ignore
    |> Async.RunSynchronously

    let role = IdentityManagement.Domain.DAL.RoleManagement.findRoleByName roleName
    printfn "Using Role %s with roleId %A and external id %A" role.Name role.Id role.ExternalId

    RoleManagementCommand.AddPrincipal group.Id
    |> envelopWithDefaults
        userId
        (TransId.create ())
        roleStreamId
        (Version.box 0s)
    |> roleCommandRequestReplyCanceled.Ask
    |> Async.AwaitTask
    |> Async.Ignore
    |> Async.RunSynchronously

    printfn "Finished adding group to the role"

    actorGroups

let actorGroups = initialize ()
