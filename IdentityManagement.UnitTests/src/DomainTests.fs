module IdentityManagement.UnitTests.GroupDomain

open System
open Xunit
open Common.FSharp

open FSharp.Data



open EventSourceGherkin
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
module GroupManagementGherkin =
    open IdentityManagement.Domain.GroupManagement

    // TODO: Evaluate the following in relation to command testing
    // let raiseVersionedEvent (version:Version) event =
    //     incrementVersion version
    // //  raiseVersionedEvent mailbox'.Self cmdenv
    // let commandHandlers = CommandHandlers raiseVersionedEvent
    // let handle' = handle commandHandlers

    let buildState = List.fold evolve 

    // let evolve' s e = evolve s e 
    // let buildState state el =
    //     el
    //     |> List.fold evolve' state

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
