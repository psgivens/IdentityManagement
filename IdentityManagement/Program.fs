

open Suave
open Suave.Filters
open Suave.Operators

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Utils.Collections
open IdentityManagement.Models

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

let createUser firstName =
  use context = new IdentityManagementDbContext () 
  context.Users.Add(
    User(
      FirstName = firstName,
      LastName = "sampleUser"))
  |> ignore
  context.SaveChanges () |> ignore



let inline private addContext ctxName username ctx = 
  { ctx with userState = ctx.userState |> Map.add ctxName (box username) }

let verifyheadersAsync<'a> (protectedPart:WebPart<HttpContext>) ctx =
  let p = ctx.request

  let h = 
    ["user_id"; "transaction_id"]
    |> List.map (p.header >> Option.ofChoice)

  match h with
  | [Some userId; Some transId] -> 
    addContext "user_id" userId
    >> addContext "trans_id" transId
    >> protectedPart
  | _ -> never

let verifyheaders (protectedPart:WebPart<HttpContext>) ctx = 
  verifyheadersAsync protectedPart ctx

let app =
   verifyheaders 
    (choose
      [ GET >=> choose
          [ path "/" >=> OK "Index"
            path "/hello" >=> OK "Hello!" 
            pathRegex "/h.*llo" >=> OK "Heeeeeeello!" 
            pathScanCi "/body/%d" (fun num -> 
              printf "%d" num
              OK (sprintf "Heeeeeeello %d!" num) )

            pathCi "/echo" >=> request (fun r -> 
              match r.query ^^ "name" with
              | Choice1Of2 name -> 
                name
                |> sprintf "Hello %s!" 
                |> OK
              | Choice2Of2 error ->
                error |> OK)
          ]

        POST >=> choose
          [ path "/hello" >=> choose
              [ fun ctx -> None |> async.Return

                request (fun r ->
                  match r.header "foo" 
                    |> Option.ofChoice 
                    with
                    | Some (foo) -> 
                        OK (sprintf "foo: %s" foo)
                    | None -> never
                    | _ -> never)

                OK "Hello POST!" ] ] ])

[<EntryPoint>]
let main argv =
    startWebServer defaultConfig app
    0
