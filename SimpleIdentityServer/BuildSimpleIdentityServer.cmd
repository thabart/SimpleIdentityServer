call dotnet restore src\SimpleIdentityServer.Core.Common
call dotnet restore src\SimpleIdentityServer.Logging
call dotnet restore src\System.Security.Cryptography.Algorithms.Extensions
call dotnet restore src\SimpleIdentityServer.Core.Jwt
call dotnet restore src\SimpleIdentityServer.Core
call dotnet restore src\SimpleIdentityServer.DataAccess.SqlServer
call dotnet restore src\SimpleIdentityServer.DataAccess.Fake
call dotnet restore src\SimpleIdentityServer.RateLimitation
call dotnet restore src\SimpleIdentityServer.Host
call dotnet restore src\SimpleIdentityServer.Startup

call dotnet build src\SimpleIdentityServer.Core.Common
call dotnet build src\SimpleIdentityServer.Logging
call dotnet build src\System.Security.Cryptography.Algorithms.Extensions
call dotnet build src\SimpleIdentityServer.Core.Jwt
call dotnet build src\SimpleIdentityServer.Core
call dotnet build src\SimpleIdentityServer.DataAccess.SqlServer
call dotnet build src\SimpleIdentityServer.DataAccess.Fake
call dotnet build src\SimpleIdentityServer.RateLimitation
call dotnet build src\SimpleIdentityServer.Host
call dotnet build src\SimpleIdentityServer.Startup

