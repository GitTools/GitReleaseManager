![Icon](https://raw.github.com/GitTools/GitReleaseManager/develop/Icons/package_icon_no_credit.png)

[![License](http://img.shields.io/:license-mit-blue.svg)](http://gep13.mit-license.org)
[![Coverage Status](https://coveralls.io/repos/GitTools/GitReleaseManager/badge.svg?branch=develop)](https://coveralls.io/r/GitTools/GitReleaseManager?branch=develop)
[![Coverity Scan Build Status](https://scan.coverity.com/projects/5110/badge.svg)](https://scan.coverity.com/projects/5110)

Do you detest creating release notes for your software applications hosted on GitHub?  If so, this is the tool for you.

Using a simple set of configuration properties, you can fully automate the creation and export of Release Notes from your GitHub hosted project.

As an example see this [Release](https://github.com/chocolatey/ChocolateyGUI/releases/tag/0.12.0) for [Chocolatey GUI](https://github.com/chocolatey/ChocolateyGUI) which was created using GitReleaseManager.

GitReleaseManager allows you to:

- Create Draft Releases from a milestone
- Attach assets to an existing release
- Close a milestone
- Publish a Draft Release
- Export all Release Notes for a Project

## Installation

You can install GitReleaseManager via Chocolatey by executing:

`choco install gitreleasemanager.portable`

**NOTE:**
Depending on which version of Chocolatey you are using, you may be required to confirm the installation of the application. You can avoid this prompt using the following command:

`choco install gitreleasemanager.portable -y`

If you are interested in trying out the latest pre-release version of GitReleaseManager then you can use the following installation command:

`choco install gitreleasemanager.portable -source https://www.myget.org/F/grm_develop/ -pre`

This uses the public GitReleaseManager feed which is hosted on [MyGet.org](https://www.myget.org/)

## Build Status

AppVeyor
-------------
[![AppVeyor Build status](https://ci.appveyor.com/api/projects/status/hfad7hkscfx4423p/branch/develop?svg=true)](https://ci.appveyor.com/project/GitTools/gitreleasemanager/branch/develop)

## Chat Room

Come join in the conversation about GitReleaseManager in our Gitter Chat Room

[![Join the chat at https://gitter.im/GitTools/GitReleaseManager](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/GitTools/GitReleaseManager?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Contributing

If you would like to contribute code or help squash a bug or two, that's awesome.  Please familiarize yourself with [CONTRIBUTING](https://github.com/GitTools/GitReleaseManager/blob/develop/CONTRIBUTING.md).

## Committers

Committers, you should be very familiar with [COMMITTERS](https://github.com/GitTools/GitReleaseManager/blob/develop/COMMITTERS.md).

## Documentation

The documentation for GitReleaseManager can be found on [ReadTheDocs](http://gitreleasemanager.readthedocs.org/en/develop/).

## Credits

GitReleaseManager is brought to you by quite a few people and frameworks.  See [CREDITS](https://github.com/GitTools/GitReleaseManager/blob/develop/Documentation/Legal/CREDITS.md) for full information.

Full original credit has to go to the people at [Particular Software](http://www.particular.net/), without whom this project would not have been possible.  They originally created the [GitHubReleaseNotes](https://github.com/Particular/GitHubReleaseNotes) project, which GitReleaseManager is based on, and draws a lot of inspiration from.

Where GitHubReleaseNotes uses a set of fixed configuration, based on Particular's internal usage requirements, GitReleaseManager attempts to be fully configurable, so that the end user can decide what should be done when creating and exporting Release Notes on GitHub.  Huge thanks to the people at Particular for their support in helping me create this project.  For more information about what has changed between GitHubReleaseNotes and GitReleaseManager, see this [issue](https://github.com/GitTools/GitReleaseManager/issues/24).

In addition, a large thank you has to go to again [Particular Software](http://www.particular.net/) and the contributors behind the [GitVersion](https://github.com/ParticularLabs/GitVersion) Project.  GitReleaseManager draws on the work done in that project in terms of initializing and using a YAML configuration file to allow setting of configuration properties at run-time.

## Icon

<a href="http://thenounproject.com/term/pull-request/116189/" target="_blank">Pull-request</a> designed by <a href="http://thenounproject.com/richard.slater/" target="_blank">Richard Slater</a> from The Noun Project.
