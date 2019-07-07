#!/usr/bin/pwsh

# Maybe I have to do this all the time with microk8s
# see: https://microk8s.io/docs/

kubectl create -f ./kubernetes/iam-id-mgmt-db-persistent-volume-pgsql.yaml

kubectl create -f ./kubernetes/iam-id-mgmt-db-statefulset.yaml

Write-Host "Wait for the service to become online..."
$retries=10
foreach ($i in 1..$retries) {
    $cmd = "kubectl get pods -o json -l app=iam-id-mgmt-db"
    $phase = Invoke-Expression $cmd | ConvertFrom-Json | ForEach-Object {$_.items.status.phase}
    Write-Host "phase: $phase"
    if ($phase -eq 'Running') { 
        Write-Host "System ready"
        break }
    if ($i -eq $retries) { 
        Write-Host "Match not found after $retries retries"
        exit
    }
    Start-Sleep 1
}

kubectl create -f ./kubernetes/iam-id-mgmt-api-configmap.yaml

Write-Host "Initializing the database with 'dotnet ef database update"
kubectl create -f ./kubernetes/iam-id-mgmt-api-initialize-db.yaml

Write-Host "Wait for database initialization job to finish..."
$retries=40
foreach ($i in 1..$retries) {
    $cmd = "kubectl get pods -o json -l app=batch-job-initialize-db"
    $items = Invoke-Expression $cmd | ConvertFrom-Json | Foreach-Object { $_.items }
    if ($items.Count -lt 1) {
        Write-Host "No running pods found!"
    }
    $phase = $items | Foreach-Object { $_.status.phase } | Select-Object -First 1
    Write-Host "phase: $phase"
    if ($phase -eq 'Succeeded') { 
        Write-Host "Deleting job"
        kubectl delete -f ./kubernetes/iam-id-mgmt-api-initialize-db.yaml
        break; }
    if ($i -eq $retries) { 
        Write-Host "Match not found after $retries retries"
        exit }
    Start-Sleep 5
}
