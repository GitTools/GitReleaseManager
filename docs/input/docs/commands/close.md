---
Order: 40
Title: Close
---

Out of the box, publishing a release on GitHub does not close the milestone
associated with the release. This command, when executed, closes the specified
milestone.

## **Required Parameters**

- `-u, --username`: The username to access GitHub with. This can't be used when
    using the token parameter.
- `-p, --password`: The password to access GitHub with. This can't be used when
    using the token parameter.
- `--token`: The access token to access GitHub with. This can't be used when
    using the username and password parameters.
- `-o, --owner`: The owner of the repository.
- `-r, --repository`: The name of the repository.
- `-m, --milestone`: The milestone to use.

## **Optional Parameters**

- `-d, --targetDirectory`: The directory on which GitReleaseManager should be
    executed. Defaults to current directory.
- `-l, --logFilePath`: Path to where log file should be created. Defaults to
    logging to console.

## **Notes**

For Authentication use either username and password, or token parameter

## **Examples**

```bash
gitreleasemanager.exe close -m 0.1.0 -u bob -p password -o repoOwner -r repo

gitreleasemanager.exe close --milestone 0.1.0 --username bob --password password --owner repoOwner --repository repo

gitreleasemanager.exe close --milestone 0.1.0 --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo
```
