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
open Common.FSharp

open Suave
open Common.FSharp.Suave

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
    |> groupCommandRequestReplyCanceled.Ask
    |> Async.AwaitTask
    |> Async.Ignore
    |> Async.RunSynchronously

    let group = IdentityManagement.Domain.DAL.GroupManagement.findGroupByName "masters"
    printfn "Using group %s with groupId %A" group.Name group.Id

    GroupManagementCommand.AddUser (user.Id |> UserId.box)
    |> envelopWithDefaults
        userId
        (TransId.create ())
        groupStreamId
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
    |> roleCommandRequestReplyCanceled.Ask
    |> Async.AwaitTask
    |> Async.Ignore
    |> Async.RunSynchronously

    printfn "Finished adding group to the role"

    actorGroups

let actorGroups = initialize ()


type DomainContext = {
  UserId: UserId
  TransId: TransId
}

let inline private addContext (item:DomainContext) (ctx:HttpContext) = 
  { ctx with userState = ctx.userState |> Map.add "domain_context" (box item) }

let inline private getDomainContext (ctx:HttpContext) :DomainContext =
  ctx.userState |> Map.find "domain_context" :?> DomainContext

let authenticationHeaders (p:HttpRequest) = 
  let h = 
    ["user_id"; "transaction_id"]
    |> List.map (p.header >> Option.ofChoice)

  match h with
  | [Some userId; Some transId] -> 
    let (us, uid) = userId |> Guid.TryParse
    let (ut, tid) = transId |> Guid.TryParse
    if us && ut then 
        addContext { 
            UserId = UserId.box uid; 
            TransId = TransId.box tid 
        } 
        >> Some 
        >> async.Return
    else noMatch
  | _ -> noMatch

let envelopWithDefaults (ctx:HttpContext) = 
    let domainContext = getDomainContext ctx
    Common.FSharp.Envelopes.Envelope.envelopWithDefaults
        domainContext.UserId
        domainContext.TransId

let tellActor<'a> (actor:ActorIO<'a>) (streamId:StreamId) (cmd:'a) (ctx:HttpContext) = 
  cmd
  |> envelopWithDefaults ctx streamId
  |> actor.Tell
  
  ctx |> Some |> async.Return 