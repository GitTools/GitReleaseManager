---
Order: 70
Title: Export
---

This command will export all the release notes for a given repository on GitHub.
The generated file will be in Markdown format, and the contents of the exported
file is configurable using the GitReleaseManager.yaml file, per repository.

There are two modes of operation when exporting Release Notes. GitReleaseManager
can either export all Release Notes, or can export only a specific Release,
using the tagName parameter.

## **Required Parameters**

- `--token`: The access token to access GitHub with. This can't be used when
    using the username and password parameters.
- `-o, --owner`: The owner of the repository.
- `-r, --repository`: The name of the repository.
- `-f, --fileOutputPath`: Path to the file export releases.

## **Optional Parameters**

- `-t, --tagName`: The name of the release (Typically this is the generated
    SemVer Version Number).
- `-d, --targetDirectory`: The directory on which GitReleaseManager should be
    executed. Defaults to current directory.
- `-l, --logFilePath`: Path to where log file should be created. Defaults to
    logging to console.

<?! Include "_deprecated-args.md /?>

## **Notes**

<?! Include "_auth-notes.md" /?>

## **Examples**

Use GitReleaseManager to export all Release Notes:

```bash
gitreleasemanager.exe export --token fsdfsf67657sdf5s7d5f -o repoOwner -r repo -f c:\temp\releases.md

gitreleasemanager.exe export --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo --fileOutputPath c:\temp\releases.md

gitreleasemanager.exe export --username bob --password password --owner repoOwner --repository repo --fileOutputPath c:\temp\releases.md
```
