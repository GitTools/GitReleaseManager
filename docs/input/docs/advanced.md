---
Order: 20
Title: Advanced Workflow
---

# Advanced Workflow

In order to best understand the use case for GitReleaseManager let's take a look at an example workflow currently in use by the ChocolateyGUI project.

:::{.alert .alert-info}
ChocolateyGUI uses a number of concepts, for example, GitFlow, psake build script engine, AppVeyor Continuous Integration, as well as using GitReleaseManager.  In order to understand the usage of GitReleaseManager for this project, you sort of have to understand the entire process.  As a result, this walk-through steps you through the entire end to end process, which as a result, means it is quite lengthy.
:::

# ChocolateyGUI
[ChocolateyGUI](https://github.com/chocolatey/ChocolateyGUI) is an open source project, hosted on GitHub that makes use of GitReleaseManager to create and export it's release notes.  Before we can get into how GitReleaseManager is used, we need to take a look at how ChocolateyGUI is setup.

## GitHub Setup
  * All Issues are tracked using the[ GitHub Issues List](https://github.com/chocolatey/ChocolateyGUI/issues)
  * Each Issue is assigned to a [project milestone](https://github.com/chocolatey/ChocolateyGUI/milestones)
  * Each Issue is appropriately tagged using one of the [pre-defined labels](https://github.com/chocolatey/ChocolateyGUI/labels)

## GitFlow
  * ChocolateyGUI uses the [GitFlow Branching Model](http://nvie.com/posts/a-successful-git-branching-model/)
  * [GitVersion](https://github.com/ParticularLabs/GitVersion) is used to determine the current version number, based on the current state of the repository, i.e. what branch is being worked on, and what tags have been assigned, and how many commits have been made to the repository

## Build Artifacts
Every build of ChocolateyGUI generates a number of Build Artifacts, these include:
  * An MSI package for installing ChocolateyGUI
  * A Chocolatey package to ease the installation of ChocolateyGUI

## Continuous Integration

  * ChocolateyGUI uses [AppVeyor](http://www.appveyor.com/) as it's Continuous Integration Server.
  * Any time a **Pull Request** is created, an AppVeyor build is triggered, but no deployment of the build artifacts takes place
  * Any time a commit is made into the **develop **branch, an AppVeyor build is triggered, and the build artifacts are deployed to the [MyGet Develop Feed](https://www.myget.org/feed/Packages/ghrm_develop)
  * Any time a commit is made into the **master **branch, an AppVeyor build is triggered, and the build artifacts are deployed to the [MyGet Master Feed](https://www.myget.org/feed/Packages/ghrm_master)
  * Any time a **tag **is applied to the repository, an AppVeyor build is triggered, and the build artifacts are deployed to [Chocolatey.org](https://chocolatey.org/) for public consumption.

## Ok, so where does GitReleaseManager come into play?

The role of GitReleaseManager really comes into play when moving between a **release** or **hotfix** branch and the **master **branch.

Let's say you have done a bunch of work on the develop branch, you want to move all of that work into the master branch, via a release branch.  When you do this, you are effectively saying that the milestone that you were working on is almost ready for release.  It is at this point that you should really know, via the issues list, everything that is being included in the release, and now would seem like a great time to create some release notes.  And this is exactly what the build process for ChocolateyGUI does.  Let's break this down further...

  * A **release **branch is created from **develop **branch
  * The release branch is merged into master branch, triggering a build (with deployment to MyGet Master Feed).
  * During this build, GitReleaseManager is executed, using the **create** command, and the version number which was provided by GitVersion, to create a set of release notes for this milestone - [source](https://github.com/chocolatey/ChocolateyGUI/blob/09b78495ebc9d334fedf351b021fd7e215c5cf87/BuildScripts/default.ps1#L687).
  * This set of release notes is created in draft format, ready for review, in the GitHub UI.
  * The build artifacts which have been deployed to MyGet Master Feed are tested
  * The release notes are reviewed, and ensured to be correct
  * Assuming that everything is verified to be correct, the draft release is then published through the GitHub UI, which creates a tag in the repository, triggering another AppVeyor build, this time with deployment to Chocolatey.org
  * During this build, GitReleaseManager is executed using the **export** command, so that all release notes can be bundled into the application - [source](https://github.com/chocolatey/ChocolateyGUI/blob/09b78495ebc9d334fedf351b021fd7e215c5cf87/BuildScripts/default.ps1#L707)
  * In addition, GitReleaseManager is executed using the **addasset** command to add the build artifacts to the GitHub release - [source](https://github.com/chocolatey/ChocolateyGUI/blob/09b78495ebc9d334fedf351b021fd7e215c5cf87/BuildScripts/default.ps1#L731)
  * And finally, GitReleaseManager is executed using the **close** command to close the milestone associated with the release that has just been published - [source](https://github.com/chocolatey/ChocolateyGUI/blob/09b78495ebc9d334fedf351b021fd7e215c5cf87/BuildScripts/default.ps1#L753)

The end result of this process can be seen [here](https://github.com/chocolatey/ChocolateyGUI/releases/tag/0.12.0).  This release included 218 commits, and 75 issues.  Personally, I simply wouldn't have created such a comprehensive set of release notes manually.  Instead, I would have written something like...

```
#0.12.0

This release included lots of bug fixes, and tonnes of new features.  Enjoy!
```

By leveraging GitVersion, and GitReleaseManager, and a small amount of process (which you are likely already doing), I hope you will agree that you can very easily create a comprehensive set of release notes for your application.