---
name: ramjet-plugin
description: Write, build, test and hot-reload C# plugins for the Ramjet Minecraft Java server (namespace Mc.Scripting, .NET 10). Use when creating or changing a Ramjet plugin project, writing event handlers, commands, ECS components, systems or scheduled tasks against Mc.Scripting.Api, porting a Paper/Bukkit plugin to Ramjet, or when a project has a plugin.json or references Mc.Scripting.Api.
---

# Writing Ramjet plugins

Ramjet is a C++23, vanilla-compatible Minecraft Java server (flecs ECS, native event bus) with a C# plugin layer. A plugin is a .NET 10 class library that compiles against the `Ramjet.Api` NuGet package (a reference assembly: the server supplies the real API when it loads the plugin). Each plugin loads into its own collectible context and is **hot reloaded on every rebuild**. Plugins run in a sandboxed worker process; all of a plugin's code runs on that worker's tick thread.

## When to use this skill

- creating, editing, building or debugging a plugin (`Plugin` subclass, `[PluginInfo]`, `plugin.json`);
- subscribing to events, registering commands, ECS components, queries, systems, scheduled tasks;
- porting a Paper/Bukkit plugin (map concepts with the table at the end);
- anything importing `Mc.Scripting`.

## Project setup

```bash
# one-time: install the template
dotnet new install Ramjet.Templates     # from nuget.org; the project references Ramjet.Api from nuget.org too
dotnet new ramjet-plugin -n MyPlugin --Author "me" --PluginDescription "what it does"
cd MyPlugin

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
5. Component types are plain unmanaged structs owned by exactly one plugin and registered with Ecs.RegisterComponent<T>(key) (plugin.Ecs).
6. Register everything through the plugin context (Events, Commands, Systems, Scheduler, Ecs, Permissions, ...); it is released automatically on unload and hot reload.
<!-- END GENERATED:hard-rules -->

More detail for each rule:

- **Threading**: the game API runs on the tick thread; off it, calls such as `Events.On`, `Commands.Register`, `Ecs.RegisterComponent` or `Entity.Get` throw `WrongThreadException`. Thread-safe: `Scheduler.Post`, `RunSync`, `RunAsync`, `ToTick()`, `ToAsync()`, `Log`, `Server.IsTickThread`, `Server.CurrentTick`, `Http.Client`, `Config<T>.Value`. Never block (`Wait()`, `.Result`, `Thread.Sleep`). Re-check `player.IsOnline` / `entity.IsAlive` after any hop.
- **Budgets**: an awaited event (cancellable, or with a writable field) waits at most 5 ms for your handler (`scripting.ipc.event_budget_us`). On overrun the event goes ahead unchanged, and repeated overruns mute the plugin's handlers until reload. The tick has 20 ms for awaited events in total.
- **Allocation-free events**: handlers on hot events (`PlayerMove`, `TickStart/End`, `EntityDamage`, block events, systems) must not allocate: no LINQ, no `$"..."`, no `string.Format`, no boxing, no `params`, no capturing lambdas created per call. Create delegates once. Compare `Utf8String` bytes in place (`EqualsAscii("x"u8)`); call `ToString()` only when a string must be kept.
- **Borrowed data**: event strings (`Utf8String`) and `NativeBytes` are valid only during the dispatch. The event is a `ref` parameter: it cannot be captured or stored.
- **Handles, not objects**: `Entity` is a flecs id. After `Set`, `Remove`, `Destroy` or anything that adds or removes components or spawns entities, a `ref` from `Get`/`GetOrAdd`/a query column may dangle: keep the `Entity`, re-fetch, or copy the value out first. Do not change components of entities you are iterating; collect them and change them after the loop.
- **Components**: unmanaged structs only, registered with `Ecs.RegisterComponent<T>("plugin:name")` in `OnEnable` (`plugin.Ecs`). Same key and layout after a reload keeps the values. One CLR type belongs to one key. Engine components can be bound by name to read them; writes change only a copy, so change engine state through the typed handles (`Player`, `World`, `Entity`).
- **State and hot reload**: fields of the plugin object die on reload; components and files survive. Do not start threads or timers that outlive `OnDisable`, and never store plugin types in static fields of other assemblies (the runtime logs "was unloaded but is still referenced").
- **Registration**: only through the plugin's members (`Events`, `Commands`, `Systems`, `Ecs`, `Scheduler`, `Permissions`, `Services`, `Sidebars`, `BossBars`, ...). Anything else implements `IPluginResource` and goes through `Context.Track(...)`. Everything registered is released on unload; entities are not owned, so despawn the ones that must not outlive the plugin.
- **HTTP**: use the shared `Http.Client` (never dispose it); the server's `scripting.http` allow and deny lists apply.
- **Do not guess the API**: check `references/api.md` and `references/events.md`. A member that is not there does not exist.

## What is wired today

<!-- BEGIN GENERATED:status -->
- Events the engine fires today: ServerStartEvent, ServerStopEvent, TickStartEvent, TickEndEvent, ServerListPingEvent, CommandsRegisteredEvent, PluginEnableEvent, PluginDisableEvent, PermissionChangedEvent, PermissionsReloadedEvent, PlayerPreLoginEvent, PlayerJoinEvent, PlayerQuitEvent, PlayerMoveEvent, PlayerChatEvent, PrivateMessageEvent, PlayerCommandEvent, PlayerInteractBlockEvent, PlayerKickEvent, PlayerRespawnEvent, PlayerDeathEvent, PlayerCreativeSlotEvent, PlayerItemHeldEvent, PlayerSwapHandsEvent, PlayerDropItemEvent, PlayerPickupItemEvent, PlayerConsumeEvent, PlayerInteractEvent, PlayerInteractEntityEvent, PlayerToggleSneakEvent, PlayerToggleSprintEvent, PlayerToggleFlightEvent, PlayerGameModeChangeEvent, PlayerTeleportEvent, PlayerPortalEvent, PlayerChangedWorldEvent, PlayerResourcePackStatusEvent, PlayerLevelChangeEvent, BlockBreakEvent, BlockPlaceEvent, BlockDamageEvent, BlockIgniteEvent, BlockBurnEvent, BlockSpreadEvent, BlockFadeEvent, BlockFormEvent, BlockGrowEvent, BlockPistonExtendEvent, BlockPistonRetractEvent, BlockRedstoneEvent, EntitySpawnEvent, EntityDamageEvent, EntityDeathEvent, EntityCombustEvent, InventoryOpenEvent, InventoryCloseEvent, InventoryClickEvent, InventoryDragEvent, CraftItemEvent, PrepareCraftEvent, FurnaceSmeltEvent, FurnaceBurnEvent, EnchantItemEvent, PrepareAnvilEvent, PacketReceiveEvent, PacketSendEvent, ChunkLoadEvent, ChunkUnloadEvent, TimeSkipEvent, WeatherChangeEvent, PortalCreateEvent, MenuClickEvent, MenuCloseEvent, CustomItemUseEvent, CustomBlockPlaceEvent, CustomBlockBreakEvent, CustomBlockInteractEvent, NpcInteractEvent.
- `Server.SendMessage` / `Broadcast` reach players: yes.
- `Server.CurrentTick` is real: yes.
- Plugin commands (`Commands.Register`) run from in-game chat: yes.
- `plugin.json` manifest read by the loader: yes.
<!-- END GENERATED:status -->

Everything in `references/api.md` is implemented on this checkout. The event catalogue (`references/events.md`) lists every event you can subscribe to; events such as `BlockExplodeEvent` or `EntityTargetEvent` are not declared yet and do not exist for plugins.

## Recipes

These compile against the API on this checkout.

### Events

```csharp
[PluginInfo("ChatRules", Version = "1.0.0")]
public sealed class ChatRulesPlugin : Plugin
{
    protected override void OnEnable()
    {
        Events.On((ref PlayerChatEvent e) =>
        {
            if (e.Message.Contains("spam", StringComparison.OrdinalIgnoreCase)) e.Cancelled = true;
        }, EventPriority.High);                       // lower value runs first; Monitor runs last

        Events.On((ref PlayerChatEvent e) => Log.Info(e.Cancelled ? "blocked" : "delivered"),
                  EventPriority.Monitor, receiveCancelled: true);   // observe only, do not modify

        Events.On((ref EntityDamageEvent e) => e.Amount *= 0.5f);   // mutable field: the engine reads it back

        Events.On((ref PlayerCommandEvent e) =>                     // a typed command line, byte compare
        {
            if (!e.Command.EqualsAscii("ping"u8)) return;
            e.Cancelled = true;
            e.Player.SendMessage("pong");
        });
    }
}
```

Custom events go on the same bus. Fields must be blittable; the first field is the `EventHeader`:

```csharp
public struct HomeSetEvent : IEvent
{
    public EventHeader Header;
    public Entity Player;
    public int HomeCount;
    public static string Key => "homes:set";           // "plugin:name"
    public static bool IsCancellable => true;
}

