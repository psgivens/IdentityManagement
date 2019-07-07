#!/usr/bin/pwsh

# Maybe I have to do this all the time with microk8s
# see: https://microk8s.io/docs/

sudo iptables -P FORWARD ACCEPT

. ./scripts/Initialize-IdMgmtDb.ps1

. ./scripts/Initialize-IdMgmtApi.ps1

# TODO: Create a job for running tests and launch it. 



