set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd tests/SimpleBus.RabbitMq.Tests && dotnet run -f net461"
echo Applications are running ...