# Export

This command will export all the release notes for a given repository on GitHub.  The generated file will be in Markdown format, and the contents of the exported file is configurable using the GitReleaseManager.yaml file, per repository.

There are two modes of operation when exporting Release Notes. GitReleaseManager can either export all Release Notes, or can export only a specific Release, using the tagName parameter.

## **Required Parameters**
  * `-u, -username`: The username to access GitHub with.
  * `-p, -password`: The password to access GitHub with.
  * `-o, -owner`: The owner of the repository.
  * `-r, -repository`: The name of the repository.
  * `-f, -fileOutputPath`: Path to the file export releases.

## **Optional Parameters**
  * `-t, -tagName`: The name of the release (Typically this is the generated SemVer Version Number).
  * `-d, -targetDirectory`: The directory on which GitReleaseManager should be executed. Defaults to current directory.
  * `-l, -logFilePath`: Path to where log file should be created. Defaults to logging to console.

## **Examples**

Use GitReleaseManager to export all Release Notes:

```
gitreleasemanager.exe export -u bob -p password -o repoOwner -r repo -f c:\temp\releases.md

gitreleasemanager.exe export -username bob -password password -owner repoOwner -repository repo -fileOutputPath c:\temp\releases.md
```