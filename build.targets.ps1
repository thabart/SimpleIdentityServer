$rootFolder = "SimpleIdentityServer"

$outputXml = "$rootFolder\CodeCoverageResults.xml"
$coverageReportDir = "$rootFolder\CodeCoverage\"

$openCoverExe = "$rootFolder\packages\OpenCover.4.6.166\tools\OpenCover.Console.exe"
$reportGeneratorExe = "$rootFolder\packages\ReportGenerator.2.3.5.0\tools\ReportGenerator.exe"
$nunitConsoleExe = "$rootFolder\packages\NUnit.Console.3.0.1\tools\nunit3-console.exe"

$utDllPath = "$rootFolder\SimpleIdentityServer.Api.UnitTests\bin\Debug\SimpleIdentityServer.Api.UnitTests.dll"
$targetDll = "$rootFolder\SimpleIdentityServer.Core\bin\Debug\SimpleIdentityServer.Core.dll"

function RunTestWithCoverage($fullTestDllPaths) {    
    $arguments =  "-target:""$nunitConsoleExe"" -targetargs:""$utDllPath"" -output:$outputXml -register:user"
    Start-Process $openCoverExe -ArgumentList $arguments
}

RunTestWithCoverage "$utDllPath"