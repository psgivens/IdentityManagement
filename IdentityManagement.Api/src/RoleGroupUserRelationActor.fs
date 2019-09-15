[<RequireQualifiedAccess>]
module IdentityManagement.Api.RoleGroupUserRelationActor

open System

open Akka.Actor
open Akka.FSharp

open Common.FSharp.Envelopes
open Common.FSharp.CommandHandlers
open Common.FSharp.Actors
open Common.FSharp.Actors.Infrastructure

open IdentityManagement.Domain.RoleManagement
open IdentityManagement.Domain.GroupManagement

module Commands = 
    type RoleGroupUserCommands = 
        | RemoveRoleGroupMappings
        | UpdateRoleGroupMappings
        | RemoveGroupUserMappings of UserId
        | AddGroupUserMappings of UserId

    type RoleGroupUserEnvelope<'a> = {
        roleId: Guid
        groupId: Guid
        command: RoleGroupUserCommands
        trigger: Envelope<'a>
    }

// Type to inject which will handle the database updating
type RoleGroupUserRelationDal = {
    removeGroupUsers:    Guid -> Guid -> unit
    updateGroupUsers:    Guid -> Guid -> unit
    removeRoleGroupUser: Guid -> Guid -> Guid -> unit
    addRoleGroupUser:    Guid -> Guid -> Guid -> unit
    getRoles:            Guid -> Guid list
}

let spawn
   (    dal:RoleGroupUserRelationDal,
        name,
        sys        
        ) :IActorRef = 

    let aggregateActor (mailbox:Actor<obj>) = 
        let rec loop () = actor {
            let! msg = mailbox.Receive ()

            match msg with 
            | :? Commands.RoleGroupUserEnvelope<RoleManagementEvent> as env -> 
                match env.command with
                | Commands.RoleGroupUserCommands.UpdateRoleGroupMappings ->
                    dal.updateGroupUsers env.roleId env.groupId

                | Commands.RoleGroupUserCommands.RemoveRoleGroupMappings ->                    
                    dal.removeGroupUsers env.roleId env.groupId
                    
                | _ -> ()

            | :? Commands.RoleGroupUserEnvelope<GroupManagementEvent> as env -> 
                match env.command with
                | Commands.RoleGroupUserCommands.AddGroupUserMappings userId -> 
                    dal.addRoleGroupUser env.roleId env.groupId (UserId.unbox userId)

                | Commands.RoleGroupUserCommands.RemoveGroupUserMappings userId -> 
                    dal.removeRoleGroupUser env.roleId env.groupId (UserId.unbox userId)

                | _ -> ()

            | _ -> ()
            return! loop ()
        }
        loop ()

    spawnOpt sys (name + "_RoleGroupUserActorActor") 
    <| aggregateActor

    <| [Akka.Routing.ConsistentHashingPool (10, fun msg -> 
            match msg with
            | :? Commands.RoleGroupUserEnvelope<RoleManagementEvent> as env -> 
                (env.roleId, env.groupId) :> obj
            | :? Commands.RoleGroupUserEnvelope<GroupManagementEvent> as env -> 
                (env.roleId, env.groupId) :> obj
            | _ -> msg )
        :> Akka.Routing.RouterConfig
        |> SpawnOption.Router]



// 
// ***** TODO: Replace hashing pool with this group ****** 

        // <| [Akka.Routing.ConsistentHashingGroup ([
        //         "sample"
        //    ])
        //    :> Akka.Routing.RouterConfig
        //    |> SpawnOption.Router ]
