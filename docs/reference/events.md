# Event catalogue

Every event of the plugin API with its fields, mutability and cancellability.

78 events are in the catalogue. All of them are declared in the `Mc.Scripting.Events` namespace and can be subscribed to with `Events.On`; see [Events](../concepts/events.md) for subscribing, priorities and cancellation. Plugins can add their own events (see [custom events](../concepts/events.md#custom-events)).

**Cancellable** events can be vetoed with `e.Cancelled = true`. **Mutable** fields can be changed by handlers and are read back by the code that fired the event: numbers, flags and handles are assigned directly (`e.Amount *= 0.5f`), text and components are replaced with the `SetX(...)` method shown in the table (`e.SetJoinMessage(Component.Legacy("&eHi"))`). All other fields are read-only. Text fields of type `Utf8String` are only valid during the dispatch; copy with `ToString()` to keep them. Component fields are parsed on access (and allocate), so read them only when you need them.

Every event except `PacketSendEvent` is fired by the server. `PacketSendEvent` is reserved: you can subscribe to it, but nothing fires it yet.

Handlers of events that fire very often (`PlayerMoveEvent`, `TickStartEvent`, `TickEndEvent`) must not allocate; see [performance](../concepts/performance.md).


## Server and tick

### ServerStartEvent

Fired once after the server finished starting, before the first tick. Not cancellable.

### ServerStopEvent

Fired once when the server begins shutting down. Not cancellable.

### TickStartEvent

Fired at the start of every server tick. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Tick` | `ulong` | no | The tick number. |

### TickEndEvent

Fired at the end of every server tick. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Tick` | `ulong` | no | The tick number. |
| `TickNanos` | `long` | no | Wall time spent in this tick. |

### ServerListPingEvent

A client asked for the server list status. Change the MOTD, the player counts or hide the player sample. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Connection` | `ulong` | no | Connection id. |
| `Address` | `Utf8String` | no | Client address. |
| `ProtocolVersion` | `int` | no | Protocol version the client announced. |
| `Motd` | `Component` | **yes**, `SetMotd(...)` | The message of the day. |
| `MaxPlayers` | `int` | **yes** | Maximum player count shown. |
| `OnlinePlayers` | `int` | **yes** | Online player count shown. |
| `HidePlayers` | `bool` | **yes** | Do not send the player sample. |

### CommandsRegisteredEvent

The command tree changed (a plugin registered or removed commands, or a permission changed). Fired once per tick at most, before the trees are resent to the players. Not cancellable.

### PluginEnableEvent

A plugin finished enabling. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `PluginId` | `Utf8String` | no | The plugin id. |
| `PluginName` | `Utf8String` | no | The plugin name. |
| `Version` | `Utf8String` | no | The plugin version. |

### PluginDisableEvent

A plugin was disabled (unload or hot reload); its registrations are already released. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `PluginId` | `Utf8String` | no | The plugin id. |
| `PluginName` | `Utf8String` | no | The plugin name. |
| `Version` | `Utf8String` | no | The plugin version. |

### PermissionChangedEvent

A permission entry of a player changed (or of a group or everything: AllPlayers is true). Fired after the change; the command trees of the players are resent in the same tick. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Uuid` | no | The profile whose permissions changed (zero when AllPlayers). |
| `AllPlayers` | `bool` | no | A group, the default group or the whole backend changed. |

### PermissionsReloadedEvent

The permission backend was reloaded from disk (/perm reload or Permissions.Reload). Not cancellable.


## Player

### PlayerPreLoginEvent

A client finished authentication and is about to enter the configuration phase. Handlers run on the TICK thread: a blocking call (a database or web lookup) freezes the whole server, so do not look anything up here. Cancel to refuse the login (set the kick message first), or call `Defer()` and answer later from `Scheduler.RunAsync`: the login waits up to 30 seconds, continues when every deferral allowed it and is refused as soon as one denies. If your plugin unloads before it answered, the login is refused and the player can simply reconnect. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Connection` | `ulong` | no | Connection id. |
| `Uuid` | `Uuid` | no | The authenticated profile id. |
| `Username` | `Utf8String` | no | The profile name. |
| `Address` | `Utf8String` | no | Client address. |
| `KickMessage` | `Component` | **yes**, `SetKickMessage(...)` | Shown to the client when the login is cancelled (default: a generic message). |
| `Deferrals` | `int` | no | How many plugins hold the login (each Defer() adds one): it continues when all of them allowed it, and any denial refuses it at once. |

### PlayerJoinEvent

A player entity entered the play state. The join message is broadcast after the handlers ran. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Uuid` | `Uuid` | no | The profile id. |
| `Username` | `Utf8String` | no | The profile name. |
| `JoinMessage` | `Component` | **yes**, `SetJoinMessage(...)` | The message broadcast to everybody (set to an empty component to suppress it). |

### PlayerQuitEvent

A player disconnected (the entity is still alive during dispatch). Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Uuid` | `Uuid` | no | The profile id. |
| `Username` | `Utf8String` | no | The profile name. |
| `Reason` | `Utf8String` | no | Why the connection ended: disconnected, kicked, timeout, protocol_error, connection_lost or server_shutdown. |
| `QuitMessage` | `Component` | **yes**, `SetQuitMessage(...)` | The message broadcast to everybody (set to an empty component to suppress it). |

### PlayerMoveEvent

A player moved or rotated. Cancel to teleport the player back; 'to', 'yaw' and 'pitch' may be rewritten. Fires for every accepted movement packet: keep handlers cheap. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `From` | `Vec3d` | no | Position before the move. |
| `To` | `Vec3d` | **yes** | Position after the move. |
| `Yaw` | `float` | **yes** | Yaw after the move. |
| `Pitch` | `float` | **yes** | Pitch after the move. |
| `OnGround` | `bool` | no | Whether the client reports being on the ground. |

### PlayerChatEvent

A player sent a chat message. Cancel to drop it. Changing Message or Rendered sends the line as an unsigned server message instead of the player's signed one. SenderName, ChatType, Decoration and the filter fields restyle the signed line without unsigning it: the signature covers only the typed text. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The sender. |
| `Message` | `Utf8String` | **yes**, `SetMessage(...)` | What the player typed. Changing it re-renders the line with the default format. |
| `Rendered` | `Component` | **yes**, `SetRendered(...)` | The complete line shown to everybody (default: `<name> message`). Replace it to change the format; takes precedence over Message. |
| `SenderName` | `Component` | **yes**, `SetSenderName(...)` | The sender name as the line shows it (hover, click and insertion included). Changing it keeps the message signed. |
| `ChatType` | `ChatKind` | **yes** | The chat type of the line: Chat, EmoteCommand or SayCommand. The message kinds need a target and are refused here (Chat is used). |
| `Decoration` | `Component` | **yes**, `SetDecoration(...)` | The body as the line shows it (unsigned content: colours, highlights, glyphs). Equal to the typed text means none. Keeps the message signed. |
| `FilterKind` | `ChatFilterKind` | **yes** | How the recipients who filter text see the message (vanilla FilterMask). Any other value fully filters the message. |
| `FilterBits` | `Utf8String` | **yes**, `SetFilterBits(...)` | For PartiallyFiltered: one 0 or 1 per UTF-16 unit of the typed text, 1 hides the unit. A different length or character fully filters the message. |

### PrivateMessageEvent

A player sent a private message (/msg, /tell, /w) to one player. Cancel to drop the copy for that target. Changing the message sends it unsigned; a changed decoration keeps it signed. Console senders do not fire it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Sender` | `Player` | no | The player who typed the command. |
| `Target` | `Player` | no | The recipient. A /msg with several targets fires once per target. |
| `Message` | `Utf8String` | **yes**, `SetMessage(...)` | The typed text. Changing it sends the message unsigned. |
| `Decoration` | `Component` | **yes**, `SetDecoration(...)` | The body as the line shows it (unsigned content). Equal to the typed text means none. Keeps the message signed. |

### PlayerCommandEvent

A player ran a command (without the leading slash). Cancel to suppress it; the command line may be rewritten. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Command` | `Utf8String` | **yes**, `SetCommand(...)` | The command line without the slash. |

### PlayerInteractBlockEvent

A player right-clicked a block (fired after PlayerInteract). Cancel to stop the block interaction and item use. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Pos` | `BlockPos` | no | The clicked block. |
| `Face` | `byte` | no | The clicked face (BlockFace). |
| `Hand` | `byte` | no | The hand (Hand). |
| `Item` | `ItemView` | no | The item in that hand. |

### PlayerInteractEvent

A player clicked (left or right, air or block). Cancel to stop the default action: block breaking start, block use, item use. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Action` | `InteractAction` | no | What was clicked. |
| `Hand` | `Hand` | no | The hand. |
| `Item` | `ItemView` | no | The item in that hand. |
| `Pos` | `BlockPos` | no | The clicked block (only for block actions). |
| `Face` | `BlockFace` | no | The clicked face (only for block actions). |
| `HasBlock` | `bool` | no | Whether pos/face are valid. |

### PlayerInteractEntityEvent

A player right-clicked an entity. Cancel to stop the interaction. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Target` | `Entity` | no | The clicked entity. |
| `Hand` | `Hand` | no | The hand. |
| `Item` | `ItemView` | no | The item in that hand. |

### PlayerKickEvent

A player is about to be kicked by a command or plugin. Cancel to keep the player. Not fired for protocol errors or disconnects. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Reason` | `Component` | **yes**, `SetReason(...)` | The disconnect screen text. |
| `LeaveMessage` | `Component` | **yes**, `SetLeaveMessage(...)` | The message broadcast to everybody (empty = the default quit message). |

### PlayerRespawnEvent

A player is respawning. The destination may be changed. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Reason` | `RespawnReason` | no | Why. |
| `Position` | `Vec3d` | **yes** | Where the player appears. |
| `World` | `World` | **yes** | The dimension the player appears in. |
| `IsSpawnPoint` | `bool` | no | True when a bed or respawn anchor of the player is used, false for the world spawn. |

### PlayerDeathEvent

A player died. The death message, inventory/experience drops and level keeping can be changed. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Killer` | `Entity` | no | The entity responsible (none if there is none). |
| `Cause` | `DamageCause` | no | The damage cause. |
| `DeathMessage` | `Component` | **yes**, `SetDeathMessage(...)` | Broadcast to everybody (empty = none). |
| `DropInventory` | `bool` | **yes** | Drop the items (false keeps them, like keepInventory). Only the items: the experience is controlled by the two fields below. |
| `KeepLevel` | `bool` | **yes** | Keep the experience level through the respawn (default: the keepInventory rule). True drops no experience orbs. |
| `DropExperience` | `int` | **yes** | Experience points dropped as orbs at the death position (default the vanilla amount min(level*7, 100), 0 with keepInventory); independent of drop_inventory. |

### PlayerCreativeSlotEvent

A creative-mode player put a stack into an inventory slot from the creative menu (slot -1: threw one out of the menu). Cancel to refuse; the client is resynced. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Slot` | `int` | no | The menu slot (1-45), or -1 for a drop. |
| `Item` | `ItemView` | no | The stack the client wants. |

### PlayerItemHeldEvent

A player selected another hotbar slot. Cancel to revert the selection. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `PreviousSlot` | `int` | no | Hotbar slot before (0-8). |
| `NewSlot` | `int` | no | Hotbar slot selected (0-8). |

### PlayerSwapHandsEvent

A player swapped the main hand and off hand items (F key). Cancel to keep them. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `MainHand` | `ItemView` | no | The item that moves to the off hand. |
| `OffHand` | `ItemView` | no | The item that moves to the main hand. |

### PlayerDropItemEvent

A player drops an item from the hotbar or inventory (drop key). Cancel to keep it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Item` | `ItemView` | no | The item (a single one, or the whole stack for the control-drop). |
| `WholeStack` | `bool` | no | Control-drop of the whole stack. |

### PlayerPickupItemEvent

A player is about to pick up an item entity. Cancel to leave it on the ground. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `ItemEntity` | `Entity` | no | The item entity. |
| `Item` | `ItemView` | no | The stack on the ground. |

### PlayerConsumeEvent

A player finished eating or drinking. Cancel to prevent the effects and the item consumption. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Item` | `ItemView` | no | The consumed item. |
| `Hand` | `Hand` | no | The hand. |

### PlayerToggleSneakEvent

A player started or stopped sneaking. Cancel to ignore the change. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Sneaking` | `bool` | no | The new sneaking state. |

### PlayerToggleSprintEvent

A player started or stopped sprinting. Cancel to ignore the change. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Sprinting` | `bool` | no | The new sprinting state. |

### PlayerToggleFlightEvent

A player started or stopped flying (creative / may-fly). Cancel to ignore the change. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Flying` | `bool` | no | The new flying state. |

### PlayerGameModeChangeEvent

A player's game mode is about to change. Cancel to keep the old one. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `OldMode` | `GameMode` | no | The current mode. |
| `NewMode` | `GameMode` | no | The new mode. |
| `Cause` | `GameModeChangeCause` | no | Why. |

### PlayerTeleportEvent

A player is about to be teleported (command, plugin, ender pearl, ...). Cancel to stay; the destination may be changed. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `From` | `Vec3d` | no | Current position. |
| `To` | `Vec3d` | **yes** | Destination position. |
| `FromWorld` | `World` | no | Current dimension. |
| `ToWorld` | `World` | no | Destination dimension. |
| `Cause` | `TeleportCause` | no | Why. |

### PlayerPortalEvent

A player is about to travel through a portal (nether portal, end portal, end gateway). Cancel to stay. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `From` | `Vec3d` | no | Current position. |
| `FromWorld` | `World` | no | Current dimension. |
| `ToWorld` | `World` | no | Destination dimension. |
| `Cause` | `TeleportCause` | no | Which portal. |

### PlayerChangedWorldEvent

A player arrived in another dimension. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `FromWorld` | `World` | no | The dimension left. |
| `ToWorld` | `World` | no | The dimension entered. |

### PlayerResourcePackStatusEvent

A client reported the state of a pushed resource pack. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `PackId` | `Uuid` | no | The pack id given in the push. |
| `Status` | `ResourcePackStatus` | no | The reported state. |

### PlayerLevelChangeEvent

A player's experience level changed. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `OldLevel` | `int` | no | Level before. |
| `NewLevel` | `int` | no | Level after. |


## Block

### BlockBreakEvent

A player is about to break a block. Cancel to keep the block (the client is resynced). **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Pos` | `BlockPos` | no | The block. |
| `BlockState` | `BlockState` | no | The block state about to be broken. |
| `DropItems` | `bool` | **yes** | Whether the block drops its loot (the experience is controlled by experience). |
| `Experience` | `int` | **yes** | The experience orbs dropped by the block, pre-filled with what vanilla would drop (ores, spawners, sculk; silk touch applied). Set 0 to drop none or any amount to drop that many; independent of drop_items. |
| `Tool` | `ItemView` | no | The item in the main hand. |

### BlockPlaceEvent

A player is about to place a block. Cancel to refuse; 'block_state' may be rewritten. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Pos` | `BlockPos` | no | Where the block goes. |
| `Against` | `BlockPos` | no | The clicked block. |
| `BlockState` | `BlockState` | **yes** | The state that will be placed. |
| `Hand` | `byte` | no | The hand (Hand). |
| `Item` | `ItemView` | no | The block item used. |

### BlockDamageEvent

A player started digging a block. Cancel to stop the dig. Set InstaBreak to break it at once. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Pos` | `BlockPos` | no | The block. |
| `Face` | `BlockFace` | no | The face being dug. |
| `Tool` | `ItemView` | no | The item in the main hand. |
| `InstaBreak` | `bool` | **yes** | Break the block immediately (as in creative mode). |

### BlockIgniteEvent

Fire is about to be placed in the world. Cancel to prevent it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | Where the fire goes. |
| `Cause` | `IgniteCause` | no | What ignites it. |
| `IgnitingEntity` | `Entity` | no | The player or entity responsible, if any. |

### BlockBurnEvent

Fire is about to destroy a block. Cancel to keep it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The burning block. |
| `BlockState` | `BlockState` | no | Its state. |

### BlockSpreadEvent

A block is spreading to a neighbour (grass, mycelium, vines, fire). Cancel to stop it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The target position. |
| `Source` | `BlockPos` | no | The spreading block. |
| `NewState` | `BlockState` | no | The state that would be placed. |

### BlockFadeEvent

A block is melting, drying or fading (ice, snow, farmland, fire). Cancel to keep it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The block. |
| `BlockState` | `BlockState` | no | The current state. |
| `NewState` | `BlockState` | no | The state it fades to. |

### BlockFormEvent

A block is forming through the environment (snow, ice, concrete, obsidian ...). Cancel to stop it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The position. |
| `BlockState` | `BlockState` | no | The current state. |
| `NewState` | `BlockState` | no | The state that forms. |

### BlockGrowEvent

A plant is growing (crops, stems, cactus, sugar cane, ...). Cancel to stop it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The block. |
| `BlockState` | `BlockState` | no | The current state. |
| `NewState` | `BlockState` | no | The state after growth. |

### BlockPistonExtendEvent

A piston is about to extend. Cancel to keep it retracted. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The piston. |
| `Direction` | `BlockFace` | no | The push direction. |
| `Sticky` | `bool` | no | Whether it is a sticky piston. |
| `MovedBlocks` | `int` | no | Number of blocks that move. |

### BlockPistonRetractEvent

A piston is about to retract. Cancel to keep it extended. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The piston. |
| `Direction` | `BlockFace` | no | The push direction. |
| `Sticky` | `bool` | no | Whether it is a sticky piston. |
| `MovedBlocks` | `int` | no | Number of blocks that move. |

### BlockRedstoneEvent

A redstone component's output power changes. The new power may be changed. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The block. |
| `OldPower` | `int` | no | Power before (0-15). |
| `NewPower` | `int` | **yes** | Power after (0-15). |


## Entity

### EntitySpawnEvent

An entity is about to be added to the world. Cancel to prevent it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Entity` | `Entity` | no | The entity (already created). |
| `EntityType` | `string` | no | The entity type key, e.g. minecraft:zombie. |
| `Position` | `Vec3d` | no | Where it appears. |
| `Reason` | `SpawnReason` | no | Why it spawns. |

### EntityDamageEvent

A living entity is about to take damage. Cancel to negate it. 'amount' (the damage after armor and enchantments, before absorption) may be rewritten. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Target` | `Entity` | no | The damaged entity. |
| `Attacker` | `Entity` | no | The responsible entity, none when there is no attacking entity. |
| `DamageType` | `string` | no | The damage type key, e.g. minecraft:fall. |
| `Amount` | `float` | **yes** | Final damage after armor and enchantments. |
| `Cause` | `DamageCause` | no | The cause derived from the damage type. |
| `RawAmount` | `float` | no | Damage before armor and enchantments. |
| `BlockSource` | `BlockPos` | no | Position of the block that caused the damage. |
| `HasBlockSource` | `bool` | no | Whether block_source is valid (cactus, magma, fire, ...). |

### EntityDeathEvent

A living entity died. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Entity` | `Entity` | no | The entity. |
| `Killer` | `Entity` | no | The responsible entity (none if there is none). |
| `DamageType` | `string` | no | The damage type key, e.g. minecraft:player_attack. |
| `Cause` | `DamageCause` | no | The cause derived from the damage type. |

### EntityCombustEvent

An entity is about to be set on fire. Cancel to prevent it; the duration may be changed. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Entity` | `Entity` | no | The entity. |
| `DurationTicks` | `int` | **yes** | Burn time in ticks. |
| `Source` | `Entity` | no | The entity that ignites it, if any. |


## Inventory and crafting

### InventoryOpenEvent

A player is about to open a container (chest, furnace, crafting table, ...). Cancel to keep it closed. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Kind` | `InventoryKind` | no | The container kind. |
| `HasPos` | `bool` | no | Whether the container is a block. |
| `Pos` | `BlockPos` | no | The block position. |

### InventoryCloseEvent

A player closed a container. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Kind` | `InventoryKind` | no | The container kind. |

### InventoryClickEvent

A player clicked a slot of an open container (also the player inventory). Cancel to undo the click (the client is resynced). **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Kind` | `InventoryKind` | no | The container kind. |
| `Slot` | `int` | no | The clicked slot index of the container (-999 outside the window). |
| `ClickType` | `ClickType` | no | How it was clicked. |
| `Button` | `int` | no | Raw button of the packet. |
| `Clicked` | `ItemView` | no | The item in the clicked slot. |
| `Cursor` | `ItemView` | no | The item on the cursor. |

### InventoryDragEvent

A player finished dragging a stack over several slots. Cancel to undo. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Kind` | `InventoryKind` | no | The container kind. |
| `DragType` | `DragType` | no | The drag mode. |
| `Slots` | `NativeBytes` | no | The dragged slots as packed little-endian int32 values. |
| `Cursor` | `ItemView` | no | The cursor stack being distributed. |

### CraftItemEvent

A player took the result of a crafting recipe (cancel the InventoryClick event of the result slot to prevent crafting). Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Recipe` | `Utf8String` | no | Recipe id. |
| `Result` | `ItemView` | no | The crafted item. |
| `ShiftClick` | `bool` | no | Whether it is a quick-craft. |

### PrepareCraftEvent

The crafting grid matched a recipe. Cancel to show no result. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Recipe` | `Utf8String` | no | Recipe id. |
| `Result` | `ItemView` | no | The result that would be shown. |

### FurnaceSmeltEvent

A furnace, smoker or blast furnace finished smelting an item. Cancel to keep the input unsmelted. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The furnace. |
| `Source` | `ItemView` | no | The ingredient. |
| `Result` | `ItemView` | no | The product. |

### FurnaceBurnEvent

A furnace is about to consume fuel. Cancel to not burn it; the burn time may be changed. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The furnace. |
| `Fuel` | `ItemView` | no | The fuel. |
| `BurnTicks` | `int` | **yes** | Burn time in ticks. |

### EnchantItemEvent

A player is enchanting an item at an enchanting table. Cancel to refuse. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Item` | `ItemView` | no | The item being enchanted. |
| `Option` | `int` | no | The chosen option (0-2). |
| `Cost` | `int` | no | Levels charged. |

### PrepareAnvilEvent

The anvil inputs changed and a result was computed. Cancel to show no result; the cost may be changed. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `First` | `ItemView` | no | Left input. |
| `Second` | `ItemView` | no | Right input. |
| `Result` | `ItemView` | no | The computed result. |
| `RepairCost` | `int` | **yes** | Levels the player pays. |
| `RenameText` | `Utf8String` | no | The text typed in the rename field. |


## World

### ChunkLoadEvent

A chunk became loaded. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Dimension` | `int` | no | Dimension index. |
| `X` | `int` | no | Chunk x. |
| `Z` | `int` | no | Chunk z. |
| `NewlyGenerated` | `bool` | no | True when the chunk was generated, false when loaded from disk. |

### ChunkUnloadEvent

A chunk is about to be unloaded. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Dimension` | `int` | no | Dimension index. |
| `X` | `int` | no | Chunk x. |
| `Z` | `int` | no | Chunk z. |

### TimeSkipEvent

The day time jumped (sleeping through the night, /time set or /time add). Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Reason` | `TimeSkipReason` | no | Why. |
| `SkipAmount` | `long` | no | Ticks the clock advanced. |

### WeatherChangeEvent

The rain started or stopped (overworld weather). Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Raining` | `bool` | no | Whether it will be raining. |
| `Thundering` | `bool` | no | Whether it will be thundering. |

### PortalCreateEvent

A nether portal frame is about to be filled or a destination portal built. Cancel to prevent it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `World` | `World` | no | The dimension. |
| `Reason` | `PortalCreateReason` | no | Why. |
| `BlockCount` | `int` | no | Portal blocks created. |


## Network

### PacketReceiveEvent

A decoded-frame packet arrived (payload excludes the packet id). Cancel to drop it. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Connection` | `ulong` | no | Connection id. |
| `Player` | `Player` | no | The player entity (none before play). |
| `State` | `byte` | no | Connection state. |
| `PacketId` | `int` | no | Packet id. |
| `Payload` | `NativeBytes` | no | Packet body (borrowed). |

### PacketSendEvent

A packet is about to be sent to one connection. **Cancellable.** **Reserved: not fired yet.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Connection` | `ulong` | no | Connection id. |
| `Player` | `Player` | no | The player entity (none before play). |
| `State` | `byte` | no | Connection state. |
| `PacketId` | `int` | no | Packet id. |
| `Payload` | `NativeBytes` | no | Packet body (borrowed). |


## Menus, custom content and NPCs

### MenuClickEvent

A player clicked in a custom GUI (Menu) of a plugin, after InventoryClick. Menus are locked: the click is denied (the client is resynced) unless a handler sets Allow, and a cancel always denies it. A click that could move items out of or into the menu (a slot of the menu, shift click, double click, drag, number key, off hand key) is delivered here with the slot of the menu's own numbering (top slots first, then the player's 27 + 9 slots). **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Menu` | `int` | no | The id of the menu. |
| `Slot` | `int` | no | The clicked slot, -999 outside the window. |
| `ClickType` | `ClickType` | no | How it was clicked. |
| `Button` | `int` | no | Raw button of the packet. |
| `Clicked` | `ItemView` | no | The item in the clicked slot. |
| `Cursor` | `ItemView` | no | The item on the cursor. |
| `Allow` | `bool` | **yes** | Set by a handler to let the click change items like in a chest (default false: the menu is a display). |

### MenuCloseEvent

A plugin menu was closed for a player (for any reason). The player's cursor stack has already been given back. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Menu` | `int` | no | The id of the menu. |
| `Reason` | `MenuCloseReason` | no | Why it closed. |

### CustomItemUseEvent

A player used a custom item (right click in the air or on a block). Fired by the content layer after PlayerInteract. Cancel to stop the item's own handler and the default action. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `CustomId` | `Utf8String` | no | The id of the custom item, "myplugin:ruby". |
| `Hand` | `Hand` | no | The hand. |
| `Item` | `ItemView` | no | The item stack. |
| `HasBlock` | `bool` | no | Whether the click was on a block. |
| `Pos` | `BlockPos` | no | The clicked block (only when has_block). |
| `Face` | `BlockFace` | no | The clicked face (only when has_block). |

### CustomBlockPlaceEvent

A player is placing a custom block with a custom item. Cancel to refuse. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `CustomId` | `Utf8String` | no | The id of the custom block. |
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | Where it is placed. |
| `Against` | `BlockPos` | no | The block that was clicked. |
| `Hand` | `Hand` | no | The hand. |

### CustomBlockBreakEvent

A player is breaking a custom block (fired by the content layer after BlockBreak). Cancel to keep the block. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `CustomId` | `Utf8String` | no | The id of the custom block. |
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The block. |
| `DropItems` | `bool` | **yes** | Whether the block's drops are spawned. |

### CustomBlockInteractEvent

A player right-clicked a custom block (fired by the content layer after PlayerInteractBlock). Cancel to stop the block handler and the item use. **Cancellable.**

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `CustomId` | `Utf8String` | no | The id of the custom block. |
| `World` | `World` | no | The dimension. |
| `Pos` | `BlockPos` | no | The block. |
| `Face` | `BlockFace` | no | The clicked face. |
| `Hand` | `Hand` | no | The hand. |
| `Item` | `ItemView` | no | The item in that hand. |

### NpcInteractEvent

A player clicked an NPC made with Npcs.Create (a left click is an attack, a right click an interaction). Observed only: the click has no other effect. Not cancellable.

| Field | Type | Mutable | Description |
|---|---|---|---|
| `Player` | `Player` | no | The player. |
| `Npc` | `ulong` | no | The NPC's handle. |
| `Action` | `NpcClick` | no | The click. |
| `Hand` | `Hand` | no | The hand (a left click is the main hand). |
| `Sneaking` | `bool` | no | Whether the player was sneaking. |
