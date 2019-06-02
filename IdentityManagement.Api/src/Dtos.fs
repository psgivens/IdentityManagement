module IdentityManagement.Api.Dtos

open IdentityManagement.Data.Models

type UserDto = { 
    id : string
    first_name : string 
    last_name : string
    email : string }

type GroupMemberDto = {
  id : string
  name : string }

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

let convertToGroupDto (group:Group) = {
  GroupDto.id = group.Id.ToString ()
  name = group.Name
  groups = []
  users = []
}

let convertToRoleDto (role:Role) = {
  RoleDto.id = role.Id.ToString ()
  ext_id = role.ExternalId.ToString ()
  name = role.Name 
  members = []
}