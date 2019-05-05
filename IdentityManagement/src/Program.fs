

open Suave
open Suave.Filters
open Suave.Operators

open Suave.Successful
open Suave.RequestErrors
open Suave.Utils.Collections

open Common.FSharp.Suave
open Common.FSharp.Envelopes

open IdentityManagement.ProcessingSystem
open IdentityManagement.Domain.UserManagement



// http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open IdentityManagement.UserCommands
open IdentityManagement.GroupCommands

let app =
  verifyheaders 
    <| choose
        [ GET >=> pathCi "/" >=> OK "Hello Dave"
          GET >=> choose
            [ pathCi "/users" >=> OK "Should return users" 
            ]
          pathCi "/users" >=> handleUserPost
          pathScanCi "/users/%s" handleUpdateUser
          pathCi "/groups" >=> handleGroupPost
          pathScanCi "/groups/%s/users/%s" <| handleUpdateGroup "users" 
          pathScanCi "/groups/%s/subgroups/%s" <| handleUpdateGroup "subgroups"
          BAD_REQUEST "Perhaps you left off the headers"
        ]

let greetings q =
  defaultArg (Option.ofChoice (q ^^ "name")) "World" |> sprintf "Hello %s"

let defaultArgument x y = defaultArg y x

let readUserState ctx key : 'value =
  ctx.userState |> Map.tryFind key |> Option.map (fun x -> x :?> 'value) |> Option.get

let addUserMessage (message : string) : WebPart =
  context (fun ctx ->
    let read = readUserState ctx
    let existing =
      match ctx.userState |> Map.tryFind "context" with
      | Some _ ->
          read "context"
      | _ ->
          []
    Writers.setUserData "context" (message :: existing))


[<EntryPoint>]
let main argv =
    printfn "main argv"

    let config = { defaultConfig with  bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" 8080 ]}

    // System.Threading.Thread.Sleep (60 * 60 * 1000)
    startWebServer config app
    0

// let createUser firstName =
//   use context = new IdentityManagementDbContext () 
//   context.Users.Add(
//     User(
//       FirstName = firstName,
//       LastName = "sampleUser"))
//   |> ignore
//   context.SaveChanges () |> ignore

// let handleUsers = 
//   choose
//     [ GET >=> OK "Should return users" 
//       POST >=> OK "Expecting a new user posted"
//     ]

// let handleUser userId = 
//   choose 
//     [ GET >=> OK (sprintf "Sending %s" userId)
//       POST >=> OK (sprintf "Received %s" userId)
//     ]

    // (choose
    //   [ GET >=> choose
    //       [ pathCi "/users" >=> handleUsers
    //         pathScanCi "/user/%s" handleUser

    //         path "/" >=> OK "Index"
    //         path "/hello" >=> OK "Hello!" 
    //         pathRegex "/h.*llo" >=> OK "Heeeeeeello!" 
    //         pathScanCi "/body/%d" (fun num -> 
    //           printf "%d" num
    //           OK (sprintf "Heeeeeeello %d!" num) )

    //         pathCi "/echo" >=> request (fun r -> 
    //           match r.query ^^ "name" with
    //           | Choice1Of2 name -> 
    //             name
    //             |> sprintf "Hello %s!" 
    //             |> OK
    //           | Choice2Of2 error ->
    //             error |> OK)
    //       ]

    //     POST >=> choose
    //       [ path "/hello" >=> choose
    //           [ fun ctx -> None |> async.Return

    //             request (fun r ->
    //               match r.header "foo" 
    //                 |> Option.ofChoice 
    //                 with
    //                 | Some (foo) -> 
    //                     OK (sprintf "foo: %s" foo)
    //                 | None -> never)

    //             OK "Hello POST!" ] ] ])

