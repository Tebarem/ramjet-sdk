---
name: ramjet-plugin
description: Write, build, test and hot-reload C# plugins for the Ramjet Minecraft Java server (namespace Mc.Scripting, .NET 10). Use when creating or changing a Ramjet plugin project, writing event handlers, commands, ECS components, systems or scheduled tasks against Mc.Scripting.Api, porting a Paper/Bukkit plugin to Ramjet, or when a project has a plugin.json or references Mc.Scripting.Api.
---

# Writing Ramjet plugins

Ramjet is a C++23, vanilla-compatible Minecraft Java server (flecs ECS, native event bus) with a C# plugin layer. A plugin is a .NET 10 class library that compiles against the `Ramjet.Api` NuGet package (the API as a reference assembly; the server supplies the real API at run time), is loaded into a collectible load context, and is **hot reloaded on every rebuild**. All plugin code runs on the server's single tick thread.

## When to use this skill

- creating, editing, building or debugging a plugin (`Plugin` subclass, `[PluginInfo]`, `plugin.json`);
- subscribing to events, registering commands, ECS components, queries, systems, scheduled tasks;
- porting a Paper/Bukkit plugin (map concepts with the table at the end);
- anything importing `Mc.Scripting`.

## Project setup

```bash
# one-time: install the template
dotnet new install Ramjet.Templates     # preview: dotnet new install <folder>/Ramjet.Templates.<version>.nupkg
dotnet new ramjet-plugin -n MyPlugin --Author "me" --PluginDescription "what it does"
cd MyPlugin

# preview only: the Ramjet.Api package comes from a local folder until it is on nuget.org
#   dotnet nuget add source <folder with Ramjet.Api.*.nupkg> -n ramjet
export RAMJET_PLUGINS=/path/to/server/plugins           # optional: every build copies the plugin there = hot reload
dotnet build -c Release
```

- The csproj references the `Ramjet.Api` package: compile-time only (a reference assembly, no runtime files), so nothing from it lands next to the plugin. **Never copy a `Mc.Scripting.Api.dll` next to the plugin**.
- Layout the server loads: `plugins/Name.dll` or `plugins/Name/Name.dll` (+ private dependencies, + `plugin.json`).
- `plugin.json` is the manifest the loader reads (id, name, version, main class, dependencies, permissions); keep it consistent with `[PluginInfo]`.
- One public `Plugin` subclass per assembly (the first public `IPlugin` with a parameterless constructor is used). No work in the constructor: use `OnEnable`.
- The project must stay `net10.0`, `EnableDynamicLoading=true`.

## Hard rules

<!-- BEGIN GENERATED:hard-rules -->
1. All plugin code runs on the server tick thread; Scheduler.Post is the only thread-safe entry point.
2. Event handlers receive the event by ref and must not allocate: no LINQ, no string formatting or interpolation on the common path, no closures created per call.
3. Utf8String / NativeBytes event fields are borrowed for the duration of the dispatch; call ToString()/ToArray() to keep a copy.
4. Entity is a handle (flecs id), not an object: check IsAlive, never store the ref returned by Get<T>() across calls that can add or remove components or destroy entities.
5. Component types are plain unmanaged structs owned by exactly one plugin and registered with World.RegisterComponent<T>(key).
6. Register everything through the plugin context (Events, Commands, Systems, Scheduler, World); it is released automatically on unload and hot reload.
<!-- END GENERATED:hard-rules -->

More detail for each rule:

- **Threading**: `Scheduler.Post(Action)` is the only thread-safe API on main. Do slow work with `Task.Run`, copy what you need first (`Utf8String.ToString()`, `Entity` handles), and return with `Scheduler.Post`; re-check `entity.IsAlive` on return. Never `Task.Wait()` / `.Result` on the tick thread. Never touch the API from `Timer`, `FileSystemWatcher` or `async` continuations without posting back.
- **Allocation-free events**: handlers on hot events (`PlayerMove`, `TickStart/End`, `EntityDamage`, block events, systems) must not allocate: no LINQ, no `$"..."`, no `string.Format`, no boxing, no new capturing lambdas, no `params`. Create delegates once. Compare `Utf8String` bytes in place (`EqualsAscii("x"u8)`), call `ToString()` only when a string is truly needed.
- **Borrowed data**: event strings (`Utf8String`) and `NativeBytes` are valid only during the dispatch. The event is a `ref` struct parameter: it cannot be captured or stored.
- **Handles, not objects**: `Entity` is a flecs id. After `entity.Set/Remove/Destroy` (or any call that may add/remove components or create/destroy entities) every `ref T` obtained earlier may dangle: keep the `Entity`, re-fetch the `ref`, or copy the value out first. Inside a query loop, structural changes are deferred.
- **Components**: unmanaged structs only (no strings/arrays/references), owned by one plugin, registered with `World.RegisterComponent<T>("plugin:name")` in `OnEnable`. Same key + same layout after a reload keeps values.
- **State and hot reload**: fields of the plugin object die on reload; ECS components and files survive. Do not start unmanaged threads or timers that outlive `OnDisable`, and never store plugin types in static fields of other assemblies (leaks the load context; the runtime logs "was unloaded but is still referenced").
- **Registration**: only through the plugin's services (`Events`, `Commands`, `Systems`, `Scheduler`, `World`); anything else implement `IPluginResource` and `Context.Track(...)`.
- **Do not guess the API**: check `references/api.md` and `references/events.md`. If a member is not there, it does not exist.

