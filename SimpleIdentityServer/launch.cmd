REM for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd src/Apis/SimpleIdServer/SimpleIdentityServer.Startup && dotnet run -f net46 https://localhost:5443"
REM START cmd /k "cd src/SimpleIdentityServer.EventStore.Host && dotnet run -f net46 http://localhost:5301"
START cmd /k "bash ./src/wait-for-it.sh localhost:5443 -t 300 && cd src/Apis/Uma/SimpleIdentityServer.Uma.Startup && dotnet run -f net46 https://localhost:5445"
REM START cmd /k "bash ./src/wait-for-it.sh localhost:5445 -t 300 && cd src/SimpleIdentityServer.Manager.Host.Startup && dotnet run -f net46 http://localhost:5002"
REM START cmd /k "bash ./src/wait-for-it.sh localhost:5002 -t 300 && cd src/SimpleIdentityServer.Configuration.Startup && dotnet run -f net46 http://localhost:5004"
echo Applications are running ...