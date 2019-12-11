---
Order: 60
Title: Init
---

The Init command is used to create GitReleaseManager.yaml which controls the
configurable options of GitReleaseManager

## **Optional Parameters**

* `-d, --targetDirectory`: The directory on which GitReleaseManager should be
executed. Defaults to current directory.
* `-l, --logFilePath`: Path to where log file should be created. Defaults to
logging to console.

## **Notes**

For Authentication use either username and password, or token parameter

## **Examples**

Create a new GitReleaseManager.yaml file in the current working directory:

```bash
gitreleasemanager.exe init
```

Create a new GitReleaseManager.yaml file in a specific directory:

```bash
gitreleasemanager.exe init -d c:\temp

gitreleasemanager.exe init --targetDirectory c:\temp
```
