# AGENTS.md: Ramjet plugin project

This is a C# plugin for **Ramjet** (a C++23, vanilla-compatible Minecraft Java server with a hot-reloading .NET 10 plugin layer). Read this file before changing code. A Claude Code skill with recipes and the full API listing is in `.claude/skills/ramjet-plugin/` (`SKILL.md`, `references/api.md`, `references/events.md`).

## Layout

- `MyPlugin.csproj`: net10.0 class library. References the `Ramjet.Api` package (the API as a reference assembly, compile-time only; never copy a `Mc.Scripting.Api.dll` next to the plugin). Keep `EnableDynamicLoading=true`.
- `PluginMain.cs`: the plugin class (`Plugin` subclass, `[PluginInfo]`, `OnEnable` / `OnDisable`). One public plugin class per assembly; no work in its constructor.
- `plugin.json`: manifest for Plugin API v1 (ignored by servers that do not read it yet). Keep `id`, `name`, `version`, `main` consistent with `[PluginInfo]` and the class.

## Build, run, reload

```bash
# Ramjet.Api comes from NuGet; until it is published: dotnet nuget add source <folder with Ramjet.Api.*.nupkg> -n ramjet
export RAMJET_PLUGINS=/path/to/run/plugins     # optional: each build copies the plugin there; the server hot-reloads it
dotnet build -c Release
```

The server must be started with `--plugins <dir>`. Hot reload: the server watches that folder; a rebuilt dll replaces the running plugin (new version enabled first, old one released after; a failing new version leaves the old one running).

For a small plugin you can skip the project: a single `.cs` file (or a folder of `.cs` files) in the plugins folder is compiled by the server itself (`plugins.allow_source`, default true), with `[assembly: McPlugin("id", version: "1.0.0")]` as the manifest; see the "Source plugins" section of `.claude/skills/ramjet-plugin/SKILL.md`.

## Hard rules

<!-- BEGIN GENERATED:hard-rules -->
1. All plugin code runs on the server tick thread; Scheduler.Post is the only thread-safe entry point.
2. Event handlers receive the event by ref and must not allocate: no LINQ, no string formatting or interpolation on the common path, no closures created per call.
3. Utf8String / NativeBytes event fields are borrowed for the duration of the dispatch; call ToString()/ToArray() to keep a copy.
4. Entity is a handle (flecs id), not an object: check IsAlive, never store the ref returned by Get<T>() across calls that can add or remove components or destroy entities.
5. Component types are plain unmanaged structs owned by exactly one plugin and registered with World.RegisterComponent<T>(key).
6. Register everything through the plugin context (Events, Commands, Systems, Scheduler, World); it is released automatically on unload and hot reload.
<!-- END GENERATED:hard-rules -->

Consequences you must respect:

1. **Hot paths do not allocate**: handlers of `PlayerMove`, `TickStart/End`, `EntityDamage`, block events, systems and frequently repeating tasks must not use LINQ, string interpolation/formatting, boxing, `params` or fresh capturing lambdas. Create delegates once in `OnEnable`. Read `Utf8String` in place; call `ToString()` only when a managed string is really needed.
2. **Never keep** the event (`ref` struct), `Utf8String`/`NativeBytes` fields, or a `ref T` from `Entity.Get<T>()` beyond the current call or across a structural change (`Set/Remove/Destroy`, spawning). Keep the `Entity` handle and re-fetch.
3. **Never call the API off the tick thread**. Do slow work in `Task.Run`, copy the data you need first, and come back with `Scheduler.Post(...)`; check `entity.IsAlive` again.
4. **State**: plugin fields die on hot reload; ECS components (registered with `World.RegisterComponent<T>("plugin:name")`, unmanaged structs only) and files survive.
5. **Register only through the plugin's services** (`Events`, `Commands`, `Systems`, `Scheduler`, `World`) so unloading releases everything. No static references from other assemblies to plugin types, no threads or timers that outlive `OnDisable`.

## Inventories, custom GUIs, custom items and blocks

