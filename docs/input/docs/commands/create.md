---
Order: 10
Title: Create
---

This is the main command of GitReleaseManager and it is used to create a draft set of release notes based on a milestone, which has been set up in GitHub.

There are two modes of operation when creating a Release.  GitReleaseManager can take as an input the name of the milestone to generate the release notes from.  Or, it can take as an input the name of a file which contains the release notes to include in the Release.

## **Required Parameters**

* `-u, --username`: The username to access GitHub with.  This can't be used when using the token parameter.
* `-p, --password`: The password to access GitHub with.  This can't be used when using the token parameter.
* `--token`: The access token to access GitHub with.  This can't be used when using the username and password parameters.
* `-o, --owner`: The owner of the repository.
* `-r, --repository`: The name of the repository.

## **Optional Parameters**

* `-m, --milestone`: The milestone to use.
* `-n, --name`: The name of the release (Typically this is the generated SemVer Version Number).
* `-i, --inputFilePath`: The path to the file to be used as the content of the release notes.
* `-e, --pre`: Creates the release as a pre-release.
* `-a, --assets`: Path(s) to the file(s) to include in the release.  This is a comma separated list of files to include
* `-c, --targetcommitish`: The commit to tag. Can be a branch or SHA. Defaults to repository's default branch.
* `-d, --targetDirectory`: The directory on which GitReleaseManager should be executed. Defaults to current directory.
* `-l, --logFilePath`: Path to where log file should be created. Defaults to logging to console.

## **Notes**

For Authentication use either username and password, or token parameter

## **Examples**

Use GitReleaseManager to create a Release, generating the release notes based on Milestone:

```bash
gitreleasemanager.exe create -m 0.1.0 -u bob -p password -o repoOwner -r repo

gitreleasemanager.exe create --milestone 0.1.0 --username bob --password password --owner repoOwner --repository repo

gitreleasemanager.exe create --milestone 0.1.0 --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo
```

Use GitReleaseManager to create a Release, taking the release notes as an input parameter:

```bash
gitreleasemanager.exe create -i c:\temp\releasenotes.md -n 0.1.0 -u bob -p password -o repoOwner -r repo

gitreleasemanager.exe create --inputFilePath c:\temp\releasenotes.md --name 0.1.0 --username bob --password password --owner repoOwner --repository repo

gitreleasemanager.exe create --inputFilePath c:\temp\releasenotes.md --name 0.1.0 --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo
```
