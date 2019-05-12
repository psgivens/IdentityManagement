module IdentityManagement.RoleCommands

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

open IdentityManagement.Dtos
open IdentityManagement.ProcessingSystem

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let private tellActor = tellActor actorGroups.RoleManagementActors 

let postNewRole (dto:RoleDto) = 
  let newRoleId = StreamId.create ()

  match dto.ext_id |> System.Guid.TryParse with
  | true, erid ->  
    (dto.name, erid)
    |> RoleManagementCommand.Create 
    |> tellActor newRoleId
    >=> 
    (newRoleId 
    |> sprintf "Role being created with id %A"
    |> OK)
  | _ -> noMatch


let deleteRole (roleName:string) =
  let role = DAL.RoleManagement.findRoleByName roleName

  RoleManagementCommand.Delete |> tellActor (StreamId.box role.Id)
  >=>
  OK "Role deleted"
  

let putRole (roleName:string) (dto:RoleDto) =
  let role = DAL.RoleManagement.findRoleByName roleName

  dto.name
  |> RoleManagementCommand.UpdateName
  |> tellActor (StreamId.box role.Id)
  >=>
  (dto.name |> toJson |> OK)
  


let addPrincipalToRole (roleName:string) (dto:RoleMemberDto) = 
  let role = DAL.RoleManagement.findRoleByName roleName

  match dto.id |> System.Guid.TryParse with
  | true, uid ->
    uid
    |> RoleManagementCommand.AddPrincipal
    |> tellActor (StreamId.box role.Id)
    >=> 
    (uid |> toJson |> OK)
  | _ -> noMatch


let removePrincipalFromRole (roleName:string) (dto:UserDto) = 
  let role = DAL.RoleManagement.findRoleByName roleName
  
  match dto.id |> System.Guid.TryParse with
  | true, uid ->
    uid
    |> RoleManagementCommand.RemovePrincipal
    |> tellActor (StreamId.box role.Id)
    >=>
    (uid |> toJson |> OK)
  | _ -> noMatch

