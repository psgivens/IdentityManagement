module IdentityManagement.UnitTests.GroupDomain

open System
open Xunit
open Common.FSharp

open FSharp.Data


open EventSourceGherkin
module GroupManagementGherkin =
    open IdentityManagement.Domain.GroupManagement
    let evolve' s e = evolve s e |> Some 
    let buildState state el =
        el
        |> List.fold evolve' state

    let testing = 
        TestConditions<GroupManagementCommand, GroupManagementEvent, GroupManagementState> 
            (buildState None, buildState)
  
    let Given = testing.Given
    let When  = testing.When
    let Then  = testing.Then









open IdentityManagement.Domain.GroupManagement

let initialState =
    { GroupManagementState.Name="sample"
      GroupManagementState.Users=[]
      GroupManagementState.Groups=[]
      GroupManagementState.Deleted=false }

let evolve evts = 
    // let folder = 
    // evts |> IdentityManagement.Domain.GroupManagement.ev

    let folder state item =
        state

    evts |> Seq.fold folder initialState

// let openTests = TestConditions<

type DomainTests ()  =

    [<Fact>]
    member this.``My test`` () =
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
