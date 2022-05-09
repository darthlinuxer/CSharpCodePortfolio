#!/bin/bash
clear
Raven_BindPort=8080
Raven_BindTcpPort=38888
defaultPort=8080
defaultTcpPort=38888

sudo docker swarm init

function StartRaven
{
    mode=$1; port=$2; tcpPort=$3; hostname=$4; publicIP=$5 ; runInCloud=$6
    if [ $runInCloud == "yes" ] || [ $runInCloud == "y" ] || [ $runInCloud == "Y" ]; then
        RAVEN_URL="RAVEN_PublicServerUrl";
        RAVEN_TCP="RAVEN_PublicServerUrl_Tcp";
    else
        RAVEN_URL="RAVEN_ServerUrl";
        RAVEN_TCP="RAVEN_ServerUrl_Tcp";
    fi
    echo "Start Raven Container with parameters:"
    echo "hostname = $hostname:$port; publicIP = $publicIP; runInCloud = $runInCloud"
    echo "RAVEN_URL = $RAVEN_URL"
    echo "RAVEN_TCP = $RAVEN_TCP"
    echo "Killing old containers with the same name if exist: docker kill $hostname"
    sudo docker kill $hostname
    sudo docker container prune --force
    echo "Creating bridge network raven_net"
    sudo docker network create --opt encrypted --driver overlay --attachable raven_net
    echo "Creating volume data_$hostname"
    sudo docker volume create data_$hostname
    read -p "Press any key to Start container $hostname ... "
    clear
    sudo docker service create -t -d --name $hostname \
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
            --hostname=$hostname --network raven_net \
            --replicas=1 \
            --mount source=data_$hostname,target=/opt/RavenDB/Server/RavenData \
            ravendb/ravendb:latest
}

function AddNodeToCluster {  
    master=$1
    slave=$2
    uri="http://$master:$Raven_BindPort/admin/cluster/node?url=http://$slave:$Raven_BindPort"
    curlCmd="curl -L -X PUT $uri"
    echo "Adding Slave node $slave to Master $master ..."
    echo sudo docker exec -it $(sudo docker ps -q -f name=$master) bash -c "$curlCmd"
    sudo docker exec -it $(sudo docker ps -q -f name=$master) bash -c "$curlCmd"
    sudo docker logs $(sudo docker ps -q -f name=$master)
    sleep 5
}

function menu
{
    clear
    echo "0. Exit"
    echo "1. Start RavenDb Service in current Node"
    echo "2. Kill all Raven Services"
    echo "3. Kill a Raven Service"
    echo "4. See Master Node Token"
    echo "5. See Worker Node Token"
    echo "6. Join Node"
    echo "100. Docker logs <node>"
    echo "101. Docker ps -a"
    echo "102. Docker exec <cmd>"
    echo "103. Leave Swarm"
    read -p "Choose an option:"
    option=${REPLY}
    if [ -z $option ]; then showMenu=1 ; fi
    if [ $option == 0 ]; then showMenu=0; fi
    if [ $option == 1 ]; then
        read -p "Is container running in cloud? [no] (yes/no) :";
        if [ -z ${REPLY} ] || [ ${REPLY} == "no" ] || [ ${REPLY} == "NO" ]; then runInCloud="no"; else runInCloud="yes"; fi
        echo "runInCloud: $runInCloud"
        read -p "Choose the service hostname: ";
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
            sudo docker service rm $container;
        done
        sudo docker container prune --force;
        sudo docker ps -a;
    fi
    if [ $option == 3 ]; then
        read -p "Kill which container hostname ? ";
        sudo docker service rm ${REPLY};
        sudo docker container prune --force;
    fi
    if [ $option == 4 ]; then sudo docker swarm join-token manager; fi
    if [ $option == 5 ]; then sudo docker swarm join-token worker; fi
    if [ $option == 6 ]; then 
        read -p "Type the Join token command: "
        ${REPLY} 
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
    if [ $option == 103 ]; then sudo docker swarm leave --force; fi
}

showMenu=1
while [ $showMenu == 1 ];
do
    menu;
    read -p "press any key to continue..."  
done
