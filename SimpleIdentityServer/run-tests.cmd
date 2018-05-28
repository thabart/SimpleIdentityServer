REM SIMPLEIDSERVER
dotnet test tests\SimpleIdentityServer.Core.Jwt.UnitTests
dotnet test tests\SimpleIdentityServer.Core.UnitTests
dotnet test tests\SimpleIdentityServer.Host.Tests
dotnet test tests\SimpleIdentityServer.Module.Loader.Tests

REM UMA
dotnet test tests\SimpleIdentityServer.Uma.Core.UnitTests
dotnet test tests\SimpleIdentityServer.Uma.Host.Tests

REM SCIM
dotnet test tests\SimpleIdentityServer.Scim.Core.Tests
dotnet test tests\SimpleIdentityServer.Scim.Client.Tests

REM EVENTSTORE
dotnet test tests\SimpleIdentityServer.EventStore.Tests