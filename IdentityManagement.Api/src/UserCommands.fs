module IdentityManagement.Api.UserCommands

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open Common.FSharp.Suave
open Common.FSharp.Envelopes

open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain
open IdentityManagement.Data.Models

open IdentityManagement.Api.ProcessingSystem
open IdentityManagement.Api.Dtos

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let private dtoToNewUser (dto:NewUserDto) = 
  { 
      FirstName=dto.first_name
      LastName=dto.last_name
      Email=dto.email
  }

let private dtoToUser (dto:UserDto) = 
  { 
      UserDetails.Email=dto.email
      FirstName=dto.first_name
      LastName=dto.last_name
  }

let private tellActor = sendEnvelope actorGroups.UserManagementActors.Tell 

let postUser (dto:NewUserDto) =  
  let newUserId = StreamId.create ()

  let commandToActor = 
    dto
    |> dtoToNewUser    
    |> UserManagementCommand.Create
    |> tellActor newUserId

  let respond = newUserId |> toJson |> OK

  commandToActor >=> respond


let deactivateUser (user:User) = 
  let commandToActor = 
    UserManagementCommand.Deactivate 
    |> tellActor (StreamId.box user.Id)

  let respond = 
    OK (sprintf "Deactivating %s" (user.Id.ToString ()))

  commandToActor >=> respond


let putUser userEmail (dto:UserDto) =
  let user = DAL.UserManagement.findUserByEmail userEmail

  let commandToActor = 
    dto
    |> dtoToUser
    |> UserManagementCommand.Update
    |> tellActor (StreamId.box user.Id)

  commandToActor >=> OK "Updating user..."


let deleteUser userEmail =
  let user = DAL.UserManagement.findUserByEmail userEmail

  let commandToActor = 
    UserManagementCommand.Deactivate
    |> tellActor (StreamId.box user.Id)

  let webpart = 
    user.Id
    |> sprintf "User with id %A deleted"
    |> OK

  commandToActor >=> webpart







  