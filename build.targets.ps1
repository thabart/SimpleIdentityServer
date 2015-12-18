$rootFolder = "SimpleIdentityServer"

$outputXml = "$rootFolder\CodeCoverageResults.xml"
$coverageReportDir = "$rootFolder\CodeCoverage\"

$openCoverExe = "$rootFolder\packages\OpenCover.4.6.166\tools\OpenCover.Console.exe"
$reportGeneratorExe = "$rootFolder\packages\ReportGenerator.2.3.5.0\tools\ReportGenerator.exe"
$nunitConsoleExe = "$rootFolder\packages\NUnit.Console.3.0.1\tools\nunit3-console.exe"
$coverallsExe = "$rootFolder\packages\coveralls.io.1.3.4\tools\coveralls.net.exe"

$utDllPath = "$rootFolder\SimpleIdentityServer.Api.UnitTests\bin\Debug\SimpleIdentityServer.Api.UnitTests.dll"
$targetDll = "$rootFolder\SimpleIdentityServer.Core\bin\Debug\SimpleIdentityServer.Core.dll"

function RunTestWithCoverage($fullTestDllPaths) {    
    $openCoverArguments =  "-target:""$nunitConsoleExe"" -targetargs:""$utDllPath"" -output:$outputXml -register:user"
	$coverallsArguments = "--opencover $outputXml --full sources"
    Start-Process $openCoverExe -ArgumentList $openCoverArguments
}

RunTestWithCoverage "$utDllPath"