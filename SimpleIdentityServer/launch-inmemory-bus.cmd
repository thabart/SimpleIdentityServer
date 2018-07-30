set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd tests/SimpleBus.InMemory.Tests && dotnet run -f net461"
START cmd /k "cd src/Lib/Bus/SimpleBus.Signalr && dotnet run -f net461"
echo Applications are running ...