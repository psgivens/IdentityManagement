module IdentityManagement.UserCommands

open Suave
open Suave.Filters
open Suave.Operators

open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open IdentityManagement.Domain.UserManagement
open Common.FSharp.Envelopes

open IdentityManagement.ProcessingSystem

type Foo =
  { foo : string }

type Bar =
  { bar : string }







let handleUpdateUser userIdString =
  choose 
    [ PUT >=> OK "Recieved a PUT"
      DELETE >=> OK "Received a DELETE"
    ]

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

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

let handleUserPost =
  POST 
    >=> request (getResourceFromReq >> processRequest >> toJson >> OK)
    >=> Writers.setMimeType "application/json; charset=utf-8"
