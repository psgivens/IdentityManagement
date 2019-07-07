#!/usr/bin/pwsh

# Maybe I have to do this all the time with microk8s
# see: https://microk8s.io/docs/

sudo iptables -P FORWARD ACCEPT

. ./scripts/Start-IdMgmtDb.ps1

. ./scripts/Start-IdMgmtApi.ps1

Write-Host "Waiting for api service to come online."

$retries=70
foreach ($i in 1..$retries) {
    $cmd = "kubectl get pods -o json -l app=iam-id-mgmt-api-service"
    $phase = Invoke-Expression $cmd | ConvertFrom-Json | ForEach-Object {$_.items.status.containerStatuses.ready}
    Write-Host "Ready: $phase"
    if ($phase -eq 'True') { 
        Write-Host "API Service is running."
        break 
    }
    if ($i -eq $retries) { 
        Write-Host "Match not found after $retries retries"
        exit
    }
    Start-Sleep 1
}

Write-Host "Run a job with the integration tests"
kubectl create -f ./kubernetes/iam-id-mgmt-api-job-integration-tests.yaml

Write-Host "Waiting 10 seconds for integration test job to start."
Start-Sleep 10 
kubectl log --selector=app=batch-job-integration-tests -f

kubectl delete -f ./kubernetes/iam-id-mgmt-api-job-integration-tests.yaml

. ./scripts/Stop-IdMgmtAll.ps1
