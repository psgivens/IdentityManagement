module IdentityManagement.Domain.GroupManagement

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes
// open IdentityManagement.Domain.DomainTypes

type GroupManagementCommand =
    | Create of string
    | Delete
    | AddUser of UserId
    | AddGroup of Guid
    | RemoveUser of UserId
    | RemoveGroup of Guid
    | UpdateName of string

type GroupManagementEvent = 
    | Created of string
    | Deleted
    | UserAdded of UserId
    | GroupAdded of Guid
    | UserRemoved of UserId
    | GroupRemoved of Guid
    | NameUpdated of string

type GroupManagementState =
    { Name:string; Users: UserId list; Groups: Guid list; Deleted: bool }

let handle (command:CommandHandlers<GroupManagementEvent, Version>) (state:GroupManagementState option) (cmdenv:Envelope<GroupManagementCommand>) =    
    match state, cmdenv.Item with 
    | None, Create name -> Created name
    | _, Create _ -> failwith "Cannot create a group which already exists"
    | None, _ -> failwith "Group does not exist"
    | _, Delete -> Deleted
    | _, AddUser userId -> UserAdded userId
    | _, AddGroup groupId -> GroupAdded groupId
    | _, RemoveUser userId -> UserRemoved userId
    | _, RemoveGroup groupId -> GroupRemoved groupId
    | _, UpdateName name -> NameUpdated name
    |> command.event

let remove item list =
    let notMatch x = item <> x
    list |> List.filter notMatch

let evolve (state:GroupManagementState option) (event:GroupManagementEvent) =
    match state, event with 
    | None, Created name -> { Name=name; Users = []; Groups = []; Deleted=false}
    | _, Created _ -> failwith "Cannot create a group which already exists"
    | None, _ -> failwith "Group does not exist"
    | _, Deleted -> { Name=""; Users=[]; Groups=[]; Deleted=true }
    | Some st, UserAdded userId -> { st with Users = userId :: st.Users }
    | Some st, GroupAdded groupId -> { st with Groups = groupId :: st.Groups }
    | Some st, UserRemoved userId -> { st with Users = st.Users |> remove userId }
    | Some st, GroupRemoved groupId -> { st with Groups = st.Groups |> remove groupId }
    | Some st, NameUpdated name -> { st with Name = name }


