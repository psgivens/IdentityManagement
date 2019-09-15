module IdentityManagement.Domain.DAL.RoleManagement

open IdentityManagement.Data.Models
open Common.FSharp.Envelopes
open IdentityManagement.Domain.DomainTypes
open IdentityManagement.Domain.RoleManagement

open Microsoft.EntityFrameworkCore

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (options:DbContextOptions<IdentityManagementDbContext>) (userId:UserId) (streamId:StreamId) (state:RoleManagementState option) =
    use context = new IdentityManagementDbContext (options)
    let entityId = StreamId.unbox streamId

    // NOTE: By querying group members, we are introducing a race condition.
    // This is an acceptable risk for the applicaiton we are building because
    // the expected collisions is extremely low. 
    //
    // In a high throughput system this would be done in the event-sourcing / actor layer

    let getGroup gid = query {
        for gm in context.Groups.Include "MemberRelations" do
        where (gm.Id = gid)
        select gm
        exactlyOneOrDefault
    }

    let entity = query {
        for r in context.Roles.Include "Members" do
        where (r.Id = entityId)
        select r
        exactlyOneOrDefault
    }

    let addMember (r:Role) id =         
        r.Members.Add(
            RolePrincipalMap(
                PrincipalId=id,
                GroupId=System.Nullable ()
            ))

        match getGroup id with
        | null -> ()
        | group -> 
            group.MemberRelations
            |> Seq.iter (fun m -> 
              r.Members.Add (
                RolePrincipalMap (
                    PrincipalId=m.PrincipalId,
                    GroupId=System.Nullable id))
                )        

    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let role = 
            Role (
                Id = StreamId.unbox streamId,
                Name = item.Name,
                ExternalId = item.ExternalId
            )

        item.Principals |> List.iter (role |> addMember)
        context.Roles.Add role |> ignore
        printfn "Persist mh: (%s)" item.Name
    | _, Option.None -> 
        context.Roles.Remove entity |> ignore        
    | _, Some(item) -> 
        entity.Name <- item.Name

        entity.Members.Clear ()
        item.Principals |> List.iter (entity |> addMember)
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



