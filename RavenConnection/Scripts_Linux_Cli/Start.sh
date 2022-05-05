#!/bin/bash
clear
Raven_BindPort=8080
Raven_BindTcpPort=38888
defaultPort=8080
defaultTcpPort=38888

function StartRaven
{
    mode=$1; port=$2; tcpPort=$3; hostname=$4; publicIP=$5 ; runInCloud=$6
    if [ $runInCloud == "yes "]; then 
        RAVEN_URL="RAVEN_PublicServerUrl";
        RAVEN_TCP="RAVEN_PublicServerUrl_Tcp";
    else
        RAVEN_URL="RAVEN_ServerUrl";
        RAVEN_TCP="RAVEN_ServerUrl_Tcp";
    fi
    sudo docker kill "raven$node"
    sudo docker container prune --force
    clear
    sudo docker network create raven_net
    sudo docker volume create data_$hostname
    sudo docker run -it -d --name $hostname \
            -e RAVEN_ARGS="--log-to-console" \
            -e RAVEN_Setup_Mode=$mode \
            -e RAVEN_Logs_Mode="Information" \
            -e RAVEN_License_Eula_Accepted="true" \
            -e RAVEN_Security_UnsecuredAccessAllowed="PublicNetwork" \
            -e RAVEN_BindPort=$Raven_BindPort \
            -e RAVEN_BindTcpPort=$Raven_BindTcpPort \
            -e $RAVEN_URL="http://$publicIp:$Raven_BindPort" \
            -e $RAVEN_TCP="tcp://$publicIp:$Raven_BindTcpPort" \
            -e RAVEN_AUTO_INSTALL_CA="true" \
            -e RAVEN_IN_DOCKER="true" \
            -p $port:$Raven_BindPort -p $tcpPort:$Raven_BindTcpPort  \
            -h $hostname --network raven_net \
            -v data_$hostname:/opt/RavenDB/Server/RavenData \
            ravendb/ravendb:latest
}

function AddNodeToCluster {  
    master=$1
    slave=$2
    uri="http://$master:$Raven_BindPort/admin/cluster/node?url=http://$slave:$Raven_BindPort"
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
    option=${REPLY}
    if [ -z $option ]; then showMenu=1 ; fi
    if [ $option == 0 ]; then showMenu=0; fi
    if [ $option == 1 ]; then 
        read -p "Is container running in cloud? [no] (yes/no) :";
        if [ -z ${REPLY} ]; then runInCloud="no"; else runInCloud="yes"; fi
        echo "runInCloud: $runInCloud"
        read -p "Choose the node hostname: ";
        hostname=${REPLY};
        echo "hostname: $hostname";
        if [ $runInCloud == "yes" ]; then 
            read -p "what is the node publicIP ?";
            publicIp=${REPLY};
        else
            publicIp=$hostname;
        fi
        echo "publicIp=$publicIp";
        read -p "Choose the Port [$defaultPort] : ";
        if [ -z ${REPLY} ] || [ "${REPLY}" == "no" ]; then port=$defaultPort; else port=${REPLY}; fi
        echo "port=$port"
        read -p "Choose the TCP Port [$defaultTcpPort] : ";
        if [ -z ${REPLY} ]; then tcpPort=$defaultTcpPort; else tcpPort=${REPLY}; fi
        echo "tcpPort=$tcpPort"
        read -p "press any key to launch container..."
        StartRaven "None" $port $tcpPort $hostname $publicIp $runInCloud; 
    fi  
    if [ $option == 2 ]; then
        for container in $(sudo docker ps -q -f name=raven)
        do
            sudo docker kill $container;
        done
        sudo docker container prune --force;
        sudo docker ps -a;
    fi
    if [ $option == 3 ]; then
        read -p "Kill which container hostname ? ";
        sudo docker kill ${REPLY};
        sudo docker container prune --force;
    fi
    if [ $option == 4 ]; then
        read -p "Master Node container name: ";
        master=${REPLY};
        read -p "Slave Node container name(if local) or PublicIp: ";
        slave=${REPLY};
        AddNodeToCluster $master $slave;   
    fi
    if [ $option == 100 ]; then
        read -p "Which Node hostname ? ";
        sudo docker logs ${REPLY};
    fi
    if [ $option == 101 ]; then sudo docker ps -a; fi
    if [ $option == 102 ]; then
        read -p "Which Node hostname ? ";
        hostname=${REPLY} ;
        read -p "Command: " ;
        command=${REPLY};
        sudo docker exec -it $hostname $command;
    fi 
}

showMenu=1
while [ $showMenu == 1 ];
do
    menu;
    read -p "press any key to continue..."  
done
