#!/usr/bin/pwsh

# kubectl exec -it iam-im-single -- /bin/bash


Write-Host "Connecting with identitymanagement-api-service"
kubectl run `
    -it temp-identitymanagement `
    --image="localhost:32000/identitymanagement-api-service" `
    --rm `
    --restart=Never -- /bin/bash

