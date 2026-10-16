# Create a plugin from the template

Install the ramjet-plugin dotnet template, add the API package source, create a project, point it at your test server and build it.

The `ramjet-plugin` template creates a ready-to-build class library with the right project settings, a small plugin class, a `plugin.json`, and an `AGENTS.md` for AI coding agents.

## Install the template

```bash
dotnet new install Ramjet.Templates
```

Until the template package is on nuget.org, install it from the `.nupkg` file you were given:

```bash
dotnet new install <path to Ramjet.Templates.*.nupkg>
```

To remove it again: `dotnet new uninstall Ramjet.Templates`.

## Add the API package source

The project references **`Ramjet.Api`**, the plugin API as a NuGet package. It holds a reference assembly: every public type and member of the API, with its documentation (IntelliSense), and no code. Your plugin compiles against it, and the server supplies the real API when it loads the plugin, so nothing from the package is copied next to your dll. You do not need a server to compile a plugin.

Until the package is on nuget.org, add the folder that holds `Ramjet.Api.<version>.nupkg` as a package source, once:

```bash
dotnet nuget add source /path/to/folder-with-the-nupkg -n ramjet
```

The package also gives your project sensible defaults: `EnableDynamicLoading`, implicit usings, and global usings for the API namespaces (`Mc.Scripting`, `Mc.Scripting.Commands`, `Mc.Scripting.Events`, `Mc.Scripting.Text`, `Mc.Scripting.Worlds`, ...), which is why the code in these docs needs few `using` lines. Set `<RamjetGlobalUsings>false</RamjetGlobalUsings>` in the project file to turn the global usings off.

## Create the project

```bash
dotnet new ramjet-plugin -n Homes --Author "Your Name" --PluginDescription "Set and teleport to homes"
cd Homes
```

| Option | Default | Effect |
|---|---|---|
| `-n`, `--name` | `MyPlugin` | assembly, namespace and plugin name; also the lower-cased `id` in `plugin.json` |
| `--Author` | `Your Name` | `[PluginInfo(Author = ...)]` and `plugin.json` |
| `--PluginDescription` | `A plugin for ramjet.` | `[PluginInfo(Description = ...)]` and `plugin.json` |
| `--RamjetApiVersion` | `1.0.0-preview.1` | the version of the `Ramjet.Api` package to reference |

## Point the build at your test server

Optional, and the most convenient setting: tell the build where your test server's `plugins` folder is, and every build copies the plugin there, which triggers the server's hot reload.

```bash
export RAMJET_PLUGINS=/path/to/server/plugins
```

PowerShell: `$env:RAMJET_PLUGINS = 'C:\path\to\server\plugins'`. Or per build: `dotnet build -c Release -p:RamjetPluginsDir=/path/to/server/plugins`.

The folder has to be reachable as a normal path: a server you run yourself, or a hosted server's `plugins` folder mounted through SFTP or a synced folder. If you cannot mount it, leave the variable unset and upload the files by hand (see [build and hot reload](build-and-hot-reload.md)).

## Build

```bash
dotnet build -c Release
```

With `RAMJET_PLUGINS` set, the end of the output tells you where the plugin went:

```
  Homes -> .../bin/Release/net10.0/Homes.dll
  Homes deployed to /path/to/server/plugins/Homes/
```

That is all it takes. Continue with the [project layout](project-layout.md) to see what the template produced, or jump to [build and hot reload](build-and-hot-reload.md) to watch the plugin load.
