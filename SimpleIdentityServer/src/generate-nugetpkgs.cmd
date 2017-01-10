set revisionSuffix=%1
echo %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Proxy --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Uma.Client --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.UmaManager.Client --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Client --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Core.Common --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Core.Jwt --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.UmaIntrospection.Authentication --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Uma.Authorization --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Uma.Common --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Authentication.Common --version-suffix %revisionSuffix%
dotnet pack --output feed System.Security.Cryptography.Algorithms.Extensions --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Oauth2Instrospection.Authentication --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.UserInformation.Authentication --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Core --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.Logging --version-suffix %revisionSuffix%
dotnet pack --output feed SimpleIdentityServer.DataAccess.SqlServer --version-suffix %revisionSuffix%