defaultContainerSecurity="unsecure"
containerSecurity=$defaultContainerSecurity
read -p "Stop Secure or Unsecure Containers?  [Default:$defaultContainerSecurity] :"
if [ ! -z ${REPLY} ]; then containerSecurity=${REPLY}; fi
if [ $containerSecurity == "secure" ]; then 
    dockercomposefile="docker-compose-secure.yml";
else 
    dockercomposefile="docker-compose-unsecure.yml"
fi

echo docker-compose -f $dockercomposefile down
docker-compose -f $dockercomposefile down