`player.Inventory` (`Give`, `AddItem`, `RemoveItem`, `MainHand` ...), immutable `ItemStack` values (`WithName`, `WithLore`, `WithEnchantment`, `WithCustomData`, `WithComponent`), `BlockInventory`, locked custom GUIs
(`Menus.Chest(rows, title).Slot(i, icon, click => ...)`, `Open(player)`: a click on a menu slot is denied unless the handler calls `click.Allow()`, a failing handler denies it), `CustomItem` / `CustomBlock` registered with `Content.Items.Register` / `Content.Blocks.Register`
in `OnEnable`, `ResourcePackBuilder` and `ResourcePackHost`. Everything is tick-thread only and released when the plugin unloads. See the "Inventories, custom GUIs and custom content" section of `.claude/skills/ramjet-plugin/SKILL.md`.

## What the engine actually fires and wires

<!-- BEGIN GENERATED:status -->
- Events the engine fires today: ServerStartEvent, ServerStopEvent, TickStartEvent, TickEndEvent, ServerListPingEvent, CommandsRegisteredEvent, PluginEnableEvent, PluginDisableEvent, PermissionChangedEvent, PermissionsReloadedEvent, PlayerPreLoginEvent, PlayerJoinEvent, PlayerQuitEvent, PlayerMoveEvent, PlayerChatEvent, PrivateMessageEvent, PlayerCommandEvent, PlayerInteractBlockEvent, PlayerKickEvent, PlayerRespawnEvent, PlayerDeathEvent, PlayerCreativeSlotEvent, PlayerItemHeldEvent, PlayerSwapHandsEvent, PlayerDropItemEvent, PlayerPickupItemEvent, PlayerConsumeEvent, PlayerInteractEvent, PlayerInteractEntityEvent, PlayerToggleSneakEvent, PlayerToggleSprintEvent, PlayerToggleFlightEvent, PlayerGameModeChangeEvent, PlayerTeleportEvent, PlayerPortalEvent, PlayerChangedWorldEvent, PlayerResourcePackStatusEvent, PlayerLevelChangeEvent, BlockBreakEvent, BlockPlaceEvent, BlockDamageEvent, BlockIgniteEvent, BlockBurnEvent, BlockSpreadEvent, BlockFadeEvent, BlockFormEvent, BlockGrowEvent, BlockPistonExtendEvent, BlockPistonRetractEvent, BlockRedstoneEvent, EntitySpawnEvent, EntityDamageEvent, EntityDeathEvent, EntityCombustEvent, InventoryOpenEvent, InventoryCloseEvent, InventoryClickEvent, InventoryDragEvent, CraftItemEvent, PrepareCraftEvent, FurnaceSmeltEvent, FurnaceBurnEvent, EnchantItemEvent, PrepareAnvilEvent, PacketReceiveEvent, PacketSendEvent, ChunkLoadEvent, ChunkUnloadEvent, TimeSkipEvent, WeatherChangeEvent, PortalCreateEvent, MenuClickEvent, MenuCloseEvent, CustomItemUseEvent, CustomBlockPlaceEvent, CustomBlockBreakEvent, CustomBlockInteractEvent, NpcInteractEvent.
- `Server.SendMessage` / `Broadcast` reach players: yes.
- `Server.CurrentTick` is real: yes.
- Plugin commands (`Commands.Register`) run from in-game chat: yes.
- `plugin.json` manifest read by the loader: yes.
<!-- END GENERATED:status -->

Exact signatures of every public type are in `.claude/skills/ramjet-plugin/references/api.md`: do not guess the API.

## Events (catalogue)

Lower priority value runs first (`Lowest` ... `Monitor`). `e.Cancelled = true` vetoes a cancellable event; later handlers are skipped unless subscribed with `receiveCancelled: true`. Mutable fields are written by the handler and read back by the engine.

