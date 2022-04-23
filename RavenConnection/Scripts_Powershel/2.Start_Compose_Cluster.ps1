Clear-Host
docker-compose down
docker container prune --force
Clear-Host
$composeCommand = "docker-compose up -d"
 
$defaultContainerType="Linux"
$ContainerOS = "Linux"
$ContainerOS = Read-Host "ContainerOS (Windows or Linux) [Default:$defaultContainerType]"
$sh = @()
if ($ContainerOS.Equals('Windows')) {
    $sh = "pwsh"
    $env:container_image="ravendb/ravendb:5.2-windows-latest"
} else {
    $sh = "bash"
    $env:container_image="ravendb/ravendb:5.2-ubuntu-latest"
}
  
Invoke-Expression -Command "$composeCommand $composeArgs"
Start-Sleep -Seconds 10

while(($containersStarted -ne "Y"))
{
    Invoke-Expression -Command "docker-compose ps -a"
    $containersStarted = Read-Host "Have all containers started (Y/N)?"
}


$DontSetupReplication = Read-Host "Do you want to automatically setup replication (Y/N) "
if ($DontSetupReplication -ne "Y") {
    exit 0
}
  
$nodes = @(
    "http://raven1:8080",
    "http://raven2:8080"
);
  
function AddNodeToCluster() {
    param($FirstNodeUrl, $OtherNodeUrl, $AssignedCores = 1)
  
    $otherNodeUrlEncoded = $OtherNodeUrl
    $uri = "$($FirstNodeUrl)/admin/cluster/node?url=$($otherNodeUrlEncoded)"
    $curlCmd = "curl -L -X PUT $uri"
    docker exec -it $(docker ps -q -f name=raven_raven1) $sh -c $curlCmd
    Write-Host
    docker logs $(docker ps -q -f name=raven_raven1)
    Start-Sleep -Seconds 5
}
  
    
$firstNodeIp = $nodes[0]
$nodeAcoresReassigned = $false
foreach ($node in $nodes | Select-Object -Skip 1) {
    write-Host "Add node $node to cluster";
    AddNodeToCluster -FirstNodeUrl $firstNodeIp -OtherNodeUrl $node
  
    if ($nodeAcoresReassigned -eq $false) {
        write-host "Reassign cores on A to 1"
        $uri = "$($firstNodeIp)/admin/license/set-limit?nodeTag=A&newAssignedCores=1"
        $curlCmd = "curl -L -X PUT $uri"
        docker exec -it $(docker ps -q -f name=raven_raven1) $sh -c $curlCmd
        docker logs $(docker ps -q -f name=raven_raven1)
        Start-Sleep -Seconds 5
    }
  
}
  
write-host "Access raven1 on localhost:8081"
write-host "Access raven2 on localhost:8082"  