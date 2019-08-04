module IdentityManagement.Domain.RoleManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open IdentityManagement.Domain.DomainTypes

type RoleManagementCommand =
    | Create of (string * Guid)
    | Delete
    | AddPrincipal of Guid
    | RemovePrincipal of Guid
    | UpdateName of string

type RoleManagementEvent = 
    | Created of (string * Guid)
    | Deleted
    | PrincipalAdded of Guid
    | PrincipalRemoved of Guid
    | NameUpdated of string

type RoleManagementState =
    { Name:string; ExternalId:Guid; Principals: Guid list; Deleted: bool }

let handle (command:CommandHandlers<RoleManagementEvent, Version>) (state:RoleManagementState option) (cmdenv:Envelope<RoleManagementCommand>) =    
    match state, cmdenv.Item with 
    | None, Create roleInfo -> Created roleInfo
    | _, Create _ -> failwith "Cannot create a role which already exists"
    | None, _ -> failwith "Role does not exist"
    | _, Delete -> Deleted
    | _, AddPrincipal id -> PrincipalAdded id
    | _, RemovePrincipal id -> PrincipalRemoved id
    | _, UpdateName name -> NameUpdated name
    |> command.event

let evolve (state:RoleManagementState option) (event:RoleManagementEvent) =
    match state, event with 
    | None, Created (name, id) -> { Name=name; ExternalId=id; Principals = []; Deleted=false }
    | _, Created _ -> failwith "Cannot create a group which already exists"
    | None, _ -> failwith "Group does not exist"
    | Some st, Deleted -> { st with Name=""; Principals = []; Deleted=true }
    | Some st, PrincipalAdded id -> { st with Principals = id :: st.Principals }
    | Some st, PrincipalRemoved id -> { st with Principals = st.Principals |> List.filter ((<>) id) }
    | Some st, NameUpdated name -> { st with Name = name }
    |> Some

