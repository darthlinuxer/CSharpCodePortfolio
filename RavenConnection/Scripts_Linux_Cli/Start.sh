clear

function StartRaven
{
    mode=$1; port=$2; tcpPort=$3; node=$4
    docker kill "raven$node"
    docker container prune --force
    clear
    docker run -it -d --name raven$node \
            -e RAVEN_Setup_Mode=$mode \
            -e RAVEN_License_Eula_Accepted=true \
            -e RAVEN_ServerUrl=http://raven$node:8080 \
            -p $port:8080  \
            ravendb/ravendb:5.2-ubuntu-latest
    docker ps -a
}

function menu
{
    clear
    echo "0. Exit"
    echo "1. Start RavenDb"
    echo "100. Docker logs"
    echo "101. Docker ps -a"
    echo "102. Docker exec <cmd>"
    read -p "Choose an option:"
    if [ -z ${REPLY} ]; then showMenu=1 ; fi
    if [ ${REPLY} == 0 ]; then showMenu=0; fi
    if [ ${REPLY} == 1 ]; then 
        read -p "Choose the node name: "
        nodeName=${REPLY}
        read -p "Choose the Port: "
        port=${REPLY}
        StartRaven "None" $port 30000 $nodeName; 
    fi  
    if [ ${REPLY} == 100 ]; then
        read -p "Which Node ? " 
        docker logs raven${REPLY}; 
    fi
    if [ ${REPLY} == 101 ]; then docker ps -a; fi
    if [ ${REPLY} == 102 ]; then
        read -p "command: " 
        docker exec raven$node ${REPLY};
    fi 
}

showMenu=1
while [ $showMenu == 1 ];
do
    menu;
    read -p "press any key to continue..."  
done
