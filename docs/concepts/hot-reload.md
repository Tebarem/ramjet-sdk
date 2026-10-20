# Hot reload

How plugins are reloaded without a restart, what survives, what does not, and the rules that keep reload leak-free.

Hot reload is the default workflow, not an afterthought: rebuilding a plugin replaces it in the running server (the delay is a short debounce plus load time), and **ECS state survives it**.

## What the server does

1. A file watcher on the plugins folder sees a changed `*.dll` (any dll inside `plugins/Name/` counts, so a private dependency change reloads `Name` too) and waits **250 ms** after the last write.
2. It checks the file can be opened (a half-written dll is retried later), then **loads the new version first**: a new collectible `AssemblyLoadContext`, assemblies read from bytes, so files are never locked and a rebuild can overwrite them while the server runs.
3. The new instance's `OnEnable` runs **while the old one is still registered**. Names the old version holds (commands, systems) can be taken over by the successor, and components or events it re-declares are re-bound.
4. Only if that succeeded is the old version unloaded: `OnDisable`, every tracked registration disposed in reverse order, the old context unloaded and collected.
5. If the new version fails to load, **nothing changes**: the old one keeps running and a warning says why.

`Mc.Scripting.Api` always resolves to the server's own instance, in every plugin context, so types unify across plugins and across reloads.

## What survives

| | Survives a reload? |
|---|---|
| ECS component values with an unchanged layout and key | **yes** |
| Entities | yes (they are not owned by plugins) |
| Your plugin object, its fields, static fields, caches, delegates | **no**: it is a new instance in a new load context |
| Subscriptions, commands, systems, queries, scheduled and posted tasks | released, then re-registered by the new `OnEnable` |
| Files on disk, config | yes |

A component keeps its values when its **size, alignment and field layout** (types and offsets, computed from the struct) and its key stay the same. Change a field type or order, and the values are dropped and the component recreated, but only if the reloading plugin (or the one it replaces) is the only one bound to the key; otherwise registration fails with a message in the log. Renaming a field does not change the layout.

Because plugin state in fields is lost, design for it: keep what must survive in **components or on disk**, and rebuild caches in `OnEnable`.

## Rules to keep reload leak-free

Unloading a collectible context only completes when **nothing references it**. The server checks this after every unload and logs `<name> was unloaded but is still referenced (leak)` if you break it. The ways to break it:

1. **Do not store anything from your assembly in a static field of another assembly** (a framework or API class, a `ThreadLocal<T>`, `AppDomain` data, a `ConditionalWeakTable` of your types...). That pins your context.
2. **Do not start threads or timers that outlive `OnDisable`**: a `Task.Run` loop, a `System.Threading.Timer`, a `FileSystemWatcher` of your own. Stop them in `OnDisable`, or use the [scheduler](scheduler.md), which is cancelled for you.
3. **Register through the plugin's services** (`Events`, `Commands`, `Systems`, `Scheduler`, `Ecs`) rather than a global registry, so the context can release it. Anything custom: implement `IPluginResource` and call `Context.Track(...)`.
4. **Do not keep `ref T` component references or event structs across ticks**: see [entities](entities-and-ecs.md).
5. A plugin's own dependencies (NuGet packages) are loaded privately into its context. Share *types between plugins* only through the API assembly or a shared assembly the plugins both reference and the server can resolve (see [inter-plugin services](../guides/inter-plugin-services.md)).
6. After a failed reload, if the failed version already re-declared a component or event layout, that declaration stays. Fix and reload again.

## Triggering reloads

- Build into the watched folder (`RAMJET_PLUGINS`, see [create a plugin](../getting-started/create-a-plugin.md)); the build copies dependencies and the pdb first and the main dll last, because the dll is what the watcher reacts to.
- Copying a dll over the old one works the same way; deleting it unloads the plugin; adding a new `Name.dll` or `Name/Name.dll` loads it.
- A source plugin (`.cs` files in the plugins folder) is recompiled and reloaded when its sources change.
- Adding a new plugin loads it within a few seconds; the server restarts the plugin worker for it, which reloads every plugin (see [build and hot reload](../getting-started/build-and-hot-reload.md#plugins-added-while-the-server-runs)).
- An operator (level 4, or the console) can run `/plugins reload` to restart all plugins, for example when hot reload is off or after changing a file the server does not watch.

## The first run costs more

The server gives the plugins a budget of **20 ms per tick** (`scripting.ipc.tick_budget_us` in `ramjet.toml`): the time it waits for the plugin worker to finish its part of a tick. A plugin that is **enabled or hot-reloaded while the server is running** is loaded inside a tick, so loading it, and the first run of its code (the runtime compiles each method the first time it runs), count against that budget. The first run can easily be slower than 20 ms, and then the log shows this warning (at most one a second):

```
[WARN] scripting: the plugins' tick took 63 ms (budget 20 ms)
```

That is expected after a reload or after adding a plugin, and harmless: the tick was only slow, and the next ticks are normal. Plugins loaded when the server starts are not counted, because they load before the first tick. Look into it only if the warning repeats on ticks where nothing was reloaded: then a handler or system of yours is slow, see [performance](performance.md). A tick that the worker does not finish within 250 ms (`scripting.ipc.tick_end_grace_us`) is treated as a stall: the worker is restarted and every plugin loads again.

## Dependents and the ownership table

- **Hot reload of a plugin also reloads its hard dependents** (`depends` in the manifest): they are disabled in reverse order, the plugin is reloaded, and the dependents are enabled again, because their services and shared types come from it. Soft dependents only see the service unregistered/registered notifications.
- Besides events, commands, systems and scheduled tasks, these are released on unload: permissions and permission providers, services, `Config<T>` watchers, boss bars, sidebars, tab-list text and name tags your plugin set (removed or reset), chunk tickets, custom GUIs (open instances are closed for every viewer), holograms, NPCs and custom content. Resource-pack pushes and spawned entities are **not** owned: the client keeps its pack and entities stay in the world (mark them with a component or tag if you want cleanup in `OnDisable`).
