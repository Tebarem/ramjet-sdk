# Migrating from Paper / Bukkit

A concept-by-concept mapping from the Bukkit API to the Ramjet C# plugin API, with side-by-side code and the surprises to expect.

If you have written Paper plugins you already know most of the concepts. This page maps them and says what is **different on purpose**.

## Concept mapping

### Plugin model

| Paper / Bukkit | Ramjet |
|---|---|
| `JavaPlugin` | `Plugin` (derive; `[PluginInfo]` for metadata) |
| `onEnable()` / `onDisable()` | `OnEnable()` / `OnDisable()` |
| `onLoad()` | none: do the work in `OnEnable` (the manifest is read before your code loads) |
| `plugin.yml` | [`plugin.json`](../reference/plugin-json.md) (and `[PluginInfo]`) |
| `depend` / `softdepend` / `loadbefore` | `depends` / `softDepends` / `loadBefore` (with version constraints) |
| `api-version` | `"api": "1.0"` (checked against the server's API version) |
| `getLogger()` | `Log` |
| `getDataFolder()`, `saveDefaultConfig()`, `getConfig()` | `DataFolder`, `Config.Load(...)` returning a typed `Config<T>` (source-generated JSON) |
| Maven/Gradle, shading | `dotnet build`, NuGet packages loaded privately into the plugin |
| PlugMan / `/reload` | hot reload on every rebuild, built in |
| `Bukkit.getServer()` static access | `Server` static class |

### Events

| Paper / Bukkit | Ramjet |
|---|---|
| `class X implements Listener` + `@EventHandler` methods | `Events.On((ref FooEvent e) => ...)` delegate, no reflection scanning |
| `Bukkit.getPluginManager().registerEvents(...)` | `Events.On(...)` returns a `Subscription` |
| `EventPriority.LOWEST ... MONITOR` | `EventPriority.Lowest ... Monitor` (same order: lowest first) |
| `@EventHandler(ignoreCancelled = true)` | the **default**: cancelled events skip your handler |
| `@EventHandler` (ignoreCancelled false: sees cancelled) | `receiveCancelled: true` |
| `event.setCancelled(true)` | `e.Cancelled = true` (only on cancellable events) |
| `event.getPlayer()` | `e.Player` (a `Player` handle) |
| `Cancellable`, custom `Event` + `HandlerList` | a struct implementing `IEvent` with a `Key`; `Events.Fire(ref e)` |
| `AsyncPlayerChatEvent` | `PlayerChatEvent` (synchronous, one event) |
| `PlayerJoinEvent#setJoinMessage` | `e.SetJoinMessage(component)` |
| the event catalogue | [89 events](../reference/events.md): player, block, entity, inventory, world, server. Paper has more; [Events compared with Paper](../reference/events-compared-with-paper.md) maps every Paper event to a Ramjet event and lists what is missing and planned |

### Scheduling and threads

| Paper / Bukkit | Ramjet |
|---|---|
| `runTaskLater(plugin, r, delay)` | `Scheduler.RunLater(delayTicks, action)` |
| `runTaskTimer(plugin, r, delay, period)` | `Scheduler.RunEvery(period, action, initialDelay)` |
| `BukkitTask#cancel()` | `ScheduledTask.Cancel()` / `Dispose()` |
| `runTask` (next tick, from async) | `Scheduler.Post(action)` (thread-safe) / `RunSync` |
| `runTaskAsynchronously` | `Scheduler.RunAsync(...)` or `Task.Run`, then `Scheduler.Post` |
| main thread rules | the **tick thread** owns everything; same rule, and calling from another thread throws `WrongThreadException` |
| `isPrimaryThread()` | `Server.IsTickThread` |

### Commands and permissions

| Paper / Bukkit | Ramjet |
|---|---|
| `commands:` in `plugin.yml` + `CommandExecutor.onCommand(sender, cmd, label, args)` | `Commands.Register(Command.Literal(...)...)`: a real Brigadier node |
| `TabCompleter` | `.Suggests(ctx => ...)` on an argument node |
| manual `args[]` parsing | typed arguments (`Arg.Int`, `Arg.Player`, `Arg.BlockPos`, ...) validated by client and server |
| `sender instanceof Player` | `ctx.Player` / `ctx.RequirePlayer()` |
| `return false` (usage message) | `throw new CommandFailure("...")` |
| `player.hasPermission("x")` | `player.HasPermission("x")` / `.Permission("x")` on the node |
| `permissions:` in `plugin.yml`, `Permission` defaults (`true/false/op/not op`) | `Permissions.Register(node, PermissionDefault.True/False/Op/NotOp, doc)` or the manifest |
| Vault-style permission plugins | `Permissions.SetProvider(IPermissionProvider)` |

### Data, entities and the world

| Paper / Bukkit | Ramjet |
|---|---|
| `Entity` / `Player` object | `Entity` / `Player` **handle** (a struct); the data is in ECS components |
| `entity.isValid()` / `player.isOnline()` | `entity.IsAlive` / `player.IsOnline` |
| `PersistentDataContainer`, `setMetadata` | your own component: `entity.Set(new Mana { ... })` |
| `Player#getLocation`, `World`, `Location` | `player.Location`, `player.World` (a handle), `Location` |
| `Block`, `BlockState`, `world.getBlockAt` | `world.GetBlock(pos)` returns a `BlockState` value |
| `Bukkit.getWorld(name)`, `Bukkit.getWorlds()` | `Server.GetWorld("the_nether")`, `Server.Worlds` ([worlds](../concepts/worlds.md)) |
| `block.setType` / `setBlockData` | `world.SetBlock(pos, "minecraft:stone")` (no `BlockPlaceEvent`; update flags choose the neighbour updates) |
| `world.addPluginChunkTicket`, `chunk.load()` | `ChunkTickets.Add(world, cx, cz)` (asynchronous: wait for `IsLoaded`) |
| `world.spawnEntity`, `world.dropItem` | `world.SpawnEntity("minecraft:zombie", pos, nbt)`, `world.DropItem(pos, stack)` |
| `player.teleport(location)` to another world | `player.Teleport(new Location(world, x, y, z))` ([travelling between worlds](worlds-and-teleporting.md)) |
| `WorldCreator`, `Bukkit.createWorld`, `unloadWorld`, Multiverse | not available: a server has the three dimensions of its world folder |
| `Bukkit.getOnlinePlayers()` | `Server.Players` (snapshot) |
| `ItemStack`, `Inventory`, GUI via `InventoryClickEvent` | `ItemStack` value, `player.Inventory`, `Menus.Chest(...)` ([custom GUIs](custom-guis.md)) |
| `BossBar`, `Scoreboard` sidebar, `Title` | `BossBars.Create`, `Sidebars.Create`, `player.ShowTitle` ([boss bars](boss-bars-and-titles.md), [sidebar](scoreboard-sidebar.md)) |
| Adventure `Component`, MiniMessage | `Component` (`Component.Text`, `Component.Parse`, `Component.Legacy`) |
| `ServicesManager` | `Services.Register/TryGet/Changed` ([guide](inter-plugin-services.md)) |
| NMS / packet libraries (ProtocolLib) | `PacketReceiveEvent`, and the typed packets of `Mc.Scripting.Packets` |
| Custom items/blocks via resource pack tricks | `Mc.Scripting.Content` (`Content.Items`, `Content.Blocks`, `ResourcePackBuilder`) |
| Citizens-style NPCs, hologram plugins | `Npcs`, `Holograms` |

## Side by side

### A listener

```java
// Paper
public final class ChatListener implements Listener {
    @EventHandler(priority = EventPriority.HIGH, ignoreCancelled = true)
    public void onChat(AsyncPlayerChatEvent e) {
        if (e.getMessage().toLowerCase().contains("spam")) e.setCancelled(true);
    }
}
// in onEnable
getServer().getPluginManager().registerEvents(new ChatListener(), this);
```

```csharp
// ramjet
Events.On((ref PlayerChatEvent e) =>
{
    if (e.Message.Contains("spam", StringComparison.OrdinalIgnoreCase)) e.Cancelled = true;
}, EventPriority.High);
```

### A scheduler task

```java
// Paper
new BukkitRunnable() { public void run() { getLogger().info("tick"); } }.runTaskTimer(this, 0L, 20L * 60);
```

```csharp
// ramjet
Scheduler.RunEvery(20 * 60, () => Log.Info("tick"));
```

### A command

```java
// Paper: plugin.yml declares "sethome", then
@Override public boolean onCommand(CommandSender s, Command c, String l, String[] a) {
    if (!(s instanceof Player p)) { s.sendMessage("Players only."); return true; }
    if (a.length != 1) return false;
    homes.put(p.getUniqueId(), a[0], p.getLocation());
    return true;
}
```

```csharp
// ramjet
Commands.Register(Command.Literal("sethome")
    .Permission("homes.set")
    .Then(Command.Argument("name", Arg.Word())
        .Executes(ctx =>
        {
            Player p = ctx.RequirePlayer();                    // fails with a proper message for the console
            homes.Set(p.Uuid, ctx.GetString("name"), p.Location);
            return 1;
        })));
```

## What surprises Paper developers

1. **Events are structs passed by `ref`.** You cannot capture the event in a lambda or keep it after the handler returns. Copy what you need. String fields are borrowed UTF-8 views (`Utf8String`): `ToString()` to keep one.
2. **Allocation matters.** The tick thread has no spare time for GC pauses; hot-path handlers must not allocate. See [performance](../concepts/performance.md).
3. **State in fields does not survive a reload; state in components does.** Plugins are rebuilt and reloaded constantly; design for it ([hot reload](../concepts/hot-reload.md)). Avoid the `getInstance()` static singleton: statics die with the plugin's load context, and a static in another assembly that points into your plugin leaks it.
4. **There is no `Player` object to hold on to.** Hold a handle and check `IsOnline` (or `IsAlive` for an entity) when you come back later. Stale handles fail safe: reads return defaults and writes do nothing.
5. **Ticks, not milliseconds.** Durations are ticks (20 per second) unless a parameter is a `TimeSpan`.
6. **Commands are Brigadier-native**, so there is no `plugin.yml` command section, no `label`, no hand-written tab completion for the basics.
7. **No reflection in the API.** No `@EventHandler` scanning, no annotation processing: registrations are explicit calls, which is also what makes ownership and unloading exact.
8. **The game's own components are read-only.** You change the game through the typed calls (`player.Health`, `player.Teleport`, `world.SetBlock`); components you register yourself are yours to read and write.
