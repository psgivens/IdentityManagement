module IdentityManagement.Domain.DAL.Database

open IdentityManagement.Data.Models


let initializeDatabase () =
    use context = new IdentityManagementDbContext ()
    context.Database.EnsureDeleted () |> ignore
    context.Database.EnsureCreated () |> ignore
