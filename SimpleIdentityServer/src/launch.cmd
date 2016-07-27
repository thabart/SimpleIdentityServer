for /d /r . %%d in (obj) do @if exist "%%d" rd /s/q "%%d"
START cmd /k "cd SimpleIdentityServer.Startup && dotnet run -f net46 --server.urls=http://*:5000;https://*:5443"
START cmd /k "bash ./wait-for-it.sh localhost:5000 -t 300 && cd SimpleIdentityServer.Uma.Host && dotnet run -f net46 --server.urls=http://*:5001;https://*:5445"
START cmd /k "bash ./wait-for-it.sh localhost:5001 -t 300 && cd SimpleIdentityServer.Manager.Host.Startup && dotnet run -f net46 --server.urls=http://*:5002"
START cmd /k "bash ./wait-for-it.sh localhost:5002 -t 300 && cd SimpleIdentityServer.Configuration.Startup && dotnet run -f net46 --server.urls=http://*:5004"
echo Applications are running ...