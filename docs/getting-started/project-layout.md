# Project layout and plugin.json

What the template creates, how the server finds plugins and what plugin.json is for.

## What the template creates

```
Homes/
├── Homes.csproj      class library, net10.0, EnableDynamicLoading, references the Ramjet.Api package
├── PluginMain.cs     the plugin class: derives from Plugin, [PluginInfo] attribute, OnEnable / OnDisable
├── plugin.json       manifest: id, version, API version, dependencies, permissions
├── AGENTS.md         rules and API cheat sheet for AI agents working in this project
├── CLAUDE.md         one line: @AGENTS.md (Claude Code reads CLAUDE.md)
├── .claude/skills/ramjet-plugin/   a Claude Code skill for plugin development
└── .gitignore
```

### The plugin class

```csharp
using Mc.Scripting;

namespace Homes;

[PluginInfo("Homes", Version = "1.0.0", Author = "Your Name", Description = "Set and teleport to homes")]
public sealed class PluginMain : Plugin
{
    protected override void OnEnable()
    {
        Events.On((ref PlayerJoinEvent e) => Log.Info($"{e.Username.ToString()} joined"));
    }

    protected override void OnDisable() { }
}
```

The server instantiates the **first public, non-abstract type with a parameterless constructor that implements `IPlugin`** (that is what `Plugin` does) in the assembly. Keep exactly one per project; if you must have several, name the right one in the `main` field of `plugin.json`. `[PluginInfo]` is optional metadata: without it (and without a manifest) the plugin is named after its assembly. The name prefixes every log line (`[Homes] ...`) and namespaces your system names.

### The project file

Three things matter and the template sets them for you:

- `EnableDynamicLoading` makes the build usable in an unloadable, collectible load context (this is what makes hot reload possible);
- the reference to `Ramjet.Api` is compile-time only: it is a reference assembly, so **no API dll is copied** next to your plugin. Never copy one there yourself: the server always uses its own;
- the dependencies you add yourself (NuGet packages) are copied next to your dll and loaded privately into your plugin's context.

## How the server finds plugins

The server loads plugins from its `plugins/` folder:

| Layout | Used for |
|---|---|
| `plugins/Name.dll` | a single-file plugin (manifest, if any: `plugins/Name.plugin.json`) |
| `plugins/Name/Name.dll` | a plugin with private dependencies: every other dll inside `plugins/Name/` is loaded into the plugin's own context; changing any dll in that folder reloads the plugin (manifest: `plugins/Name/plugin.json`) |
| `plugins/Name.cs` or `plugins/Name/*.cs` | a **source plugin**: the server compiles the `.cs` files itself, see below |

The folder is watched, so adding, replacing or deleting a dll loads, hot-reloads or unloads the plugin. `dotnet build` with `RAMJET_PLUGINS` set uses the folder layout, and also copies `plugin.json` along.

Operators can list the loaded plugins with `/plugins` and restart all of them with `/plugins reload`.

### Source plugins

For a small plugin you can skip the project: drop a single `.cs` file, or a folder of `.cs` files, into `plugins/` and the server compiles it and loads the result; editing the file reloads it. A source plugin does not need a project or a `plugin.json`: it declares itself with an assembly attribute at the top of one of its files (a `plugin.json` next to the sources works too and takes precedence):

```csharp
[assembly: McPlugin("hello", version: "1.0.0", description: "A source plugin")]

public sealed class Hello : Plugin
{
    protected override void OnEnable() => Log.Info("enabled");
}
```

The arguments must be string literals (or arrays of them): the server reads them from the syntax before it compiles, to work out the load order. `[assembly: McPermission("hello.use", Default = "true", Description = "Use /hello")]` declares a permission node the same way. The API namespaces are imported for you, as in a project built with `Ramjet.Api`. Source plugins can be switched off in the server's settings, so check that your server accepts them before you rely on them; a compiled plugin always works.

#### Compile errors

A source plugin that does not compile is not loaded (or, after an edit, its previous version keeps running). The errors are in the log with file, line and column, and in `Plugins.Problems`; see [source plugins that do not compile](../concepts/lifecycle.md#source-plugins-that-do-not-compile).

#### Memory and the compile cache

Compiling needs the C# compiler, and the compiler is the expensive part of a source plugin: while it is loaded, the plugin worker uses roughly 60 MB more than without it (a measurement on a single-core server with about 300 MiB, two small source plugins: about 128 MB with the compiler loaded, about 64 MB without), and the first compile after a start takes a few seconds on a single core. On a small server (256 MiB) that difference matters, so:

- **The server keeps the compiled result** in `plugins/.cache/`, one file per source plugin (the newest build only), valid for exactly those sources, that manifest and that API version. A start with unchanged sources loads the cached assembly in milliseconds and **never loads the compiler**, so once compiled, a source plugin costs about what a compiled plugin does. All source plugins on the server share this one folder.
- **Any change compiles again**, and the compiler then stays loaded until the plugin worker restarts (`/plugins reload` or a server restart). After a round of edits on a small server, restart once to give the memory back.
- **The folder must be a plain folder** that the server can write. The server creates it; if it is missing or unwritable (a file or a link in its place), every start compiles every source plugin again. You can delete it at any time: the next start compiles and fills it again.
- For a plugin you are done editing, a compiled plugin (`dotnet build`, see [build and hot reload](build-and-hot-reload.md)) needs no compiler at all.

## plugin.json

The manifest describes the plugin before its code is loaded, so the server can order plugins by their dependencies and refuse an incompatible one with a precise message instead of a stack trace. The template includes one:

```json
{
  "id": "homes",
  "name": "Homes",
  "version": "1.0.0",
  "api": "1.0",
  "main": "Homes.PluginMain",
  "authors": ["Your Name"],
  "description": "Set and teleport to homes",
  "depends": [],
  "softDepends": [],
  "permissions": []
}
```

Keep `id`, `name` and `version` consistent with `[PluginInfo]`. The `id` also names the plugin's data folder, `plugins/<id>/data/`, the only folder the plugin may write. See the [plugin.json reference](../reference/plugin-json.md) for every field.

Next: [build and hot reload](build-and-hot-reload.md).
