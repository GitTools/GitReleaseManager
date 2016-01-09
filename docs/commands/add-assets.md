# Add Assets

Once a draft set of release notes has been created, it is possible to add additional assets to the release using the addasset command.

## **Required Parameters**
  * `-u, -username`: The username to access GitHub with.
  * `-p, -password`: The password to access GitHub with.
  * `-o, -owner`: The owner of the repository.
  * `-r, -repository`: The name of the repository.
  * `-t, -tagName`: The name of the release (Typically this is the generated SemVer Version Number).
  * `-a, -assets`: Path(s) to the file(s) to include in the release.  This is a comma separated list of files to include

## **Optional Parameters**
  * `-d, -targetDirectory`: The directory on which GitReleaseManager should be executed. Defaults to current directory.
  * `-l, -logFilePath`: Path to where log file should be created. Defaults to logging to console.

## **Examples**

```
gitreleasemanager.exe addasset -t 0.1.0 -u bob -p password -o repoOwner -r repo -a c:\buildartifacts\setup.exe,c:\buildartifacts\setup.nupkg

gitreleasemanager.exe create -tagName 0.1.0 -username bob -password password -owner repoOwner -repository repo -assets c:\buildartifacts\setup.exe,c:\buildartifacts\setup.nupkg
```