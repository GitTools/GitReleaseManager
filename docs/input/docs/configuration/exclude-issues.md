---
Order: 50
Title: Issues to exclude
---

From time to time, you may want to include issues within a milestone, however,
you don't want any information about these issues to appear in the release notes
that are generated for that milestone. For example, let's say you were doing
some internal refactoring work. This information is not required for the end
user, but you as the administrator would want to know when that work was done.
GitReleaseManager caters for this requirement using the issue-labels-exclude
section of the GitReleaseManager.yaml file.

Out of the box, GitReleaseManager is configured to exclude issues that are
tagged with the following labels:

```yaml
issue-labels-exclude:
    - Internal Refactoring
```

:::{.alert .alert-info}
You can add as many issue labels into this section as required. Any issue,
included within a milestone that contains a label specified within this list
will NOT be included within the generated release notes.
:::

:::{.alert .alert-warning}
All issues assigned to a milestone have to have a label which matches to one
listed in the include to exclude sections of the GitReleaseManager.yaml file or
the default configuration.
:::
