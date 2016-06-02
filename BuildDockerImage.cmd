# clean volumes
docker volume rm $(docker volume ls -qf dangling=true)
docker-machine regenerate-certs dev

SET DOCKER_TLS_VERIFY=1
SET DOCKER_HOST=tcp://192.168.56.102:2376
SET DOCKER_CERT_PATH=C:\Users\habar\.docker\machine\machines\dev
SET DOCKER_MACHINE_NAME=dev

docker-machine start dev
docker build -t simpleidserver -f Dockerfile-Authorization .
docker run -t -d -p 5000:5000 simpleidserver
docker run -t -i simpleidserver /bin/bash

# Run postgresql
docker run --name postgresql -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=password -e POSTGRES_DB=postgres -d postgres

# Run simpleidserver
docker run --name simpleidserver -e DB_ALIAS=postgresql -e DB_PORT=5432 --link postgresql -it simpleidserver /bin/bash