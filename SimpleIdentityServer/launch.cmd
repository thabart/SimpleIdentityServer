set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd src/Apis/SimpleIdServer/SimpleIdentityServer.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/EventStore/SimpleIdentityServer.EventStore.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/Uma/SimpleIdentityServer.Uma.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/Scim/SimpleIdentityServer.Scim.Startup && dotnet run -f net461"
echo Applications are running ...