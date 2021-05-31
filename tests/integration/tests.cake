#load "buildData.cake"
#load "xunit.cake"
#load "utilities.cake"

var target = Argument<string>("target", "Run-All-Tests");

Setup<BuildData>(ctx =>
{
    var buildDir = "../../BuildArtifacts/temp/_PublishedApplications";
    var grmExecutable = ctx.GetFiles(buildDir + "/**/*.exe").First();

	ctx.Information("Registering Built GRM executable...");
	ctx.Tools.RegisterFile(grmExecutable);

    CleanDirectory("./output");

    return new BuildData(ctx);
});

Task("Create-Release")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerCreate(
        data.GitHubToken,
        data.GitHubOwner,
        data.GitHubRepository,
        new GitReleaseManagerCreateSettings {
            Milestone         = data.GrmMilestone,
            TargetCommitish   = "master"
        });
});

Task("Add-Asset-To-Release")
    .Does(() =>
{
  // Without running something like Octokit directly against the draft release,
  // not sure how to verify that the assets get added as expected.
});

Task("Export-Release")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerExport(
        data.GitHubToken,
        data.GitHubOwner,
        data.GitHubRepository,
        "./output/releasenotes.md",
        new GitReleaseManagerExportSettings {
            TagName = data.GrmMilestone
        });

    // Then
    Assert.True(FileExists("./output/releasenotes.md"));
    Assert.True(FileHashEquals("./expected/releasenotes.md", "./output/releasenotes.md"));
});

Task("Close-Milestone")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerClose(
        data.GitHubToken,
        data.GitHubOwner,
        data.GitHubRepository,
        data.GrmMilestone
    );
});

Task("Discard-Release")
    .Does<BuildData>((data) =>
{
    var settings = new GitReleaseManagerCreateSettings();
    settings.Milestone = data.GrmMilestone;

    // TODO: This can be replaced when a discard alias is added to Cake
    settings.ArgumentCustomization = args => {
        var newArgs = new ProcessArgumentBuilder()
                        .Append("discard");

        Array.ForEach(
            args.Skip(1).ToArray(),
            newArgs.Append
            );

        return newArgs;
    };

    GitReleaseManagerCreate(
        data.GitHubToken,
        data.GitHubOwner,
        data.GitHubRepository,
        settings);
});

Task("Open-Milestone")
    .Does<BuildData>((data) =>
{
    var settings = new GitReleaseManagerCloseMilestoneSettings();

    // TODO: This can be replaced when a open alias is added to Cake
    settings.ArgumentCustomization = args => {
        var newArgs = new ProcessArgumentBuilder()
                        .Append("open");

        Array.ForEach(
            args.Skip(1).ToArray(),
            newArgs.Append
            );

        return newArgs;
    };

    GitReleaseManagerClose(
        data.GitHubToken,
        data.GitHubOwner,
        data.GitHubRepository,
        data.GrmMilestone,
        settings);
});

Task("Run-All-Tests")
    .IsDependentOn("Create-Release")
    .IsDependentOn("Add-Asset-To-Release")
    .IsDependentOn("Export-Release")
    .IsDependentOn("Close-Milestone")
    .IsDependentOn("Discard-Release")
    .IsDependentOn("Open-Milestone");

RunTarget(target);

// Things that still need to be tested further
// - Exporting with configuration file for footer, replacements, etc
// - Creating with configuration file for including hashes, etc.

// Aliases which are not currently tested
// - GitReleaseManagerLabel
// - GitReleaseManagerPublish
