REM for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd src/Apis/SimpleIdServer/SimpleIdentityServer.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/EventStore/SimpleIdentityServer.EventStore.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/Uma/SimpleIdentityServer.Uma.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/ResourceManager/SimpleIdentityServer.ResourceManager.API.Host && dotnet run -f net461"
START cmd /k "cd src/Apis/ResourceManager/SimpleIdentityServer.ResourceManager.Host && dotnet run -f net461"
START cmd /k "cd src/Apis/Manager/SimpleIdentityServer.Manager.Host.Startup && dotnet run -f net461"
REM START cmd /k "bash ./src/wait-for-it.sh localhost:5002 -t 300 && cd src/SimpleIdentityServer.Configuration.Startup && dotnet run -f net46 http://localhost:5004"
echo Applications are running ...