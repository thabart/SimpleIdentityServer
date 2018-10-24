set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd samples/SimpleIdentityServer.Openid.Server && dotnet run -f net461"
START cmd /k "cd samples/SimpleIdentityServer.Protected.Api && dotnet run -f net461"
echo Applications are running ...