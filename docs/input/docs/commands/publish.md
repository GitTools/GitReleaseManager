---
Order: 60
Title: Publish
---

While it would be possible to automatically publish a set of release notes in a
single command, it is envisioned that some manual intervention is required to
ensure that all release notes are valid, and any additional information is added
to the release, prior to publishing. As a result, a second command is required
to actually publish a release.

## **Required Parameters**

- `-u, --username`: The username to access GitHub with. This can't be used when
    using the token parameter.
- `-p, --password`: The password to access GitHub with. This can't be used when
    using the token parameter.
- `--token`: The access token to access GitHub with. This can't be used when
    using the username and password parameters.
- `-o, --owner`: The owner of the repository.
- `-r, --repository`: The name of the repository.
- `-t, --tagName`: The name of the release (Typically this is the generated
    SemVer Version Number).

## **Optional Parameters**

- `-d, -targetDirectory`: The directory on which GitReleaseManager should be
    executed. Defaults to current directory.
- `-l, -logFilePath`: Path to where log file should be created. Defaults to
    logging to console.

## **Notes**

For Authentication use either username and password, or token parameter

## **Examples**

```bash
gitreleasemanager.exe publish -t 0.1.0 -u bob -p password -o repoOwner -r repo

gitreleasemanager.exe publish --tagName 0.1.0 --username bob --password password --owner repoOwner --repository repo

gitreleasemanager.exe publish --tagName 0.1.0 --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo
```
