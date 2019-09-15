module IdentityManagement.Domain.DAL.Database

open IdentityManagement.Data.Models
open Microsoft.EntityFrameworkCore


let initializeDatabase (options:DbContextOptions<IdentityManagementDbContext>) =
    use context = new IdentityManagementDbContext (options)
    context.Database.EnsureDeleted () |> ignore
    context.Database.EnsureCreated () |> ignore
