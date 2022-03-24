# this will delete docker orderapi image and remove all volumes
docker-compose down -v
docker rmi testapi:dev
docker image prune -f
docker container prune -f
docker volume prune -f