var ev = new HomeSetEvent { Player = player, HomeCount = count };
if (!Events.Fire(ref ev)) return;                    // Fire returns false when a handler cancelled it
```

### Commands and permissions

```csharp
[PluginInfo("Homes", Version = "1.0.0")]
public sealed class HomesPlugin : Plugin
{
    private readonly Dictionary<Uuid, Location> _homes = [];

    protected override void OnEnable()
    {
        Permissions.Register("homes.use", PermissionDefault.True, "Use /sethome and /home");
        Permissions.Register(new Permission("homes.admin", PermissionDefault.OpLevel(3), "Visit other players' homes"));

        Commands.Register(Command.Literal("sethome").Permission("homes.use")
            .Executes(ctx => SetHome(ctx.RequirePlayer())),
            description: "Remember this place");

        Commands.Register(Command.Literal("home").Permission("homes.use")
            .Executes(ctx => GoHome(ctx.RequirePlayer(), ctx.RequirePlayer().Uuid))
            .Then(Command.Argument("player", Arg.Player()).Permission("homes.admin")
                .Executes(ctx => GoHome(ctx.RequirePlayer(), ctx.GetPlayer("player").Uuid))),
            aliases: ["h"], description: "Teleport to your home");

        Commands.Register("hello", ctx =>                   // flat form: the words after the name in ctx.Args
        {
            ctx.Reply($"Hello, {(ctx.Args.Length > 0 ? ctx.Args[0] : "world")}!");
            ctx.Result = ctx.Args.Length;
        }, "Says hello");
    }

