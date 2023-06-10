---
Order: 100
Title: Label
---

When first setting up a repository, it is nice to be able to configure a set of
default labels, so that you can have some consistency across your various
projects.  While GitHub, and other Source Control systems provide a set of
default labels, they are not always exactly what you want.  This command will
remove all the current labels configured within a repository, and create a new
set of them. However, if RenameFrom is specified in the label configuration, the 
label can be renamed instead of being deleted and recreated.

:::{.alert .alert-info}
**NOTE:**

The available list of labels that are created by default is currently hard-coded into
GitReleaseManager, it is possible to configure them by overriding the list.  See
[Default Configuration](../configuration/default-configuration.md) for more details.
:::

## **Required Parameters**

- `--token`: The access token to access GitHub with.
- `-o, --owner`: The owner of the repository.
- `-r, --repository`: The name of the repository.

## **Optional Parameters**

- `-d, -targetDirectory`: The directory on which GitReleaseManager should be
    executed. Defaults to current directory.
- `-l, -logFilePath`: Path to where log file should be created. Defaults to
    logging to console.

## **Examples**

```bash
gitreleasemanager.exe label --token fsdfsf67657sdf5s7d5f -o repoOwner -r repo

gitreleasemanager.exe label --token fsdfsf67657sdf5s7d5f --owner repoOwner --repository repo
```
