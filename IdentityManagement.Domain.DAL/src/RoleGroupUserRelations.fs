namespace IdentityManagement.Domain.DAL.RoleGroupUserRelations

open System
open IdentityManagement.Data.Models
open Microsoft.EntityFrameworkCore

type RoleUserMappingDAL (options:DbContextOptions<IdentityManagementDbContext>) = 
    let addMapping (context:IdentityManagementDbContext) roleId groupId userId = 
        context.RolePrincipalMaps.Add (
            RolePrincipalMap (
                RoleId=roleId,
                GroupId=Nullable groupId,
                PrincipalId=userId
            )
        ) |> ignore
    
    member this.GetRoles (groupId:Guid) :Guid list =
        use context = new IdentityManagementDbContext (options)
        let roles = query {
                for map in context.RolePrincipalMaps do
                where (map.GroupId = System.Nullable(groupId))
                select map.PrincipalId
            } 
        roles |> Seq.toList

    member this.RemoveGroupUsersFromRole (roleId:Guid) (groupId:Guid) :unit = 
        use context = new IdentityManagementDbContext (options)
        let rolePrincipalMaps = query {
                for rpm in context.RolePrincipalMaps do
                where (rpm.RoleId = roleId 
                    && rpm.GroupId = System.Nullable(groupId))
                select rpm
            } 
        context.RolePrincipalMaps.RemoveRange (rolePrincipalMaps |> Seq.toArray)
        |> ignore
        context.SaveChanges () |> ignore

    member this.AddGroupUsersToRole (roleId:Guid) (groupId:Guid) :unit = 
        use context = new IdentityManagementDbContext (options)

        // Remove old records to avoid unique key conflicts
        let rolePrincipalMaps = query {
                for rpm in context.RolePrincipalMaps do
                where (rpm.RoleId = roleId 
                    && rpm.GroupId = System.Nullable(groupId))
                select rpm
            } 
        context.RolePrincipalMaps.RemoveRange (rolePrincipalMaps |> Seq.toArray)

        let userIds = query {
            for gum in context.GroupPrincipalMaps do 
            where (gum.GroupId = groupId)
            select gum.PrincipalId
        }

        let addMapping' = addMapping context roleId groupId
        let ids = userIds |> Seq.toList
        ids |> Seq.iter addMapping'
        context.SaveChanges () |> ignore

    member this.RemoveRoleGroupUser (roleId:Guid) (groupId:Guid) (userId:Guid) :unit = 
        use context = new IdentityManagementDbContext (options)
        context.RemoveRange 
            (query {
                for rpm in context.RolePrincipalMaps do
                where (rpm.RoleId = roleId 
                    && rpm.GroupId = System.Nullable(groupId)
                    && rpm.PrincipalId = userId)
                select rpm
              }  |> Seq.toArray )
        |> ignore
        context.SaveChanges () |> ignore

    member this.AddRoleGroupUser (roleId:Guid) (groupId:Guid) (userId:Guid) :unit = 
        use context = new IdentityManagementDbContext (options)

        let addMapping' = addMapping context roleId groupId        
        addMapping' userId
        context.SaveChanges () |> ignore

