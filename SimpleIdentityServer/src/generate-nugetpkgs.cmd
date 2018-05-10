set packageVersion=%1
echo %packageVersion%

REM UMA
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Client /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Common /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.EF /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.EF.InMemory /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.EF.Postgre /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.EF.Sqlite /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.EF.SqlServer /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Host /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Uma\SimpleIdentityServer.Uma.Store.Redis /p:PackageVersion=%packageVersion%

REM SIMPLEIDSERVER
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Authenticate.Basic /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Client /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Core.Common /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Core.Jwt /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.EF /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.EF.InMemory /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.EF.Postgre /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.EF.Sqlite /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.EF.SqlServer /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.EventStore.Handler /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Handler /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Host /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Logging /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.OAuth2Introspection /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.Store.Redis /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.TwoFactorAuthentication.Email /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.TwoFactorAuthentication.Twilio /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\SimpleIdServer\SimpleIdentityServer.UserInfoIntrospection /p:PackageVersion=%packageVersion%

REM SCIM
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Client /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Common /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Db.EF /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Db.EF.InMemory /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Db.EF.Postgre /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Db.EF.Sqlite /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Db.EF.SqlServer /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.EventStore.Handler /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Handler /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\Scim\SimpleIdentityServer.Scim.Host /p:PackageVersion=%packageVersion%

REM EVENT STORE
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.EF /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.Host /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.InMemory /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.Postgre /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.Sqlite /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Apis\EventStore\SimpleIdentityServer.EventStore.SqlServer /p:PackageVersion=%packageVersion%

REM LIB
dotnet pack --output ..\..\feed Lib\System.Security.Cryptography.Algorithms.Extensions /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\feed Lib\WsFederation /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\Serilog\Serilog.Sinks.ElasticSearch /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\Serilog\Serilog.Sinks.RabbitMQ /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Concurrency /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Storage /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Storage.InMemory /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\WebApiContrib\WebApiContrib.Core.Storage.Redis /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\Bus\SimpleBus.Core /p:PackageVersion=%packageVersion%
dotnet pack --output ..\..\..\feed Lib\Bus\SimpleBus.InMemory /p:PackageVersion=%packageVersion%