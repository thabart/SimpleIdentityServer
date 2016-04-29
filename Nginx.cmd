# Attach bash to running container
docker exec -i -t docker-nginx bash

# Install Nginx
docker run --name docker-nginx -p 80:80 -d nginx

# Run nginx-proxy
docker run -d -p 80:80 -v /var/run/docker.sock:/tmp/docker.sock -t jwilder/nginx-proxy

# Configure the proxy with something like
*.identityserver.com

# Run the container
docker run -e VIRTUAL_HOST=idserver.localhost -e HOST=http://localhost -t -d identitycontrib/identityserver
