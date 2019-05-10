module IdentityManagement.RestQuery

open Suave
open Suave.Filters
open Suave.Operators

open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open IdentityManagement.Data.Models
open IdentityManagement.Domain
open Common.FSharp.Envelopes
open Suave.State.CookieStateStore


type Foo =
  { foo : string }

type UserDto = { 
    Id : string
    FirstName : string 
    LastName : string
    Email : string }

type GroupMemberDto = {
  Id : string
  Name : string }

type GroupDto = { 
  Id : string
  Name : string
  Users : GroupMemberDto []
  Groups : GroupMemberDto [] }

let convertToDto (user:User) = { 
  UserDto.Email = user.Email
  FirstName = user.FirstName
  LastName = user.LastName
  Id = user.Id.ToString () }

let getUser userIdString =
  DAL.UserManagement.findUserByEmail "one@three.com"
  |> convertToDto
  |> toJson 
  |> OK

let getUsers (ctx:HttpContext) =
  DAL.UserManagement.getAllUsers ()
  |> List.map convertToDto
  |> toJson
  |> OK

