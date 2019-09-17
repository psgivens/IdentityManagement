namespace IdentityManagement.UnitTests

open IdentityManagement.Domain.UserManagement
open IdentityManagement.Domain.GroupManagement
open IdentityManagement.Domain.RoleManagement

open IdentityManagement.Api.Composition
open System
open Xunit
open Common.FSharp
open Common.FSharp.Actors
open Common.FSharp.Envelopes
open Common.FSharp.EventSourceGherkin

open Akka.Actor
open Akka.FSharp

open IdentityManagement.Data.Models
open Microsoft.Data.Sqlite
open Microsoft.EntityFrameworkCore

type RolesTests ()  =

    [<Fact>]
    member this.``Access database`` () =

      use connection = new SqliteConnection("DataSource=:memory:")
      connection.Open()
      let options = (new DbContextOptionsBuilder<IdentityManagementDbContext> ()).UseSqlite(connection).Options;

      use context = new IdentityManagementDbContext (options)
      context.Database.EnsureCreated() |> ignore

      let id = Guid.NewGuid()
      use context' = new IdentityManagementDbContext (options)
      context'.UserEvents.Add (
          UserEventEnvelopeEntity (  Id = id,
                                  StreamId = Guid.NewGuid (),
                                  UserId = Guid.NewGuid (),
                                  TransactionId = Guid.NewGuid (),
                                  Version = 0s,
                                  TimeStamp = DateTimeOffset.Now ,
                                  Event = "{'key':'value'}"
                                  )) |> ignore         
      context'.SaveChanges () |> ignore

      use context'' = new IdentityManagementDbContext (options)
      let x = context''.UserEvents |> Seq.head
      Assert.Equal (id, x.Id)

    [<Fact>]
    member this.``Create role_ add user_ update title`` () =
      use testResources = new Composition.TestSystemResources ()
      let harness = ``Test: Create role_ add user_ update title`` testResources
      harness.Gherkin ()
      harness.Preconditions ()
      harness.ActionUnderTest ()
      harness.VerifyState ()

    [<Fact>]
    member this.``Create role_ add user to group_ add group to role`` () =
      use testResources = new Composition.TestSystemResources ()
      let harness = ``Test: Create role, add user to group, add group to role`` testResources
      harness.Gherkin ()
      harness.Preconditions ()
      harness.ActionUnderTest ()
      harness.VerifyState ()

    [<Fact>]
    member this.``Create role_ add group to role_ add user to group`` () =
      use testResources = new Composition.TestSystemResources ()
      let harness = ``Create role, add group to role, add user to group`` testResources
      harness.Gherkin ()
      harness.Preconditions ()
      harness.ActionUnderTest ()
      harness.VerifyState ()


    [<Fact>]
    member this.``Remove user from a role`` () =
      use testResources = new Composition.TestSystemResources ()
      let harness = ``Remove user from a role`` testResources
      harness.Gherkin ()
      harness.Preconditions ()
      harness.ActionUnderTest ()
      harness.VerifyState ()

    [<Fact>]
    member this.``Delete role`` () =      
      use testResources = new Composition.TestSystemResources ()
      let harness = ``Delete role`` testResources
      harness.Gherkin ()
      harness.Preconditions ()
      harness.ActionUnderTest ()
      harness.VerifyState ()

