module Common.FSharp.Suave

open Suave
open Suave.RequestErrors

type UserContext = {
  UserId: string
  TransId: string
}

let inline private addContext (item:UserContext) ctx = 
  { ctx with userState = ctx.userState |> Map.add "user_context" (box item) }

let verifyheadersAsync<'a> (protectedPart:WebPart<HttpContext>) ctx =
    let p = ctx.request

    let h = 
      ["user_id"; "transaction_id"]
      |> List.map (p.header >> Option.ofChoice)

    let webPart = 
      match h with
      | [Some userId; Some transId] -> 
        addContext { UserId= userId; TransId=transId } 
        >> protectedPart
      | _ -> BAD_REQUEST "Perhaps you left off the headers"

    ctx |> webPart

let verifyheaders (protectedPart:WebPart<HttpContext>) = 
  verifyheadersAsync protectedPart

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

open Suave.Operators
open Suave.Successful

open Common.FSharp
let toJson v =
  let jsonSerializerSettings = JsonSerializerSettings()
  jsonSerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver ()

  JsonConvert.SerializeObject(v, jsonSerializerSettings)

let fromJson<'a> json =
  JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

let getResourceFromReq<'a> (req : HttpRequest) =
  let getString (rawForm:byte []) =
    System.Text.Encoding.UTF8.GetString rawForm
  req.rawForm |> getString |> fromJson<'a>
