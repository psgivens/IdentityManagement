module IdentityManagement.GroupCommands

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

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

let handleUpdateGroup category (groupIdString, userIdString) =
  match category with 
  | "users" -> 
    choose
      [ DELETE >=> OK "Should remove user form group"
        PUT >=> OK "Should add user to group"
      ]
  | "subgroups" -> 
    choose
      [ DELETE >=> OK "Should remove user form group"
        PUT >=> OK "Should add sbugroup to group"
      ]
  | _ -> never

let handleGroupPost =
  POST >=> OK "Should acknowledge that the group is being processed"