    private int SetHome(Player player)
    {
        _homes[player.Uuid] = player.Location;
        player.SendMessage("Home set");
        return 1;
    }

    private int GoHome(Player who, Uuid owner)
    {
        if (!_homes.TryGetValue(owner, out var home))
            throw new CommandFailure("You have no home. Use /sethome");   // red message to the sender
        who.Teleport(home);
        return 1;
    }
}
```

- Build trees from `Command.Literal` and `Command.Argument(name, Arg.*)`; the root must be a literal. Types: `Arg.Int(min, max)`, `Arg.Word()`, `Arg.String()`, `Arg.GreedyString()`, `Arg.Player()`, `Arg.Vec3()`, `Arg.Item()`, `Arg.Time(min)`, ... (`references/api.md`, class `Arg`). No enum argument yet: `Arg.Word()` plus `Suggests`.
- Node methods: `Permission(node)`, `OpLevel(n)`, `Requires(src => ...)`, `Suggests(...)`, `Then(child)`, `Executes(...)`. The console passes `Permission` checks; `RequirePlayer()` throws `CommandFailure` for it.
- `CommandFailure` ends the command with a message (result 0, nothing logged). Any other exception shows a generic error and is logged. `ctx.Source.Failure(msg)` shows a message without ending.
- A name owned by vanilla or another plugin throws `InvalidOperationException` at registration.
- Permission defaults: `True`, `False`, `Op`, `OpLevel(n)`, `NotOp`. Checks: `player.HasPermission(node)`, `Permissions.Has(player, node)` (works with a `Uuid` too), `PermissionManager.GetExplicit(uuid, node)` for the tri-state `PermissionValue`. Undeclared nodes are denied; the console has all permissions. Resolution is most specific entry first, then groups, then the node default.

### Config and data files

```csharp
using System.Text.Json.Serialization;

public sealed class HomesConfig
{
    public int MaxHomes { get; set; } = 3;
    public int CooldownSeconds { get; set; } = 5;
}

[JsonSerializable(typeof(HomesConfig))]               // source generation: no reflection at run time
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class HomesJson : JsonSerializerContext { }

[PluginInfo("HomesConfig", Version = "1.0.0")]
public sealed class HomesConfigPlugin : Plugin
{
    private Config<HomesConfig> _config = null!;

