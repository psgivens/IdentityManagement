# IdentityManagement
Identity Management for infrastructure

# Environment setup 

In a common folder, fetch tge following
* Architecture
* IdentityManagement

Point the variable $ENV:POMODORO_REPOS at that folder
Add the values to your $ENV:PATH
* $ENV:POMODORO_REPOS/Architecture/scripts
* $ENV:POMODORO_REPOS/IdentityManagement/scripts

# Build

Build-CoreDbgStage.ps1
Publish-CoreDbImage.ps1

# Run

Run-IdMgmtTests

# Build performance

https://stackoverflow.com/questions/45763755/dotnet-core-2-long-build-time-because-of-long-restore-time