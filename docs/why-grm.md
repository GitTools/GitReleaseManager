# Why would I want to use GitReleaseManager?

There are a number of reasons that you would want to incorporate GitReleaseManager into your workflow.

<div class="admonition note">
    <p class="first admonition-title">Note</p>
    <p class="last">
        GitReleaseManager works best when included within an automated Build Process, using something like psake, or Octopus Deploy, etc.  However, it can also be used as a standalone tool, running directly at the command line.
    </p>
</div>

Here are a few examples:

## Create Milestone Release Notes

Assuming that you are already using the concept of milestones in GitHub, once a milestone is completed, and you have a number of closed issues, you can use the [create command](commands/create.md) to generate a draft set of release notes, which includes all the closed issues (assuming that they match the [set of labels](configuration/include-issues.md) which are configured to be included.

## Publish Release

Even if you don't want to use GitReleaseManager to create the release notes on GitHub, you may still want the ability to Publish a Release (which has the result of creating a tag in your repository).  This can be done using the [publish command](commands/publish.md).

<div class="admonition note">
    <p class="first admonition-title">Note</p>
    <p class="last">
        Publishing a Release also closes the associated Milestone for the Release.
    </p>
</div>

## Add Asset to Release

As part of your Release process, you may want to include assets into the GitHub Release.  This could be the final MSI package for your application, or a nuget package.  GitReleaseManager allows you to do this in two ways.  The first is using the [create command](commands/create.md), which includes the ability to add an asset at the time of Release creation.  However, at the time of Release creation, you might not have all the assets that you want to add.  As a result, there is a separate [add asset](commands/add-assets.md) command that you can use to add an asset to an existing Release.

## Close Milestone

When working directly in GitHub, publishing a Release doesn't close the associated milestone.  When using the GitReleaseManager's [publish command](commands/publish.md) the associated milestone is also closed.  However, you can also use the [close command](commands/doc:close] directly if you are not using the publish workflow as part of your process.

## Export Release Notes

When working on a project, you might want to include all the Release Notes within the application itself, let's say in the About dialog.  GitReleaseManager's [export command](commands/export.md) makes it really simple to export the entire history of your application/product, by creating a markdown file with all the information contained in the releases section of GitHub.