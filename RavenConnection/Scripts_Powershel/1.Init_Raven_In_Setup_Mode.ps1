docker kill ravenInitial
docker container prune --force
Clear-Host

$defaultContainerType="Linux"
$ContainerOS = Read-Host "ContainerOS (Windows or Linux) [Default:$defaultContainerType] "
$sh = @()
if ($ContainerOS.Equals('Windows')) {
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
docker ps -a

Write-Host "Now access http://localhost:8080"
