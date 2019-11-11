# Label Aliases

When there are more than one issue associated with a particular label, GitReleaseManager will attempt to pluralize the label name.  For example, `Bugs` instead of `Bug`.  However, there are times when this basic pluralization will not work.  For example, `Documentation` will be pluralized as `Documentations` which doesn't really make sense.  In these situations, it is possible to override the automatic title.

This is possible for both the singular and plural cases.

Here is an example of how to configure two label aliases.  The title `Foo` will be used instead of `Bug`, and `Baz` instead of `Improvement`.  If each label contains more than one feature, `Bar` and `Qux` will be used instead in the release notes.

```
issue-labels-alias:
    - name:    Bug
      header:  Foo
      plural:  Bar

    - name:    Improvement
      header:  Baz
      plural:  Qux
```

<div class="admonition note">
    <p class="first admonition-title">Note</p>
    <p class="last">
        You can add as many label aliases as required.
    </p>
</div>