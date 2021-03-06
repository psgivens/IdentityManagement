module IdentityManagement.Domain.DAL.UserManagement

open IdentityManagement.Data.Models
open Common.FSharp.Envelopes
open IdentityManagement.Domain.DomainTypes
open IdentityManagement.Domain.UserManagement

open Microsoft.EntityFrameworkCore

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (options:DbContextOptions<IdentityManagementDbContext>) (userId:UserId) (streamId:StreamId) (state:UserManagementState option) =
    use context = new IdentityManagementDbContext (options) 
    let entity = context.Users.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let details = item.Details
        context.Users.Add (
            User (
                Id = StreamId.unbox streamId,
                FirstName = details.FirstName,
                LastName = details.LastName,
                Email=details.Email
            )) |> ignore
        printfn "Persist new: (%s)" details.Email
    | _, Option.None -> context.Users.Remove entity |> ignore        
    | _, Some(item) -> 
        let details = item.Details
        entity.FirstName <- details.FirstName
        entity.LastName <- details.LastName
        entity.Email <- details.Email 
        printfn "Persist update: (%s)" details.Email
    context.SaveChanges () |> ignore
    
let execQuery (q:IdentityManagementDbContext -> System.Linq.IQueryable<'a>) =
    use context = new IdentityManagementDbContext () 
    q context
    |> Seq.toList

let getAllUsers () =
    execQuery (fun ctx -> ctx.Users :> System.Linq.IQueryable<User>)

let getHeadUser () =
    let getUser' (ctx:IdentityManagementDbContext) = 
        query { 
            for user in ctx.Users do
            select user
        }
    getUser'
    |> execQuery
    |> Seq.head


let find (userId:UserId) (streamId:StreamId) =
    use context = new IdentityManagementDbContext () 
    context.Users.Find (StreamId.unbox streamId)

let findUserByEmail email =
    use context = new IdentityManagementDbContext () 
    query { for user in context.Users do            
            where (user.Email = email)
            select user
            exactlyOne }
