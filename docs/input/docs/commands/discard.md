---
Order: 20
Title: Discard
---

After creating a draft release, it might be necessary to also discard it.  This
could be for a number of reasons, which don't need to be detailed here, but
if/when required, this command will discard a draft release.

:::{.alert .alert-info}
**NOTE:**
The command will only work for releases that are in the draft state.  It won't
delete a published release.
:::

## **Required Parameters**

- `--token`: The access token to access GitHub with. This can't be used when
    using the username and password parameters.
- `-o, --owner`: The owner of the repository.
- `-r, --repository`: The name of the repository.
- `-m, --milestone`: The name of the release (Typically this is the generated
    SemVer Version Number).

## **Optional Parameters**

- `-d, -targetDirectory`: The directory on which GitReleaseManager should be
    executed. Defaults to current directory.
- `-l, -logFilePath`: Path to where log file should be created. Defaults to
    logging to console.

<?! Include "_deprecated-args.md /?>

## **Notes**

<?! Include "_auth-notes.md" /?>

## **Examples**

```bash
gitreleasemanager.exe discard -m 0.1.0 --token fsdfsf67657sdf5s7d5f -o repoOwner -r repo

gitreleasemanager.exe discard --milestone 0.1.0 --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo

gitreleasemanager.exe discard --milestone 0.1.0 --username bob --password password --owner repoOwner --repository repo
```
