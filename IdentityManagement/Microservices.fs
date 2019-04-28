module Common.FSharp.Suave

open Suave

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
      | _ -> never

    ctx |> webPart

let verifyheaders (protectedPart:WebPart<HttpContext>) = 
  verifyheadersAsync protectedPart