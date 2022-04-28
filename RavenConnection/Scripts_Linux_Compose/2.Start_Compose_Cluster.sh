clear
echo "Version 1.0.0"
export Docker_Host_IP=$(docker network inspect bridge --format='{{(index .IPAM.Config 0).Gateway}}')
echo "Docker_Host_IP = $Docker_Host_IP"
defaultContainerSecurity="unsecure"
containerSecurity=$defaultContainerSecurity
read -p "Start containers in Secure or Unsecure mode?  [Default:$defaultContainerSecurity] :"
if [ ! -z ${REPLY} ]; then containerSecurity=${REPLY}; fi
if [ $containerSecurity == "secure" ]; then 
    dockercomposefile="docker-compose-secure.yml";
else 
    dockercomposefile="docker-compose-unsecure.yml"
fi

echo "docker-compose -f $dockercomposefile down"
docker-compose -f $dockercomposefile down
docker container prune --force
echo "docker container prune --force"
clear
 
echo "executing $containerSecurity containers script..."
defaultContainerType="Linux"
ContainerOS=$defaultContainerType
read -p "ContainerOS (Windows or Linux) [Default:$defaultContainerType] :"
if [ ! -z ${REPLY} ]; then ContainerOS = ${REPLY}; fi

if [ $ContainerOS == 'Windows' ]; then
    sh="pwsh";
    export container_image="ravendb/ravendb:5.2-windows-latest";
else 
    sh="bash";
    export container_image="ravendb/ravendb:latest";
fi
  
echo "Starting containers in $containerSecurity mode..."
echo "docker-compose -f $dockercomposefile up -d"
docker-compose -f $dockercomposefile up -d;
sleep 10

containersStarted="N"
while [ $containersStarted == "N" ] || [ $containersStarted == "n" ]
do
    clear
    docker-compose -f $dockercomposefile ps -a
    read -p "Have all containers started (Y/N)?"
    if [ -z ${REPLY} ]; then containersStarted="N"; else containersStarted=${REPLY}; fi 
done

read -p "Do you want to automatically setup replication (Y/N) "
if [ -z ${REPLY} ]; then SetupReplication="Y"; else SetupReplication=${REPLY}; fi 
if [ $SetupReplication == "N" ] || [ $SetupReplication == "n" ]; then 
    echo "You will have to manually configure the cluster";
    echo "Access raven1 on localhost:8081";
    echo "Access raven2 on localhost:8082"; 
    exit;
fi
  
declare -a nodes
nodes=("http://raven1:8080" "http://raven2:8080")
  
function AddNodeToCluster {  
    firstNodeUrl=$1
    otherNodeUrlEncoded=$2
    uri="$firstNodeUrl/admin/cluster/node?url=$otherNodeUrlEncoded"
    curlCmd="curl -L -X PUT $uri"
    echo docker exec -it $(docker ps -q -f name=raven_raven1) $sh -c "$curlCmd"
    docker exec -it $(docker ps -q -f name=raven_raven1) $sh -c "$curlCmd"
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
        uri="$firstNodeIp/admin/license/set-limit?nodeTag=A&newAssignedCores=1";
        curlCmd="curl -L -X PUT $uri";
        echo docker exec -it $(docker ps -q -f name=raven_raven1) $sh -c "$curlCmd";
        docker exec -it $(docker ps -q -f name=raven_raven1) $sh -c "$curlCmd";
        docker logs $(docker ps -q -f name=raven_raven1);
        sleep 5;
    fi
done
  
echo "Access raven1 on localhost:8081"
echo "Access raven2 on localhost:8082"  