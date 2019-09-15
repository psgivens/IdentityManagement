[<RequireQualifiedAccess>]
module IdentityManagement.Api.TranslationActor

open System

open Akka.Actor
open Akka.FSharp

open Common.FSharp.Envelopes
open Common.FSharp.CommandHandlers
open Common.FSharp.Actors
open Common.FSharp.Actors.Infrastructure

open IdentityManagement.Domain.RoleManagement
open IdentityManagement.Domain.GroupManagement
open  IdentityManagement.Api.RoleGroupUserRelationActor.Commands


let spawn
    (   roleGroupUserUpdater:IActorRef,
        getRoles: Guid -> Guid list,
        name,
        sys
    ) :IActorRef = 

    let roleGroupUserActor (mailbox:Actor<obj>) = 
        let rec loop () = actor {
            let! msg = mailbox.Receive ()

            match msg with 
            | :? Envelope<RoleManagementEvent> as cmdenv -> 
                match cmdenv.Item with 
                | PrincipalAdded gid -> 
                    roleGroupUserUpdater 
                        <! { roleId=  StreamId.unbox cmdenv.StreamId
                             groupId= gid 
                             command= RoleGroupUserCommands.UpdateRoleGroupMappings
                             trigger= cmdenv
                           }
                | PrincipalRemoved gid ->
                    roleGroupUserUpdater 
                        <! { roleId=  StreamId.unbox cmdenv.StreamId
                             groupId= gid 
                             command= RoleGroupUserCommands.RemoveRoleGroupMappings
                             trigger= cmdenv
                           }
                | _ -> ()

            | :? Envelope<GroupManagementEvent> as cmdenv -> 
                let gid = StreamId.unbox cmdenv.StreamId
                let roleIds = getRoles gid
                match cmdenv.Item with
                | UserAdded userId -> 
                    roleIds
                    |> Seq.iter ( fun rid -> 
                        roleGroupUserUpdater 
                            <! { roleId=  rid
                                 groupId= gid
                                 command= RoleGroupUserCommands.RemoveGroupUserMappings userId
                                 trigger= cmdenv
                               })
                | UserRemoved userId -> 
                    roleIds
                    |> Seq.iter ( fun rid -> 
                        roleGroupUserUpdater 
                            <! { roleId=  rid
                                 groupId= gid
                                 command= RoleGroupUserCommands.AddGroupUserMappings userId
                                 trigger= cmdenv
                               })
                | _ -> ()

            | _ -> ()
            return! loop ()
        }
        loop ()

    spawn sys (name + "_RoleMappingActor") roleGroupUserActor


