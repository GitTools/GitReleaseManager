---
Order: 30
Title: Via .Net Global Tool
---

It is possible to install GitReleaseManager as a .Net Global Tool. Simply
execute the following command:

```bash
dotnet tool install --global GitReleaseManager.Tool
```

:::{.alert .alert-warning}
This will require that .Net Core is installed on the machine which you are
trying to install the .Net Global Tool.
:::

Once installed, GitReleaseManager should be immediately available on the command
line. You can call:

```bash
dotnet-gitreleasemanager
```
