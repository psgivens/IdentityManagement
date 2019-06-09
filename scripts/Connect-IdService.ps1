#!/usr/bin/pwsh

# kubectl exec -it iam-im-single -- /bin/bash


Write-Host "Connecting with iam-identitymanagement"
kubectl run `
    -it temp-identitymanagement `
    --image="localhost:32000/iam-identitymanagement" `
    --rm `
    --restart=Never -- /bin/bash

