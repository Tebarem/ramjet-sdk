# Worlds, blocks and chunks

Which worlds a server has, the `World` handle, reading and changing blocks, keeping chunks loaded, spawning things, time and weather, moving players between worlds, what is saved, and the thread rules.

## The worlds of a server

A server runs the three vanilla dimensions of its world folder: the **overworld**, the **nether** and the **end**. They exist from server start to server stop. In this version of the API a plugin cannot create, load, unload, copy or delete worlds (see [what is not available yet](#what-is-not-available-yet)); the server admin chooses the world folder and how a new one is generated.

A `World` is a **handle**: a small value naming one dimension, like an [`Entity`](entities-and-ecs.md). It is cheap to copy, compares by value, and stays valid for the life of the server.

| Code | Gives |
|---|---|
| `World.Overworld`, `World.Nether`, `World.End` (also `Server.Overworld`) | the vanilla dimensions |
| `Server.Worlds` | every dimension, overworld first |
| `Server.GetWorld("the_nether")` | the dimension with that key, or null (`"minecraft:the_nether"`; the `minecraft:` namespace may be left out) |
| `player.World`, `ctx.World`, `location.World` | where a player, a command source or a location is |

| Member | Meaning |
|---|---|
| `Key` | the dimension key, `"minecraft:overworld"`; also what `ToString()` returns |
| `Index` | the dimension slot (overworld 0, nether 1, end 2): the number `ChunkLoadEvent.Dimension` and `ChunkUnloadEvent.Dimension` carry (`new World(e.Dimension)`) |
| `IsValid` | whether the handle names a dimension of this server |
| `MinY`, `Height`, `MaxY` | the build limits (overworld: -64, 384 layers, 319) |
| `SeaLevel` | the sea level (overworld: 63) |
| `LoadedChunkCount`, `GeneratedChunkCount` | chunks in memory now; chunks generated from scratch since the server started (sample it twice for a rate) |

When you save a location in your own data, save the world's **key** with the coordinates and resolve it with `Server.GetWorld` when you load it.

## Reading and changing blocks

```csharp
var world = player.World;
var feet = player.Location.BlockPos;
var below = new BlockPos(feet.X, feet.Y - 1, feet.Z);

BlockState state = world.GetBlock(below);                 // air for an unloaded chunk or outside the build height
if (world.TryGetBlock(below, out var s) && s.BlockKey == "minecraft:grass_block")
    world.SetBlock(below, "minecraft:moss_block");        // like /setblock

world.SetBlock(below, BlockState.Parse("minecraft:oak_stairs[facing=north]"));
world.DestroyBlock(below, dropItems: true);               // breaks it like a player would: particles, sound, loot
int surface = world.HighestBlockY(below.X, below.Z);      // from the height maps: cheap
```

- **Only loaded chunks.** A read of an unloaded chunk returns air and does not load it (`TryGetBlock` and `IsBlockLoaded` tell you); a write to a chunk that is not fully loaded is dropped and `SetBlock` returns false. Load the chunk first with a [chunk ticket](#keeping-chunks-loaded).
- **`SetBlock` is not a player action.** It works like `/setblock`: the old block entity goes, the new block's placement logic and the neighbour updates run, the clients are told. No `BlockPlaceEvent` or `BlockBreakEvent` fires. It returns false when the block was already in that state.
- **Update flags** (`BlockUpdateFlags`, combined with `|`) choose what a change propagates. The default `All` notifies the neighbours (redstone, doors, plants that lose their support) and the clients. `Clients` alone skips the neighbour updates, which is faster for decoration where nothing around should react. `None` writes the block silently and the players do not see it: rarely what you want. The other flags are the game's own (`SuppressDrops`, `KnownShape`, `SkipOnPlace` ...); the API reference lists them.
- **Block states** are values: `BlockState.Parse("minecraft:oak_stairs[facing=north]")` (missing properties take the block's defaults; `TryParse` does not throw), `state.Key` (the full text), `state.BlockKey` (`"minecraft:oak_stairs"`). Parse once and keep the value: parsing looks the text up in the block registry. `IsAir` and `BlockState.Air` are `minecraft:air` only; caves and the void use `minecraft:cave_air` and `minecraft:void_air`, which are other states.
- **Block entities** (chests, signs, spawners ...): `GetBlockEntityData(pos)` returns a copy of the data as `/data get block` shows it (`NbtCompound`, or null); `SetBlockEntityData(pos, data, merge: true)` works like `/data merge block`. Containers have a typed view: `world.GetInventory(pos)`.
- **Big edits**: the game runs one tick in 50 ms and your code runs inside it. Spread an edit of many thousands of blocks over several ticks (a queue drained by `Scheduler.RunEvery`, a slice per tick) instead of doing it in one handler. See [performance](performance.md).

## Keeping chunks loaded

A chunk is in memory while a player is near it or something holds it. A plugin holds chunks with **chunk tickets** (`ChunkTickets`, a property of your plugin):

```csharp
ChunkTicket ticket = ChunkTickets.Add(world, chunkX, chunkZ);           // chunk coordinates: block >> 4
IReadOnlyList<ChunkTicket> area = ChunkTickets.AddArea(world, cx, cz, radius: 2);   // 5 x 5 chunks

if (ticket.IsLoaded) { /* blocks of the chunk can be read and changed */ }
ticket.Release();                                                         // or Dispose(); safe to call twice
```

- **Loading is asynchronous.** A ticket asks for the chunk; it is loaded from disk or generated over the next ticks (generation takes longer). Check `ticket.IsLoaded` (or `world.IsChunkLoaded`) from a repeating task and do the work when it is true; the [world travel guide](../guides/worlds-and-teleporting.md) has the full pattern.
- **Tickets are counted.** A chunk stays loaded until the last holder lets go, and may unload a little later. A held chunk also ticks like any other when a player is near enough.
- **Limits**: at most `ChunkTickets.MaxTicketsPerPlugin` (4096) tickets per plugin at once, `AddArea` radius at most 32, chunks inside the world border (blocks within 30,000,000 of the origin). `AddArea` is all or nothing.
- **Cleanup is automatic**: all of a plugin's tickets are released when it is disabled, unloaded or hot reloaded. Release a ticket as soon as you no longer need the chunk: a held chunk costs memory and, near players, tick time.

`ChunkLoadEvent` (with `NewlyGenerated`: true for a chunk generated now, false for one read from disk) and `ChunkUnloadEvent` tell you about chunks coming and going in any dimension. Both carry the dimension as `Dimension` (an index) and the chunk's `X` and `Z`.

## Entities, sounds and effects

| Member | What it does |
|---|---|
| `SpawnEntity(type, position, nbt, yaw, pitch)` | like `/summon`: mobs get their normal spawn logic (random attributes and equipment without NBT, the data as given with NBT); returns `Entity.Null` when the type is unknown, the chunk is not loaded, the difficulty forbids it or a handler cancelled `EntitySpawnEvent` |
| `DropItem(position, stack)` | an item entity |
| `Entities`, `Players` | snapshots of the entities and players of the dimension |
| `Explode(position, power, setFire, breakBlocks, source)` | an explosion like a creeper's (TNT is 4; above 32 is refused) |
| `PlaySound(sound, position, category, volume, pitch)` | a sound for the players in hearing range; returns how many got it |
| `SpawnParticle(particle, position, count, offset, speed, force)` | particles as `/particle` writes them (`"flame"`, `"dust 1 0 0 1"`); returns how many players got them |

`Entities` and `Players` build a new list on every call: read them once per handler, not in a loop.

## Time, weather and spawn

In this server version **only the overworld has a day/night clock, weather and a world spawn**. The nether and the end have a fixed time, always report clear weather, and their spawn is a placeholder.

| Member | Meaning |
|---|---|
| `GameTime` | ticks the dimension has been running (does not change with `/time set`) |
| `HasDayTime` | false in the nether and the end |
| `DayTime` (get, set), `AddDayTime(ticks)` | the time of day clock: 0 sunrise, 6000 noon, 12000 sunset, 18000 midnight; setting it fires `TimeSkipEvent`. Throws `InvalidOperationException` where there is no clock |
| `DayTimePaused`, `DayTimeRate` | `/time pause`; the clock speed (1 = normal) |
| `Weather`, `IsRaining`, `IsThundering`, `SetWeather(type, durationTicks)` | like `/weather`; fires `WeatherChangeEvent`; `SetWeather` returns false where there is no weather |
| `SpawnLocation`, `SetSpawn(pos, yaw, pitch)` | the world spawn (like `/setworldspawn`, saved with the world); `SetSpawn` returns false outside the overworld |

## World border

Every dimension has a **world border**: the square the server keeps players in, the one `/worldborder` changes. `world.Border` is the handle; it is a light view of the border, so read it again whenever you need the current state.

| Member | Meaning |
|---|---|
| `Center` (get, set) | the centre `(x, z)`; setting it moves the border at once, like `/worldborder center` |
| `Size`, `TargetSize`, `IsMoving`, `LerpTicksRemaining` | the size now, the size a moving border is heading to, whether a lerp is running and how many ticks it has left |
| `SetSize(size, lerpTimeMs = 0)` | like `/worldborder set <size> <time>`: at once when the time is 0, otherwise the border moves to `size` over that time (rounded up to whole 50 ms ticks) |
| `DamagePerBlock`, `SafeZone` (get, set) | the damage a living entity takes per block outside the border, and the distance inside it where the damage does not start yet (defaults 0.2 and 5) |
| `WarningDistance`, `WarningTime` (get, set) | how many blocks from the border the red warning shows (default 5), and how long before the border reaches a player, in ticks (default 300) |
| `IsInside(pos)`, `IsInside(location)` | whether a block or a position lies inside the square; a location in another dimension is outside |

Sizes run from 1 to 59,999,968 blocks, centre coordinates up to 29,999,984 in absolute value, and the settings are not negative. Any other value throws `ArgumentOutOfRangeException` and changes nothing. Every change reaches the players of the dimension at once, the physics (collisions, mob spawning) uses the new square from the next tick, and the border is saved with the world. A moving border stops moving while the game is frozen (`/tick freeze`), as in vanilla.

The packets `Packets.Clientbound.SetBorderCenter`, `SetBorderSize` and `SetBorderLerpSize` are a different thing: they change only what one client draws, so a border that exists for one player only. They do not move the server's border; use `world.Border` for that.

## Moving players between worlds

`player.Teleport(location)` moves a player anywhere, **also into another dimension**: the `Location` carries the world.

```csharp
var nether = Server.GetWorld("the_nether")!.Value;
bool moved = player.Teleport(new Location(nether, 0.5, 70, 0.5, yaw: 90f, pitch: 0f));
```

- It fires `PlayerTeleportEvent` with cause `Plugin`. A handler may cancel it (`Teleport` returns false) or change the destination position (`To`); the destination world (`ToWorld`) cannot be changed by the handler.
- After a change of dimension, whatever caused it, `PlayerChangedWorldEvent` fires with `FromWorld` and `ToWorld`.
- Portals are the game's own travel: `PlayerPortalEvent` (cancellable) fires before a player goes through a nether portal, an end portal or an end gateway, and `PortalCreateEvent` (cancellable) before a nether portal is lit or a destination portal is built.
- The server loads the chunks around the player at the destination as for any teleport. To choose a **safe height** from the terrain there (`HighestBlockY`, `GetBlock`), the chunk must be loaded first: hold a ticket, wait for `IsLoaded`, look, teleport, release the ticket. The [world travel guide](../guides/worlds-and-teleporting.md) does exactly that.
- `Entity.Teleport` on other entities (mobs, items) moves them **within their dimension** only.

## What is saved

- Blocks, block entities and entities that your plugin changes or spawns are part of the world: the server saves them with everything else (the autosave, `/save-all`, server stop). There is nothing to call.
- Your plugin's own knowledge about the world (warps, arenas, claimed regions, a block's owner) is yours to keep: write it to your [data folder](config-and-data.md), keyed by world key and position. There is no per-chunk or per-block plugin storage in the world itself.
- Reading or changing a world does not keep it loaded and does not save it on the spot.

## Threads

Every `World` member, `ChunkTickets` and `ChunkTicket.Release` must be called on the **tick thread**; elsewhere they throw `WrongThreadException`. Event handlers, commands, systems and scheduled tasks already run there. From async code, come back first with `await Scheduler.ToTick()` or `Scheduler.Post(...)` ([threading](threading.md)). Do slow work (HTTP, a database) off the tick thread, then apply the result to the world on it.

## What is not available yet

These are not part of the plugin API in this version. Do not build on them; this list says what exists today:

- Creating, loading, unloading, copying or deleting worlds at run time (Bukkit's `WorldCreator`, Multiverse-style multi-world servers), world templates, temporary instanced worlds (one world per match or per party), and saving a world into or loading it from a single file or a database (SlimeWorldManager-style worlds). A server has exactly the three dimensions of its world folder.
- A day/night clock, weather and a world spawn in the nether and the end.
- Changing a world's game rules or difficulty from the API (use the commands from the console). The world border can be changed from the API (see [World border](#world-border)).

Workarounds that work today: build arenas or lobbies in a far-away area of an existing dimension and keep their chunks loaded with tickets while they are in use; reset an arena by writing its blocks back from your own copy, spread over several ticks.
