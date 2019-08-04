[<AutoOpen>]
module IdentityManagement.Api.Composition

open System
open Akka.Actor
open Akka.FSharp

open IdentityManagement.Domain
open Common.FSharp.Envelopes
open IdentityManagement.Domain.DomainTypes
open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain
open Common.FSharp.Actors

open IdentityManagement.Domain.DAL.IdentityManagementEventStore
open Common.FSharp.Actors.Infrastructure

open IdentityManagement.Domain.DAL.Database
open Akka.Dispatch.SysMsg
open IdentityManagement.Domain.RoleManagement
open Common.FSharp

open Suave
open Common.FSharp.Suave

type ActorGroups = {
    UserManagementActors:ActorIO<UserManagementCommand>
    GroupManagementActors:ActorIO<GroupManagementCommand>
    RoleManagementActors:ActorIO<RoleManagementCommand>
    }

type Persistence = {
  userManagementStore: IEventStore<UserManagementEvent>
  groupManagementStore: IEventStore<GroupManagementEvent>
  roleManagementStore: IEventStore<RoleManagementEvent>
  persistUserState: UserId -> StreamId -> UserManagementState option -> unit
  persistGroupState: UserId -> StreamId -> GroupManagementState option -> unit
  persistRoleState: UserId -> StreamId -> RoleManagementState option -> unit
}

let composeActors system persistence =
    // Create member management actors
    let userManagementActors = 
        EventSourcingActors.spawn 
            (
             persistence.userManagementStore,
             persistence.persistUserState,
             "userManagement", 
             system,
             buildState UserManagement.evolve,
             // Dependency Injection would happen here by passing it to `handle`
             UserManagement.handle
             )

    let groupManagementActors = 
        EventSourcingActors.spawn
            (
             persistence.groupManagementStore,
             persistence.persistGroupState,
             "groupManagement",
             system,
             buildState GroupManagement.evolve,
             // Dependency Injection would happen here by passing it to `handle`
             GroupManagement.handle
             )

    let roleManagementActors =
        EventSourcingActors.spawn   
            (
             persistence.roleManagementStore,
             persistence.persistRoleState,
             "roleManagement",
             system,
             buildState RoleManagement.evolve,
             // Dependency Injection would happen here by passing it to `handle`
             RoleManagement.handle
            )
             
    { UserManagementActors=userManagementActors
      GroupManagementActors=groupManagementActors
      RoleManagementActors=roleManagementActors }



