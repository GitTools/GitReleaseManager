---
Order: 10
Title: Configure Logging
---

It is possible to view all the output from GitReleaseManager, either in the
console window, or by writing out to a file.

By default, all command output is written to the console window, however, it is
possible to specify a text file where command output should be written to.

## **Examples**

Enable logging to a file located in c:\temp\log.txt using the following command:

```bash
gitreleasemanager.cli.exe init -l c:\temp\log.txt

gitreleasemanager.cli.exe init -logFilePath c:\temp\log.txt
```

:::{.alert .alert-info}
Assuming you simply wanted to **pipe** the output from GitReleaseManager to a
file, when executing directly from the command line, it is not required to use
the -l parameter. The log messages written to the console window, would simply
be piped to the output file.
:::
