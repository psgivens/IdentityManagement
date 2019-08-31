#!/usr/bin/pwsh

sudo iptables -P FORWARD ACCEPT

# Extra
# $domain = "http://localhost:2020"
# $domain = "http://localhost:32081"


$contextHeaders = @{
    user_id='638d111c-c7bb-4710-8ea2-91c4bc3ea530'
    transaction_id='638d111c-c7bb-4710-8ea2-91c4bc3ea530'
}

$domain = "http://localhost:32080"

Write-Host $domain
# Default route
Invoke-WebRequest `
  -Method GET `
  -Uri "$domain/ping" 

# Default route
Invoke-WebRequest `
  -Method GET `
  -Uri $domain `
  -Headers $contextHeaders 
    

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


Invoke-WebRequest `
  -Method GET `
  -Uri "$domain/ping" 





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








