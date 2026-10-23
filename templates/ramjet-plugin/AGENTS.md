# AGENTS.md: Ramjet plugin project

This is a C# plugin for **Ramjet** (a C++23, vanilla-compatible Minecraft Java server with a hot-reloading .NET 10 plugin layer). Read this file before changing code. A Claude Code skill with recipes and the full API listing is in `.claude/skills/ramjet-plugin/` (`SKILL.md`, `references/api.md`, `references/events.md`).

## Layout

- `MyPlugin.csproj`: net10.0 class library. References the `Ramjet.Api` package (the API as a reference assembly, compile-time only; never copy a `Mc.Scripting.Api.dll` next to the plugin). Keep `EnableDynamicLoading=true`.
- `PluginMain.cs`: the plugin class (`Plugin` subclass, `[PluginInfo]`, `OnEnable` / `OnDisable`). One public plugin class per assembly; no work in its constructor.
- `plugin.json`: the manifest the server's loader reads (id, version, `api`, dependencies, permissions). Keep `id`, `name`, `version`, `main` consistent with `[PluginInfo]` and the class.

## Build, run, reload

```bash
# the Ramjet.Api package is restored from nuget.org (no extra source needed)
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
5. Component types are plain unmanaged structs owned by exactly one plugin and registered with Ecs.RegisterComponent<T>(key) (plugin.Ecs).
6. Register everything through the plugin context (Events, Commands, Systems, Scheduler, Ecs, Permissions, ...); it is released automatically on unload and hot reload.
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
- Events the engine fires today: ServerStartEvent, ServerStopEvent, TickStartEvent, TickEndEvent, ServerListPingEvent, CommandsRegisteredEvent, PluginEnableEvent, PluginDisableEvent, PermissionChangedEvent, PermissionsReloadedEvent, PlayerPreLoginEvent, PlayerJoinEvent, PlayerQuitEvent, PlayerMoveEvent, PlayerChatEvent, PrivateMessageEvent, PlayerCommandEvent, PlayerInteractBlockEvent, PlayerKickEvent, PlayerRespawnEvent, PlayerDeathEvent, PlayerCreativeSlotEvent, PlayerItemHeldEvent, PlayerSwapHandsEvent, PlayerDropItemEvent, PlayerPickupItemEvent, PlayerConsumeEvent, PlayerInteractEvent, PlayerInteractEntityEvent, PlayerToggleSneakEvent, PlayerToggleSprintEvent, PlayerToggleFlightEvent, PlayerGameModeChangeEvent, PlayerTeleportEvent, PlayerPortalEvent, PlayerChangedWorldEvent, PlayerResourcePackStatusEvent, PlayerLevelChangeEvent, BlockBreakEvent, BlockPlaceEvent, BlockDamageEvent, BlockIgniteEvent, BlockBurnEvent, BlockSpreadEvent, BlockFadeEvent, BlockFormEvent, BlockGrowEvent, BlockPistonExtendEvent, BlockPistonRetractEvent, BlockRedstoneEvent, EntitySpawnEvent, EntityDamageEvent, EntityDeathEvent, EntityCombustEvent, InventoryOpenEvent, InventoryCloseEvent, InventoryClickEvent, InventoryDragEvent, CraftItemEvent, PrepareCraftEvent, FurnaceSmeltEvent, FurnaceBurnEvent, EnchantItemEvent, PrepareAnvilEvent, PacketReceiveEvent, PacketSendEvent, ChunkLoadEvent, ChunkUnloadEvent, TimeSkipEvent, WeatherChangeEvent, PortalCreateEvent, MenuClickEvent, MenuCloseEvent, CustomItemUseEvent, CustomBlockPlaceEvent, CustomBlockBreakEvent, CustomBlockInteractEvent, NpcInteractEvent, PlayerPreQuitEvent, CombatTagStartEvent, CombatTagEndEvent, SignChangeEvent, PlayerTakeLecternBookEvent, PlayerEditBookEvent, PlayerOpenSignEvent, WorldLoadEvent, WorldInitEvent, WorldUnloadEvent, RainChangeEvent, ThunderChangeEvent, LightningStrikeEvent, SpawnChangeEvent, ExplosionPrimeEvent, EntityExplodeEvent, BlockExplodeEvent, BlockFromToEvent, FluidLevelChangeEvent, LeavesDecayEvent, MoistureChangeEvent, BlockPhysicsEvent, BlockDestroyEvent, CauldronLevelChangeEvent, BlockFertilizeEvent, EntityBlockFormEvent, StructureGrowEvent, PlayerRecipeBookClickEvent, WorldSaveEvent, TntPrimeEvent, PlayerBucketEmptyEvent, PlayerBucketFillEvent, PlayerBucketEntityEvent, PlayerShearEntityEvent, BlockShearEntityEvent, BlockMultiPlaceEvent, BlockCanBuildEvent, PiglinBarterEvent, BellRingEvent, BeaconEffectEvent, BlockReceiveGameEventEvent, ChunkPopulateEvent, EntitiesLoadEvent, EntitiesUnloadEvent, StructuresLocateEvent, RaidTriggerEvent, RaidFinishEvent, GenericGameEventEvent, PlayerBedEnterEvent, PlayerBedLeaveEvent, PlayerLocaleChangeEvent, PlayerArmSwingEvent, PlayerArmorChangeEvent, PlayerJumpEvent, PlayerSetSpawnEvent, PlayerStartSpectatingEvent, PlayerStopSpectatingEvent, BlockDispenseEvent, BlockPreDispenseEvent, BlockDispenseArmorEvent, InventoryMoveItemEvent, InventoryPickupItemEvent, UnknownCommandEvent, ServerCommandEvent, TabCompleteEvent, PlayerCommandSendEvent, PlayerServerFullCheckEvent, VehicleCreateEvent, VehicleDamageEvent, VehicleDestroyEvent, VehicleMoveEvent, VehicleBlockCollisionEvent, VehicleEntityCollisionEvent, VehicleEnterEvent, VehicleExitEvent, VehicleUpdateEvent, EntityMountEvent, EntityDismountEvent, HorseJumpEvent, EntityRegainHealthEvent, EntityPotionEffectEvent, EntityKnockbackEvent, EntityResurrectEvent, EntityToggleGlideEvent, EntityToggleSwimEvent, EntityPoseChangeEvent, PlayerFishEvent, FoodLevelChangeEvent, PlayerItemBreakEvent, PlayerItemDamageEvent, PlayerItemMendEvent, PlayerExhaustionEvent, PlayerVelocityEvent, PlayerAdvancementDoneEvent, BlockDropItemEvent, BlockExpEvent, FurnaceStartSmeltEvent, FurnaceExtractEvent, BrewingStartEvent, BrewEvent, CampfireStartEvent, CampfireCookEvent, PrepareSmithingEvent, SmithItemEvent, StonecutterSelectEvent, AnvilDamagedEvent, TradeSelectEvent, PlayerTradeEvent, EntityPortalEnterEvent, EntityPortalEvent, EntityPortalExitEvent, EntityRemoveEvent, EntityTeleportEvent, ProjectileLaunchEvent, ProjectileHitEvent, PlayerLaunchProjectileEvent, EntityShootBowEvent, PlayerPickupArrowEvent, PlayerRiptideEvent, FireworkExplodeEvent, PotionSplashEvent, LingeringPotionSplashEvent, AreaEffectCloudApplyEvent, ExpBottleEvent, PlayerEggThrowEvent, EntityChangeBlockEvent, EntityInteractEvent, PlayerLeashEntityEvent, EntityBreakDoorEvent, PlayerUnleashEntityEvent, EntityUnleashEvent, ItemDespawnEvent, ItemMergeEvent, EntityDropItemEvent, EntityPickupItemEvent, PlayerExpChangeEvent, PlayerPickupExperienceEvent, ExperienceOrbMergeEvent, HangingPlaceEvent, HangingBreakEvent, HangingBreakByEntityEvent, PlayerArmorStandManipulateEvent, EntityPlaceEvent, EntityBreakEvent, EntityBreakByEntityEvent, PlayerItemFrameChangeEvent, EntityTransformEvent, EntityTameEvent, EntityBreedEvent, EntityEnterLoveModeEvent, EntityToggleSitEvent, SlimeSplitEvent, CreeperPowerEvent, SheepDyeWoolEvent, EntityFertilizeEggEvent, EntityTargetEvent, PreCreatureSpawnEvent, PlayerNaturalSpawnRangeEvent, EntityAddToWorldEvent, EntityRemoveFromWorldEvent, EntityMoveEvent, EntityPortalReadyEvent, BroadcastMessageEvent, EntityEnterBlockEvent, DragonEggFormEvent, EntityConstructEvent, EntityAirChangeEvent, ArrowBodyCountChangeEvent, EntityDamageItemEvent, PlayerShieldDisableEvent, PlayerExpCooldownChangeEvent, WaterBottleSplashEvent, ThrownEggHatchEvent, FishHookStateChangeEvent, TurtleGoHomeEvent, TurtleStartDiggingEvent, TurtleLayEggEvent, PufferFishStateChangeEvent, ElderGuardianAppearanceEvent, SkeletonHorseTrapEvent, WardenAngerChangeEvent, SulfurCubeSwallowItemEvent, ShulkerDuplicateEvent, ClientTickEndEvent, PlayerClientLoadedWorldEvent, PlayerClientOptionsChangeEvent, PlayerChangedMainHandEvent, PlayerCustomClickEvent, PlayerBundleItemSelectEvent, PlayerUseUnknownEntityEvent, PrePlayerAttackEntityEvent, PlayerPostRespawnEvent, PlayerFailMoveEvent, PlayerChunkLoadEvent, PlayerChunkUnloadEvent, BlockDamageAbortEvent, BlockBreakProgressUpdateEvent, EntityAttemptSmashAttackEvent, EntityAttemptSpinAttackEvent, EntityLungeEvent, EntityLoadCrossbowEvent, PlayerReadyArrowEvent, PlayerStopUsingItemEvent, PlayerSwapWithEquipmentSlotEvent, PlayerItemCooldownEvent, PlayerItemGroupCooldownEvent, PlayerElytraBoostEvent, PlayerConnectionInitialConfigureEvent, PlayerConnectionCloseEvent, PlayerCodeOfConductSendEvent, PlayerSpawnLocationEvent, PlayerRegisterChannelEvent, PlayerUnregisterChannelEvent, ProfileWhitelistVerifyEvent, WhitelistToggleEvent, WhitelistStateUpdateEvent, ServerResourcesReloadedEvent, SlimeChangeDirectionEvent, SlimeSwimEvent, SlimeWanderEvent, WitchReadyPotionEvent, WitchConsumePotionEvent, WitchThrowPotionEvent, EndermanAttackPlayerEvent, EndermanEscapeEvent, PigZombieAngerEvent, CreeperIgniteEvent, EntitySpellCastEvent, BatToggleSleepEvent, WorldBorderBoundsChangeEvent, WorldBorderBoundsChangeFinishEvent, WorldBorderCenterChangeEvent, WorldDifficultyChangeEvent, WorldGameRuleChangeEvent, MapInitializeEvent, LootGenerateEvent, RaidSpawnWaveEvent, RaidStopEvent, PlayerFlowerPotManipulateEvent, PlayerHarvestBlockEvent, PlayerShearBlockEvent, EntityCompostItemEvent, NotePlayEvent, PlayerBedFailEnterEvent, PlayerDeepSleepEvent, BlockBreakBlockEvent, BrewingStandFuelEvent, CartographyItemEvent, PlayerLoomPatternSelectEvent, PrepareItemEnchantEvent, PrepareGrindstoneEvent, CrafterCraftEvent, ItemCraftedEvent, BlockFailedDispenseEvent, HopperInventorySearchEvent, PrepareHopperTransferEvent, CompostItemEvent, PlayerInventorySlotChangeEvent, BlockLockCheckEvent, EnderDragonChangePhaseEvent, EnderDragonShootFireballEvent, EnderDragonFireballHitEvent, EnderDragonFlameEvent, VillagerAcquireTradeEvent, VillagerCareerChangeEvent, VillagerReplenishTradeEvent, BellResonateEvent, BeaconActivatedEvent, BeaconDeactivatedEvent, PlayerChangeBeaconEffectEvent, SculkBloomEvent, VaultChangeStateEvent, VaultDisplayItemEvent, BlockDispenseLootEvent, PlayerInsertLecternBookEvent, PlayerLecternPageChangeEvent, PlayerSignCommandPreprocessEvent, UncheckedSignChangeEvent, PlayerNameEntityEvent, EntityDyeEvent, PlayerStatisticIncrementEvent, EntityEquipmentChangedEvent, PlayerInputEvent, PlayerPickBlockEvent, PlayerPickEntityEvent, PlayerRecipeBookSettingsChangeEvent, PhantomPreSpawnEvent, EntityEffectTickEvent, SheepRegrowWoolEvent, PlayerToggleEntityAgeLockEvent, PlayerMapFilledEvent, PlayerAdvancementCriterionGrantEvent, PlayerRecipeDiscoverEvent, PlayerLinksSendEvent.
- `Server.SendMessage` / `Broadcast` reach players: yes.
- `Server.CurrentTick` is real: yes.
- Plugin commands (`Commands.Register`) run from in-game chat: yes.
- `plugin.json` manifest read by the loader: yes.
<!-- END GENERATED:status -->

Do not guess API names: check `.claude/skills/ramjet-plugin/references/api.md`. If a member is not listed there, it does not exist.

## Events (catalogue)

Lower priority value runs first (`Lowest` ... `Monitor`). `e.Cancelled = true` vetoes a cancellable event; later handlers are skipped unless subscribed with `receiveCancelled: true`. Mutable fields are written by the handler and read back by the engine.

<!-- BEGIN GENERATED:events-table -->
| Event struct | Id | Cancellable | Engine | Mutable fields | Read-only fields |
|---|---:|---|---|---|---|
| `ServerStartEvent` | 1 | no | fired | - | - |
| `ServerStopEvent` | 2 | no | fired | - | - |
| `TickStartEvent` | 3 | no | fired | - | Tick:ulong |
| `TickEndEvent` | 4 | no | fired | - | Tick:ulong, TickNanos:long |
| `ServerListPingEvent` | 100 | no | fired | Motd, MaxPlayers, OnlinePlayers, HidePlayers, Sample, Favicon, VersionName, ShownProtocol | Connection:ulong, Address:Utf8String, ProtocolVersion:int |
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
| `PlayerKickEvent` | 110 | yes | fired | Reason, LeaveMessage | Player:player, Cause:enum:KickCause |
| `PlayerRespawnEvent` | 111 | no | fired | Position, World | Player:player, Reason:enum:RespawnReason, IsSpawnPoint:bool |
| `PlayerDeathEvent` | 112 | no | fired | DeathMessage, DropInventory, KeepLevel, DropExperience | Player:player, Killer:Entity, Cause:enum:DamageCause |
| `PlayerCreativeSlotEvent` | 129 | yes | fired | - | Player:player, Slot:int, Item:item |
| `PlayerItemHeldEvent` | 113 | yes | fired | - | Player:player, PreviousSlot:int, NewSlot:int |
| `PlayerSwapHandsEvent` | 114 | yes | fired | - | Player:player, MainHand:item, OffHand:item |
| `PlayerDropItemEvent` | 115 | yes | fired | - | Player:player, Item:item, WholeStack:bool |
| `PlayerPickupItemEvent` | 116 | yes | fired | - | Player:player, ItemEntity:Entity, Item:item |
| `PlayerConsumeEvent` | 117 | yes | fired | - | Player:player, Item:item, Hand:enum:Hand |
| `PlayerInteractEvent` | 118 | yes | fired | - | Player:player, Action:enum:InteractAction, Hand:enum:Hand, Item:item, Pos:BlockPos, Face:enum:BlockFace, HasBlock:bool |
| `PlayerInteractEntityEvent` | 119 | yes | fired | - | Player:player, Target:Entity, Hand:enum:Hand, Item:item, ClickPos:Vec3d, AtPosition:bool |
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
| `EntityDeathEvent` | 32 | yes | fired | DropExperience, DropItems | Entity:Entity, Killer:Entity, DamageType:key.damage_type, Cause:enum:DamageCause |
| `EntityCombustEvent` | 221 | yes | fired | DurationTicks | Entity:Entity, Source:Entity, SourceBlock:BlockPos, HasSourceBlock:bool |
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
| `PlayerPreQuitEvent` | 306 | no | fired | QuitMessage | Player:player, Reason:Utf8String |
| `CombatTagStartEvent` | 307 | no | fired | - | Player:player, Opponent:Entity, Ticks:int |
| `CombatTagEndEvent` | 308 | no | fired | - | Player:player, Reason:enum:CombatTagEndReason |
| `SignChangeEvent` | 160 | yes | fired | Line1, Line2, Line3, Line4 | Player:player, Pos:BlockPos, Front:bool |
| `PlayerTakeLecternBookEvent` | 161 | yes | fired | - | Player:player, Pos:BlockPos, Book:item |
| `PlayerEditBookEvent` | 162 | yes | fired | Title, Pages | Player:player, Slot:int, Signing:bool |
| `PlayerOpenSignEvent` | 163 | yes | fired | - | Player:player, Pos:BlockPos, Front:bool, Cause:enum:SignOpenCause |
| `WorldLoadEvent` | 72 | no | fired | - | World:world, Fresh:bool |
| `WorldInitEvent` | 73 | no | fired | - | World:world |
| `WorldUnloadEvent` | 74 | yes | fired | - | World:world, Save:bool, Evacuate:bool |
| `RainChangeEvent` | 75 | yes | fired | - | World:world, Raining:bool, Cause:enum:WeatherCause |
| `ThunderChangeEvent` | 76 | yes | fired | - | World:world, Thundering:bool, Cause:enum:WeatherCause |
| `LightningStrikeEvent` | 77 | yes | fired | - | World:world, Bolt:Entity, Position:Vec3d, VisualOnly:bool, Cause:enum:LightningCause, Player:Entity |
| `SpawnChangeEvent` | 78 | no | fired | - | World:world, OldPos:BlockPos, NewPos:BlockPos, OldYaw:float, OldPitch:float, NewYaw:float, NewPitch:float |
| `ExplosionPrimeEvent` | 309 | yes | fired | Radius, Fire | Entity:Entity |
| `EntityExplodeEvent` | 310 | yes | fired | Keep, Yield | Entity:Entity, World:world, Position:Vec3d, Radius:float, Blocks:NativeBytes, Result:enum:ExplosionResult, Fire:bool |
| `BlockExplodeEvent` | 311 | yes | fired | Keep, Yield | World:world, Pos:BlockPos, BlockState:blockstate, Position:Vec3d, Radius:float, Blocks:NativeBytes, Result:enum:ExplosionResult, Fire:bool |
| `BlockFromToEvent` | 190 | yes | fired | - | World:world, Pos:BlockPos, To:BlockPos, Face:enum:BlockFace, BlockState:blockstate |
| `FluidLevelChangeEvent` | 191 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate, NewState:blockstate |
| `LeavesDecayEvent` | 192 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate |
| `MoistureChangeEvent` | 193 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate, NewState:blockstate |
| `BlockPhysicsEvent` | 194 | yes | fired | - | World:world, Pos:BlockPos, BlockState:blockstate, SourceBlock:blockstate |
| `BlockDestroyEvent` | 195 | yes | fired | Drop | World:world, Pos:BlockPos, BlockState:blockstate, Breaker:Entity |
| `CauldronLevelChangeEvent` | 196 | yes | fired | NewLevel | World:world, Pos:BlockPos, Player:player, Reason:enum:CauldronChangeReason, OldState:blockstate |
| `BlockFertilizeEvent` | 197 | yes | fired | - | World:world, Pos:BlockPos, Player:player, Blocks:NativeBytes |
| `EntityBlockFormEvent` | 198 | yes | fired | NewState | World:world, Entity:Entity, Pos:BlockPos, BlockState:blockstate |
| `StructureGrowEvent` | 302 | yes | fired | - | World:world, Pos:BlockPos, Species:enum:GrowSpecies, FromBonemeal:bool, Player:player, Blocks:NativeBytes |
| `PlayerRecipeBookClickEvent` | 164 | yes | fired | - | Player:player, Recipe:Utf8String, MakeAll:bool |
| `WorldSaveEvent` | 79 | no | fired | - | World:world, Cause:enum:WorldSaveCause |
| `TntPrimeEvent` | 312 | yes | fired | - | World:world, Pos:BlockPos, Cause:enum:TntPrimeCause, PrimingEntity:Entity, HasPrimingBlock:bool, PrimingBlock:BlockPos |
| `PlayerBucketEmptyEvent` | 130 | yes | fired | - | Player:player, Pos:BlockPos, Clicked:BlockPos, Face:enum:BlockFace, Hand:enum:Hand, Bucket:item |
| `PlayerBucketFillEvent` | 131 | yes | fired | - | Player:player, Pos:BlockPos, Face:enum:BlockFace, Hand:enum:Hand, Bucket:item, Filled:item |
| `PlayerBucketEntityEvent` | 132 | yes | fired | - | Player:player, Target:Entity, Bucket:item, Filled:item |
| `PlayerShearEntityEvent` | 133 | yes | fired | - | Player:player, Target:Entity, Item:item |
| `BlockShearEntityEvent` | 134 | yes | fired | - | TargetBlock:BlockPos, Entity:Entity, Item:item |
| `BlockMultiPlaceEvent` | 135 | yes | fired | - | Player:player, Pos:BlockPos, BlockState:blockstate, SecondPos:BlockPos, SecondState:blockstate, SecondOldState:blockstate, Hand:enum:Hand |
| `BlockCanBuildEvent` | 136 | no | fired | Buildable | Player:player, Pos:BlockPos, BlockState:blockstate, Hand:enum:Hand |
| `PiglinBarterEvent` | 389 | yes | fired | Keep | Piglin:Entity, World:world, Input:item, Outcomes:Utf8String |
| `BellRingEvent` | 390 | yes | fired | - | World:world, Pos:BlockPos, Face:enum:BlockFace, Entity:Entity |
| `BeaconEffectEvent` | 391 | yes | fired | Effect, Amplifier | World:world, Pos:BlockPos, Player:player, Primary:bool |
| `BlockReceiveGameEventEvent` | 392 | yes | fired | - | World:world, Pos:BlockPos, GameEvent:Utf8String, Source:Entity |
| `ChunkPopulateEvent` | 400 | no | fired | - | World:world, X:int, Z:int |
| `EntitiesLoadEvent` | 401 | no | fired | - | World:world, X:int, Z:int, Entities:NativeBytes |
| `EntitiesUnloadEvent` | 402 | no | fired | - | World:world, X:int, Z:int, Entities:NativeBytes |
| `StructuresLocateEvent` | 403 | yes | fired | Origin, Structures, Radius, FindUnexplored, HasResult, Result | World:world, Source:enum:LocateSource |
| `RaidTriggerEvent` | 404 | yes | fired | - | World:world, Player:player, Center:BlockPos |
| `RaidFinishEvent` | 405 | no | fired | - | World:world, Center:BlockPos, Waves:int, OmenLevel:int, Winners:NativeBytes |
| `GenericGameEventEvent` | 406 | no | fired | - | World:world, GameEvent:Utf8String, Position:Vec3d, Entity:Entity |
| `PlayerBedEnterEvent` | 52 | yes | fired | UseBed | Player:player, Bed:BlockPos, Result:enum:BedEnterResult |
| `PlayerBedLeaveEvent` | 53 | yes | fired | - | Player:player, Bed:BlockPos, Forced:bool |
| `PlayerLocaleChangeEvent` | 54 | no | fired | - | Player:player, OldLocale:Utf8String, NewLocale:Utf8String |
| `PlayerArmSwingEvent` | 55 | yes | fired | - | Player:player, Hand:enum:Hand |
| `PlayerArmorChangeEvent` | 56 | no | fired | - | Player:player, Slot:enum:ArmorSlot, OldItem:item, NewItem:item |
| `PlayerJumpEvent` | 57 | no | fired | - | Player:player, From:Vec3d, To:Vec3d |
| `PlayerSetSpawnEvent` | 58 | yes | fired | Notify | Player:player, Dimension:Utf8String, Pos:BlockPos, Forced:bool, Cause:enum:SpawnSetCause |
| `PlayerStartSpectatingEvent` | 59 | yes | fired | - | Player:player, Current:Entity, Target:Entity |
| `PlayerStopSpectatingEvent` | 60 | yes | fired | - | Player:player, Target:Entity |
| `BlockDispenseEvent` | 276 | yes | fired | Velocity | World:world, Pos:BlockPos, Item:item, Direction:enum:BlockFace, IsDropper:bool |
| `BlockPreDispenseEvent` | 277 | yes | fired | - | World:world, Pos:BlockPos, Slot:int, Item:item, IsDropper:bool |
| `BlockDispenseArmorEvent` | 278 | yes | fired | - | World:world, Pos:BlockPos, Target:Entity, Item:item |
| `InventoryMoveItemEvent` | 279 | yes | fired | - | World:world, Initiator:enum:MoveInitiator, InitiatorPos:BlockPos, HasSourcePos:bool, SourcePos:BlockPos, SourceEntity:Entity, HasDestPos:bool, DestPos:BlockPos, DestEntity:Entity, Item:item |
| `InventoryPickupItemEvent` | 280 | yes | fired | - | World:world, InventoryPos:BlockPos, InventoryEntity:Entity, ItemEntity:Entity, Item:item |
| `UnknownCommandEvent` | 369 | no | fired | Message | Player:Entity, Command:Utf8String |
| `ServerCommandEvent` | 370 | yes | fired | Command | - |
| `TabCompleteEvent` | 371 | yes | fired | Completions, Handled | Player:Entity, Buffer:Utf8String |
| `PlayerCommandSendEvent` | 372 | no | fired | Commands | Player:Entity |
| `PlayerServerFullCheckEvent` | 373 | no | fired | Allow, KickMessage | Uuid:Uuid, Username:Utf8String, Full:bool |
| `VehicleCreateEvent` | 339 | yes | fired | - | Vehicle:Entity, EntityType:key.entity_type |
| `VehicleDamageEvent` | 340 | yes | fired | Damage | Vehicle:Entity, Attacker:Entity, DamageType:key.damage_type, Cause:enum:DamageCause |
| `VehicleDestroyEvent` | 341 | yes | fired | - | Vehicle:Entity, Attacker:Entity, DamageType:key.damage_type, Cause:enum:DamageCause, Creative:bool |
| `VehicleMoveEvent` | 342 | no | fired | - | Vehicle:Entity, From:Vec3d, To:Vec3d, FromYaw:float, ToYaw:float |
| `VehicleBlockCollisionEvent` | 343 | no | fired | - | Vehicle:Entity, Pos:BlockPos, State:blockstate, Velocity:Vec3d |
| `VehicleEntityCollisionEvent` | 344 | yes | fired | - | Vehicle:Entity, Entity:Entity, Pickup:bool |
| `VehicleEnterEvent` | 345 | yes | fired | - | Vehicle:Entity, Entered:Entity |
| `VehicleExitEvent` | 346 | yes | fired | - | Vehicle:Entity, Exited:Entity, CanCancel:bool |
| `VehicleUpdateEvent` | 347 | no | fired | - | Vehicle:Entity |
| `EntityMountEvent` | 348 | yes | fired | - | Entity:Entity, Mount:Entity |
| `EntityDismountEvent` | 349 | yes | fired | - | Entity:Entity, Dismounted:Entity, CanCancel:bool |
| `HorseJumpEvent` | 350 | yes | fired | Power | Horse:Entity, Rider:player |
| `EntityRegainHealthEvent` | 222 | yes | fired | Amount | Entity:Entity, Reason:enum:RegainReason, IsFastRegen:bool |
| `EntityPotionEffectEvent` | 223 | yes | fired | NewAmplifier, NewDuration, IsOverride | Entity:Entity, Action:enum:EffectAction, Cause:enum:EffectCause, Effect:Utf8String, HasOld:bool, OldAmplifier:int, OldDuration:int, HasNew:bool, Source:Entity |
| `EntityKnockbackEvent` | 224 | yes | fired | Velocity | Entity:Entity, Source:Entity, Cause:enum:KnockbackCause, Damage:float |
| `EntityResurrectEvent` | 225 | yes | fired | - | Entity:Entity, Hand:enum:Hand, Totem:item |
| `EntityToggleGlideEvent` | 226 | yes | fired | - | Player:player, Gliding:bool |
| `EntityToggleSwimEvent` | 227 | no | fired | - | Player:player, Swimming:bool |
| `EntityPoseChangeEvent` | 228 | no | fired | - | Player:player, Pose:enum:EntityPose, OldPose:enum:EntityPose |
| `PlayerFishEvent` | 170 | yes | fired | Exp | Player:player, Hook:Entity, Caught:Entity, CaughtItem:item, State:enum:FishState, Hand:enum:Hand |
| `FoodLevelChangeEvent` | 171 | yes | fired | FoodLevel | Player:player, OldLevel:int, Item:item |
| `PlayerItemBreakEvent` | 172 | no | fired | - | Player:player, Item:item, Slot:enum:EquipmentSlot |
| `PlayerItemDamageEvent` | 173 | yes | fired | Damage | Player:player, Item:item, Slot:enum:EquipmentSlot, OriginalDamage:int |
| `PlayerItemMendEvent` | 174 | yes | fired | Repair | Player:player, Item:item, Slot:enum:EquipmentSlot |
| `PlayerExhaustionEvent` | 175 | yes | fired | Amount | Player:player, Reason:enum:ExhaustionReason |
| `PlayerVelocityEvent` | 176 | yes | fired | Velocity | Player:player |
| `PlayerAdvancementDoneEvent` | 177 | no | fired | Message | Player:player, Advancement:Utf8String |
| `BlockDropItemEvent` | 178 | yes | fired | - | Player:player, Pos:BlockPos, BlockState:blockstate, Item:item, Index:int, Count:int |
| `BlockExpEvent` | 179 | no | fired | Experience | Pos:BlockPos, BlockState:blockstate |
| `FurnaceStartSmeltEvent` | 286 | no | fired | TotalCookTime | World:world, Pos:BlockPos, Source:item |
| `FurnaceExtractEvent` | 287 | no | fired | - | World:world, Pos:BlockPos, Player:player, Item:item, Amount:int |
| `BrewingStartEvent` | 288 | no | fired | TotalBrewTime | World:world, Pos:BlockPos, Source:item |
| `BrewEvent` | 289 | yes | fired | - | World:world, Pos:BlockPos, Ingredient:item, Fuel:int, Bottle0:item, Bottle1:item, Bottle2:item |
| `CampfireStartEvent` | 290 | yes | fired | TotalCookTime | World:world, Pos:BlockPos, Player:player, Item:item |
| `CampfireCookEvent` | 291 | yes | fired | - | World:world, Pos:BlockPos, Source:item, Result:item |
| `PrepareSmithingEvent` | 292 | yes | fired | - | Player:player, TemplateItem:item, Base:item, Addition:item, Result:item |
| `SmithItemEvent` | 293 | no | fired | - | Player:player, Result:item |
| `StonecutterSelectEvent` | 294 | yes | fired | - | Player:player, Index:int, Result:item |
| `AnvilDamagedEvent` | 295 | yes | fired | NewState | Player:player, World:world, Pos:BlockPos, BlockState:blockstate |
| `TradeSelectEvent` | 296 | yes | fired | - | Player:player, Merchant:Entity, Index:int |
| `PlayerTradeEvent` | 297 | no | fired | RewardExp, IncreaseUses | Player:player, Merchant:Entity, BuyA:item, BuyB:item, Sell:item |
| `EntityPortalEnterEvent` | 415 | no | fired | - | Entity:Entity, Pos:BlockPos, Cause:enum:TeleportCause, World:world |
| `EntityPortalEvent` | 416 | yes | fired | To | Entity:Entity, From:Vec3d, FromWorld:world, ToWorld:world, Cause:enum:TeleportCause |
| `EntityPortalExitEvent` | 417 | no | fired | AfterVelocity | Entity:Entity, Position:Vec3d, World:world, BeforeVelocity:Vec3d |
| `EntityRemoveEvent` | 418 | no | fired | - | Entity:Entity, EntityType:key.entity_type, Cause:enum:EntityRemoveCause, Position:Vec3d |
| `EntityTeleportEvent` | 419 | yes | fired | To | Entity:Entity, From:Vec3d, FromWorld:world, ToWorld:world, Cause:enum:TeleportCause |
| `ProjectileLaunchEvent` | 319 | yes | fired | - | Projectile:Entity, Shooter:Entity, ProjectileType:key.entity_type, Position:Vec3d, Velocity:Vec3d |
| `ProjectileHitEvent` | 320 | yes | fired | - | Projectile:Entity, Shooter:Entity, ProjectileType:key.entity_type, HitEntity:Entity, HasBlock:bool, HitPos:BlockPos, HitFace:enum:BlockFace, HitLocation:Vec3d |
| `PlayerLaunchProjectileEvent` | 321 | yes | fired | Consume | Player:player, Projectile:Entity, Item:item |
| `EntityShootBowEvent` | 322 | yes | fired | ConsumeItem | Shooter:Entity, Bow:item, Consumable:item, Projectile:Entity, Hand:enum:Hand, Force:float |
| `PlayerPickupArrowEvent` | 323 | yes | fired | - | Player:player, Arrow:Entity, Item:item, CreativeOnly:bool |
| `PlayerRiptideEvent` | 324 | yes | fired | - | Player:player, Item:item, Velocity:Vec3d |
| `FireworkExplodeEvent` | 325 | yes | fired | - | Entity:Entity, Shooter:Entity, Attached:Entity, EffectCount:int |
| `PotionSplashEvent` | 326 | yes | fired | Keep, IntensityScale | Potion:Entity, Shooter:Entity, HitEntity:Entity, Affected:NativeBytes, Intensities:NativeBytes |
| `LingeringPotionSplashEvent` | 327 | yes | fired | - | Potion:Entity, Cloud:Entity, Shooter:Entity, HitEntity:Entity |
| `AreaEffectCloudApplyEvent` | 328 | yes | fired | Keep | Cloud:Entity, Affected:NativeBytes |
| `ExpBottleEvent` | 329 | no | fired | Experience, ShowEffect | Bottle:Entity, Shooter:Entity |
| `PlayerEggThrowEvent` | 330 | no | fired | Hatching, NumHatches, HatchType | Player:player, Egg:Entity |
| `EntityChangeBlockEvent` | 33 | yes | fired | - | Entity:Entity, World:world, Pos:BlockPos, BlockState:blockstate, NewState:blockstate |
| `EntityInteractEvent` | 34 | yes | fired | - | Entity:Entity, World:world, Pos:BlockPos, BlockState:blockstate |
| `PlayerLeashEntityEvent` | 35 | yes | fired | - | Player:player, Entity:Entity, Holder:Entity, Hand:enum:Hand |
| `EntityBreakDoorEvent` | 36 | yes | fired | - | Entity:Entity, World:world, Pos:BlockPos, BlockState:blockstate |
| `PlayerUnleashEntityEvent` | 37 | yes | fired | DropLead | Player:player, Entity:Entity |
| `EntityUnleashEvent` | 38 | no | fired | DropLead | Entity:Entity, Reason:enum:UnleashReason |
| `ItemDespawnEvent` | 359 | yes | fired | - | ItemEntity:Entity, Item:item, Pos:Vec3d |
| `ItemMergeEvent` | 360 | yes | fired | - | ItemEntity:Entity, Target:Entity, Amount:int |
| `EntityDropItemEvent` | 361 | yes | fired | - | Entity:Entity, ItemEntity:Entity, Item:item |
| `EntityPickupItemEvent` | 362 | yes | fired | - | Entity:Entity, ItemEntity:Entity, Item:item, Remaining:int |
| `PlayerExpChangeEvent` | 363 | no | fired | Amount | Player:player, Source:Entity |
| `PlayerPickupExperienceEvent` | 364 | yes | fired | - | Player:player, Orb:Entity, Amount:int |
| `ExperienceOrbMergeEvent` | 365 | yes | fired | - | Target:Entity, Source:Entity, Value:int |
| `HangingPlaceEvent` | 150 | yes | fired | - | Entity:Entity, Player:player, Pos:BlockPos, Face:enum:BlockFace, Hand:enum:Hand, Item:item |
| `HangingBreakEvent` | 151 | yes | fired | - | Entity:Entity, Cause:enum:HangingBreakCause, Remover:Entity |
| `HangingBreakByEntityEvent` | 152 | yes | fired | - | Entity:Entity, Cause:enum:HangingBreakCause, Remover:Entity |
| `PlayerArmorStandManipulateEvent` | 153 | yes | fired | - | Player:player, Stand:Entity, Slot:enum:EquipmentSlot, PlayerItem:item, StandItem:item, Hand:enum:Hand |
| `EntityPlaceEvent` | 154 | yes | fired | - | Entity:Entity, Player:player, Pos:BlockPos, Face:enum:BlockFace, Hand:enum:Hand |
| `EntityBreakEvent` | 155 | yes | fired | - | Entity:Entity, Cause:enum:HangingBreakCause, Remover:Entity |
| `EntityBreakByEntityEvent` | 156 | yes | fired | - | Entity:Entity, Cause:enum:HangingBreakCause, Remover:Entity |
| `PlayerItemFrameChangeEvent` | 157 | yes | fired | - | Player:player, Entity:Entity, Action:enum:ItemFrameChangeAction, Item:item |
| `EntityTransformEvent` | 236 | yes | fired | - | Entity:Entity, Transformed:Entity, Reason:enum:TransformReason, Source:Entity |
| `EntityTameEvent` | 237 | yes | fired | - | Entity:Entity, Owner:player |
| `EntityBreedEvent` | 238 | yes | fired | Experience | Child:Entity, Parent:Entity, Partner:Entity, Breeder:player |
| `EntityEnterLoveModeEvent` | 239 | yes | fired | TicksInLove | Entity:Entity, Player:player |
| `EntityToggleSitEvent` | 240 | yes | fired | - | Entity:Entity, Sitting:bool, CanCancel:bool |
| `SlimeSplitEvent` | 241 | yes | fired | Count | Entity:Entity |
| `CreeperPowerEvent` | 242 | yes | fired | - | Entity:Entity, Bolt:Entity |
| `SheepDyeWoolEvent` | 243 | yes | fired | Color | Entity:Entity, Player:player, OldColor:int |
| `EntityFertilizeEggEvent` | 244 | yes | fired | - | Entity:Entity, Partner:Entity, Breeder:player |
| `EntityTargetEvent` | 248 | yes | fired | Target | Entity:Entity, OldTarget:Entity, Reason:enum:TargetReason |
| `PreCreatureSpawnEvent` | 249 | yes | fired | AbortAttempt | Dimension:int, EntityType:key.entity_type, Position:Vec3d, Reason:enum:SpawnReason, HasSpawner:bool, SpawnerPos:BlockPos |
| `PlayerNaturalSpawnRangeEvent` | 250 | yes | fired | - | Player:player |
| `EntityAddToWorldEvent` | 478 | no | fired | - | Entity:Entity, World:world |
| `EntityRemoveFromWorldEvent` | 479 | no | fired | - | Entity:Entity, World:world |
| `EntityMoveEvent` | 480 | yes | fired | - | Entity:Entity, From:Vec3d, To:Vec3d, FromYaw:float, FromPitch:float, ToYaw:float, ToPitch:float |
| `EntityPortalReadyEvent` | 481 | yes | fired | TargetWorld | Entity:Entity, Portal:enum:TeleportCause, FromWorld:world |
| `BroadcastMessageEvent` | 482 | yes | fired | Message | - |
| `EntityEnterBlockEvent` | 483 | yes | fired | - | Entity:Entity, Pos:BlockPos |
| `DragonEggFormEvent` | 484 | yes | fired | - | World:world, Pos:BlockPos |
| `EntityConstructEvent` | 485 | yes | fired | - | EntityType:key.entity_type, Position:BlockPos, World:world |
| `EntityAirChangeEvent` | 462 | yes | fired | Amount | Entity:Entity |
| `ArrowBodyCountChangeEvent` | 463 | yes | fired | NewAmount | Entity:Entity, OldAmount:int, IsReset:bool |
| `EntityDamageItemEvent` | 464 | yes | fired | Damage | Entity:Entity, Item:item |
| `PlayerShieldDisableEvent` | 465 | yes | fired | CooldownTicks | Player:player, Attacker:Entity |
| `PlayerExpCooldownChangeEvent` | 466 | no | fired | NewCooldown | Player:player, Reason:enum:ExpCooldownReason |
| `WaterBottleSplashEvent` | 467 | yes | fired | Keep | Projectile:Entity, Position:Vec3d, Affected:NativeBytes |
| `ThrownEggHatchEvent` | 468 | no | fired | Hatching, NumHatches, HatchingType | Egg:Entity |
| `FishHookStateChangeEvent` | 469 | no | fired | - | Hook:Entity, State:enum:FishHookState |
| `TurtleGoHomeEvent` | 199 | yes | fired | - | Entity:Entity |
| `TurtleStartDiggingEvent` | 200 | yes | fired | - | Entity:Entity, Pos:BlockPos |
| `TurtleLayEggEvent` | 201 | yes | fired | EggCount | Entity:Entity, Pos:BlockPos |
| `PufferFishStateChangeEvent` | 202 | yes | fired | - | Entity:Entity, NewState:int |
| `ElderGuardianAppearanceEvent` | 203 | yes | fired | - | Entity:Entity, Player:player |
| `SkeletonHorseTrapEvent` | 204 | yes | fired | - | Entity:Entity, Player:player |
| `WardenAngerChangeEvent` | 205 | yes | fired | NewAnger | Entity:Entity, Target:Entity, OldAnger:int |
| `SulfurCubeSwallowItemEvent` | 206 | yes | fired | - | Entity:Entity, Player:player, ItemEntity:Entity, Item:item |
| `ShulkerDuplicateEvent` | 207 | yes | fired | - | Entity:Entity, Parent:Entity |
| `ClientTickEndEvent` | 420 | no | fired | - | Player:player |
| `PlayerClientLoadedWorldEvent` | 421 | no | fired | - | Player:player, TimedOut:bool |
| `PlayerClientOptionsChangeEvent` | 422 | no | fired | - | Player:player, OldLocale:Utf8String, NewLocale:Utf8String, OldViewDistance:int, NewViewDistance:int, OldChatVisibility:enum:ChatVisibility, NewChatVisibility:enum:ChatVisibility, OldChatColors:bool, NewChatColors:bool, OldSkinParts:byte, NewSkinParts:byte, OldMainHand:enum:MainArm, NewMainHand:enum:MainArm, OldTextFiltering:bool, NewTextFiltering:bool, OldAllowListing:bool, NewAllowListing:bool |
| `PlayerChangedMainHandEvent` | 423 | no | fired | - | Player:player, OldHand:enum:MainArm, NewHand:enum:MainArm |
| `PlayerCustomClickEvent` | 424 | no | fired | - | Player:player, Id:Utf8String, Payload:Utf8String |
| `PlayerBundleItemSelectEvent` | 425 | yes | fired | - | Player:player, Slot:int, SelectedIndex:int |
| `PlayerUseUnknownEntityEvent` | 426 | no | fired | - | Player:player, EntityId:int, Attack:bool, Hand:enum:Hand, ClickPos:Vec3d, HasClickPos:bool |
| `PrePlayerAttackEntityEvent` | 427 | yes | fired | - | Player:player, Target:Entity, WillAttack:bool |
| `PlayerPostRespawnEvent` | 428 | no | fired | - | Player:player, Position:Vec3d, World:world, IsBedSpawn:bool |
| `PlayerFailMoveEvent` | 429 | no | fired | Allowed, LogWarning | Player:player, FailReason:enum:MoveFailReason, From:Vec3d, To:Vec3d |
| `PlayerChunkLoadEvent` | 430 | no | fired | - | Player:player, World:world, ChunkX:int, ChunkZ:int |
| `PlayerChunkUnloadEvent` | 431 | no | fired | - | Player:player, World:world, ChunkX:int, ChunkZ:int |
| `BlockDamageAbortEvent` | 448 | no | fired | - | Player:player, Pos:BlockPos, Item:item |
| `BlockBreakProgressUpdateEvent` | 449 | no | fired | - | Pos:BlockPos, Progress:float, Entity:Entity |
| `EntityAttemptSmashAttackEvent` | 450 | no | fired | Result | Attacker:player, Target:Entity, Item:item, OriginalResult:bool |
| `EntityAttemptSpinAttackEvent` | 451 | yes | fired | - | Entity:Entity, Target:Entity |
| `EntityLungeEvent` | 452 | yes | fired | LungePower | Entity:Entity, Item:item |
| `EntityLoadCrossbowEvent` | 453 | yes | fired | ConsumeItem | Entity:Entity, Crossbow:item, Hand:enum:Hand |
| `PlayerReadyArrowEvent` | 454 | yes | fired | - | Player:player, Bow:item, Arrow:item |
| `PlayerStopUsingItemEvent` | 455 | no | fired | - | Player:player, Item:item, TicksHeld:int |
| `PlayerSwapWithEquipmentSlotEvent` | 456 | yes | fired | - | Player:player, Item:item, Slot:enum:EquipmentSlot |
| `PlayerItemCooldownEvent` | 457 | yes | fired | CooldownTicks | Player:player, Item:item |
| `PlayerItemGroupCooldownEvent` | 458 | yes | fired | CooldownTicks | Player:player, Group:Utf8String |
| `PlayerElytraBoostEvent` | 459 | yes | fired | ConsumeItem | Player:player, Item:item, Hand:enum:Hand |
| `PlayerConnectionInitialConfigureEvent` | 434 | no | fired | - | Connection:ulong, Uuid:Uuid, Username:Utf8String |
| `PlayerConnectionCloseEvent` | 435 | no | fired | - | Connection:ulong, Uuid:Uuid, Username:Utf8String, Address:Utf8String, Reason:Utf8String |
| `PlayerCodeOfConductSendEvent` | 436 | yes | fired | Text | Connection:ulong, Uuid:Uuid |
| `PlayerSpawnLocationEvent` | 438 | no | fired | Position, World, Yaw, Pitch | Uuid:Uuid, NewPlayer:bool |
| `PlayerRegisterChannelEvent` | 439 | no | fired | - | Connection:ulong, Uuid:Uuid, Channel:Utf8String |
| `PlayerUnregisterChannelEvent` | 440 | no | fired | - | Connection:ulong, Uuid:Uuid, Channel:Utf8String |
| `ProfileWhitelistVerifyEvent` | 441 | no | fired | Whitelisted, KickMessage | Uuid:Uuid, Username:Utf8String, WhitelistEnabled:bool, IsOp:bool |
| `WhitelistToggleEvent` | 442 | yes | fired | - | Enabled:bool |
| `WhitelistStateUpdateEvent` | 443 | yes | fired | - | Uuid:Uuid, Username:Utf8String, Status:enum:WhitelistStatus |
| `ServerResourcesReloadedEvent` | 444 | no | fired | - | Cause:enum:ReloadCause |
| `SlimeChangeDirectionEvent` | 500 | yes | fired | NewYaw | Entity:Entity |
| `SlimeSwimEvent` | 501 | yes | fired | - | Entity:Entity |
| `SlimeWanderEvent` | 502 | yes | fired | - | Entity:Entity |
| `WitchReadyPotionEvent` | 503 | yes | fired | Potion | Entity:Entity |
| `WitchConsumePotionEvent` | 504 | yes | fired | Potion | Entity:Entity |
| `WitchThrowPotionEvent` | 505 | yes | fired | Potion | Entity:Entity, Target:Entity |
| `EndermanAttackPlayerEvent` | 506 | yes | fired | - | Entity:Entity, Player:player |
| `EndermanEscapeEvent` | 507 | yes | fired | - | Entity:Entity, Reason:enum:EndermanEscapeReason |
| `PigZombieAngerEvent` | 508 | yes | fired | AngerTicks | Entity:Entity, Target:Entity |
| `CreeperIgniteEvent` | 509 | yes | fired | - | Entity:Entity, Player:player |
| `EntitySpellCastEvent` | 510 | yes | fired | - | Entity:Entity, Spell:enum:SpellKind |
| `BatToggleSleepEvent` | 511 | yes | fired | - | Entity:Entity, Awake:bool |
| `WorldBorderBoundsChangeEvent` | 488 | yes | fired | NewSize, DurationMs | World:world, Kind:enum:BorderChangeKind, OldSize:double |
| `WorldBorderBoundsChangeFinishEvent` | 489 | no | fired | - | World:world, OldSize:double, NewSize:double, DurationMs:long |
| `WorldBorderCenterChangeEvent` | 490 | yes | fired | NewX, NewZ | World:world, OldX:double, OldZ:double |
| `WorldDifficultyChangeEvent` | 491 | no | fired | - | World:world, OldDifficulty:byte, NewDifficulty:byte |
| `WorldGameRuleChangeEvent` | 492 | yes | fired | Value | World:world, Rule:Utf8String, OldValue:Utf8String |
| `MapInitializeEvent` | 493 | no | fired | - | World:world, MapId:int, CenterX:int, CenterZ:int, Scale:int |
| `LootGenerateEvent` | 494 | yes | fired | Keep | World:world, Table:Utf8String, Pos:BlockPos, HasPos:bool, Holder:Entity, Items:NativeBytes |
| `RaidSpawnWaveEvent` | 495 | no | fired | - | World:world, Center:BlockPos, Wave:int, Raiders:int, Leader:Entity |
| `RaidStopEvent` | 496 | no | fired | - | World:world, Center:BlockPos, Reason:enum:RaidStopReason |
| `PlayerFlowerPotManipulateEvent` | 61 | yes | fired | - | Player:player, Pos:BlockPos, Item:item, Placing:bool |
| `PlayerHarvestBlockEvent` | 62 | yes | fired | Keep | Player:player, Pos:BlockPos, Item:item, Items:NativeBytes |
| `PlayerShearBlockEvent` | 63 | yes | fired | - | Player:player, Pos:BlockPos, Item:item, Hand:enum:Hand |
| `EntityCompostItemEvent` | 64 | yes | fired | WillRaiseLevel | Entity:Entity, Pos:BlockPos, Item:item |
| `NotePlayEvent` | 65 | yes | fired | Note | World:world, Pos:BlockPos, Instrument:Utf8String |
| `PlayerBedFailEnterEvent` | 66 | yes | fired | Message | Player:player, Bed:BlockPos, Problem:enum:BedEnterProblem, WillExplode:bool |
| `PlayerDeepSleepEvent` | 67 | yes | fired | - | Player:player |
| `BlockBreakBlockEvent` | 68 | no | fired | Keep | World:world, Pos:BlockPos, BlockState:blockstate, Source:BlockPos, Drops:NativeBytes |
| `BrewingStandFuelEvent` | 374 | yes | fired | FuelPower, Consuming | World:world, Pos:BlockPos, Fuel:item |
| `CartographyItemEvent` | 375 | yes | fired | - | Player:player, Result:item |
| `PlayerLoomPatternSelectEvent` | 376 | yes | fired | - | Player:player, Pattern:Utf8String, Button:int |
| `PrepareItemEnchantEvent` | 377 | yes | fired | Cost0, Cost1, Cost2 | Player:player, Item:item, Bookshelves:int, Hint0:Utf8String, Hint0Level:int, Hint1:Utf8String, Hint1Level:int, Hint2:Utf8String, Hint2Level:int |
| `PrepareGrindstoneEvent` | 378 | yes | fired | - | Player:player, Result:item |
| `CrafterCraftEvent` | 379 | yes | fired | - | World:world, Pos:BlockPos, Recipe:Utf8String, Result:item |
| `ItemCraftedEvent` | 380 | no | fired | - | Player:player, Item:item, Amount:int |
| `BlockFailedDispenseEvent` | 381 | no | fired | PlayEffect | World:world, Pos:BlockPos |
| `HopperInventorySearchEvent` | 382 | yes | fired | - | World:world, Pos:BlockPos, Kind:enum:HopperSearchKind, SearchPos:BlockPos, Found:bool |
| `PrepareHopperTransferEvent` | 383 | yes | fired | - | World:world, Initiator:enum:MoveInitiator, InitiatorPos:BlockPos, HasSourcePos:bool, SourcePos:BlockPos, SourceEntity:Entity, HasDestPos:bool, DestPos:BlockPos, DestEntity:Entity |
| `CompostItemEvent` | 384 | yes | fired | WillRaiseLevel | World:world, Pos:BlockPos, Item:item |
| `PlayerInventorySlotChangeEvent` | 385 | no | fired | ShouldTriggerAdvancements | Player:player, RawSlot:int, Slot:int, OldItem:item, NewItem:item |
| `BlockLockCheckEvent` | 386 | no | fired | Result, LockMessage | Player:player, Pos:BlockPos |
| `EnderDragonChangePhaseEvent` | 80 | yes | fired | NewPhase | Entity:Entity, OldPhase:enum:DragonPhase |
| `EnderDragonShootFireballEvent` | 81 | yes | fired | - | Entity:Entity, Target:Vec3d |
| `EnderDragonFireballHitEvent` | 82 | yes | fired | - | Fireball:Entity, Position:Vec3d, Hit:NativeBytes |
| `EnderDragonFlameEvent` | 83 | yes | fired | - | Entity:Entity, Position:Vec3d |
| `VillagerAcquireTradeEvent` | 84 | yes | fired | MaxUses, VillagerXp, RewardExp | Entity:Entity, Result:item, First:item, Second:item |
| `VillagerCareerChangeEvent` | 85 | yes | fired | - | Entity:Entity, OldProfession:Utf8String, NewProfession:Utf8String, Reason:enum:CareerChangeReason |
| `VillagerReplenishTradeEvent` | 86 | yes | fired | - | Entity:Entity, Index:int, BonusUses:int |
| `BellResonateEvent` | 137 | yes | fired | Keep | World:world, Pos:BlockPos, Raiders:NativeBytes |
| `BeaconActivatedEvent` | 138 | no | fired | - | World:world, Pos:BlockPos, Levels:int |
| `BeaconDeactivatedEvent` | 139 | no | fired | - | World:world, Pos:BlockPos |
| `PlayerChangeBeaconEffectEvent` | 140 | yes | fired | Primary, Secondary, ConsumeItem | World:world, Pos:BlockPos, Player:player |
| `SculkBloomEvent` | 141 | yes | fired | Charge | World:world, Pos:BlockPos |
| `VaultChangeStateEvent` | 142 | yes | fired | - | World:world, Pos:BlockPos, Player:player, OldState:enum:VaultState, NewState:enum:VaultState |
| `VaultDisplayItemEvent` | 143 | yes | fired | - | World:world, Pos:BlockPos, Item:item |
| `BlockDispenseLootEvent` | 144 | yes | fired | Keep | World:world, Pos:BlockPos, Player:player, Items:Utf8String |
| `PlayerInsertLecternBookEvent` | 393 | yes | fired | - | Player:player, Pos:BlockPos, Book:item |
| `PlayerLecternPageChangeEvent` | 394 | yes | fired | NewPage | Player:player, Pos:BlockPos, OldPage:int, Direction:enum:PageDirection |
| `PlayerSignCommandPreprocessEvent` | 395 | yes | fired | Command | Player:player, Pos:BlockPos, Front:bool |
| `UncheckedSignChangeEvent` | 396 | no | fired | - | Player:player, Pos:BlockPos, Front:bool, Line0:Utf8String, Line1:Utf8String, Line2:Utf8String, Line3:Utf8String |
| `PlayerNameEntityEvent` | 397 | yes | fired | Name, Persistent | Player:player, Entity:Entity |
| `EntityDyeEvent` | 398 | yes | fired | Color | Entity:Entity, Player:player |
| `PlayerStatisticIncrementEvent` | 331 | yes | fired | NewValue | Player:player, StatType:Utf8String, Stat:Utf8String, OldValue:int |
| `EntityEquipmentChangedEvent` | 332 | no | fired | - | Entity:Entity, ChangedSlots:uint |
| `PlayerInputEvent` | 333 | no | fired | - | Player:player, Forward:bool, Backward:bool, Left:bool, Right:bool, Jump:bool, Sneak:bool, Sprint:bool |
| `PlayerPickBlockEvent` | 334 | yes | fired | SourceSlot, TargetSlot | Player:player, Pos:BlockPos, IncludeData:bool |
| `PlayerPickEntityEvent` | 335 | yes | fired | SourceSlot, TargetSlot | Player:player, Entity:Entity |
| `PlayerRecipeBookSettingsChangeEvent` | 336 | no | fired | - | Player:player, BookType:enum:RecipeBookType, Open:bool, Filtering:bool |
| `PhantomPreSpawnEvent` | 486 | yes | fired | AbortAttempt | Player:player, Position:Vec3d |
| `EntityEffectTickEvent` | 470 | yes | fired | - | Entity:Entity, Effect:Utf8String, Amplifier:int |
| `SheepRegrowWoolEvent` | 208 | yes | fired | - | Entity:Entity |
| `PlayerToggleEntityAgeLockEvent` | 399 | yes | fired | - | Player:player, Entity:Entity, Lock:bool |
| `PlayerMapFilledEvent` | 497 | no | fired | - | Player:player, MapId:int |
| `PlayerAdvancementCriterionGrantEvent` | 337 | yes | fired | - | Player:player, Advancement:Utf8String, Criterion:Utf8String |
| `PlayerRecipeDiscoverEvent` | 338 | yes | fired | - | Player:player, Recipe:Utf8String |
| `PlayerLinksSendEvent` | 437 | yes | fired | Links | Connection:ulong, Uuid:Uuid |
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
