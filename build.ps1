$SCRIPT_NAME = "recipe.cake"

Write-Host "Restoring .NET Core tools"
dotnet tool restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Bootstrapping Cake"
dotnet cake $SCRIPT_NAME --bootstrap
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Running Build"
dotnet cake $SCRIPT_NAME @args
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }