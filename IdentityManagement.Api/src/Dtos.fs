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
  users : GroupMemberDto []
  groups : GroupMemberDto [] }

type RoleMemberDto = {
  id : string
  name : string }

type RoleDto = { 
  ext_id : string
  id : string
  name : string
  members : RoleMemberDto [] }

let convertToDto (user:User) = { 
  UserDto.email = user.Email
  first_name = user.FirstName
  last_name = user.LastName
  id = user.Id.ToString () }
