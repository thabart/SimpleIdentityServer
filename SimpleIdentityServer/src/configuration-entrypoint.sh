#!/bin/sh

./wait-for-it.sh $DB_ALIAS:$DB_PORT && bash -c "cd SimpleIdentityServer.Configuration.EF && dotnet ef database update" && dotnet run --project SimpleIdentityServer.Configuration.Startup/project.json --server.urls=http://*:5004