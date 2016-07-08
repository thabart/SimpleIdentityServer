REM Create nuget packages
dotnet pack --output feed SimpleIdentityServer.Proxy
dotnet pack --output feed SimpleIdentityServer.Uma.Client
dotnet pack --output feed SimpleIdentityServer.UmaManager.Client
dotnet pack --output feed SimpleIdentityServer.Client
dotnet pack --output feed SimpleIdentityServer.Core.Common
dotnet pack --output feed SimpleIdentityServer.Core.Jwt
dotnet pack --output feed SimpleIdentityServer.UmaIntrospection.Authentication
dotnet pack --output feed SimpleIdentityServer.Uma.Authorization
dotnet pack --output feed SimpleIdentityServer.Uma.Common
dotnet pack --output feed SimpleIdentityServer.Authentication.Common
dotnet pack --output feed System.Security.Cryptography.Algorithms.Extensions
dotnet pack --output feed SimpleIdentityServer.Oauth2Instrospection.Authentication
dotnet pack --output feed SimpleIdentityServer.UserInformation.Authentication