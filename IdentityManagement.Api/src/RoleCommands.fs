module IdentityManagement.Api.RoleCommands

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open Common.FSharp.Envelopes

open IdentityManagement.Domain.RoleManagement
open IdentityManagement.Domain

open IdentityManagement.Api.Dtos
open IdentityManagement.Api.ProcessingSystem

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let private tellActor = sendEnvelope actorGroups.RoleManagementActors.Tell 

let postNewRole (dto:RoleDto) = 
  let newRoleId = StreamId.create ()

  match dto.ext_id |> System.Guid.TryParse with
  | true, erid ->  

    let commandToActor = 
      (dto.name, erid)
      |> RoleManagementCommand.Create 
      |> tellActor newRoleId
    
    let respond = 
      (newRoleId 
      |> sprintf "Role being created with id %A"
      |> OK)

    commandToActor >=> respond

  | _ -> noMatch


let deleteRole (roleName:string) =
  let role = DAL.RoleManagement.findRoleByName roleName

  let commandToActor = 
    RoleManagementCommand.Delete 
    |> tellActor (StreamId.box role.Id)
 
  commandToActor >=> OK "Role deleted"  

let putRole (roleName:string) (dto:RoleDto) =
  let role = DAL.RoleManagement.findRoleByName roleName

  let commandToActor = 
    dto.name
    |> RoleManagementCommand.UpdateName
    |> tellActor (StreamId.box role.Id)

  let respond = dto.name |> toJson |> OK

  commandToActor >=> respond


let addPrincipalToRole (roleName:string) (dto:RoleMemberDto) = 
  let role = DAL.RoleManagement.findRoleByName roleName

  match dto.id |> System.Guid.TryParse with
  | true, uid ->
    let commandToActor = 
      uid
      |> RoleManagementCommand.AddPrincipal
      |> tellActor (StreamId.box role.Id)

    let respond = uid |> toJson |> OK

    commandToActor >=> respond

  | _ -> noMatch


let removePrincipalFromRole (roleName:string) (dto:UserDto) = 
  let role = DAL.RoleManagement.findRoleByName roleName
  
  match dto.id |> System.Guid.TryParse with
  | true, uid ->
    let commandToActor = 
      uid
      |> RoleManagementCommand.RemovePrincipal
      |> tellActor (StreamId.box role.Id)

    let respond = uid |> toJson |> OK

    commandToActor >=> respond
  | _ -> noMatch

