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

let postNewRole (ctx:HttpContext) = 
  let processRequest (dto:RoleDto) =
    let newRoleId = StreamId.create ()
    let (b,erid) = dto.ext_id |> System.Guid.TryParse

    let send = 
      envelopWithDefaults ctx newRoleId
      >> actorGroups.RoleManagementActors.Tell

    if b then 
      (dto.name, erid)
      |> RoleManagementCommand.Create 
      |> send

      newRoleId
    else 
      StreamId.Empty 

  ctx |> restWebPart processRequest


let deleteRole (roleName:string) (ctx:HttpContext) =
  let role = DAL.RoleManagement.findRoleByName roleName
  let send = 
    envelopWithDefaults ctx (StreamId.box role.Id)
    >> actorGroups.RoleManagementActors.Tell

  RoleManagementCommand.Delete |> send

  let webpart = 
    OK "Role deleted"
  
  ctx |> webpart

let putRole (roleName:string) (ctx:HttpContext) =
  let role = DAL.RoleManagement.findRoleByName roleName
  let send = 
    envelopWithDefaults ctx (StreamId.box role.Id)
    >> actorGroups.RoleManagementActors.Tell

  let processRequest (dto:RoleDto) = 
    dto.name
    |> RoleManagementCommand.UpdateName
    |> send

    dto.name
  
  ctx |> restWebPart processRequest

let addPrincipalToRole (roleName:string) (ctx:HttpContext) = 
  let role = DAL.RoleManagement.findRoleByName roleName
  let send = 
    envelopWithDefaults ctx (StreamId.box role.Id)
    >> actorGroups.RoleManagementActors.Tell

  let processRequest (dto:RoleMemberDto) =
    let (us, uid) = dto.id |> System.Guid.TryParse
    if us then
      uid
      |> RoleManagementCommand.AddPrincipal
      |> send

      uid
    else
      System.Guid.Empty

  ctx |> restWebPart processRequest


let removePrincipalFromRole (roleName:string) (ctx:HttpContext) = 
  let role = DAL.RoleManagement.findRoleByName roleName
  let send = 
    envelopWithDefaults ctx (StreamId.box role.Id)
    >> actorGroups.RoleManagementActors.Tell
  
  let processRequest (dto:UserDto) =
    let (us, uid) = dto.id |> System.Guid.TryParse
    if us then
      uid
      |> RoleManagementCommand.RemovePrincipal
      |> send
      
      uid
    else
      System.Guid.Empty

  ctx |> restWebPart processRequest

