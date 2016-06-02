# Build the docker image
docker build -t simpleidserver -f Dockerfile-Authorization .
docker run -t -d -p 5000:5000 simpleidserver
docker run -t -i simpleidserver /bin/bash

# Run postgresql
docker run --name postgresql -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=password -e POSTGRES_DB=postgres -d postgres

# Run simpleidserver
docker run --name simpleidserver -e DB_ALIAS=postgresql -e DB_PORT=5432 --link postgresql -it simpleidserver /bin/bash