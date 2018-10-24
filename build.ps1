param
(
    $config = 'Release'
)

$hostSln = Resolve-Path .\SimpleIdentityServer\SimpleIdentityServer.Host.sln
$umaSln = Resolve-Path .\SimpleIdentityServer\SimpleIdentityServer.Uma.sln
$scimSln = Resolve-Path .\SimpleIdentityServer\SimpleIdentityServer.Scim.sln
$busSln = Resolve-Path .\SimpleIdentityServer\SimpleBus.sln

dotnet build $hostSln -c $config
dotnet build $umaSln -c $config
dotnet build $scimSln -c $config
dotnet build $busSln -$config