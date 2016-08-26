#!/bin/sh

./wait-for-it.sh $DB_ALIAS:$DB_PORT && dotnet run --project SimpleIdentityServer.Configuration.Startup/project.json --server.urls=http://*:5004