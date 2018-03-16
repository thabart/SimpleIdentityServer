set packageVersion=%1
echo %packageVersion%

REM UMA
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Client /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Common /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.EF /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Store.Redis /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Host /p:PackageVersion=%packageVersion%

REM SIMPLEIDSERVER
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Client /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Core.Common /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Core.Jwt /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Authentication.Common /p:PackageVersion=%packageVersion%
REM dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Oauth2Instrospection.Authentication /p:PackageVersion=%packageVersion%
REM dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.UserInformation.Authentication /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Logging /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.DataAccess.SqlServer /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Store.Redis /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Host /p:PackageVersion=%packageVersion%

REM SCIM
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Client /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Common /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Db.EF /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Host /p:PackageVersion=%packageVersion%

REM EVENT STORE
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.EF /p:PackageVersion=%packageVersion%

REM CONFIGURATION
dotnet pack --output ..\..\..\feed Apis\Configuration\SimpleIdentityServer.Configuration.Client /p:PackageVersion=%packageVersion%

REM LIB
dotnet pack --output ..\..\feed Lib\System.Security.Cryptography.Algorithms.Extensions /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\feed Lib\WsFederation /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\Serilog\Serilog.Sinks.ElasticSearch /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\Serilog\Serilog.Sinks.RabbitMQ /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Concurrency /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Storage /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Storage.InMemory /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Storage.Redis /p:PackageVersion=%packageVersion%
REM dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Storage.SqlServer /p:PackageVersion=%packageVersion%