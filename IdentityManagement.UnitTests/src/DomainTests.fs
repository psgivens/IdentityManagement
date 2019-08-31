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

    let buildState = List.fold evolve 

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
    |> Some

let evolve evts = 
    let evolve' = IdentityManagement.Domain.GroupManagement.evolve
    evts |> Seq.fold evolve' initialState

open GroupManagementGherkin
type DomainTests ()  =

    [<Fact>]
    member this.``My test`` () =

        Given (State None)
        |> When ([GroupManagementEvent.Created "sampleGroup"] |> Events)
        |> Then (expectState (
                    { Name="sampleGroup"
                      Users=[]
                      Groups=[] 
                      Deleted=false }
                    |> Some))

    [<Fact>]
    member this.``My second test`` () =
        Given (State None)
        |> When ([GroupManagementEvent.Created "sampleGroup"] |> Events)
        |> Then (expectState (
                    { Name="sampleGroup"
                      Users=[]
                      Groups=[] 
                      Deleted=false }
                    |> Some))
