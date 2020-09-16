---
Order: 11
Title: Template Configuration
---

Welcome to the documentation on how to configure each available step
of release notes generation by using Scriban templates.
While you can still use the old way of configuring templates (footer only) in
the yaml file, going forward it is recommended to use the new aproach by
extracting and editing the template files you wish to change instead.

## How are templates resolved

:::{.alert .alert-info}
If you will be passing in a custom absolute path to a template,
then you can skip this section entirely (assuming you won't be importing
any files either).
:::

Before we can go into how you can edit your templates, we first need to talk
about how are the templates resolved.
First of all, there is a new property that can be used to specify the base
directory of the templates directory located in the yaml configuration file.
This configuration is called `templates-dir` and will be used for all relative
paths (and for template names).

The templates are resolved in the following order (`<base>` is the value
specified in the `templates-dir` directory, and `<name>` is the name passed
to GitReleaseManage. _defaults to `default`_).

We will be using the [create](../commands/create) as an example in these paths.
Additional commands are planned to be supported, but for now these templates
can only be used when calling `GitReleaseManager create`.

**For abbreviation, file extensions are omitted from the pats.
File extension are expected to be either `.sbn` or `.scriban` unless otherwise
specified when passing in the template name when calling `GitReleaseManager`.**

### Resolving the initial index file

1. `<base>/<name>/create/index`
2. `<base>/<name>/create/<name>`
3. `<base>/create/<name>/index`
4. `<base>/create/<name>/<name>`
5. `<base>/<name>/index`
6. `<base>/<name>/<name>`
7. `<base>/create/index` (_will disable fallback to embedded resource if found_)
8. `<base>/create/<name>` (_will disable fallback to embedded resource if found_)
9. `<base>/<name>` (_will disable fallback to embedded resource if found_)

In the above resolution, if there is no file that can be found, GitReleaseManager
will try to fall back into the resources that are embedded within.

### Resolving child files

Each index file (and subsequent children) can include additional files that
needs to be imported (Please see the [Scriban documentation]() for this).

When this is being used, some paths are removed and others may be added.
In these paths the following substitution values are being used.

- `<base>` == The base directory as specified by the `templates-dir` property.
- `<template>` == The name of the template being used without a file extension
  (_may nat be available in all scenarios_)
- `<relative-path>` == The directory that the previous template file was
  located in.
- `<name>` == The name of the file (excluding any directories)
- `<name-dir>` == The parent directory of the file specified in the `<name>` argument.

Resolution is done in two seperate ways, depending on where the previous template
file was located.

Let us start with the simplest form, when the previous file was located on the
file system.
In this case, the resolution will only probe two distinct paths with the folliwng:

1. `<relative-path>/<name>`
2. `<relative-path>/<name-dir>/<name>`
3. Fallback to embedded resource when possible.

When the previous template file was an embedded resource file, then the resolution
follows a similar procedure as the initial resolution of the main/index template.

1. `<base>/<template>/create/<name>`
2. `<base>/create/<template>/<name>`
3. `<base>/<template>/<name>`
4. `<base>/create/<name>` (_will disable fallback to embedded resource if found_)
5. `<base>/<name>` (_will disable fallback to embedded resource if found_)
6. Fallback to embedded resource when possible

## Extracting embedded templates

Now on to the more fun parts, we will start exploring how you can make changes to
the template.
But first, we need some templates to work on.
While you could manually create and edit them by hand, the easier solution would
be to extract the existing templates that GitReleaseManager already makes use of.

This is very simple to do.
First ensure that you have set the `template-dir` property in the yaml configuration
file to the directory you want to use as the base of the templates (_this defaults
to `.target`_).
Then just run the following command:

```console
GitReleaseManager init --templates
```

or

```console
dotnet gitreleasemanager init --templates
```

The above commands will extract all embedded templates that GitReleaseManager
are aware of to you local file system (_you can delete the template files
you are not interested in_).

## Editing the templates

The following json object is made available on the global state in each template.
Some template files may have additional objects available (_assuming parent template
have not been modified_).

