---
Order: 10
Title: Default Configuration
---

GitReleaseManager configuration can be controlled using a GitReleaseManager.yaml
file, which is typically stored at the root of your project.

GitReleaseManager ships with the following default set of configuration (i.e.
when no yaml file is placed in the root directory):

```yaml
create:
  include-footer: false
  footer-heading:
  footer-content:
  footer-includes-milestone: false
  milestone-replace-text:
  include-sha-section: false
  sha-section-heading: "SHA256 Hashes of the release artifacts"
  sha-section-line-format: "- `{1}\t{0}`"
export:
  include-created-date-in-title: false
  created-date-string-format:
  perform-regex-removal: false
  regex-text:
  multiline-regex: false
issue-labels-include:
- Bug
- Feature
- Improvement
issue-labels-exclude:
- Internal Refactoring
```

Essentially, the only settings that are enabled by default are those that
specify which labels to include and which to exclude.

:::{.alert .alert-info}
Not all options are required.  For example, footer-content, is an empty string
by default.
:::

## Create Options

When creating a Release, there are a number of options which can be set to
control the look and feel of the generated release notes.

* **include-footer**
  * A boolean value which indicates that a footer should be included within the
  release notes.  Default is false.
* **footer-heading**
  * A string value which contains the heading text for the footer.  Default is
  an empty string.
* **footer-content**
  * A string value which contains the main body text for the footer.  This can
  be anything.  A typical example might be to provide information about where
  the release can be installed from.  Default is an empty string.
* **footer-includes-milestone**
  * A boolean value which indicates that the footer content contains a
  milestone, which should be replaced with the actual milestone value.  As an
  example, let's say you want to provide a link to where you can download your
  release, and the URL could be something like
  http://mydomain.com/releases/0.1.0.  You don't want to have to hard code the
  milestone number into your yaml configuration, so instead, you can use a
  replacement string in your footer-content, which will then be replaced with
  the actual milestone release number, when the release is created.  Default is
  false.
* **milestone-replace-text**
  * A string value which contains the string which should be replaced in the
  footer-content with the actual milestone release number. Default is an empty
  string.
* **include-sha-section**
  * A boolean value which indicates that the calculated SHA256 hash of the
  assets which are added to a release should be included within the release
  notes.  Default is false.  **NOTE:** This configuration option was added
  in version 0.9.0 of GitReleaseManager.
* **sha-section-heading**
  * A string value which contains the heading text for the SHA256 hash section.
  Default is `SHA256 Hashes of the release artifacts`  **NOTE:** This configuration option was added
  in version 0.9.0 of GitReleaseManager.
* **sha-section-line-format**
  * A string value which contains the .Net String Format value which will be
  used when creating the SHA256 hash entries in the release notes.
  Default is ```- `{1}\t{0}` ```  **NOTE:** This configuration option was added
  in version 0.9.0 of GitReleaseManager.

See the [example create configuration section](create-configuration) to see an
example of how a footer can be configured.

## Export Options

* **include-created-date-in-title**
  * A boolean value which indicates whether the date of which a Release occurred
  should be included within the heading section of generated release notes.
  Default is false.
* **created-date-string-format**
  * A string value which contains the Date Time Format string which should be
  used when including the created date in the title of the release notes.
  Default is an empty string.
* **perform-regex-removal**
  * A boolean value which inidcates whether a regular expression should be
  performed on the generated release notes to remove some text.  Default is
  false.
* **regex-text**
  * A string value which contains the regular expression to match against the
  generated release notes, in order to remove text.  Default in an empty string.
* **multiline-regex**
  * A boolean value which indicates that the regular expression should span
  multiple lines.  Default is false.

See the [example export configuration section](export-configuration) to see an
example of how the export can be configured.

## Issues to include

See the [Issues to include](include-issues) section.

## Issues to exclude

See the [Issues to exclude](exclude-issues) section.
