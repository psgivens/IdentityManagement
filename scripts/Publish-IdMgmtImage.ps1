#!/usr/bin/pwsh

Publish-MicroService.ps1 `
  -ServiceName iam-id-mgmt `
  -Dockerfile "IdentityManagement/watch.Dockerfile" `
  -ImageName "api-service"

