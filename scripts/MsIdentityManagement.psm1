#!/usr/bin/pwsh

Function Test-PomMissing {
    if (-not $env:POMODORO_REPOS) {
        Write-Host "Please set the $env:POMODORO_REPOS to the location of this repo."
        return $true
    }   
}

Function Use-PomDirectory {
    if (Test-PomMissing) { RETURN }
    Set-Location "$env:POMODORO_REPOS/PersonalTracker.Api"
}

Function Start-PmsIdentityManagement {
    param(
        # [Parameter(
        #     Mandatory=$true, 
        #     HelpMessage="Starts IdentiyManagement microservices.",
        #     ParameterSetName="Individual")]
        # [ValidateSet(
        #     "pomodoro-pgsql",
        #     "pomodoro-idserver",
        #     "pomodoro-identity", 
        #     "pomodoro-resource", 
        #     "pomodoro-privilege", 
        #     "pomodoro-reverse-proxy",
        #     "watch-pomo-rapi",
        #     "pomo-ping-rapi",
        #     "pomodoro-client"
        # )] 
        # [string]$Container,
        [Parameter(
            Mandatory=$false, 
            HelpMessage="Use 'dotnet run'")]
        [switch]$Runs
    )

    if (Test-PomMissing) { RETURN }

    Write-Host "Starting pms-identitymanagement..."

    if ($Runs) {
        # Cannot attach a debugger, but can have the app auto reload during development.
        # https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md
        docker run `
            --name pms-identitymanagement `
            --rm -it `
            -p 8080:8080 `
            --network pomodoro-net `
            --entrypoint "/bin/bash" `
            -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement/src/:/app/IdentityManagement/IdentityManagement/src/ `
            -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement/secrets/:/app/IdentityManagement/IdentityManagement/secrets/ `
            -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement.Domain/src/:/app/IdentityManagement/IdentityManagement.Domain/src/ `
            -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement.Domain.DAL/src/:/app/IdentityManagement/IdentityManagement.Domain.DAL/src/ `
            -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement.UnitTests/src/:/app/IdentityManagement/IdentityManagement.UnitTests/src/ `
            pms-identitymanagement
#            pms-identitymanagement "run" "--project" "IdentityManagement"
    } else {
        # Cannot attach a debugger, but can have the app auto reload during development.
        # https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md
        docker run `
        --name pms-identitymanagement `
        --rm -it `
        -p 2005:8080 `
        --network pomodoro-net `
        -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement/src/:/app/IdentityManagement/IdentityManagement/src/ `
        -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement/secrets/:/app/IdentityManagement/IdentityManagement/secrets/ `
        -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement.Domain/src/:/app/IdentityManagement/IdentityManagement.Domain/src/ `
        -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement.Domain.DAL/src/:/app/IdentityManagement/IdentityManagement.Domain.DAL/src/ `
        -v $env:POMODORO_REPOS/IdentityManagement/IdentityManagement.UnitTests/src/:/app/IdentityManagement/IdentityManagement.UnitTests/src/ `
        pms-identitymanagement

    }
}
Function Build-PmsIdentityManagement {
    <#
    .SYNOPSIS
        Builds the docker container related to the pomodor project.
    .DESCRIPTION
        Builds the docker container related to the pomodor project.
    .PARAMETER Image
        One of the valid images for the pomodoro project
    .EXAMPLE
    .NOTES
        Author: Phillip Scott Givens
    #>    
    param(
        [Parameter(Mandatory=$false)]
        [ValidateSet(
            "docker", 
            "microk8s.docker",
            "azure"
            )] 
        [string]$Docker="docker"
    )

    if (Test-PomMissing) { RETURN }
    if ($Docker) {
        Set-Alias dkr $Docker -Option Private
    }

    $buildpath = "$env:POMODORO_REPOS/IdentityManagement"
    dkr build `
        -t pms-identitymanagement `
        -f "$buildpath/watch.Dockerfile" `
        "$buildpath/.."
}

Function Update-PmsIdentityManagement {
    if (Test-PomMissing) { RETURN }

    $MyPSModulePath = "{0}/.local/share/powershell/Modules" -f (ls -d ~)
    mkdir -p $MyPSModulePath/MsIdentityManagement

    Write-Host ("Linking {0}/IdentityManagement/scripts/MsIdentityManagement.psm1 to {1}/MsIdentityManagement/" -f $env:POMODORO_REPOS,  $MyPSModulePath)
    ln -s $env:POMODORO_REPOS/IdentityManagement/scripts/MsIdentityManagement.psm1  $MyPSModulePath/MsIdentityManagement/MsIdentityManagement.psm1

    Write-Host "Force import-module PomodorEnv"
    Import-Module -Force MsIdentityManagement -Global

}



Export-ModuleMember -Function Build-PmsIdentityManagement
Export-ModuleMember -Function Start-PmsIdentityManagement
Export-ModuleMember -Function Update-PmsIdentityManagement