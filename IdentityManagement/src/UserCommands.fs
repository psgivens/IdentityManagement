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

let private tellActor = tellActor actorGroups.UserManagementActors 

let postUser (dto:UserDto)=  
  let newUserId = StreamId.create ()

  let commandToActor = 
    dto
    |> dtoToUser    
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







  