REM for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd src/SimpleIdentityServer.Startup && dotnet run -f net46 https://*:5443"
START cmd /k "cd src/SimpleIdentityServer.EventStore.Host && dotnet run -f net46 http://*:5301"
START cmd /k "bash ./src/wait-for-it.sh localhost:5443 -t 300 && cd src/SimpleIdentityServer.Uma.Host && dotnet run -f net46 https://*:5445"
START cmd /k "bash ./src/wait-for-it.sh localhost:5445 -t 300 && cd src/SimpleIdentityServer.Manager.Host.Startup && dotnet run -f net46 http://*:5002"
START cmd /k "bash ./src/wait-for-it.sh localhost:5002 -t 300 && cd src/SimpleIdentityServer.Configuration.Startup && dotnet run -f net46 http://*:5004"
echo Applications are running ...