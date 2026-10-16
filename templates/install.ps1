# Installs the Ramjet plugin template for `dotnet new` (from this folder, no nupkg needed).
#   powershell -ExecutionPolicy Bypass -File templates/install.ps1            # install / update
#   powershell -ExecutionPolicy Bypass -File templates/install.ps1 -Uninstall
# Then:  dotnet new ramjet-plugin -n MyPlugin
param([switch]$Uninstall)
$ErrorActionPreference = 'Stop'
$template = Join-Path $PSScriptRoot 'ramjet-plugin'
if ($Uninstall) {
    dotnet new uninstall $template
} else {
    dotnet new install $template --force
}
