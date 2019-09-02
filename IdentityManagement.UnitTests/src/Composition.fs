namespace IdentityManagement.UnitTests


open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain.RoleManagement
open IdentityManagement.Domain

open IdentityManagement.Api.Composition
open System
open Xunit
open Common.FSharp
open Common.FSharp.Actors
open Common.FSharp.Envelopes
open Common.FSharp.EventSourceGherkin

open Akka.Actor
open Akka.FSharp

open IdentityManagement.Data.Models
open Microsoft.Data.Sqlite
open Microsoft.EntityFrameworkCore

module Composition =
    let getDbContextOptions () = 
        let connection = new SqliteConnection("DataSource=:memory:")
        connection.Open()
        let options = (new DbContextOptionsBuilder<IdentityManagementDbContext> ()).UseSqlite(connection).Options;
  
        use context = new IdentityManagementDbContext (options)
        context.Database.EnsureCreated() |> ignore
        options

    let createPersistenceLayer () =
        let options = getDbContextOptions ()
        let persistence = {
            userManagementStore = InMemoryEventStore<UserManagementEvent> ()
            groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
            roleManagementStore = InMemoryEventStore<RoleManagementEvent> ()
            persistUserState = DAL.UserManagement.persist options
            persistGroupState = DAL.GroupManagement.persist options
            persistRoleState = DAL.RoleManagement.persist options
        }
        
        persistence
  