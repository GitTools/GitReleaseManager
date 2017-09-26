# Enable Many Labels Per Issue

When you are using many labels per issue in your project, you need explicitly tell GitReleaseManager about that, otherwise it won't generate report.

Here is the example of how to configure it:

```
create:
  include-footer: false
  footer-heading:
  footer-content:
  footer-includes-milestone: false
  milestone-replace-text:
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
issue-labels-many: true
```

This configuration will generate following report for you:

As part of this release we had [5 commits](https://github.com/TestUser/FakeRepository/commits/1.2.3) which resulted in [2 issues](https://github.com/FakeRepository/issues/issues?milestone=0&state=closed) being closed.


__Bug__

- [__#1__](http://example.com/1) Issue 1

__Improvement__

- [__#1__](http://example.com/1) Issue 1
- [__#2__](http://example.com/2) Issue 2

