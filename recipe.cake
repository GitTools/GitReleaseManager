#load nuget:?package=Cake.Recipe&version=3.1.1
#tool dotnet:?package=dotnet-t4&version=2.2.1
#addin nuget:?package=Cake.Git&version=1.0.0
#addin nuget:?package=Cake.Twitter&version=1.1.1

Environment.SetVariableNames(githubTokenVariable: "GITTOOLS_GITHUB_TOKEN");

var standardNotificationMessage = "A new version, {0} of {1} has just been released.  Get it from Chocolatey, NuGet, or as a .Net Global Tool.";

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "GitReleaseManager",
                            repositoryOwner: "GitTools",
                            repositoryName: "GitReleaseManager",
                            appVeyorAccountName: "GitTools",
                            shouldRunDotNetCorePack: true,
                            shouldRunIntegrationTests: true,
                            integrationTestScriptPath: "./tests/integration/tests.cake",
                            twitterMessage: standardNotificationMessage,
                            preferredBuildProviderType: BuildProviderType.GitHubActions,
                            gitterMessage: "@/all " + standardNotificationMessage,
                            shouldRunCodecov: false,
                            shouldGenerateDocumentation: false);

BuildParameters.PackageSources.Add(new PackageSourceData(Context, "GPR", "https://nuget.pkg.github.com/GitTools/index.json", FeedType.NuGet, false));

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
                            testCoverageFilter: "+[GitReleaseManager*]* -[GitReleaseManager.Core.Tests*]* -[GitReleaseManager.Tests*]*",
                            testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
                            testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs",
                            kuduSyncIgnore: ".git;CNAME;_git2");

// The recipe's legacy project parser does not recognize centrally managed
// coverlet.msbuild references, so it falls back to the .NET Framework profiler.
// Run the existing tests through Coverlet explicitly instead.
var dotNetCoreTestTask = (CakeTask)BuildParameters.Tasks.DotNetCoreTestTask.Task;
dotNetCoreTestTask.Actions.Clear();
dotNetCoreTestTask.DelayedActions.Clear();
dotNetCoreTestTask.Dependencies.RemoveAll(dependency => dependency.Name == "Install-OpenCover");
BuildParameters.Tasks.DotNetCoreTestTask.Does<DotNetCoreMSBuildSettings>((context, msBuildSettings) =>
{
    var projects = GetFiles(BuildParameters.TestDirectoryPath + (BuildParameters.TestFilePattern ?? "/**/*Tests.csproj"));
    var coverageDirectory = BuildParameters.Paths.Directories.TestCoverage.Combine("coverlet");
    EnsureDirectoryExists(coverageDirectory);

    var coverletSettings = new CoverletSettings
    {
        CollectCoverage = true,
        CoverletOutputDirectory = coverageDirectory,
        CoverletOutputFormat = CoverletOutputFormat.opencover,
        ExcludeByFile = ToolSettings.TestCoverageExcludeByFile.Split(';').ToList(),
        ExcludeByAttribute = ToolSettings.TestCoverageExcludeByAttribute.Split(';').ToList()
    };

    foreach (var filter in ToolSettings.TestCoverageFilter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
    {
        if (filter[0] == '+')
        {
            coverletSettings.WithInclusion(filter.Substring(1));
        }
        else if (filter[0] == '-')
        {
            coverletSettings.WithFilter(filter.Substring(1));
        }
    }

    var visitedSequencePoints = 0;
    foreach (var project in projects)
    {
        var projectXml = System.Xml.Linq.XDocument.Load(project.FullPath);
        var frameworks = projectXml.Descendants("TargetFrameworks")
            .Concat(projectXml.Descendants("TargetFramework"))
            .SelectMany(element => element.Value.Split(';'))
            .Where(framework => !string.IsNullOrWhiteSpace(framework))
            .Distinct()
            .ToArray();
        if (frameworks.Length == 0)
        {
            throw new Exception("No target framework found for " + project.FullPath);
        }

        foreach (var framework in frameworks)
        {
            coverletSettings.CoverletOutputName = project.GetFilenameWithoutExtension().FullPath.Replace('.', '-') + "-" + framework.Replace('.', '-');
            var reportPattern = coverageDirectory + "/" + coverletSettings.CoverletOutputName + "*.xml";
            DeleteFiles(GetFiles(reportPattern));
            DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings
            {
                Configuration = BuildParameters.Configuration,
                Framework = framework,
                NoBuild = true,
                ArgumentCustomization = args =>
                {
                    args.AppendMSBuildSettings(msBuildSettings, context.Environment);
                    return args.Append("/p:UseSourceLink=true");
                }
            }, coverletSettings);

            var reports = GetFiles(reportPattern).ToArray();
            if (reports.Length != 1)
            {
                throw new Exception("Expected one fresh coverage report for " + project.FullPath + " (" + framework + ").");
            }
            var summary = System.Xml.Linq.XDocument.Load(reports[0].FullPath).Root.Element("Summary");
            visitedSequencePoints += (int)summary.Attribute("visitedSequencePoints");
        }
    }
    if (visitedSequencePoints == 0)
    {
        throw new Exception("The test run produced no covered sequence points.");
    }
});

BuildParameters.Tasks.DotNetCoreBuildTask.Does((context) =>
{
    var buildDir = BuildParameters.Paths.Directories.PublishedApplications;

    var grmExecutable = context.GetFiles(buildDir + "/GitReleaseManager.Tool/**/*.exe").First();

    context.Information("Registering Built GRM executable... {0}", grmExecutable.FullPath);
    context.Tools.RegisterFile(grmExecutable);
});

BuildParameters.Tasks.CreateReleaseNotesTask
    .IsDependentOn(BuildParameters.Tasks.DotNetCoreBuildTask); // We need to be sure that the executable exist, and have been registered before using it

Task("Transform-TextTemplates")
    .IsDependeeOf(BuildParameters.Tasks.DotNetCoreBuildTask.Task.Name)
    .Does(() =>
{
    var templates = GetFiles("src/**/*.tt");

    foreach (var template in templates)
    {
        TransformTemplate(template);
    }
});

((CakeTask)BuildParameters.Tasks.ExportReleaseNotesTask.Task).ErrorHandler = null;
((CakeTask)BuildParameters.Tasks.PublishGitHubReleaseTask.Task).ErrorHandler = null;
BuildParameters.Tasks.PublishPreReleasePackagesTask.IsDependentOn(BuildParameters.Tasks.PublishGitHubReleaseTask);
BuildParameters.Tasks.PublishReleasePackagesTask.IsDependentOn(BuildParameters.Tasks.PublishGitHubReleaseTask);

Build.RunDotNetCore();