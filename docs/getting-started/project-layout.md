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

Keep `id`, `name` and `version` consistent with `[PluginInfo]`. The `id` is also the name of the plugin's data folder, `plugins/<id>/`. See the [plugin.json reference](../reference/plugin-json.md) for every field.

Next: [build and hot reload](build-and-hot-reload.md).
