namespace IdentityManagement.UnitTests


open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain.RoleManagement
open IdentityManagement.Domain
open IdentityManagement.Domain.DAL.RoleGroupUserRelations

open IdentityManagement.Api
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
    let getDbConnection () = 
        let connection = new SqliteConnection("DataSource=:memory:")
        connection.Open()
        connection

    let getDbContextOptions (connection:Data.Common.DbConnection) = 
        let options = (new DbContextOptionsBuilder<IdentityManagementDbContext> ()).UseSqlite(connection).Options;
          
        use context = new IdentityManagementDbContext (options)
        context.Database.EnsureCreated() |> ignore
        options

    let createPersistenceLayer connection =
        let options = getDbContextOptions connection

        let mappingDal = RoleUserMappingDAL options 
        let mappingDalMethods = {
          RoleGroupUserRelationActor.removeGroupUsers = mappingDal.RemoveGroupUsersFromRole
          RoleGroupUserRelationActor.updateGroupUsers = mappingDal.AddGroupUsersToRole
          RoleGroupUserRelationActor.removeRoleGroupUser = mappingDal.RemoveRoleGroupUser
          RoleGroupUserRelationActor.addRoleGroupUser = mappingDal.AddRoleGroupUser
        }

        let persistence = {
            userManagementStore = InMemoryEventStore<UserManagementEvent> ()
            groupManagementStore = InMemoryEventStore<GroupManagementEvent> ()
            roleManagementStore = InMemoryEventStore<RoleManagementEvent> ()
            persistUserState = DAL.UserManagement.persist options
            persistGroupState = DAL.GroupManagement.persist options
            persistRoleState = DAL.RoleManagement.persist options
            persistRoleUserMappings = mappingDalMethods
            getRoles = fun groupId -> Seq.empty<Guid>
        }
        
        persistence
  