REM Create nuget packages
dotnet pack --output feed SimpleIdentityServer.Proxy
dotnet pack --output feed SimpleIdentityServer.Uma.Client
dotnet pack --output feed SimpleIdentityServer.UmaManager.Client
dotnet pack --output feed SimpleIdentityServer.Client
dotnet pack --output feed SimpleIdentityServer.Core.Common
dotnet pack --output feed SimpleIdentityServer.Core.Jwt