    protected override void OnEnable()
    {
        _config = Config.Load(HomesJson.Default.HomesConfig);   // data/config.json; defaults are written on first run
        _config.Changed += c => Log.Info($"config reloaded: {c.MaxHomes} homes");
        _config.Watch = true;                                   // pick up edits on disk
        int max = _config.Value.MaxHomes;                       // immutable snapshot, safe from any thread

        if (Config.ReadText("homes.json") is { } text)          // your own files: atomic WriteText, null when missing
            Log.Info(text.Length.ToString());
        Config.WriteText("homes.json", "{}");
        Log.Info(max.ToString());
    }
}
```

- `DataFolder` (`plugins/<id>/data/`) holds every file the plugin writes. A broken edit keeps the previous values; `Config.Update(value)` saves and raises `Changed`.

### ECS: components, queries, systems

```csharp
public struct Mana
{
    public int Value;
    public int Max;
}

[PluginInfo("ManaPlugin", Version = "1.0.0")]
public sealed class ManaPlugin : Plugin
{
    private Query<Mana> _mana = null!;

    protected override void OnEnable()
    {
        Ecs.RegisterComponent<Mana>("mana:mana");          // key is global: "plugin:name"
        _mana = Ecs.Query<Mana>();                         // create once; up to three component types
        Events.On((ref PlayerJoinEvent e) => e.Player.Set(new Mana { Value = 20, Max = 20 }));
        Systems.Add("mana:regen", Phase.Tick, Regen);      // a method group: no per-tick allocation
    }

    private void Regen(float deltaTime)
    {
        foreach (var chunk in _mana)                       // one chunk per archetype
        {
            var mana = chunk.Field<Mana>(0);               // Span<Mana>; index = order of type arguments
            for (int i = 0; i < mana.Length; i++)
                if (mana[i].Value < mana[i].Max) mana[i].Value++;
        }
    }

    private void Drain(Player player)
    {
        int value = player.Get<Mana>().Value;              // copy out, then write back: no ref kept across Set
        player.Set(new Mana { Value = value - 1, Max = 20 });
    }
}
```

- Components hold no strings or references: keep variable data in a dictionary keyed by `Entity` (clean up when it dies) or on disk. Tags are empty structs. `Query<T1, T2, T3>` and `query.Each((Entity e, ref T1 a) => ...)` are the other forms.
- `Player` has `Get`, `TryGet`, `Has`, `Set`, `Remove` for your components. `Ecs.Spawn()` makes bare entities that are never sent to clients.
- Systems: `Systems.Add(name, Phase, callback)`, or `After`/`Before`/`Replace`/`SetEnabled` on engine systems (anchors in `references/native-systems.md`). Phases: `NetworkReceive`, `PreTick`, `Tick`, `PostTick`, `Tracking`, `NetworkSend`, `AsyncIntegration`.

### Scheduler and background work

```csharp
[PluginInfo("Tasks", Version = "1.0.0")]
public sealed class TasksPlugin : Plugin
{
    private ScheduledTask? _save;

    protected override void OnEnable()
    {
        Scheduler.RunLater(5 * 20, () => Server.Broadcast("five seconds later"));   // 20 ticks = 1 s
        _save = Scheduler.RunEvery(60 * 20, Save, initialDelayTicks: 10);

        Events.On((ref PlayerJoinEvent e) =>
        {
            Player player = e.Player;                       // copy what the worker needs; never the event
            string path = Path.Combine(DataFolder, "last-join.txt");
            _ = Scheduler.RunAsync(() =>                    // thread pool: no game API in here
            {
                string text = File.Exists(path) ? File.ReadAllText(path) : "";
                Scheduler.Post(() => { if (player.IsOnline) player.SendMessage(text); });   // back on the tick thread
            });
        });
    }

    protected override void OnDisable() => _save?.Cancel();   // optional: unloading cancels it too

    private void Save() => Log.Info("autosave");

    private async Task SaveAsync(string path, string text)
    {
        await Scheduler.ToAsync();                           // leave the tick thread before blocking work
        await File.WriteAllTextAsync(path, text);
        await Scheduler.ToTick();                            // back on the tick thread
        Log.Info($"saved {path}");
    }
}
```

- Scheduled tasks run on the tick thread; `RunLater` and `RunEvery` must be called on it. `Post`, `RunSync`, `RunAsync` and the awaitables are thread-safe. `RunSync` returns a task that completes after the action ran on the tick thread.
- A repeating task is rescheduled after it ran, so a slow run drifts later instead of piling up. Count time in ticks, not wall-clock.

### Display: titles, bars, sidebars

```csharp
[PluginInfo("Display", Version = "1.0.0")]
public sealed class DisplayPlugin : Plugin
{
    private Sidebar _sidebar = null!;
    private BossBar _bar = null!;

