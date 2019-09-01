namespace IdentityManagement.UnitTests
// 

open System
open Xunit
open Common.FSharp

open FSharp.Data

open EventSourceGherkin
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

open IdentityManagement.Domain.GroupManagement

// module GroupDomain =
//   let initialState =
//       { GroupManagementState.Name="sample"
//         GroupManagementState.Users=[]
//         GroupManagementState.Groups=[]
//         GroupManagementState.Deleted=false }
//       |> Some

// type GroupManagementCommand =
//     | Create of string
//     | Delete
//     | AddUser of UserId
//     | AddGroup of Guid
//     | RemoveUser of UserId
//     | RemoveGroup of Guid
//     | UpdateName of string

// type GroupManagementEvent = 
//     | Created of string
//     | Deleted
//     | UserAdded of UserId
//     | GroupAdded of Guid
//     | UserRemoved of UserId
//     | GroupRemoved of Guid
//     | NameUpdated of string

// type GroupManagementState =
//     { Name:string; Users: UserId list; Groups: Guid list; Deleted: bool }


type DomainTests ()  =
    
    [<Fact>]
    member this.``Create a group`` () =
        GroupGherkin.Given (State None)
        |> GroupGherkin.When ([GroupManagementEvent.Created "sampleGroup"] |> Events)
        |> GroupGherkin.Then (
          expectState (
            { Name="sampleGroup"
              Users=[]
              Groups=[] 
              Deleted=false }
            |> Some))

    [<Fact>]
    member this.``My second test`` () =
        GroupGherkin.Given (State None)
        |> GroupGherkin.When ([GroupManagementEvent.Created "sampleGroup"] |> Events)
        |> GroupGherkin.Then (
          expectState (
            { Name="sampleGroup"
              Users=[]
              Groups=[] 
              Deleted=false }
            |> Some))
