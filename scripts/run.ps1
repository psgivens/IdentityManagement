#!/usr/bin/pwsh


$domain = "http://localhost:8080"

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
    user_id='Phillip Scott Givens'
    transaction_id='somevalue'
}

# # bad request
# $err = Invoke-ErrorRequest `
#   -Uri $domain 
# $err


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
$users | measure | %{ if ($_.Count -le 1) { Write-Error "no users found." } }


# Default route
$user = Invoke-RestMethod `
  -Method GET `
  -Uri $domain/users/one@three.com `
  -Headers $contextHeaders 
if ($user -eq $null) { Write-Error "no user found." } 


$joseph = @{ 
    email='joseph@smith.com'
    first_name='joseph'
    last_name='smith'
}
$ret = Invoke-RestMethod `
  -Method Post `
  -Uri $domain/users `
  -Headers $contextHeaders 
if ($user -eq $null) { Write-Error "no user found." } 


