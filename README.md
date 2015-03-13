ReleaseNotesCompiler
====================

[![Join the chat at https://gitter.im/gep13/GitHubReleaseManager](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/gep13/GitHubReleaseManager?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

In order to improve the quality for our release notes we'll generate them based on the relevant github issues.

### Conventions

* All closed issues for a milestone will be included
* All issues must have one of the following tags `Bug`, `Feature`, `Internal refactoring`, `Improvement`. Where `Internal refactoring` will be included in a milestone but excluded from the release notes. 
* For now the text is taken from the name of the issue
* Milestones are named {major.minor.patch}
* Version is picked up from the build number (GFV) and that info is used to find the milestone
* We'll generate release notes as markdown for display on the website
* by default only the first 30 line of an issue description is included in the release noted. If you want to control exactly how many lines are included then use a `--` to add a horizontal rule. Then only the contents above that horizontal rule will be included.

### Plans

* The build server will compile the release notes either for each commit or daily
* Build will fail if release notes can't be generated
* No milestone found is considered a exception
* Want to be able to output in a manner compatible with http://www.semanticreleasenotes.org/
* We'll generate release notes as X for inclusion in our nugets
* For each milestone a corresponding GitHub release will be created with the same name and set to tag master with the same tag when published


