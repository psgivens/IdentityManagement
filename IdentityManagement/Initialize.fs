module IdentityManagement.Initialize

open System
open Akka.Actor
open Akka.FSharp

open IdentityManagement.Domain
open Common.FSharp.Envelopes
open IdentityManagement.Domain.DomainTypes
open IdentityManagement.Domain.UserManagement
// open IdentityManagement.Domain.RoleRequests
// open IdentityManagement.Domain.MemberMessages
// open IdentityManagement.Domain.RolePlacements 
// open IdentityManagement.Domain.ClubMeetings
open Common.FSharp.Actors

open IdentityManagement.Domain.DAL.IdentityManagementEventStore
open Common.FSharp.Actors.Infrastructure

type ActorGroups = {
    UserManagementActors:ActorIO<UserManagementCommand>
    // MemberManagementActors:ActorIO<MemberManagementCommand>
    // MessageActors:ActorIO<MemberMessageCommand>
    // DayOffActors:ActorIO<DayOffRequestCommand>
    // RoleRequestActors:ActorIO<RoleRequestCommand>
    // RolePlacementActors:ActorIO<RolePlacementCommand>
    // ClubMeetingActors:ActorIO<ClubMeetingCommand>
    // MemberHistoryActors:ActorIO<MemberHistoryConfirmation>
    }


let composeActors system =
    // Create member management actors
    let userManagementActors = 
        EventSourcingActors.spawn 
            (system,
             "userManagement", 
             UserManagementEventStore (),
             buildState UserManagement.evolve,
             UserManagement.handle,
             DAL.UserManagement.persist)    

    // let messageActors = 
    //     CrudMessagePersistanceActor.spawn<MemberMessageCommand>
    //         (system, 
    //          "userMessage", 
    //          Persistence.MemberMessages.persist)

    // let dayOffActors =
    //     CrudMessagePersistanceActor.spawn<DayOffRequestCommand>
    //         (system,
    //          "dayOffRequest",
    //          Persistence.MemberDayOffRequests.persist)






    // let placementRequestReplyCreate = 
    //     RequestReplyActor.spawnRequestReplyConditionalActor<RolePlacementCommand,RolePlacementEvent> 
    //         (fun cmd -> true)
    //         (fun evt -> 
    //             match evt.Item with
    //             | RolePlacementEvent.Opened _ -> true
    //             | _ -> false)
    //         system "rolePlacement_create" rolePlacementActors

    // let placementRequestReplyCancel = 
    //     RequestReplyActor.spawnRequestReplyConditionalActor<RolePlacementCommand,RolePlacementEvent> 
    //         (fun cmd -> true)
    //         (fun evt -> 
    //             match evt.Item with
    //             | RolePlacementEvent.Canceled _ -> true
    //             | _ -> false)
    //         system "rolePlacement_cancel" rolePlacementActors

        
             
    { UserManagementActors=userManagementActors
    }

open IdentityManagement.Domain.DAL.Database

let initialize () = 
    // System set up
    NewtonsoftHack.resolveNewtonsoft ()  
    initializeDatabase ()
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    let actorGroups = composeActors system
    actorGroups