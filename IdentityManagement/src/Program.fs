

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
          POST >=> pathCi "/users" >=> restful postUser
          PUT >=> pathScanCi "/users/%s" (restfulPathScan putUser)
          DELETE >=> pathScanCi "/users/%s" deleteUser

          // Group commands
          POST >=> choose 
            [
              pathCi "/groups" >=> restful postNewGroup
              pathScanCi "/groups/%s/users" (restfulPathScan addUserToGroup) 
              pathScanCi "/groups/%s/subgroups" (restfulPathScan addGroupToGroup)
            ]
          PUT >=> pathScanCi "/groups/%s" (restfulPathScan putGroup)
          DELETE >=> choose
            [ 
              pathScanCi "/groups/%s" deleteGroup
              pathScanCi "/groups/%s/users" (restfulPathScan removeUserFromGroup)
              pathScanCi "/groups/%s/subgroups" (restfulPathScan removeGroupFromGroup)
            ]

          // Role commands
          POST >=> choose 
            [
              pathCi "/roles" >=> restful postNewRole
              pathScanCi "/roles/%s/members" (restfulPathScan addPrincipalToRole)
            ]
          PUT >=> pathScanCi "/roles/%s" (restfulPathScan putRole)
          DELETE >=> choose
            [ 
              pathScanCi "/roles/%s" deleteRole
              pathScanCi "/roles/%s/users" (restfulPathScan removePrincipalFromRole)
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

