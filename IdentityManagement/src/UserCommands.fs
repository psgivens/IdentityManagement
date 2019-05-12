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

let private tellActor (streamId:StreamId) (cmd:UserManagementCommand) (ctx:HttpContext) = 
  cmd
  |> envelopWithDefaults ctx streamId
  |> actorGroups.UserManagementActors.Tell
  
  ctx |> Some |> async.Return 

let postUser (dto:UserDto)=  
  let newUserId = StreamId.create ()

  let actorWebPart = 
    dto
    |> dtoToUser    
    |> UserManagementCommand.Create
    |> tellActor newUserId

  let responseWebPart = newUserId |> toJson |> OK

  actorWebPart >=> responseWebPart


let deactivateUser (user:User) = 
  let actorWebPart = 
    UserManagementCommand.Deactivate 
    |> tellActor (StreamId.box user.Id)

  let responseWebPart = 
    OK (sprintf "Deactivating %s" (user.Id.ToString ()))

  actorWebPart >=> responseWebPart


let putUser userEmail (dto:UserDto) =
  let processRequest  = 
    let user = DAL.UserManagement.findUserByEmail userEmail

    let actorWebPart = 
      dto
      |> dtoToUser
      |> UserManagementCommand.Update
      |> tellActor (StreamId.box user.Id)

    actorWebPart >=> OK "Updating user..."

  restWebPart processRequest

let deleteUser userEmail =
  let user = DAL.UserManagement.findUserByEmail userEmail

  let actorWebPart = 
    UserManagementCommand.Deactivate
    |> tellActor (StreamId.box user.Id)

  let webpart = 
    user.Id
    |> sprintf "User with id %A deleted"
    |> OK

  actorWebPart >=> webpart







  