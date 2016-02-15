CD SimpleIdentityServer

call powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"

call dnvm upgrade
call dnu restore
call dnu build src\SimpleIdentityServer.Host
call dnu build src\SimpleIdentityServer.Core
call dnu build src\SimpleIdentityServer.Core.Jwt
call dnu build src\SimpleIdentityServer.DataAccess.Fake
call dnu build src\SimpleIdentityServer.DataAccess.SqlServer
call dnu build src\SimpleIdentityServer.RateLimitation
call dnu build src\SimpleIdentityServer.Core.Common
call dnu build src\SimpleIdentityServer.Common

call dnu pack src\SimpleIdentityServer.Common --out nuget\
call dnu pack src\SimpleIdentityServer.Core.Common --out nuget\
call dnu pack src\SimpleIdentityServer.RateLimitation --out nuget\
call dnu pack src\SimpleIdentityServer.DataAccess.SqlServer --out nuget\
call dnu pack src\SimpleIdentityServer.DataAccess.Fake --out nuget\
call dnu pack src\SimpleIdentityServer.Core.Jwt --out nuget\
call dnu pack src\SimpleIdentityServer.Core --out nuget\
call dnu pack src\SimpleIdentityServer.Host --out nuget\