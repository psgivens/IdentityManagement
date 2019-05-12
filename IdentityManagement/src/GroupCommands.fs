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

let private tellActor (streamId:StreamId) (cmd:GroupManagementCommand) (ctx:HttpContext) = 
  cmd
  |> envelopWithDefaults ctx streamId
  |> actorGroups.GroupManagementActors.Tell
  
  ctx |> Some |> async.Return 

let postNewGroup (dto:GroupDto) = 
  let newGroupId = StreamId.create ()

  let actorWebPart =
    dto.name
    |> GroupManagementCommand.Create
    |> tellActor newGroupId

  let responseWebPart = newGroupId |> toJson |> OK

  actorWebPart >=> responseWebPart

let deleteGroup (groupName:string) =
  let group = DAL.GroupManagement.findGroupByName groupName

  let actorWebPart =
    GroupManagementCommand.Delete
    |> tellActor (StreamId.box group.Id)
  
  actorWebPart >=> OK "Group deleted"

let putGroup (groupName:string) =
  let group = DAL.GroupManagement.findGroupByName groupName

  let processRequest (dto:GroupDto) = 
    let actorWebPart = 
      dto.name
      |> GroupManagementCommand.UpdateName
      |> tellActor (StreamId.box group.Id)
  
    let responseWebPart = dto.name |> toJson |> OK

    actorWebPart >=> responseWebPart

  restWebPart processRequest

let addUserToGroup (groupName:string) = 
  let group = DAL.GroupManagement.findGroupByName groupName

  let command uid = 
    let actorWebPart = 
      uid
      |> UserId.box
      |> GroupManagementCommand.AddUser
      |> tellActor (StreamId.box group.Id)

    let responseWebPart = uid |> toJson |> OK
    
    actorWebPart >=> responseWebPart

  let processRequest (dto:UserDto) =
    match dto.id |> System.Guid.TryParse with
    | true, uid -> command uid
    | _-> noMatch

  restWebPart processRequest

let addGroupToGroup (groupName:string) = 
  let group = DAL.GroupManagement.findGroupByName groupName

  let command gid =  
    let actorWebPart = 
      gid
      |> GroupManagementCommand.AddGroup
      |> tellActor (StreamId.box group.Id)

    let responseWebPart = gid |> toJson |> OK
    
    actorWebPart >=> responseWebPart

  let processRequest (dto:GroupDto) =
    match dto.id |> System.Guid.TryParse with
    | true, gid -> command gid
    | _ -> noMatch

  restWebPart processRequest

let removeUserFromGroup (groupName:string) = 
  let group = DAL.GroupManagement.findGroupByName groupName

  let command uid =  
    let actorWebPart =
      uid
      |> UserId.box
      |> GroupManagementCommand.RemoveUser
      |> tellActor (StreamId.box group.Id)

    let responseWebPart = uid |> toJson |> OK

    actorWebPart >=> responseWebPart
  
  let processRequest (dto:UserDto) =
    match dto.id |> System.Guid.TryParse with
    | true, uid -> command uid
    | _ -> noMatch

  restWebPart processRequest

let removeGroupFromGroup (groupName:string) = 
  let group = DAL.GroupManagement.findGroupByName groupName
  
  let command gid =
    let actorWebPart = 
      gid
      |> GroupManagementCommand.RemoveGroup
      |> tellActor (StreamId.box group.Id)      

    let responseWebPart = gid |> toJson |> OK

    actorWebPart >=> responseWebPart

  let processRequest (dto:GroupDto) =
    match dto.id |> System.Guid.TryParse with
    | true, gid -> command gid
    | _, _ -> noMatch

  restWebPart processRequest

