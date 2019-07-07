namespace IdentityManagement.UnitTests

open System
open Xunit
open Common.FSharp
open Newtonsoft.Json

open FSharp.Data

type UserDto = { 
    id : string 
    first_name : string 
    last_name : string
    email : string }

type NewUserDto = {
    first_name : string 
    last_name : string
    email : string }

module Utils = 
    let domainString = System.Environment.GetEnvironmentVariable ("targetDomainForTests")
    let domain =
        if String.IsNullOrWhiteSpace (domainString) 
        then "http://blairstone:32080"
        else domainString

    let toJson v =
      let jsonSerializerSettings = JsonSerializerSettings()
      jsonSerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver ()
      JsonConvert.SerializeObject(v, jsonSerializerSettings)

    let contextHeaders = [
            "user_id", "638d111c-c7bb-4710-8ea2-91c4bc3ea530";
            "transaction_id","638d111c-c7bb-4710-8ea2-91c4bc3ea530"
        ]

    let uri u = String.Join('/', domain, u)

type UsersTests ()  =

    [<Fact>]
    member this.``Add users`` () =
        let pingResponse = 
          Http.RequestString (
             Utils.uri "ping",
             httpMethod="GET")
        Assert.Equal("unauthenticated ping", pingResponse)

        let usersResponse = 
          Http.RequestString (
             Utils.uri "users",
             headers = Utils.contextHeaders,
             httpMethod="GET")             
        let users = JsonConvert.DeserializeObject(usersResponse, typeof<UserDto list>) :?> UserDto list
        Assert.Equal ((users |> Seq.head |> fun x -> x.first_name), "Phillip")

        let newUser = {
            first_name = "Bob" 
            last_name = "Hope"
            email = "bob@hope.com" } 

        let newUserBody = newUser |> Utils.toJson |> TextRequest

        let retCode = 
          Http.RequestString (
            Utils.uri "users",
            headers = Utils.contextHeaders,
            httpMethod="POST",
            body=newUserBody)

        // TODO: Connect to database and verify

        Http.RequestString("http://tomasp.net") |> fun x -> printfn "%d" x.Length

        // Download web site asynchronously
        async { let! html = Http.AsyncRequestString("http://tomasp.net")
            printfn "%d" html.Length }
        |> Async.Start
        Assert.True(true)

    [<Fact>]
    member this.``My second test`` () =
        Http.RequestString("http://tomasp.net") |> fun x -> printfn "%d" x.Length

        // Download web site asynchronously
        async { let! html = Http.AsyncRequestString("http://tomasp.net")
            printfn "%d" html.Length }
        |> Async.Start
        Assert.True(true)
