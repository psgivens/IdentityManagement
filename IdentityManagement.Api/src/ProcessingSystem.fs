[<AutoOpen>]
module IdentityManagement.Api.ProcessingSystem

open IdentityManagement.Api.Composition

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


open IdentityManagement.Data.Models

open Microsoft.EntityFrameworkCore

open IdentityManagement.Domain.DAL.RoleGroupUserRelations


let initialize () = 
    printfn "Resolve newtonsoft..."

    // System set up
    NewtonsoftHack.resolveNewtonsoft ()  

    printfn "Creating a new database..."
    
    
    let system = Configuration.defaultConfig () |> System.create "sample-system"

    let connectionString = System.IO.File.ReadAllText("config/IdentityManagementDbContext.connectionstring")
    let options = (new DbContextOptionsBuilder<IdentityManagementDbContext> ()).UseNpgsql(connectionString).Options
    initializeDatabase options

    let mappingDal = RoleUserMappingDAL options 

    let mappingDalMethods = {
      RoleGroupUserRelationActor.removeGroupUsers = mappingDal.RemoveGroupUsersFromRole
      RoleGroupUserRelationActor.updateGroupUsers = mappingDal.AddGroupUsersToRole
      RoleGroupUserRelationActor.removeRoleGroupUser = mappingDal.RemoveRoleGroupUser
      RoleGroupUserRelationActor.addRoleGroupUser = mappingDal.AddRoleGroupUser
      RoleGroupUserRelationActor.getRoles = mappingDal.GetRoles
    }

    let persistence = {
      userManagementStore = UserManagementEventStore options
      groupManagementStore = GroupManagementEventStore options
      roleManagementStore = RoleManagementEventStore options
      persistUserState = DAL.UserManagement.persist options
      persistGroupState = DAL.GroupManagement.persist options
      persistRoleState = DAL.RoleManagement.persist options
      persistRoleUserMappings = mappingDalMethods
    }

    printfn "Composing the actors..."
    let actorGroups = composeActors persistence system 

    let userCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
        system "user_management_command" actorGroups.UserManagementActors

    let groupCommandRequestReplyCanceled =
      RequestReplyActor.spawnRequestReplyActor<GroupManagementCommand, GroupManagementEvent>
        system "group_management_command" actorGroups.GroupManagementActors

    let roleCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<RoleManagementCommand, RoleManagementEvent>
        system "role_management_command" actorGroups.RoleManagementActors

    let runWaitAndIgnore = 
      Async.AwaitTask
      >> Async.Ignore
      >> Async.RunSynchronously

    let userId = UserId.create ()
    let envelop streamId = envelopWithDefaults userId (TransId.create ()) streamId

    printfn "Creating user..."
    { 
        FirstName="Phillip"
        LastName="Givens"
        Email="one@three.com"
    }
    |> UserManagementCommand.Create
    |> envelop (StreamId.create ())
    |> userCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let user = IdentityManagement.Domain.DAL.UserManagement.findUserByEmail "one@three.com"
    printfn "Created User %s with userId %A" user.Email user.Id

    let groupStreamId = StreamId.create ()
    printfn "Using group stream id: %A" groupStreamId
           
    GroupManagementCommand.Create "masters"
    |> envelop groupStreamId
    |> groupCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let group = IdentityManagement.Domain.DAL.GroupManagement.findGroupByName "masters"
    printfn "Using group %s with groupId %A" group.Name group.Id

    GroupManagementCommand.AddUser (user.Id |> UserId.box)
    |> envelop groupStreamId
    |> groupCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let roleName = "super users"
    let roleStreamId = StreamId.create ()
    let externalRoleId = Guid.NewGuid ()

    printfn "Creating role %s with external id %A" roleName externalRoleId

    (roleName, externalRoleId)
    |> RoleManagementCommand.Create
    |> envelop roleStreamId
    |> roleCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let role = IdentityManagement.Domain.DAL.RoleManagement.findRoleByName roleName
    printfn "Using Role %s with roleId %A and external id %A" role.Name role.Id role.ExternalId

    RoleManagementCommand.AddPrincipal group.Id
    |> envelop roleStreamId
    |> roleCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    printfn "Finished adding group to the role"

    actorGroups

open System.Runtime.CompilerServices
[<MethodImpl(MethodImplOptions.NoInlining)>]
let actorGroups = 
  printfn "Initializing actorGroups"
  initialize ()


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

let sendEnvelope<'a> (tell:Tell<'a>) (streamId:StreamId) (cmd:'a) (ctx:HttpContext) = 
  cmd
  |> envelopWithDefaults ctx streamId
  |> tell
  
  ctx |> Some |> async.Return 
