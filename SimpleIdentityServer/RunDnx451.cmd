call dnvm use 1.0.0-rc1-update1 -r clr

call dnu build --framework dnx451 src\SimpleIdentityServer.Core.Common
call dnu build --framework dnx451 src\SimpleIdentityServer.Logging
call dnu build --framework dnx451 src\SimpleIdentityServer.Core.Jwt
call dnu build --framework dnx451 src\SimpleIdentityServer.Core

call dnu restore tests\SimpleIdentityServer.Core.Jwt.UnitTests
call dnu restore tests\SimpleIdentityServer.Core.UnitTests
call dnu restore tests\SimpleIdentityServer.Api.Tests

call dnx -p tests\SimpleIdentityServer.Core.Jwt.UnitTests test
call dnx -p tests\SimpleIdentityServer.Core.UnitTests test
call dnx -p tests\SimpleIdentityServer.Api.Tests test