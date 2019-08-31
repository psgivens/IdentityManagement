#!/usr/bin/pwsh

# kubectl exec -it iam-im-single -- /bin/bash


Write-Host "Connecting with iam-id-mgmt-api-service"
kubectl run `
    -it temp-identitymanagement `
    --image="localhost:32000/iam-id-mgmt-api-service" `
    --rm `
    --restart=Never -- /bin/bash

