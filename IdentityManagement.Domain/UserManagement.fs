module IdentityManagement.Domain.UserManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open IdentityManagement.Domain.DomainTypes


type UserDetails = { 
    FirstName:string 
    LastName:string 
    Email:string
    }

type UserManagementCommand =
    | Create of UserDetails
    | Activate 
    | Deactivate 
    | Update of UserDetails

type UserManagementEvent = 
    | Created of UserDetails
    | Activated
    | Deactivated
    | Updated of UserDetails

type UserManagementStateValue =
    | Active
    | Inactive

type UserManagementState =
    { State:UserManagementStateValue; Details:UserDetails}

type UserHistoryState = {
    SpeechCount:int
    LastToastmaster:System.DateTime
    LastTableTopicsMaster:System.DateTime
    LastGeneralEvaluator:System.DateTime
    LastSpeechGiven:System.DateTime
    LastEvaluationGiven:System.DateTime
    LastAssignment:DateTime
    LastMinorRole:DateTime
    LastMajorRole:DateTime
    LastFunctionaryRole:DateTime
    LastFacilitatorRole:DateTime
    WillAttend:bool
    SpecialRequest:string
    EligibilityCount:int
    }

type UserHistoryConfirmation = {
    SpeechCount:int
    ConfirmationDate:System.DateTime
    DisplayName:string
    }        

let (|HasStateValue|_|) expected state =
    match state with 
    | Some(value) when value.State = expected -> Some value
    | _ -> None 

let handle (command:CommandHandlers<UserManagementEvent, Version>) (state:UserManagementState option) (cmdenv:Envelope<UserManagementCommand>) =    
    match state, cmdenv.Item with 
    | None, Create user -> Created user
    | _, Create _ -> failwith "Cannot create a user which already exists"
    | HasStateValue UserManagementStateValue.Inactive _, UserManagementCommand.Activate -> UserManagementEvent.Activated
    | _, UserManagementCommand.Activate -> failwith "User must exist and be inactive to activate"
    | HasStateValue UserManagementStateValue.Active _, UserManagementCommand.Deactivate -> UserManagementEvent.Deactivated
    | _, UserManagementCommand.Deactivate -> failwith "User must exist and be active to deactivate"
    | Some _, UserManagementCommand.Update details -> UserManagementEvent.Updated details
    | None, UserManagementCommand.Update _ -> failwith "Cannot update a user which does not exist"             
    |> command.event

let evolve (state:UserManagementState option) (event:UserManagementEvent) =
    match state, event with 
    | None, UserManagementEvent.Created user -> { State=Active; Details=user }
    | HasStateValue UserManagementStateValue.Inactive st, UserManagementEvent.Activated -> { st with State=Active }
    | HasStateValue UserManagementStateValue.Active st, UserManagementEvent.Deactivated -> { st with State=Inactive }
    | Some st, UserManagementEvent.Updated details -> { st with Details=details }
    | _, Created _ -> failwith "Cannot create a user which already exists"
    | _, UserManagementEvent.Activated -> failwith "User must exist and be inactive to activate"
    | _, UserManagementEvent.Deactivated -> failwith "User must exist and be active to deactivate"    
    | None, UserManagementEvent.Updated _ -> failwith "Cannot update a user which does not exist"

