#!/usr/bin/env sh
# Installs the Ramjet plugin template for `dotnet new` (from this folder, no nupkg needed).
#   sh templates/install.sh              # install / update
#   sh templates/install.sh --uninstall
# Then:  dotnet new ramjet-plugin -n MyPlugin
set -eu
here="$(cd "$(dirname "$0")" && pwd)"
if [ "${1:-}" = "--uninstall" ]; then
  dotnet new uninstall "$here/ramjet-plugin"
else
  dotnet new install "$here/ramjet-plugin" --force
fi
