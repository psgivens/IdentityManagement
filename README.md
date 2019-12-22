# IdentityManagement
Identity Management for infrastructure

# Environment setup 

In a common folder, fetch the following

* Architecture
* IdentityManagement
* Common.FSharp

Point the variable `$ENV:BESPIN_REPOS` at that folder.

Add the values to your `$ENV:PATH`

* $ENV:BESPIN_REPOS/Architecture/scripts
* $ENV:BESPIN_REPOS/IdentityManagement/scripts

If `dotnet` commands are complaining about the sdk, consider setting  the `env:MSBuildSDKsPath` in profile.ps1

    $env:MSBuildSDKsPath = "/usr/share/dotnet/sdk/2.2.401/Sdks/"

# Inspecting your install


## Prerequisites
Some of these may have been met on your local machine if you are working with other services. You'll need `dotnet-stage` to build service images. You can build that with this:

    Publish-CoreDbgStage.ps1

Microk8s requires iptables to be setup as per thier [docs](https://microk8s.io/docs/)

    sudo iptables -P FORWARD ACCEPT
    sudo apt-get install iptables-persistent

Microk8s dns to be enabled to reach out to nuget.org and other build activities

    microk8s.enable dns

# Build
You can use these to publish to local kubernetes registry
    
    Publish-CoreDbImage.ps1
    Publish-MicroService.ps1 -ServiceName IdentityManagement

# Start service
The following will start everything you need. 

    Start-MicroService -K8s -ServiceName IdentityManagement -Part all -Cold $true -NodePort 2020

To tear down completely.

    Stop-MicroService -K8s -ServiceName IdentityManagement -Cold $true -Part all    

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
    onfigmap/identitymanagement-dbconfigs created
    ervice/identitymanagement-db created
    statefulset.apps/identitymanagement-db created
    Wait for the service to become online...
    phase: Pending [.. omitting extra 'Pending's]
    phase: Running
    System ready
    Initializing the database with 'dotnet ef database update'
    job.batch/identitymanagement-job-initialize-db created
    Wait for database initialization job to finish...
    phase: Pending [.. omitting extra 'Pending's]
    phase: Running [.. omitting extra 'Running's]    
    phase: Succeeded
    Applying migration '20190505004232_Initial_Database'.
    Done.
    Deleting job
    job.batch "identitymanagement-job-initialize-db" deleted
    ervice/identitymanagement-db-service-nodeport created
    eplicaset.apps/identitymanagement-api-service created
    ervice/identitymanagement-api-service-public created
    ervice/identitymanagement-api-service-nodeport created

If any `phase` lasts longer than a minute, inspect the service. 

# Testing and re-building

I've made a change to source code, how do I tear down, rebuild, and try again in the quickest amount of time? Here are my considerations.

| Env                | Description         |
| ----------------- | ------------------------- |
| Azure           | Network; rebuild, redeploy, maybe have to check in. |
| Microk8st | rebuild, redeploy, retest |
| Docker        | mount file system locally, maybe we don't need to redeploy, just rebuild |
| Locally        | Maybe better than docker in that I am not in a container. Maybe worse, because I am not on a network with everything else. Tools must be installed on my machine. I also have support for my local dev tools. |

**Conclusion**: Locally using a database node port. 

# Beware

The database is deleted and recreated with every run of this service. 

# Run

Run-IdMgmtTests

# Build performance

If you are curious about improving build performance, consider reading [dotnet-core-2-long-build-time-because-of-long-restore-time](https://stackoverflow.com/questions/45763755/dotnet-core-2-long-build-time-because-of-long-restore-time)

