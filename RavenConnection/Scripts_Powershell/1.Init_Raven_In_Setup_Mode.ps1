docker kill ravenInitial
docker container prune --force
Clear-Host

$defaultContainerType="Linux"
$ContainerOS = $defaultContainerType
$ContainerOS = Read-Host "ContainerOS (Windows or Linux) [Default:$defaultContainerType]"

if ($ContainerOS.Equals('Windows') -or $ContainerOS.Equals('windows')) {
    $env:container_image="ravendb/ravendb:5.2-windows-latest"
} else {
    $env:container_image="ravendb/ravendb:5.2-ubuntu-latest"
}

docker run -d -it `
    -p 8080:8080 `
    -p 38888:38888 `
    -e RAVEN_Setup_Mode='Initial' `
    --name ravenInitial `
    $env:container_image


Write-Host "Wait Container start.."
Start-Sleep -Seconds 10
Clear-Host
docker ps -a

Write-Host "Now access http://localhost:8080"
Write-Host "Finish the configuration and get a free license"
Write-Host "Update license.env with your license details"
Write-Host "run: docker kill ravenInitial"
Write-Host "run: docker container prune"
Start-Process /min microsoft-edge:http://localhost:8080

