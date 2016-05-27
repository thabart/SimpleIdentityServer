dotnet run --framework net46 --project src\SimpleIdentityServer.Startup
dotnet run --framework net46 --server.urls=http://*:5001 --project src\SimpleIdentityServer.Uma.Host
dotnet run --framework net46 --server.urls=http://*:5002 --project src\SimpleIdentityServer.Manager.Host.Startup