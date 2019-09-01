namespace IdentityManagement.UnitTests
// 

open System
open Xunit
open Common.FSharp

open FSharp.Data

open EventSourceGherkin
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

module UserGherkin =
    open IdentityManagement.Domain.UserManagement

    let buildState = List.fold evolve 

    let testing = 
        TestConditions<UserManagementCommand, UserManagementEvent, UserManagementState> 
            (buildState None, buildState)
  
    let Given = testing.Given
    let When  = testing.When
    let Then  = testing.Then

module GroupGherkin =
    open IdentityManagement.Domain.GroupManagement

    let buildState = List.fold evolve 

    let testing = 
        TestConditions<GroupManagementCommand, GroupManagementEvent, GroupManagementState> 
            (buildState None, buildState)
  
    let Given = testing.Given
    let When  = testing.When
    let Then  = testing.Then

module RoleGherkin =
    open IdentityManagement.Domain.RoleManagement

    let buildState = List.fold evolve 

    let testing = 
        TestConditions<RoleManagementCommand, RoleManagementEvent, RoleManagementState> 
            (buildState None, buildState)
  
    let Given = testing.Given
    let When  = testing.When
    let Then  = testing.Then
