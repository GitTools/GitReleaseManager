# Show Config

The showconfig command is used to display the current configuration for GitReleaseManager when executed against a specific directory.

## **Optional Parameters**

* `-d, --targetDirectory`: The directory on which GitReleaseManager should be executed. Defaults to current directory.
* `-l, --logFilePath`: Path to where log file should be created. Defaults to logging to console.

## **Examples**

Show the configuration for the current working directory:

```bash
gitreleasemanager.exe showconfig
```

Show the configuration for a specific directory:

```bash
gitreleasemanager.exe showconfig -d c:\temp

gitreleasemanager.exe showconfig --targetDirectory c:\temp
```