## What is wired today

What the server wires today (generated from the server's sources):

<!-- BEGIN GENERATED:status -->
- Events the engine fires today: ServerStartEvent, ServerStopEvent, TickStartEvent, TickEndEvent, ServerListPingEvent, CommandsRegisteredEvent, PluginEnableEvent, PluginDisableEvent, PermissionChangedEvent, PermissionsReloadedEvent, PlayerPreLoginEvent, PlayerJoinEvent, PlayerQuitEvent, PlayerMoveEvent, PlayerChatEvent, PrivateMessageEvent, PlayerCommandEvent, PlayerInteractBlockEvent, PlayerKickEvent, PlayerRespawnEvent, PlayerDeathEvent, PlayerCreativeSlotEvent, PlayerItemHeldEvent, PlayerSwapHandsEvent, PlayerDropItemEvent, PlayerPickupItemEvent, PlayerConsumeEvent, PlayerInteractEvent, PlayerInteractEntityEvent, PlayerToggleSneakEvent, PlayerToggleSprintEvent, PlayerToggleFlightEvent, PlayerGameModeChangeEvent, PlayerTeleportEvent, PlayerPortalEvent, PlayerChangedWorldEvent, PlayerResourcePackStatusEvent, PlayerLevelChangeEvent, BlockBreakEvent, BlockPlaceEvent, BlockDamageEvent, BlockIgniteEvent, BlockBurnEvent, BlockSpreadEvent, BlockFadeEvent, BlockFormEvent, BlockGrowEvent, BlockPistonExtendEvent, BlockPistonRetractEvent, BlockRedstoneEvent, EntitySpawnEvent, EntityDamageEvent, EntityDeathEvent, EntityCombustEvent, InventoryOpenEvent, InventoryCloseEvent, InventoryClickEvent, InventoryDragEvent, CraftItemEvent, PrepareCraftEvent, FurnaceSmeltEvent, FurnaceBurnEvent, EnchantItemEvent, PrepareAnvilEvent, PacketReceiveEvent, PacketSendEvent, ChunkLoadEvent, ChunkUnloadEvent, TimeSkipEvent, WeatherChangeEvent, PortalCreateEvent, MenuClickEvent, MenuCloseEvent, CustomItemUseEvent, CustomBlockPlaceEvent, CustomBlockBreakEvent, CustomBlockInteractEvent, NpcInteractEvent.
- `Server.SendMessage` / `Broadcast` reach players: yes.
- `Server.CurrentTick` is real: yes.
- Plugin commands (`Commands.Register`) run from in-game chat: yes.
- `plugin.json` manifest read by the loader: yes.
<!-- END GENERATED:status -->

An event that is not listed as fired can be subscribed to, but nothing raises it yet. Check `references/events.md` before relying on one.

## Common recipes

Subscribe, cancel, mutate:

```csharp
Events.On((ref PlayerChatEvent e) =>
{
    if (e.Message.Contains("spam", StringComparison.OrdinalIgnoreCase)) e.Cancelled = true;   // chat is rare: decoding is fine
}, EventPriority.High);                                    // lower runs first; High runs after Normal

Events.On((ref EntityDamageEvent e) => e.Amount *= 0.5f);  // mutable field: the engine reads it back
Events.On((ref PlayerChatEvent e) => Log.Info(e.Cancelled ? "blocked" : "ok"), EventPriority.Monitor, receiveCancelled: true);
```

Component + query + system (state that survives hot reload):

```csharp
public struct Mana { public int Value, Max; }

private Query<Mana> _mana = null!;
protected override void OnEnable()
{
    World.RegisterComponent<Mana>("myplugin:mana");
    _mana = World.Query<Mana>();                       // create once, iterate every tick
    Events.On((ref PlayerJoinEvent e) => e.Player.Set(new Mana { Value = 20, Max = 20 }));
    Systems.Add("regen", Phase.Tick, _ =>
    {
        foreach (var chunk in _mana)
        {
            var mana = chunk.Field<Mana>(0);
            for (int i = 0; i < mana.Length; i++) if (mana[i].Value < mana[i].Max) mana[i].Value++;
        }
    });
}
```

Scheduler (20 ticks = 1 s) and async work:

```csharp
Scheduler.RunLater(100, () => Log.Info("5 s later"));
ScheduledTask t = Scheduler.RunEvery(20 * 60, Save);       // t.Cancel() to stop
_ = Task.Run(async () => { var data = await LoadAsync(); Scheduler.Post(() => Apply(data)); });
```

Handle a slash command today (flat `Commands.Register` is not dispatched by the game yet; check the status above):

```csharp
Events.On((ref PlayerCommandEvent e) =>
{
    if (!e.Command.EqualsAscii("ping"u8)) return;          // exact match without allocating
    e.Cancelled = true;                                    // we handled it
    Server.SendMessage(e.Player, "pong");                   // writes to the server log on main (see status)
});
```

Custom event (shared bus, other plugins can listen):

```csharp
public struct HomeSetEvent : IEvent
{
    public EventHeader Header;                // must be first
    public Entity Player;
    public static string Key => "myplugin:home_set";
    public static bool IsCancellable => true;
}
var ev = new HomeSetEvent { Player = p };
if (!Events.Fire(ref ev)) return;             // cancelled
```

Order against engine systems: `Systems.After/Before("anchor", "name", cb)`, `Systems.Replace("mobs:ai", "my_ai", cb)` (reversible), `Systems.SetEnabled("mobs:spawner", false)`; anchors are in `references/native-systems.md`.

## Testing on a server

Never use a server people play on. Use a test server you control:

1. Point the build at its plugins folder (`RAMJET_PLUGINS`, or upload `bin/Release/net10.0/` into `plugins/<Name>/` through your host's
   panel or SFTP).
2. Success line in the server log: `[MyPlugin] enabled v1.0.0 (id N)`. An exception in `OnEnable` is logged with its stack trace.
3. Rebuild: the server hot-reloads the replaced dll (a second `enabled` line; the old instance is released after the new one is up).
4. `/plugins` shows the plugin worker's state; `/plugins reload` restarts every plugin. A plugin added while the server runs is picked
   up automatically within a few seconds.
5. Join with a Minecraft client to exercise commands, events, GUIs and items.

## Where to look things up

| Need | File |
|---|---|
| exact signatures of every public type | `references/api.md` (this skill), or IntelliSense from the `Ramjet.Api` package |
| events: fields, mutability, cancellable, fired? | `references/events.md` |
| native systems to anchor on | `references/native-systems.md` |
| concepts, guides, migration table | the `docs/` folder of the Ramjet SDK repository (github.com/Tebarem/ramjet-sdk) |
| complete example plugins | the `samples/` folder of the same repository |

## Before you finish

- `dotnet build -c Release` has 0 warnings that matter and the plugin loads on a scratch server (log shows `enabled`).
- No allocation on hot handlers; no stored `ref`s, events or `Utf8String`s; no thread use of the API.
- Everything registered through the plugin context; `OnDisable` only for your own cleanup.
- Anything that must survive a reload is in a component or on disk.

## Paper/Bukkit to Ramjet

| Paper | Ramjet |
|---|---|
| `JavaPlugin`, `onEnable/onDisable` | `Plugin`, `OnEnable/OnDisable`, `[PluginInfo]` |
| `@EventHandler` method on a `Listener` | `Events.On((ref FooEvent e) => ...)` |
| `ignoreCancelled = true` | default (cancelled events skip the handler); `receiveCancelled: true` for the opposite |
| `EventPriority.LOWEST..MONITOR` | `EventPriority.Lowest..Monitor` (lowest runs first) |
| `event.setCancelled(true)` | `e.Cancelled = true` |
| `BukkitScheduler.runTaskLater/Timer` | `Scheduler.RunLater/RunEvery` (ticks) |
| `runTaskAsynchronously` + `runTask` | `Task.Run` + `Scheduler.Post` |
| `CommandExecutor` | `PlayerCommandEvent` today; Brigadier `Command.Literal(...)` in v1 |
| `PersistentDataContainer` / metadata | an ECS component on the `Entity` |
| `Player` object | `Entity` handle (v1: `Player` handle) |