<!-- BEGIN GENERATED:events-table -->
| Event struct | Id | Cancellable | Engine | Mutable fields | Read-only fields |
|---|---:|---|---|---|---|
| `ServerStartEvent` | 1 | no | fired | - | - |
| `ServerStopEvent` | 2 | no | fired | - | - |
| `TickStartEvent` | 3 | no | fired | - | Tick:ulong |
| `TickEndEvent` | 4 | no | fired | - | Tick:ulong, TickNanos:long |
| `ServerListPingEvent` | 100 | no | fired | Motd, MaxPlayers, OnlinePlayers, HidePlayers | Connection:ulong, Address:Utf8String, ProtocolVersion:int |
| `CommandsRegisteredEvent` | 101 | no | fired | - | - |
| `PluginEnableEvent` | 102 | no | fired | - | PluginId:Utf8String, PluginName:Utf8String, Version:Utf8String |
| `PluginDisableEvent` | 103 | no | fired | - | PluginId:Utf8String, PluginName:Utf8String, Version:Utf8String |
| `PermissionChangedEvent` | 104 | no | fired | - | Player:Uuid, AllPlayers:bool |
| `PermissionsReloadedEvent` | 105 | no | fired | - | - |
| `PlayerPreLoginEvent` | 10 | yes | fired | KickMessage | Connection:ulong, Uuid:Uuid, Username:Utf8String, Address:Utf8String, Deferrals:int |
| `PlayerJoinEvent` | 11 | no | fired | JoinMessage | Player:player, Uuid:Uuid, Username:Utf8String |
| `PlayerQuitEvent` | 12 | no | fired | QuitMessage | Player:player, Uuid:Uuid, Username:Utf8String, Reason:Utf8String |
| `PlayerMoveEvent` | 13 | yes | fired | To, Yaw, Pitch | Player:player, From:Vec3d, OnGround:bool |
| `PlayerChatEvent` | 14 | yes | fired | Message, Rendered, SenderName, ChatType, Decoration, FilterKind, FilterBits | Player:player |
| `PrivateMessageEvent` | 305 | yes | fired | Message, Decoration | Sender:player, Target:player |
| `PlayerCommandEvent` | 15 | yes | fired | Command | Player:player |
| `PlayerInteractBlockEvent` | 16 | yes | fired | - | Player:player, Pos:BlockPos, Face:byte, Hand:byte, Item:item |
| `PlayerKickEvent` | 110 | yes | fired | Reason, LeaveMessage | Player:player |
| `PlayerRespawnEvent` | 111 | no | fired | Position, World | Player:player, Reason:enum:RespawnReason, IsSpawnPoint:bool |
| `PlayerDeathEvent` | 112 | no | fired | DeathMessage, DropInventory, KeepLevel, DropExperience | Player:player, Killer:Entity, Cause:enum:DamageCause |
| `PlayerCreativeSlotEvent` | 129 | yes | fired | - | Player:player, Slot:int, Item:item |
| `PlayerItemHeldEvent` | 113 | yes | fired | - | Player:player, PreviousSlot:int, NewSlot:int |
| `PlayerSwapHandsEvent` | 114 | yes | fired | - | Player:player, MainHand:item, OffHand:item |
| `PlayerDropItemEvent` | 115 | yes | fired | - | Player:player, Item:item, WholeStack:bool |
| `PlayerPickupItemEvent` | 116 | yes | fired | - | Player:player, ItemEntity:Entity, Item:item |
| `PlayerConsumeEvent` | 117 | yes | fired | - | Player:player, Item:item, Hand:enum:Hand |
| `PlayerInteractEvent` | 118 | yes | fired | - | Player:player, Action:enum:InteractAction, Hand:enum:Hand, Item:item, Pos:BlockPos, Face:enum:BlockFace, HasBlock:bool |
| `PlayerInteractEntityEvent` | 119 | yes | fired | - | Player:player, Target:Entity, Hand:enum:Hand, Item:item |
| `PlayerToggleSneakEvent` | 120 | yes | fired | - | Player:player, Sneaking:bool |
| `PlayerToggleSprintEvent` | 121 | yes | fired | - | Player:player, Sprinting:bool |
| `PlayerToggleFlightEvent` | 122 | yes | fired | - | Player:player, Flying:bool |
| `PlayerGameModeChangeEvent` | 123 | yes | fired | - | Player:player, OldMode:enum:GameMode, NewMode:enum:GameMode, Cause:enum:GameModeChangeCause |
| `PlayerTeleportEvent` | 124 | yes | fired | To | Player:player, From:Vec3d, FromWorld:world, ToWorld:world, Cause:enum:TeleportCause |
| `PlayerPortalEvent` | 125 | yes | fired | - | Player:player, From:Vec3d, FromWorld:world, ToWorld:world, Cause:enum:TeleportCause |
| `PlayerChangedWorldEvent` | 126 | no | fired | - | Player:player, FromWorld:world, ToWorld:world |
| `PlayerResourcePackStatusEvent` | 127 | no | fired | - | Player:player, PackId:Uuid, Status:enum:ResourcePackStatus |
| `PlayerLevelChangeEvent` | 128 | no | fired | - | Player:player, OldLevel:int, NewLevel:int |
| `BlockBreakEvent` | 20 | yes | fired | DropItems, Experience | Player:player, Pos:BlockPos, BlockState:blockstate, Tool:item |
| `BlockPlaceEvent` | 21 | yes | fired | BlockState | Player:player, Pos:BlockPos, Against:BlockPos, Hand:byte, Item:item |
| `BlockDamageEvent` | 180 | yes | fired | InstaBreak | Player:player, Pos:BlockPos, Face:enum:BlockFace, Tool:item |
| `BlockIgniteEvent` | 181 | yes | fired | - | World:world, Pos:BlockPos, Cause:enum:IgniteCause, IgnitingEntity:Entity |
| `BlockBurnEvent` | 182 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate |
| `BlockSpreadEvent` | 183 | yes | fired | - | World:world, Pos:BlockPos, Source:BlockPos, NewState:blockstate |
| `BlockFadeEvent` | 184 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate, NewState:blockstate |
| `BlockFormEvent` | 185 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate, NewState:blockstate |
| `BlockGrowEvent` | 186 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate, NewState:blockstate |
| `BlockPistonExtendEvent` | 187 | yes | fired | - | World:world, Pos:BlockPos, Direction:enum:BlockFace, Sticky:bool, MovedBlocks:int |
| `BlockPistonRetractEvent` | 188 | yes | fired | - | World:world, Pos:BlockPos, Direction:enum:BlockFace, Sticky:bool, MovedBlocks:int |
| `BlockRedstoneEvent` | 189 | no | fired | NewPower | World:world, Pos:BlockPos, OldPower:int |
| `EntitySpawnEvent` | 30 | yes | fired | - | Entity:Entity, EntityType:key.entity_type, Position:Vec3d, Reason:enum:SpawnReason |
| `EntityDamageEvent` | 31 | yes | fired | Amount | Target:Entity, Attacker:Entity, DamageType:key.damage_type, Cause:enum:DamageCause, RawAmount:float, BlockSource:BlockPos, HasBlockSource:bool |
| `EntityDeathEvent` | 32 | no | fired | - | Entity:Entity, Killer:Entity, DamageType:key.damage_type, Cause:enum:DamageCause |
| `EntityCombustEvent` | 221 | yes | fired | DurationTicks | Entity:Entity, Source:Entity |
| `InventoryOpenEvent` | 260 | yes | fired | - | Player:player, Kind:enum:InventoryKind, HasPos:bool, Pos:BlockPos |
| `InventoryCloseEvent` | 261 | no | fired | - | Player:player, Kind:enum:InventoryKind |
| `InventoryClickEvent` | 262 | yes | fired | - | Player:player, Kind:enum:InventoryKind, Slot:int, ClickType:enum:ClickType, Button:int, Clicked:item, Cursor:item |
| `InventoryDragEvent` | 263 | yes | fired | - | Player:player, Kind:enum:InventoryKind, DragType:enum:DragType, Slots:NativeBytes, Cursor:item |
| `CraftItemEvent` | 264 | no | fired | - | Player:player, Recipe:Utf8String, Result:item, ShiftClick:bool |
| `PrepareCraftEvent` | 265 | yes | fired | - | Player:player, Recipe:Utf8String, Result:item |
| `FurnaceSmeltEvent` | 266 | yes | fired | - | World:world, Pos:BlockPos, Source:item, Result:item |
| `FurnaceBurnEvent` | 267 | yes | fired | BurnTicks | World:world, Pos:BlockPos, Fuel:item |
| `EnchantItemEvent` | 268 | yes | fired | - | Player:player, Item:item, Option:int, Cost:int |
| `PrepareAnvilEvent` | 269 | yes | fired | RepairCost | Player:player, First:item, Second:item, Result:item, RenameText:Utf8String |
| `PacketReceiveEvent` | 40 | yes | fired | - | Connection:ulong, Player:player, State:byte, PacketId:int, Payload:NativeBytes |
| `PacketSendEvent` | 41 | yes | fired | - | Connection:ulong, Player:player, State:byte, PacketId:int, Payload:NativeBytes |
| `ChunkLoadEvent` | 50 | no | fired | - | Dimension:int, X:int, Z:int, NewlyGenerated:bool |
| `ChunkUnloadEvent` | 51 | no | fired | - | Dimension:int, X:int, Z:int |
| `TimeSkipEvent` | 300 | no | fired | - | World:world, Reason:enum:TimeSkipReason, SkipAmount:long |
| `WeatherChangeEvent` | 301 | no | fired | - | World:world, Raining:bool, Thundering:bool |
| `PortalCreateEvent` | 303 | yes | fired | - | World:world, Reason:enum:PortalCreateReason, BlockCount:int |
| `MenuClickEvent` | 270 | yes | fired | Allow | Player:player, Menu:int, Slot:int, ClickType:enum:ClickType, Button:int, Clicked:item, Cursor:item |
| `MenuCloseEvent` | 271 | no | fired | - | Player:player, Menu:int, Reason:enum:MenuCloseReason |
| `CustomItemUseEvent` | 272 | yes | fired | - | Player:player, CustomId:Utf8String, Hand:enum:Hand, Item:item, HasBlock:bool, Pos:BlockPos, Face:enum:BlockFace |
| `CustomBlockPlaceEvent` | 273 | yes | fired | - | Player:player, CustomId:Utf8String, World:world, Pos:BlockPos, Against:BlockPos, Hand:enum:Hand |
| `CustomBlockBreakEvent` | 274 | yes | fired | DropItems | Player:player, CustomId:Utf8String, World:world, Pos:BlockPos |
| `CustomBlockInteractEvent` | 275 | yes | fired | - | Player:player, CustomId:Utf8String, World:world, Pos:BlockPos, Face:enum:BlockFace, Hand:enum:Hand, Item:item |
| `NpcInteractEvent` | 304 | no | fired | - | Player:player, Npc:ulong, Action:enum:NpcClick, Hand:enum:Hand, Sneaking:bool |
<!-- END GENERATED:events-table -->

## Testing

Test on a server you control, never one people play on (a test server on your host, or your own). Put the plugin into its `plugins/`
folder (`RAMJET_PLUGINS=<that folder>` makes every `dotnet build` copy it there), then watch the server log:

- `[MyPlugin] enabled v1.0.0 (id N)`: loaded. A rebuild that replaces the dll is hot-reloaded (a second `enabled` line).
- `/plugins` shows the plugin worker's state; `/plugins reload` restarts every plugin (operators only).
- An exception in `OnEnable` is logged with its stack trace, and the plugin is not loaded.

Join with a Minecraft client to check commands, events and GUIs in game.

## Style

C# `latest`, nullable enabled, file-scoped namespaces. Comments explain why, not what. Plugin log lines go through `Log` (prefixed with the plugin name).
