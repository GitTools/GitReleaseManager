---
Order: 20
Title: Example of Create Configuration
---

When creating a release, it is possible to control the look and feel of the
release notes, using settings within the GitReleaseManager.yaml file.

Out of the box, GitReleaseManager creates a simple list of Issues included
within a milestone, split into the labels that have been configured.  However,
it is possible to include additional information in the form of a footer, which
provides additional information, for example, where an installation of the
release can be located.

Take for example the GitReleaseManager.yaml file which is used by the
[Chocolatey GUI](https://github.com/chocolatey/ChocolateyGUI) project:

```yaml
create:
  include-footer: true
  footer-heading: Where to get it
  footer-content: You can download this release from [chocolatey.org](https://chocolatey.org/packages/chocolateyGUI/{milestone})
  footer-includes-milestone: true
  milestone-replace-text: '{milestone}'
```

This would result in the following
[release notes](https://github.com/chocolatey/ChocolateyGUI/releases/tag/0.13.1)
being generated:

![Example Release Notes](../images/example-release-notes.png)

:::{.alert .alert-info}
The generated URL for the link to Chocolatey.org includes the milestone number.
The complete URL is https://chocolatey.org/packages/chocolateyGUI/0.13.1.  This
was achieved by using a regular expression replacement of the
[footer-content](default-configuration), using the
[milestone-replace-text](default-configuration) property as the text to replace
with the actual milestone.

This approach can be used for any project, this example simply shows what is
done in the Chocolatey GUI project.
:::
