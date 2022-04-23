param(
    [switch]$UseDockerCompose,
    [switch]$DontSetupReplication,
    [string]$ContainerOS = 'Windows'   
)

if($UseDockerCompose) { Write-Host "UseDockerCompose: $UseDockerCompose"}
if($DontSetupReplication) { Write-Host "DontSetupReplication: $DontSetupReplication"}
if($ContainerOS) { Write-Host "ContainerOS: $ContainerOS"}
