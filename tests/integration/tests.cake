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

Task("Create-Release-With-Username-Password")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerCreate(
        data.GitHubUsername,
        data.GitHubPassword,
        data.GitHubOwner,
        data.GitHubRepository,
        new GitReleaseManagerCreateSettings {
            Milestone         = data.GrmMilestone,
            TargetCommitish   = "master"
        });
});

Task("Create-Release-With-Token")
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

Task("Add-Asset-To-Release-With-Username-Password")
    .Does(() =>
{
  // Without running something like Octokit directly against the draft release,
  // not sure how to verify that the assets get added as expected.
});

Task("Add-Asset-To-Release-With-Token")
    .Does(() =>
{
  // Without running something like Octokit directly against the draft release,
  // not sure how to verify that the assets get added as expected.
});

Task("Export-Release-With-Username-Password")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerExport(
        data.GitHubToken,
        data.GitHubOwner,
        data.GitHubRepository,
        "./output/releasenotes-with-username-password.md",
        new GitReleaseManagerExportSettings {
            TagName = data.GrmMilestone
        });

    // Then
    Assert.True(FileExists("./output/releasenotes-with-username-password.md"));
    Assert.True(FileHashEquals("./expected/releasenotes-with-username-password.md", "./output/releasenotes-with-username-password.md"));
});

Task("Export-Release-With-Token")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerExport(
        data.GitHubUsername,
        data.GitHubPassword,
        data.GitHubOwner,
        data.GitHubRepository,
        "./output/releasenotes-with-token.md",
        new GitReleaseManagerExportSettings {
            TagName = data.GrmMilestone
        });

    // Then
    Assert.True(FileExists("./output/releasenotes-with-token.md"));
    Assert.True(FileHashEquals("./expected/releasenotes-with-token.md", "./output/releasenotes-with-token.md"));
});

Task("Close-Milestone-With-Username-Password")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerClose(
        data.GitHubUsername,
        data.GitHubPassword,
        data.GitHubOwner,
        data.GitHubRepository,
        data.GrmMilestone
    );
});

Task("Close-Milestone-With-Token")
    .Does<BuildData>((data) =>
{
    GitReleaseManagerClose(
        data.GitHubToken,
        data.GitHubOwner,
        data.GitHubRepository,
        data.GrmMilestone
    );
});

Task("Discard-Release-With-Username-Password")
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
        data.GitHubUsername,
        data.GitHubPassword,
        data.GitHubOwner,
        data.GitHubRepository,
        settings);
});

Task("Discard-Release-With-Token")
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

Task("Open-Milestone-With-Username-Password")
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
        data.GitHubUsername,
        data.GitHubPassword,
        data.GitHubOwner,
        data.GitHubRepository,
        data.GrmMilestone,
        settings);
});

Task("Open-Milestone-With-Token")
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
    .IsDependentOn("Create-Release-With-Username-Password")
    .IsDependentOn("Add-Asset-To-Release-With-Username-Password")
    .IsDependentOn("Export-Release-With-Username-Password")
    .IsDependentOn("Close-Milestone-With-Username-Password")
    .IsDependentOn("Discard-Release-With-Username-Password")
    .IsDependentOn("Open-Milestone-With-Username-Password")
    .IsDependentOn("Create-Release-With-Token")
    .IsDependentOn("Add-Asset-To-Release-With-Token")
    .IsDependentOn("Export-Release-With-Token")
    .IsDependentOn("Close-Milestone-With-Token")
    .IsDependentOn("Discard-Release-With-Token")
    .IsDependentOn("Open-Milestone-With-Token");

RunTarget(target);

// Things that still need to be tested further
// - Exporting with configuration file for footer, replacements, etc
// - Creating with configuration file for including hashes, etc.

// Aliases which are not currently tested
// - GitReleaseManagerLabel
// - GitReleaseManagerPublish
