module IdentityManagement.RestQuery

open Suave
open Suave.Filters
open Suave.Operators

open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open IdentityManagement.Domain.UserManagement
open Common.FSharp.Envelopes

let myquery userIdString =
  choose 
    [ PUT >=> OK "Recieved a PUT"
      DELETE >=> OK "Received a DELETE"
    ]
