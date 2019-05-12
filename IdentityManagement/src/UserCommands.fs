module IdentityManagement.UserCommands

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open Common.FSharp.Suave
open Common.FSharp.Envelopes

open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain
open IdentityManagement.Data.Models

open IdentityManagement.ProcessingSystem
open IdentityManagement.Dtos

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let private dtoToUser dto = 
  { 
      FirstName=dto.first_name
      LastName=dto.last_name
      Email=dto.email
  }

let postUser (ctx:HttpContext) :Async<HttpContext option>=
  let processRequest (dto:UserDto) = 
    let newUserId = StreamId.create ()
    let send = 
      envelopWithDefaults ctx newUserId
      >> actorGroups.UserManagementActors.Tell

    dto
    |> dtoToUser    
    |> UserManagementCommand.Create
    |> send

    newUserId

  let webpart = 
    Writers.setMimeType "application/json; charset=utf-8"
    >=> request (getDtoFromReq >> processRequest >> toJson >> OK)
   
  ctx |> webpart

let deactivateUser (user:User) (ctx:HttpContext) = 
  let send = 
    envelopWithDefaults ctx (StreamId.box user.Id)
    >> actorGroups.UserManagementActors.Tell

  UserManagementCommand.Deactivate 
  |> send

  let webpart = 
    OK (sprintf "Deactivating %s" (user.Id.ToString ()))

  ctx |> webpart

let putUser userEmail (ctx:HttpContext) =
  let processRequest (dto:UserDto) = 
    let user = DAL.UserManagement.findUserByEmail userEmail
    let send = 
      envelopWithDefaults ctx (StreamId.box user.Id)
      >> actorGroups.UserManagementActors.Tell

    dto
    |> dtoToUser
    |> UserManagementCommand.Update
    |> send

  ctx |> restWebPart processRequest

let deleteUser userEmail (ctx:HttpContext) =
  let user = DAL.UserManagement.findUserByEmail userEmail
  let send = 
    envelopWithDefaults ctx (StreamId.box user.Id)
    >> actorGroups.UserManagementActors.Tell

  UserManagementCommand.Deactivate
  |> send

  let webpart = 
    user.Id
    |> sprintf "User with id %A deleted"
    |> OK

  ctx |> webpart







  