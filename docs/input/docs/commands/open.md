---
Order: 50
Title: Open
---

Depending on the workflow that is being followed, closing on a milestone is
something that is typically done once a release has been published.  However, it
might also be necessary to revert closing of a milestone.  There are several
reasons _why_ this might be necessary, which aren't really important to go into
here, but this command allow you to open a closed milestone.

:::{.alert .alert-info}
**NOTE:**

This command will _only_ work against a currently closed milestone.  No action
will be taken against a milestone that is already open.
:::

## **Required Parameters**

- `--token`: The access token to access GitHub with.
- `-o, --owner`: The owner of the repository.
- `-r, --repository`: The name of the repository.
- `-m, --milestone`: The milestone to use.

## **Optional Parameters**

- `-d, --targetDirectory`: The directory on which GitReleaseManager should be
    executed. Defaults to current directory.
- `-l, --logFilePath`: Path to where log file should be created. Defaults to
    logging to console.

## **Examples**

```bash
gitreleasemanager.exe open -m 0.1.0 --token fsdfsf67657sdf5s7d5f -o repoOwner -r repo

gitreleasemanager.exe open --milestone 0.1.0 --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo
```
