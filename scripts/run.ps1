#!/usr/bin/pwsh

sudo iptables -P FORWARD ACCEPT


kubectl run --generator=run-pod/v1 curl --image tutum/curl -l app=curl 

kubectl exec tutum/curl -- curl -s 'http://iam-id-mgmt-api-serivce-public:2080'


$domain = "http://localhost"

$domain = "http://localhost:2080"

$domain = "http://localhost:32081"


# Cluster-Ip of node port
$domain="http://10.152.183.53:2080"

$domain="http://10.152.183.53:32080"


$domain="http://10.152.183.157:2080"

$domain="http://10.152.183.157"

# Node port
$domain = "http://localhost:32080"

# Node port - Assumes blairstone is set in your hosts file 
$domain = "http://blairstone:32080"

# Function Invoke-ErrorRequest {
#     param(
#         [Parameter(Mandatory=$true)]
#         [string]$Uri
#     )
#     $response = try { 
#         (Invoke-WebRequest -Uri $Uri ).BaseResponse
#     } catch [System.Net.WebException] { 
#         Write-Verbose "An exception was caught: $($_.Exception.Message)"
#         $_.Exception.Response 
# 
#         #then convert the status code enum to int by doing this
#         $statusCodeInt = $response.BaseResponse.StatusCode
#     } 
#     $statusCodeInt
# }


$contextHeaders = @{
    user_id='638d111c-c7bb-4710-8ea2-91c4bc3ea530'
    transaction_id='638d111c-c7bb-4710-8ea2-91c4bc3ea530'
}


$contextHeaders = @{
    user_id='Phillip Scott Givens'
    transaction_id='somevalue'
}

# # bad request
# $err = Invoke-ErrorRequest `
#   -Uri $domain 
# $err

$domain="http://pomodoro.poms"

$domain="http://localhost"

$domain="http://10.152.183.53:32080"

Invoke-WebRequest `
  -Method GET `
  -Uri $domain/foo

# Default route
Invoke-WebRequest `
  -Method GET `
  -Uri $domain `
  -Headers $contextHeaders 
    

Write-Host $domain
# Default route
Invoke-WebRequest `
  -Method GET `
  -Uri "$domain" 



sudo iptables -P FORWARD ACCEPT

$domain = "http://10.152.183.114:32081"

$domain = "http://10.152.183.202:32080"

$domain = "http://10.152.183.90:2080"


$domain = "http://localhost:32080"

$domain = "http://localhost:32081"

Write-Host $domain
# Default route
Invoke-WebRequest `
  -Method GET `
  -Uri "$domain/ping" 





# Default route
$users = Invoke-RestMethod `
  -Method GET `
  -Uri $domain/users `
  -Headers $contextHeaders 
$users | measure | %{ if ($_.Count -lt 1) { Write-Error "no users found." } 
  else { 
    Write-Host "usres found"  
    $users | Format-Table
  }
}


# Default route
$user = Invoke-RestMethod `
  -Method GET `
  -Uri $domain/users/one@three.com `
  -Headers $contextHeaders 
if ($user -eq $null) { Write-Error "no user found." } 

$joseph = @{ 
    email='beth@blyth.com'
    first_name='beth'
    last_name='blyth'
} | ConvertTo-Json
$ret = Invoke-RestMethod `
  -Method Post `
  -Body $joseph `
  -Uri $domain/users `
  -Headers $contextHeaders 
if ($ret -eq $null) { Write-Error "no user found." } 

$joseph = @{ 
    email='adam@blyth.com'
    first_name='adam'
    last_name='blyth'
} | ConvertTo-Json
$ret = Invoke-RestMethod `
  -Method Post `
  -Body $joseph `
  -Uri $domain/users `
  -Headers $contextHeaders 
if ($ret -eq $null) { Write-Error "no user found." } 
$jane = @{ 
    email='garry@smith.com'
    first_name='garry'
    last_name='smith'
} | ConvertTo-Json
$ret = Invoke-RestMethod `
  -Method Post `
  -Body $jane `
  -Uri $domain/users `
  -Headers $contextHeaders 
if ($ret -eq $null) { Write-Error "no user found." } 
$joseph = @{ 
    email='hank@blyth.com'
    first_name='hank'
    last_name='blyth'
} | ConvertTo-Json
$ret = Invoke-RestMethod `
  -Method Post `
  -Body $joseph `
  -Uri $domain/users `
  -Headers $contextHeaders 
if ($ret -eq $null) { Write-Error "no user found." } 
$jane = @{ 
    email='iliza@smith.com'
    first_name='iliza'
    last_name='smith'
} | ConvertTo-Json
$ret = Invoke-RestMethod `
  -Method Post `
  -Body $jane `
  -Uri $domain/users `
  -Headers $contextHeaders 
if ($ret -eq $null) { Write-Error "no user found." } 




### Groups

$groups = Invoke-RestMethod `
  -Method GET `
  -Uri $domain/groups `
  -Headers $contextHeaders 
$groups | measure | %{ if ($_.Count -lt 1) { Write-Error "no groups found." } }


### Roles

$roles = Invoke-RestMethod `
  -Method GET `
  -Uri $domain/roles `
  -Headers $contextHeaders 
$roles | measure | %{ if ($_.Count -lt 1) { Write-Error "no roles found." } }





kubectl run `
  -it svclookup `
  --image=tutum/dnsutils `
  --rm `
  --restart=Never -- dig SRV identity-db-statefulset.default.svc.cluster.local








