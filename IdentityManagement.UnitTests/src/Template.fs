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



type TestTemplate () =

    [<Fact>]
    member this.``Delete role`` () =
      use testResources = new Composition.TestSystemResources ()
      let harness = ``Delete role`` testResources
      harness.Gherkin ()
      harness.Preconditions ()
      harness.ActionUnderTest ()
      harness.VerifyState ()
