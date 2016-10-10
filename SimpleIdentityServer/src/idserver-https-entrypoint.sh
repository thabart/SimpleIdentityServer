#!/bin/sh

./wait-for-it.sh $DB_ALIAS:$DB_PORT
cd IdentityServer4.Startup
dotnet run --server.urls=https://*:5443