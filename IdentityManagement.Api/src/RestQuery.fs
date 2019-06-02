module IdentityManagement.Api.RestQuery

open IdentityManagement.Api.Dtos

open Suave
open Suave.Successful

open Common.FSharp.Suave
open IdentityManagement.Domain

let getUser userIdString =
  DAL.UserManagement.findUserByEmail userIdString
  |> convertToUserDto
  |> toJson 
  |> OK

let getUsers (ctx:HttpContext) =
  DAL.UserManagement.getAllUsers ()
  |> List.map convertToUserDto
  |> toJson
  |> OK

let getGroups (ctx:HttpContext) =
  DAL.GroupManagement.getAllGroups ()
  |> List.map convertToGroupDto
  |> toJson
  |> OK

let getRoles (ctx:HttpContext) = 
  DAL.RoleManagement.getAllRoles ()
  |> List.map convertToRoleDto
  |> toJson
  |> OK




  