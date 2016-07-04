#!/bin/sh

./wait-for-it.sh $DB_ALIAS:$DB_PORT
dotnet run --project SimpleIdentityServer.Startup/project.json --server.urls=https://*:5443