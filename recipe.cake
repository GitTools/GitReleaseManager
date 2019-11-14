#load nuget:?package=Cake.Recipe&version=1.0.0

Environment.SetVariableNames(githubUserNameVariable: "GITTOOLS_GITHUB_USERNAME",
                            githubPasswordVariable: "GITTOOLS_GITHUB_PASSWORD");

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./Source",
                            title: "GitReleaseManager",
                            repositoryOwner: "GitTools",
                            repositoryName: "GitReleaseManager",
                            appVeyorAccountName: "GitTools",
                            shouldRunGitVersion: true,
                            shouldRunDotNetCorePack: true,
                            shouldDeployGraphDocumentation: false);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
                            dupFinderExcludePattern: new string[] {
                                BuildParameters.RootDirectoryPath + "/Source/GitReleaseManager.Tests/*.cs" },
                            testCoverageFilter: "+[*]* -[xunit.*]* -[Cake.Core]* -[Cake.Testing]* -[*.Tests]* -[Octokit]* -[YamlDotNet]* -[AlphaFS]* -[ApprovalTests]* -[ApprovalUtilities]*",
                            testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
                            testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs");

BuildParameters.Tasks.DotNetCoreBuildTask.Does((context) =>
{
    var buildDir = BuildParameters.Paths.Directories.PublishedApplications;

    var grmExecutable = context.GetFiles(buildDir + "/**/*.exe").First();

    context.Information("Registering Built GRM executable...");
    context.Tools.RegisterFile(grmExecutable);
});

BuildParameters.Tasks.CreateReleaseNotesTask
    .IsDependentOn(BuildParameters.Tasks.DotNetCoreBuildTask); // We need to be sure that the executable exist, and have been registered before using it

Build.RunDotNetCore();
