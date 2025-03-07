#load nuget:?package=Cake.Recipe&version=3.1.1
#tool dotnet:?package=dotnet-t4&version=2.2.1
#addin nuget:?package=Cake.Git&version=1.0.0

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
                            shouldRunCodecov: false);

BuildParameters.PackageSources.Add(new PackageSourceData(Context, "GPR", "https://nuget.pkg.github.com/GitTools/index.json", FeedType.NuGet, false));

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
                            testCoverageFilter: "+[GitReleaseManager*]* -[GitReleaseManager.Core.Tests*]* -[GitReleaseManager.Tests*]*",
                            testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
                            testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs",
                            kuduSyncIgnore: ".git;CNAME;_git2");

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