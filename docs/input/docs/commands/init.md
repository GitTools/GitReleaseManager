---
Order: 80
Title: Init
---

The Init command is used to create a GitReleaseManager configuration file which
controls the configurable options of GitReleaseManager

## **Optional Parameters**

- `-d, --targetDirectory`: The directory on which GitReleaseManager should be
    executed. Defaults to current directory.
- `-l, --logFilePath`: Path to where log file should be created. Defaults to
    logging to console.

## **Examples**

Create a new GitReleaseManager configuration file in the current working directory:

```bash
gitreleasemanager.exe init
```

Create a new GitReleaseManager configuration file in a specific directory:

```bash
gitreleasemanager.exe init -d c:\temp

gitreleasemanager.exe init --targetDirectory c:\temp
```
