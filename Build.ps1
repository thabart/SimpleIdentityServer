$revision = $env:APPVEYOR_REPO_TAG_NAME
$splitted = $revision.Split(".")
$revisionSuffix = $splitted[-1]
cd SimpleIdentityServer\src
dotnet restore
./generate-nugetpkgs.cmd $revisionSuffix