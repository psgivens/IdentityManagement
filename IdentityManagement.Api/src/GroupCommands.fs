module IdentityManagement.Api.GroupCommands

open Suave
open Suave.Filters
open Suave.Operators

open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open IdentityManagement.Domain.UserManagement
open Common.FSharp.Envelopes

open IdentityManagement.Api.ProcessingSystem
open IdentityManagement.Api.Dtos

open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let private tellActor = sendEnvelope actorGroups.GroupManagementActors.Tell 

let postNewGroup (dto:GroupDto) = 
  let newGroupId = StreamId.create ()

  let commandToActor =
    dto.name
    |> GroupManagementCommand.Create
    |> tellActor newGroupId

  let respond = newGroupId |> toJson |> OK

  commandToActor >=> respond

let deleteGroup (groupName:string) =
  let group = DAL.GroupManagement.findGroupByName groupName

  let commandToActor =
    GroupManagementCommand.Delete
    |> tellActor (StreamId.box group.Id)
  
  commandToActor >=> OK "Group deleted"

let putGroup (groupName:string) (dto:GroupDto) =
  let group = DAL.GroupManagement.findGroupByName groupName

  let commandToActor = 
    dto.name
    |> GroupManagementCommand.UpdateName
    |> tellActor (StreamId.box group.Id)

  let respond = dto.name |> toJson |> OK

  commandToActor >=> respond


let addUserToGroup (groupName:string) (dto:UserDto) = 
  let group = DAL.GroupManagement.findGroupByName groupName

  let command uid = 
    let commandToActor = 
      uid
      |> UserId.box
      |> GroupManagementCommand.AddUser
      |> tellActor (StreamId.box group.Id)

    let respond = uid |> toJson |> OK
    
    commandToActor >=> respond

  match dto.id |> System.Guid.TryParse with
  | true, uid -> command uid
  | _-> noMatch


let addGroupToGroup (groupName:string) (dto:GroupDto) = 
  let group = DAL.GroupManagement.findGroupByName groupName

  let command gid =  
    let commandToActor = 
      gid
      |> GroupManagementCommand.AddGroup
      |> tellActor (StreamId.box group.Id)

    let respond = gid |> toJson |> OK
    
    commandToActor >=> respond

  match dto.id |> System.Guid.TryParse with
  | true, gid -> command gid
  | _ -> noMatch


let removeUserFromGroup (groupName:string) (dto:UserDto) = 
  let group = DAL.GroupManagement.findGroupByName groupName

  let command uid =  
    let commandToActor =
      uid
      |> UserId.box
      |> GroupManagementCommand.RemoveUser
      |> tellActor (StreamId.box group.Id)

    let respond = uid |> toJson |> OK

    commandToActor >=> respond
  
  match dto.id |> System.Guid.TryParse with
  | true, uid -> command uid
  | _ -> noMatch


let removeGroupFromGroup (groupName:string) (dto:GroupDto) = 
  let group = DAL.GroupManagement.findGroupByName groupName
  
  let command gid =
    let commandToActor = 
      gid
      |> GroupManagementCommand.RemoveGroup
      |> tellActor (StreamId.box group.Id)      

    let respond = gid |> toJson |> OK

    commandToActor >=> respond

  match dto.id |> System.Guid.TryParse with
  | true, gid -> command gid
  | _, _ -> noMatch


