"Source\packages\OpenCover.4.5.3723\OpenCover.Console.exe" -target:"Source\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe" -targetargs:"/nologo BuildArtifacts\GitHubReleaseManager.Tests.dll /noshadow" -filter:"+[GitHubReleaseManager]GitHubReleaseManager*" -excludebyattribute:"System.CodeDom.Compiler.GeneratedCodeAttribute" -register:user -output:"_CodeCoverageResult.xml"

"Source\packages\ReportGenerator.2.1.3.0\ReportGenerator.exe" "-reports:_CodeCoverageResult.xml" "-targetdir:_CodeCoverageReport"

"Source\packages\coveralls.io.1.2.2\tools\coveralls.net.exe" --opencover _CodeCoverageResult.xml