# Issues to exclude

From time to time, you may want to include issues within a milestone, however, you don't want any information about these issues to appear in the release notes that are generated for that milestone.  For example, let's say you were doing some internal refactoring work.  This information is not required for the end user, but you as the administrator would want to know when that work was done.  GitReleaseManager caters for this requirement using the issue-labels-exclude section of the GitReleaseManager.yaml file.

Out of the box, GitReleaseManager is configured to exclude issues that are tagged with the following labels:

```
issue-labels-exclude:
- Internal Refactoring
```

<div class="admonition note">
    <p class="first admonition-title">Note</p>
    <p class="last">
        You can add as many issue labels into this section as required.  Any issue, included within a milestone that contains a label specified within this list will NOT be included within the generated release notes.
    </p>
</div>

<div class="admonition attention">
    <p class="first admonition-title">Warning</p>
    <p class="last">
        All issues assigned to a milestone have to have a label which matches to one listed in the include to exclude sections of the GitReleaseManager.yaml file or the default configuration.
    </p>
</div>

<div class="admonition error">
    <p class="first admonition-title">Note</p>
    <p class="last">
        GitReleaseManager uses a case sensitive comparison when checking for labels to include and exclude. i.e. Bug is NOT the same as bug.
    </p>
</div>