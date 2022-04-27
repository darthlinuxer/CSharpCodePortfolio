declare -a nodes
nodes=("http://raven1:8080" "http://raven2:8080")
  
function AddNodeToCluster {  
    uri="$1/admin/cluster/node?url=$2"
    curlCmd="curl -L -X PUT $uri"
    echo $curlCmd
    docker exec -it $(docker ps -q -f name=raven_raven1) bash -c $curlCmd
    docker logs $(docker ps -q -f name=raven_raven1)
    sleep 5
}
  
    
firstNodeIp=${nodes[0]}
nodeAcoresReassigned=0
unset nodes[0]
for node in "${nodes[@]}";
do
    echo "Add node $node to cluster";
    AddNodeToCluster $firstNodeIp $node;  
    if [ $nodeAcoresReassigned == 0 ]; then
        echo "Reassign cores on A to 1";
        uri="$($firstNodeIp)/admin/license/set-limit?nodeTag=A&newAssignedCores=1";
        curlCmd="curl -L -X PUT $uri";
        docker exec -it $(docker ps -q -f name=raven_raven1) $sh -c $curlCmd;
        docker logs $(docker ps -q -f name=raven_raven1);
        sleep 5;
    fi
done
