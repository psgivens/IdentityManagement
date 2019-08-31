# IdentityManagement
Identity Management for infrastructure

# Environment setup 

In a common folder, fetch the following
* Architecture
* IdentityManagement

Point the variable $ENV:POMODORO_REPOS at that folder.

Add the values to your $ENV:PATH
* $ENV:POMODORO_REPOS/Architecture/scripts
* $ENV:POMODORO_REPOS/IdentityManagement/scripts

I also had to set the env:MSBuildSDKsPath in profile.ps1

    $env:MSBuildSDKsPath = "/usr/share/dotnet/sdk/2.2.401/Sdks/"

# Inspecting your install


## Prerequisites
Some of these may have been met on your local machine if you are working with other 
services. You'll need `dotnet-stage` to build service images. You can build that with 
this:

    Publish-CoreDbgStage.psq

Microk8s requires iptables to be setup as per thier [docs](https://microk8s.io/docs/)

    sudo iptables -P FORWARD ACCEPT
    sudo apt-get install iptables-persistent

Microk8s dns to be enabled to reach out to nuget.org and other build activities

    microk8s.enable dns

# Build
You can use these to publish to local kubernetes registry
    
    Publish-CoreDbImage.ps1
    Publish-MicroService.ps1 -ServiceName iam-id-mgmt 

# Start service
The following will start everything you need. 

    Start-MicroService -ServiceName iam-id-mgmt -Part all -Cold $true -NodePort 2020

There are a lot of moving parts. You can inspect for the following. 

* Configmap
* id database pv
* id database pvc
* id database statefulset
* id api service replicaset
* id api service public service
* id api service nodeport

# Debug 

Launch the idservice and explore. 

    ./scripts/Connect-IdServices.ps1

Inspect your environment

    kubectl get all
    kubectl get pv
    kubectl get pvc

# Expected logs
After `Start-MicroService` 

    persistentvolume/persistent-volume-pgsql-identity created
    onfigmap/iam-id-mgmt-dbconfigs created
    ervice/iam-id-mgmt-db created
    statefulset.apps/iam-id-mgmt-db created
    Wait for the service to become online...
    phase: Pending [.. omitting extra 'Pending's]
    phase: Running
    System ready
    Initializing the database with 'dotnet ef database update'
    job.batch/iam-id-mgmt-job-initialize-db created
    Wait for database initialization job to finish...
    phase: Pending [.. omitting extra 'Pending's]
    phase: Running [.. omitting extra 'Running's]    
    phase: Succeeded
    Applying migration '20190505004232_Initial_Database'.
    Done.
    Deleting job
    job.batch "iam-id-mgmt-job-initialize-db" deleted
    ervice/iam-id-mgmt-db-service-nodeport created
    eplicaset.apps/iam-id-mgmt-api-service created
    ervice/iam-id-mgmt-api-service-public created
    ervice/iam-id-mgmt-api-service-nodeport created

If any `phase` lasts longer than a minute, inspect the service. 

# Testing and re-building

I've made a change to source code, how do I tear down, rebuild, and try again in the quickest
amount of time? 

Azure - Network: rebuild, redeploy, maybe have to check in. 
Microk8st - rebuild, redeploy, retest
Docker - mount file system locally, maybe we don't need to redeploy, just rebuild
Locally - Maybe better than docker in that I am not in a container. Maybe worse, because I am 
          not on a network with everything else. Tools must be installed on my machine. I also
          have support for my local dev tools. 

Conclusion: Locally using a database node port. 

# Beware

The database is deleted and recreated with every run of this service. 

# Run

Run-IdMgmtTests

# Build performance

https://stackoverflow.com/questions/45763755/dotnet-core-2-long-build-time-because-of-long-restore-time