    protected override void OnEnable()
    {
        _sidebar = Sidebars.Create(Component.Text("Stats").Bold());
        _sidebar.SetLines([Component.Text("Online: 0")]);
        _bar = BossBars.Create("display:event", Component.Text("Event").Color(TextColor.Gold), BossBarColor.Red);

        Events.On((ref PlayerJoinEvent e) =>
        {
            Player p = e.Player;
            p.ShowTitle(Component.Text("Welcome"), Component.Text("to the server"), TitleTimes.Default);
            p.SendActionBar(Component.Text("hi"));
            _sidebar.Show(p);
            _bar.AddViewer(p);
        });

        Events.On((ref PlayerQuitEvent e) => _bar.RemoveViewer(e.Player));
    }
}
```

- Text is `Component` (`Component.Text`, `Bold`, `Color(TextColor.X)`, `Append`, `Parse(markup)`); a string converts to a literal component in `SendMessage`. `Server.Broadcast`, `Server.GetPlayer(name)` and `Server.Players` cover chat and lookups.
- `Server.SendMessage(entity, text)` sends a system message to a player entity, or to the console for `Entity.Null`.

### Services between plugins

```csharp
public interface IHomeCount
{
    int CountFor(Uuid player);
}

// provider plugin (OnEnable)
Services.Register<IHomeCount>(this, ServicePriority.Normal);   // removed when the plugin unloads

// consumer plugin
IHomeCount? counts = Services.TryGet<IHomeCount>();           // null when no provider
IHomeCount required = Services.Get<IHomeCount>();             // throws ServiceUnavailableException
Services.Changed<IHomeCount>(() => counts = Services.TryGet<IHomeCount>());
```

- Share the interface type through the provider's assembly: the consumer lists the provider in `depends` if it cannot work without it, or in `softDepends` if it can. Keep the interface small and stable. `Plugins.IsLoaded(id)` checks whether a plugin is present.

## Hot reload and `/plugins reload`

- Rebuilding with `RAMJET_PLUGINS` set copies the dll; the server waits 250 ms, loads the new version first, and only then unloads the old one. A failing new version leaves the old one running and the log says why.
- Survives: components with an unchanged layout and key, entities, files. Lost: the plugin's fields, its static caches and its delegates. Rebuild caches in `OnEnable`.
- `/plugins reload` disables every plugin (`OnDisable` runs), restarts the plugin worker and loads everything again from disk. A crash does the same automatically.
- Hard depend on the reloaded plugin: it is reloaded too.

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

- `dotnet build -c Release` has no warnings that matter, and the plugin loads on a scratch server (the log shows `enabled`).
- No allocation in hot handlers; no kept `ref`s, events or `Utf8String`s; no API use off the tick thread.
- Everything registered through the plugin's members; `OnDisable` only for your own cleanup.
- Anything that must survive a reload is in a component or on disk.

## Paper/Bukkit to Ramjet

| Paper | Ramjet |
|---|---|
| `JavaPlugin`, `onEnable/onDisable`, `plugin.yml` | `Plugin`, `OnEnable/OnDisable`, `[PluginInfo]`, `plugin.json` |
| `@EventHandler` method on a `Listener` | `Events.On((ref FooEvent e) => ...)` |
| `ignoreCancelled = true` | default (a cancelled event skips the handler); `receiveCancelled: true` for the opposite |
| `EventPriority.LOWEST..MONITOR` | `EventPriority.Lowest..Monitor` (lowest runs first) |
| `event.setCancelled(true)` | `e.Cancelled = true` |
| `CommandExecutor`, `plugin.yml` commands | `Commands.Register(Command.Literal(...))` with `Permission`, `Then`, `Executes` |
| `BukkitScheduler.runTaskLater/Timer` | `Scheduler.RunLater/RunEvery` (ticks) |
| `runTaskAsynchronously` + `runTask` | `Scheduler.RunAsync` + `Scheduler.Post` (or `RunSync`) |
| `getConfig()` | `Config.Load(...)` with a source-generated JSON context |
| `PersistentDataContainer` / metadata | an ECS component on the `Entity` |
| `ServicesManager` | `Services.Register<T>` / `TryGet<T>` / `Get<T>` |
| `BossBar`, `Scoreboard` | `BossBars.Create`, `Sidebars.Create` |
