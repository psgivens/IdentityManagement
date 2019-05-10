module IdentityManagement.RoleCommands

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

let handleUpdateRole (roleIdString, userIdString) =
  choose
    [ DELETE >=> OK "Should remove principal form role"
      PUT >=> OK "Should add principal to role"
    ]

let handleRolePost =
  POST >=> OK "Should acknowledge that the role is being processed"

