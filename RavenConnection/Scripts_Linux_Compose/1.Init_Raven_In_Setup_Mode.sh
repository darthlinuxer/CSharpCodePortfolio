!/bin/bash
docker kill ravenInitial
docker container prune --force
clear

defaultContainerType="Linux"
container_os=$defaultContainerType
read -p "ContainerOS (Windows or Linux) [Default:$defaultContainerType] : "
if [ ! -z ${REPLY} ]; then container_os = ${REPLY}; fi

if [ $container_os == "Windows" ]; then 
    container_image="ravendb/ravendb:5.2-windows-latest";
else 
    container_image="ravendb/ravendb:5.2-ubuntu-latest";
fi

docker run -d -it \
    -p 8080:8080 \
    -p 38888:38888 \
    -e RAVEN_Setup_Mode="Initial" \
    --name ravenInitial \
    $container_image


echo "Wait Container start.."
sleep 10
docker ps -a

echo "Now access http://localhost:8080"