```json
{
  "commits": {
    "count": 5, // The amount of commits since the last tag
    "html_url": "<compare_commits>" // The URL to show/compare commits since last tag
  },
  "issue_labels": [
    // This list contains all of the labels associated with a closed issue/pr
    // for the current release
    "Bug",
    "Feature",
  ],
  "issues": {
    "count": 2, // The amount of issues/prs being closed since last tag
    "items": {
      // Key is the substitution of the issue label found for these issues,
      "<key>": [
        {
          "title": "First Issues", // The title of the issue
          "number": 54, // The issue number as shown on the VCS Provider
          "html_url": "<issue_link>", // The link to the issue
          "labels": [ // All of the labels associated with this issue
            {
              "name": "Bug", // The name of the label
              "color": "#456789", // The color of the label
              "description": "Important, must fix" // The description of the label
            }
          ]
        }
      ]
    }
  },
  "milestone": {
    // The previous milestone, if one was found; otherwise it will be null
    "previous": {
      "title": "Version 5.2.3", // The title of the previous milestone
      "description": "Some description", // The description of the previous milestone
      "number": 3243, // The number/identifier of the previous milestone
      "html_url": "<url>", // The url to issues associated with the previous milestone
      "url": "<url>",
      "version": {
        "major": 5, // The major version of the previous milestone
        "minor": 2, // The minor version of the previous milestone
        "build": 3, // The patch/build version of the previous milestone
        "revision": 0 // The revision of the previous milestone (typically 0)
      }
    },
    // The current milestone for this release
    "target": {
      "title": "Version 5.3.0", // The title of the current/next milestone
      "description": "I am up", // The description of the current/next milestone
      "number": 3265, // The number/identifier of the current/next milestone
      "html_url": "<url>", // The url to issues associated with the current/next milestone
      "url": "<url>",
      "version": {
        "major": 5, // The major version of the current/next milestone
        "minor": 3, // The minor version of the current/next milestone
        "build": 0, // The patch/build version of the current/next milestone
        "revision": 0 // The revision of the current/next milestone (typically 0)
      }
    }
  }
}
```

### Using a single file

While GitReleaseManager makes use of multiple template files to achieve what
a single template file could do, there is nothing that stops you to make the
necessary changes to make this happen.
Just add a single file in one of the mentioned
[index resolution paths](#resolving-the-initial-index-file) above,
and delete all of the other files (remember to not make use of the `include`
function in the template as well).

### Editing the index template

In most cases, there is no need to edit the index file, other than if you want to
include/remove other templates or merge all templates into one.
The name of the embedded/extracted index file is called `index.sbn` as its only
purpose is to bind together all the children together.

### Editing the commits/issues intro

The commits/issues intro template (called `release-info.sbn`) purpose is to
give a small intro on how many commits was included in the release, and
how many issues/prs was closed.

While the file can look a bit intimitading at first, it is actually quite simple.
The default template checks if there are any issues or commits and makes decisions
based on the if there were any.
It additionally pluralizes (simplified) when the issue/commit count is
more than 1.

This file can be used if you wish to change how the text should be displayed.

### Editing the milestone

By default, this template is very simple (called `milestone.sbn`).
It outputs only the description of the current/next milestone (or empty when
there is no description).

This file can be used if you want to provide additional details regarding the
milestone.

### Editing the issuse

Now we are getting into the meat of the templates.
The generation of the issues (actual Release Notes) are made possible in three
seperate files.
Each with their own purpose.

- We have the template `issues.sbn` which only have the purpose of iterating through
  each issue label for grouping purposes, then calling a child template to do the
  actual rendering.
- Next up is the `issue-details.sbn` template, which have the purpose of rendering
  the issue label (or category if you want), then calling another child template
  for rendering the actual issues (passing in a single issue to the child at a time).
  Additionally thanks to the parent, this template also have another global variable
  added to the global namespace called `issue_label`. The variable is the current
  label/category for the issues that should be used.
- Finally we have the `issue-note.sbn` template which renders the actual line of
  the issue, including its issue number, issue url and issue title.
  This issue template have even one more global variable called `issue` being
  made available thank to its parent.
  This issue variable is a single item located in the `issues.items` json object.

### Editing the footer

Finally for the last embedded template available, we have the footer template.
This template is responsible for generating the footer of the Release Notes.
By default it makes use of the old properties specified in the yaml configuration
file, but going forward it is recommended to edit this file manually instead of
using the yaml configuration properties.
This file is called simply `footer.sbn` and is located in the `create` directory.
