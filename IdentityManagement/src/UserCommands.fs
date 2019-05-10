module IdentityManagement.UserCommands

open Suave
open Suave.Filters
open Suave.Operators

open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain
open Common.FSharp.Envelopes
open IdentityManagement.Data.Models
open IdentityManagement.ProcessingSystem

type Foo =
  { foo : string }

type Bar =
  { bar : string }


let deactivateUser (user:User) = 
  UserManagementCommand.Deactivate 
  |> envelopWithDefaults
      (UserId.create ()) // Should get this from the context
      (TransId.create ())
      (user.Id |> StreamId.box)
      (Version.box 0s)
  |> actorGroups.UserManagementActors.Tell

  OK (sprintf "Deactivating %s" (user.Id.ToString ()))

let handleUpdateUser userIdString =
  let user = DAL.UserManagement.getHeadUser ()
  choose 
    [ PUT >=> OK "Recieved a PUT"
      DELETE >=> deactivateUser user
    ]



    

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/


let handleUserPost =
  let processRequest (foo:Foo) = 
    let newUserId = StreamId.create ()
    { 
        FirstName="Johhny"
        LastName=foo.foo
        Email="two@four.com"
    }
    |> UserManagementCommand.Create
    |> envelopWithDefaults
        (UserId.create ()) // Should get this from the context
        (TransId.create ())
        newUserId
        (Version.box 0s)
    |> actorGroups.UserManagementActors.Tell
    { Bar.bar="Johnny" }

  POST 
    >=> request (getResourceFromReq >> processRequest >> toJson >> OK)
    >=> Writers.setMimeType "application/json; charset=utf-8"
