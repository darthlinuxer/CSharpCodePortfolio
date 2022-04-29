clear

sudo docker swarm init

function StartRaven
{
    mode=$1; port=$2; tcpPort=$3; hostnameOrIp=$4
    sudo docker kill "raven$node"
    sudo docker container prune --force
    clear
    sudo docker network create raven_net
    sudo docker service create -it -d --name $hostnameOrIp \
            -e RAVEN_Setup_Mode=$mode \
            -e RAVEN_Logs_Mode=Information \
            -e RAVEN_License_Eula_Accepted=true \
            -e RAVEN_Security_UnsecuredAccessAllowed=PublicNetwork \
            -e RAVEN_ServerUrl=http://$hostnameOrIp:8080 \
            -e RAVEN_ServerUrl_Tcp=tcp://$hostnameOrIp:38888 \
            -p $port:8080 -p $tcpPort:38888 \
            -h $hostnameOrIp --network raven_net \
            ravendb/ravendb:5.2-ubuntu-latest
}

function AddNodeToCluster {  
    master=$1
    slave=$2
    uri="http://$master:8080/admin/cluster/node?url=http://$slave:8080"
    curlCmd="curl -L -X PUT $uri"
    echo sudo docker exec -it $(sudo docker ps -q -f name=$master) bash -c "$curlCmd"
    sudo docker exec -it $(sudo docker ps -q -f name=$master) bash -c "$curlCmd"
    sudo docker logs $(sudo docker ps -q -f name=$master)
    sleep 5
}

function menu
{
    clear
    echo "0. Exit"
    echo "1. Start RavenDb"
    echo "2. Kill all Raven Containers"
    echo "3. Kill a Raven Container"
    echo "4. Join Cluster"
    echo "100. Docker logs <node>"
    echo "101. Docker ps -a"
    echo "102. Docker exec <cmd>"
    read -p "Choose an option:"
    if [ -z ${REPLY} ]; then showMenu=1 ; fi
    if [ ${REPLY} == 0 ]; then showMenu=0; fi
    if [ ${REPLY} == 1 ]; then 
        read -p "Choose the node hostname or IP: "
        hostnameOrIp=${REPLY}
        read -p "Choose the Port: "
        port=${REPLY}
        read -p "Choose the TCP Port: "
        tcpPort=${REPLY}
        StartRaven "None" $port $tcpPort $hostnameOrIp; 
    fi  
    if [ ${REPLY} == 2 ]; then
        for container in $(sudo docker ps -q -f name=raven)
        do
            sudo docker kill $container
        done
        sudo docker container prune --force
        sudo docker ps -a
    fi
    if [ ${REPLY} == 3 ]; then
        read -p "Kill which container hostname ? "
        sudo docker kill ${REPLY}
        sudo docker container prune --force
    fi
    if [ ${REPLY} == 4 ]; then
        read -p "Master Node hostname or IP: "
        master=${REPLY}
        read -p "Slave Node hostname or IP: "
        slave=${REPLY}
        AddNodeToCluster $master $slave        
    fi
    if [ ${REPLY} == 100 ]; then
        read -p "Which Node hostname ? " 
        sudo docker logs ${REPLY}; 
    fi
    if [ ${REPLY} == 101 ]; then sudo docker ps -a; fi
    if [ ${REPLY} == 102 ]; then
        read -p "Which Node hostname ? "
        node = ${REPLY} 
        read -p "Command: " 
        sudo docker exec $node ${REPLY};
    fi 
}

showMenu=1
while [ $showMenu == 1 ];
do
    menu;
    read -p "press any key to continue..."  
done
