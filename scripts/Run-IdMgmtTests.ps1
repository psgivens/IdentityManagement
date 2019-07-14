#!/usr/bin/pwsh

# Maybe I have to do this all the time with microk8s
# see: https://microk8s.io/docs/

sudo iptables -P FORWARD ACCEPT

Run-MicroServiceTests.ps1 `
    -ServiceName iam-id-mgmt `
    -Cold $true
