module IdentityManagement.GroupCommands

open Suave
open Suave.Filters
open Suave.Operators

open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open IdentityManagement.Domain.UserManagement
open Common.FSharp.Envelopes

open IdentityManagement.ProcessingSystem

open IdentityManagement.Dtos
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let postNewGroup (ctx:HttpContext) = 
  let processRequest (dto:GroupDto) =
    let newGroupId = StreamId.create ()
    let envelop = envelopWithDefaults ctx newGroupId

    dto.name
    |> GroupManagementCommand.Create 
    |> envelop
    |> actorGroups.GroupManagementActors.Tell

    newGroupId

  ctx |> restWebPart processRequest

let deleteGroup (groupName:string) (ctx:HttpContext) =
  let group = DAL.GroupManagement.findGroupByName groupName
  let envelop = envelopWithDefaults ctx (StreamId.box group.Id)

  GroupManagementCommand.Delete
  |> envelop
  |> actorGroups.GroupManagementActors.Tell

  let webpart = 
    OK "Group deleted"
  
  ctx |> webpart

let putGroup (groupName:string) (ctx:HttpContext) =
  let group = DAL.GroupManagement.findGroupByName groupName
  let envelop = envelopWithDefaults ctx (StreamId.box group.Id)

  let processRequest (dto:GroupDto) = 
    dto.name
    |> GroupManagementCommand.UpdateName
    |> envelop
    |> actorGroups.GroupManagementActors.Tell
    dto.name
  
  ctx |> restWebPart processRequest

let addUserToGroup (groupName:string) (ctx:HttpContext) = 
  let group = DAL.GroupManagement.findGroupByName groupName
  let send = 
    envelopWithDefaults ctx (StreamId.box group.Id)
    >> actorGroups.GroupManagementActors.Tell

  let processRequest (dto:UserDto) =
    let (us, uid) = dto.id |> System.Guid.TryParse
    if us then
      uid
      |> UserId.box
      |> GroupManagementCommand.AddUser
      |> send

      uid
    else
      System.Guid.Empty

  ctx |> restWebPart processRequest

let addGroupToGroup (groupName:string) (ctx:HttpContext) = 
  let group = DAL.GroupManagement.findGroupByName groupName
  let send = 
    envelopWithDefaults ctx (StreamId.box group.Id)
    >> actorGroups.GroupManagementActors.Tell
  
  let processRequest (dto:GroupDto) =
    let (gs, gid) = dto.id |> System.Guid.TryParse
    if gs then
      gid
      |> GroupManagementCommand.AddGroup
      |> send

      gid
    else
      System.Guid.Empty

  ctx |> restWebPart processRequest

let removeUserFromGroup (groupName:string) (ctx:HttpContext) = 
  let group = DAL.GroupManagement.findGroupByName groupName
  let send = 
    envelopWithDefaults ctx (StreamId.box group.Id)
    >> actorGroups.GroupManagementActors.Tell
  
  let processRequest (dto:UserDto) =
    let (us, uid) = dto.id |> System.Guid.TryParse
    if us then
      uid
      |> UserId.box
      |> GroupManagementCommand.RemoveUser
      |> send

      uid
    else
      System.Guid.Empty

  ctx |> restWebPart processRequest

let removeGroupFromGroup (groupName:string) (ctx:HttpContext) = 
  let group = DAL.GroupManagement.findGroupByName groupName
  let send = 
    envelopWithDefaults ctx (StreamId.box group.Id)
    >> actorGroups.GroupManagementActors.Tell
  
  let processRequest (dto:GroupDto) =
    let (gs, gid) = dto.id |> System.Guid.TryParse
    if gs then
      gid
      |> GroupManagementCommand.RemoveGroup
      |> send
      
      gid
    else
      System.Guid.Empty

  ctx |> restWebPart processRequest

