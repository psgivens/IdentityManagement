module IdentityManagement.Api.Dtos

open IdentityManagement.Data.Models

type UserDto = { 
    id : string 
    first_name : string 
    last_name : string
    email : string }

type NewUserDto = {
    first_name : string 
    last_name : string
    email : string }

type GroupMemberDto = {
  id : string
  name : string }

type NewGroupDto = {
  name : string
}

type GroupDto = { 
  id : string
  name : string
  users : GroupMemberDto list
  groups : GroupMemberDto list }

type RoleMemberDto = {
  id : string
  name : string }

type RoleDto = { 
  ext_id : string
  id : string
  name : string
  members : RoleMemberDto list }

let convertToUserDto (user:User) = { 
  UserDto.email = user.Email
  first_name = user.FirstName
  last_name = user.LastName
  id = user.Id.ToString () }

let convertToGroupDto (group:Group) = 

  let folder (users,groups) (m:GroupPrincipalMap) = 
    match m.Principal with
    | :? User as u -> 
      (({ 
        GroupMemberDto.id=u.Id.ToString ()
        name=sprintf "%s %s" u.FirstName u.LastName
        })::users,groups)
    | :? Group as g -> (users,{
        GroupMemberDto.id=g.Id.ToString ()
        name=g.Name
        }::groups)
    | _ -> failwith "Derived type not supported"
    
  let (users,groups) = 
    group.MemberRelations
    |> Seq.fold folder ([],[])

  {
    GroupDto.id = group.Id.ToString ()
    name = group.Name
    groups = groups
    users = users
  }

let convertToRoleDto (role:Role) = {
  RoleDto.id = role.Id.ToString ()
  ext_id = role.ExternalId.ToString ()
  name = role.Name 
  members = []
}

