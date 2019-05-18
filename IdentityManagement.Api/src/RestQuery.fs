module IdentityManagement.Api.RestQuery

open IdentityManagement.Api.Dtos

open Suave
open Suave.Successful

open Common.FSharp.Suave
open IdentityManagement.Domain

let getUser userIdString =
  DAL.UserManagement.findUserByEmail userIdString
  |> convertToDto
  |> toJson 
  |> OK

let getUsers (ctx:HttpContext) =
  DAL.UserManagement.getAllUsers ()
  |> List.map convertToDto
  |> toJson
  |> OK

