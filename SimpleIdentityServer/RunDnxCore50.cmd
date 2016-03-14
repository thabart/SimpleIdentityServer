call dnvm use 1.0.0-rc1-update1 -r coreclr

call dnu build --framework dnxcore50 src\SimpleIdentityServer.Core.Common
call dnu build --framework dnxcore50 src\SimpleIdentityServer.Logging
call dnu build --framework dnxcore50 src\System.Security.Cryptography.Algorithms.Extensions
call dnu build --framework dnxcore50 src\SimpleIdentityServer.Core.Jwt
call dnu build --framework dnxcore50 src\SimpleIdentityServer.Core

call dnu restore tests\SimpleIdentityServer.Core.Jwt.UnitTests
call dnu restore tests\SimpleIdentityServer.Core.UnitTests

call dnx -p tests\SimpleIdentityServer.Core.Jwt.UnitTests test
call dnx -p tests\SimpleIdentityServer.Core.UnitTests test