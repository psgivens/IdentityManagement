module Common.FSharp.Suave

open Suave
open Suave.RequestErrors


let inline noMatch (ctx:HttpContext) = async.Return Option<HttpContext>.None


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

let getDtoFromReq<'a> (req : HttpRequest) =
  let getString (rawForm:byte []) =
    System.Text.Encoding.UTF8.GetString rawForm
  req.rawForm |> getString |> fromJson<'a>

let restWebPart processRequest = 
  Writers.setMimeType "application/json; charset=utf-8"
  >=> request (getDtoFromReq >> processRequest >> toJson >> OK)
