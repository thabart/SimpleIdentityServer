CD SimpleIdentityServer\VNEXT\

call powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"

call dnvm upgrade
call dnu restore
dnu build src\SimpleIdentityServer.Host
dnu build src\SimpleIdentityServer.Core
dnu build src\SimpleIdentityServer.Core.Jwt
dnu build src\SimpleIdentityServer.DataAccess.Fake
dnu build src\SimpleIdentityServer.DataAccess.SqlServer
dnu build src\SimpleIdentityServer.RateLimitation
dnu build src\SimpleIdentityServer.Core.Common
dnu build src\SimpleIdentityServer.Common

dnu pack src\SimpleIdentityServer.Common -out nuget\
dnu pack src\SimpleIdentityServer.Core.Common -out nuget\
dnu pack src\SimpleIdentityServer.RateLimitation -out nuget\
dnu pack src\SimpleIdentityServer.DataAccess.SqlServer -out nuget\
dnu pack src\SimpleIdentityServer.DataAccess.Fake -out nuget\
dnu pack src\SimpleIdentityServer.Core.Jwt -out nuget\
dnu pack src\SimpleIdentityServer.Core -out nuget\
dnu pack src\SimpleIdentityServer.Host -out nuget\