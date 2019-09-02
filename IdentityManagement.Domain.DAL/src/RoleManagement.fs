module IdentityManagement.Domain.DAL.RoleManagement

open IdentityManagement.Data.Models
open Common.FSharp.Envelopes
open IdentityManagement.Domain.DomainTypes
open IdentityManagement.Domain.RoleManagement

open Microsoft.EntityFrameworkCore

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (options:DbContextOptions<IdentityManagementDbContext>) (userId:UserId) (streamId:StreamId) (state:RoleManagementState option) =
    use context = new IdentityManagementDbContext (options)
    let entity = context.Roles.Find (StreamId.unbox streamId)
    let addMember (r:Role) id = 
        r.Members.Add(
            RolePrincipalMap(
                PrincipalId=id
            ))

    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let role = 
            Role (
                Id = StreamId.unbox streamId,
                Name = item.Name,
                ExternalId = item.ExternalId
            )
        item.Principals |> List.iter (addMember role)
        context.Roles.Add role |> ignore
        printfn "Persist mh: (%s)" item.Name
    | _, Option.None -> context.Roles.Remove entity |> ignore        
    | _, Some(item) -> 
        entity.Name <- item.Name
        item.Principals |> List.iter (addMember entity)
    context.SaveChanges () |> ignore
    
let execQuery (q:IdentityManagementDbContext -> System.Linq.IQueryable<'a>) =
    use context = new IdentityManagementDbContext () 
    q context
    |> Seq.toList

let find (userId:UserId) (streamId:StreamId) =
    use context = new IdentityManagementDbContext () 
    context.Users.Find (StreamId.unbox streamId)

let findRoleByName name =
    use context = new IdentityManagementDbContext () 
    query { for role in context.Roles do            
            where (role.Name = name)
            select role
            exactlyOne }

let getAllRoles () =
    execQuery (fun ctx -> ctx.Roles :> System.Linq.IQueryable<Role>)



