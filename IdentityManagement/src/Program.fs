

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors

// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open Common.FSharp.Suave

open IdentityManagement.ProcessingSystem
open IdentityManagement.UserCommands
open IdentityManagement.GroupCommands
open IdentityManagement.RoleCommands
open IdentityManagement.RestQuery

let app =
  choose 
    [ request authenticationHeaders >=> choose
        [ 
          // All requests are handled together because CQRS
          GET >=> choose
            [ pathCi "/" >=> OK "Default route"
              pathCi "/users" >=> (getUsers |> Suave.Http.context) 
              pathScanCi "/users/%s" getUser
            ]            

          // User commands
          POST >=> pathCi "/users" >=> restWebPart postUser
          PUT >=> pathScanCi "/users/%s" putUser
          DELETE >=> pathScanCi "/users/%s" deleteUser

          // Group commands
          POST >=> choose 
            [
              pathCi "/groups" >=> restWebPart postNewGroup
              pathScanCi "/groups/%s/users" addUserToGroup 
              pathScanCi "/groups/%s/subgroups" addGroupToGroup
            ]
          PUT >=> pathScanCi "/groups/%s" putGroup
          DELETE >=> choose
            [ 
              pathScanCi "/groups/%s" deleteGroup
              pathScanCi "/groups/%s/users" removeUserFromGroup
              pathScanCi "/groups/%s/subgroups" removeGroupFromGroup
            ]

          // Role commands
          POST >=> choose 
            [
              pathCi "/roles" >=> postNewRole
              pathScanCi "/roles/%s/members" addPrincipalToRole 
            ]
          PUT >=> pathScanCi "/roles/%s" putRole
          DELETE >=> choose
            [ 
              pathScanCi "/roles/%s" deleteRole
              pathScanCi "/roles/%s/users" removePrincipalFromRole
            ]

          // Role commands
          BAD_REQUEST "Request path was not found"
        ]
      Suave.RequestErrors.UNAUTHORIZED "Request is missing authentication headers"    
    ]

let defaultArgument x y = defaultArg y x


[<EntryPoint>]
let main argv =
    printfn "main argv"

    let config = { defaultConfig with  bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8080 ]}

    startWebServer config app
    0

