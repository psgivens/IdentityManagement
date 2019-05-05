module IdentityManagement.Domain.DAL.GroupManagement

open IdentityManagement.Data.Models
open Common.FSharp.Envelopes
open IdentityManagement.Domain.DomainTypes
open IdentityManagement.Domain.GroupManagement

open Newtonsoft.Json

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (userId:UserId) (streamId:StreamId) (state:GroupManagementState option) =
    use context = new IdentityManagementDbContext () 
    let entity = context.Groups.Find (StreamId.unbox streamId)
    let addMember (g:Group) id = 
        g.MemberRelations.Add(
            GroupPrincipalMap(
                PrincipalId=id
            ))

    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let group = 
            Group (
                Id = StreamId.unbox streamId,
                Name = item.Name
            )
        item.Groups |> List.iter (addMember group)
        item.Users |> List.iter (UserId.unbox >> addMember group)
        context.Groups.Add (group) |> ignore
        printfn "Persist new: (%s)" item.Name
    | _, Option.None -> context.Groups.Remove entity |> ignore        
    | _, Some(item) -> 
        entity.MemberRelations.Clear()
        item.Groups |> List.iter (addMember entity)
        item.Users |> List.iter (UserId.unbox >> addMember entity)

        entity.Name <- item.Name
        printfn "Persist update: (%s)" item.Name
    context.SaveChanges () |> ignore
    
let execQuery (q:IdentityManagementDbContext -> System.Linq.IQueryable<'a>) =
    use context = new IdentityManagementDbContext () 
    q context
    |> Seq.toList

let find (groupId:System.Guid) (streamId:StreamId) =
    use context = new IdentityManagementDbContext () 
    context.Groups.Find (StreamId.unbox streamId)

let findMemberByName name =
    use context = new IdentityManagementDbContext () 
    query { for group in context.Groups do            
            where (group.Name = name)
            select group
            exactlyOne }
