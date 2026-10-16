# Plugin lifecycle

How the server finds, loads, enables, disables and unloads a plugin, and what the plugin context owns.

## From file to running plugin

```
plugins/Homes/Homes.dll
   │  1. plugin.json (if any) is read; the load order is worked out from the manifests
   │  2. the dll is loaded from bytes into a new collectible AssemblyLoadContext (the file is never locked)
   │  3. the first public IPlugin type with a parameterless constructor is instantiated
   │  4. a PluginContext is created (id/name/version from plugin.json or [PluginInfo], or the assembly)
   ▼
OnEnable()          register events, commands, systems, components, queries, tasks
   │   ... the plugin runs ...
   ▼
OnDisable()         optional cleanup of your own state
   │  5. everything registered through the context is disposed, in reverse order
   │  6. the load context is unloaded and garbage collected
```

All of this happens **on the tick thread**: at server start (before the first tick), when a file in the plugins folder changes, and at shutdown.

## The plugin class

```csharp
[PluginInfo("Homes", Version = "1.0.0", Author = "you")]
public sealed class PluginMain : Plugin
{
    protected override void OnEnable()  { /* register everything */ }
    protected override void OnDisable() { /* optional: flush files, say goodbye */ }
}
```

`Plugin` exposes the plugin-scoped services as properties:

| Property | What it is for |
|---|---|
| `Log` | the plugin's logger (lines are prefixed with the plugin name) |
| `Events` | event subscriptions and custom events ([events](events.md)) |
| `Commands` | command registration ([commands](commands.md)) |
| `Permissions` | declaring permission nodes, asking whether a player has one ([permissions](permissions.md)) |
| `Config`, `DataFolder` | typed config and files in the plugin's data folder ([config and data](config-and-data.md)) |
| `Ecs` | custom components and queries ([entities and the ECS](entities-and-ecs.md)) |
| `Systems` | per-tick systems ([systems](systems.md)) |
| `Scheduler` | delayed and repeating tasks ([scheduler](scheduler.md)) |
| `Services` | plugin-to-plugin services ([inter-plugin services](../guides/inter-plugin-services.md)) |
| `BossBars`, `Sidebars`, `Menus`, `Holograms`, `Npcs`, `ChunkTickets`, `Channels` | things that are shown to players or kept loaded for them, removed when the plugin unloads |
| `Info`, `Context` | what the plugin declares about itself, and the raw `PluginContext` |

Everything you register through them is tracked by the context, so **you never unregister by hand**; unloading does it. `Server` is a static class and does not need to be released.

- Do no work in the constructor. Use `OnEnable`.
- You can implement `IPlugin` directly if you do not want the base class; `Enable(PluginContext)` receives the context.
- `[PluginInfo]` is optional. Without it and without a manifest the plugin is named after its assembly, with the assembly's version.

## What happens on errors

| Situation | Result |
|---|---|
| No public `IPlugin` implementation with a parameterless constructor | `no public IPlugin implementation ...` is logged, nothing is loaded |
| `OnEnable` throws | the exception is logged with the plugin path, every registration made so far is released and the plugin is **not** loaded; other plugins are unaffected (plugins that depend on it are skipped) |
| `OnDisable` throws | logged (`error in OnDisable`), unloading continues |
| A handler, task or system throws | caught at the boundary and logged with the plugin name; the server keeps running and the next call goes through |

## What the context owns

Everything you create through the plugin's services belongs to the plugin and goes away with it:

| Registered through | Released on unload |
|---|---|
| `Events.On` | the subscription |
| `Commands.Register` | the command |
| `Permissions.Register`, `Permissions.SetProvider` | the declared nodes; a replaced permission backend is restored |
| `Systems.Add/Register/After/Before/Replace` | the system (a `Replace`d system is restored) |
| `Ecs.Query<...>()` | the query |
| `Ecs.RegisterComponent<T>` | the type binding (values on entities survive, see [hot reload](hot-reload.md)) |
| `Scheduler.RunLater/RunEvery/Post` | pending tasks |
| `Services.Register<T>` | the registration |
| `Config.Load<T>` | the file watcher |
| `BossBars`, `Sidebars`, `Menus`, `Holograms`, `Npcs`, `ChunkTickets` | the bars, sidebars, GUIs (closed for every viewer), holograms, NPCs and chunk tickets |
| `Context.Track(resource)` | anything implementing `IPluginResource` |

Entities are **not** owned: they live in the world, not in the plugin. Destroy the ones you created in `OnDisable` if they should not outlive it. A resource pack you pushed to a player is not owned either: the client keeps it.

## Manifest, dependencies and load order

A [`plugin.json`](../reference/plugin-json.md) manifest is read **before any assembly is loaded**: `id`, `version`, `api` (the API version the plugin needs), `main`, `depends`, `softDepends`, `loadBefore`, `permissions`.

- Load order is a topological sort of `depends` / `softDepends` / `loadBefore`, ties broken by id. A missing hard dependency, a version mismatch or a cycle skips the plugin (and everything that depends on it) with a precise message such as `plugin 'homes' requires 'economy >=1.2' which is not installed`.
- A plugin whose `OnEnable` throws is disabled, and its dependents are skipped with that cause.
- Unload runs in reverse load order; hot-reloading a plugin also reloads its hard dependents.
- `Plugins.All`, `Plugins.Get(id)`, `Plugins.IsLoaded(id)` and `Plugins.Problems` let a plugin look at the other loaded plugins and at the problems found while loading.
- Without a manifest there is no dependency information: plugins are then loaded in alphabetical order of their file path, so do not rely on another plugin during `OnEnable` unless you declare the dependency.
