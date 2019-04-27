#!/usr/bin/pwsh


$domain = "http://localhost:8080"

Invoke-WebRequest `
  -Method GET `
  -Uri $domain 

Invoke-WebRequest `
  -Method GET `
  -Uri $domain/hello 

Invoke-WebRequest `
  -Method POST `
  -Uri $domain/hello 

Invoke-WebRequest `
  -Method POST `
  -Headers @{ 'foo'='bar1'} `
  -Uri $domain/hello 





