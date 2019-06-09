#!/usr/bin/pwsh

docker build `
  -t localhost:32000/iam-identitymanagement `
  -f IdentityManagement/watch.Dockerfile `
  .

docker push localhost:32000/iam-identitymanagement 

