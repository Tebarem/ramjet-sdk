# Events compared with Paper

How the Ramjet event catalogue compares with the events of the Paper API (Bukkit and Paper's own events): what each Paper event maps to, what is missing and what is planned.

Paper's API for Minecraft 26.3 defines **439 events** that a plugin can listen to (24 of them deprecated). Ramjet has **89 events** today. The two catalogues overlap less than the numbers suggest: Ramjet has several events Paper does not (see [Events Paper does not have](#events-paper-does-not-have)), and many Paper events exist only to work around Bukkit internals.

| | Paper events |
|---|---:|
| Covered by a Ramjet event | 75 |
| Partly covered (the Ramjet event exists, something is missing) | 16 |
| Missing | 320 |
| Not applicable (deprecated aliases, Bukkit or Folia internals) | 28 |
| **Total** | **439** |

The events plugins use most are covered: join and quit, chat, commands, block break and place, interaction, damage and death, teleport, respawn, inventory clicks, world change and the server list ping. The biggest gaps are explosions, projectiles, vehicles, fluid and block physics, buckets, signs, hanging entities, how mobs target and transform, and world load and unload. Those come first, see [What is planned](#what-is-planned).

How to read this page:

- **Covered** means a plugin can do with the Ramjet event what the Paper event allows: observe it, cancel it where Paper allows, and change the data Paper lets you change. The Ramjet name is the Paper name without its package; the C# types are in `Mc.Scripting.Events` and listed in the [event catalogue](events.md).
- **Partial** says what is missing.
- **Planned next** (145 events) are the ones that several widely installed open-source plugins listen to (we counted the event types in the listeners of about twenty of them), plus the events that sit in the same piece of server code and cost little to add alongside. **Planned later** (30) are used by a few of those plugins, **not planned yet** (161) by none of them. If you need an event that is not planned next, say so in an issue on the SDK repository and it moves up.
- An event nobody subscribes to costs one bit test on the server, so adding events does not slow it down. The work is placing each hook correctly in the ported vanilla code.

## Coverage by area

| Area | Paper events | Covered | Partial | Missing | Not applicable |
|---|---:|---:|---:|---:|---:|
| Players | 119 | 25 | 3 | 79 | 12 |
| Chat | 6 | 0 | 5 | 1 | 0 |
| Blocks | 45 | 12 | 0 | 30 | 3 |
| Entities | 115 | 10 | 4 | 98 | 3 |
| Explosions and projectiles | 23 | 0 | 0 | 23 | 0 |
| Vehicles and mounts | 13 | 0 | 0 | 13 | 0 |
| Inventories, crafting and trading | 42 | 11 | 0 | 27 | 4 |
| World and weather | 32 | 11 | 2 | 19 | 0 |
| Server, commands and connections | 44 | 6 | 2 | 30 | 6 |
| **All** | **439** | **75** | **16** | **320** | **28** |

## Covered

Paper events that a Ramjet event replaces. Several Paper events map to one Ramjet event (for example every damage event is `EntityDamageEvent`, with `Attacker` and `BlockSource` telling them apart).

| Paper event | Ramjet event | Remark |
|---|---|---|
| `AsyncPlayerPreLoginEvent` | `PlayerPreLoginEvent` |  |
| `BlockBreakEvent` | `BlockBreakEvent` |  |
| `BlockBurnEvent` | `BlockBurnEvent` |  |
| `BlockDamageEvent` | `BlockDamageEvent` |  |
| `BlockFadeEvent` | `BlockFadeEvent` |  |
| `BlockFormEvent` | `BlockFormEvent` |  |
| `BlockGrowEvent` | `BlockGrowEvent` |  |
| `BlockIgniteEvent` | `BlockIgniteEvent` |  |
| `BlockPistonExtendEvent` | `BlockPistonExtendEvent` |  |
| `BlockPistonRetractEvent` | `BlockPistonRetractEvent` |  |
| `BlockPlaceEvent` | `BlockPlaceEvent` |  |
| `BlockRedstoneEvent` | `BlockRedstoneEvent` |  |
| `BlockSpreadEvent` | `BlockSpreadEvent` |  |
| `ChunkLoadEvent` | `ChunkLoadEvent` |  |
| `ChunkUnloadEvent` | `ChunkUnloadEvent` |  |
| `CraftItemEvent` | `CraftItemEvent` |  |
| `CreatureSpawnEvent` | `EntitySpawnEvent` | Reason |
| `EnchantItemEvent` | `EnchantItemEvent` |  |
| `EntityCombustByEntityEvent` | `EntityCombustEvent` | Source |
| `EntityCombustEvent` | `EntityCombustEvent` |  |
| `EntityDamageByBlockEvent` | `EntityDamageEvent` | BlockSource |
| `EntityDamageByEntityEvent` | `EntityDamageEvent` | Attacker |
| `EntityDamageEvent` | `EntityDamageEvent` |  |
| `EntitySpawnEvent` | `EntitySpawnEvent` |  |
| `FurnaceBurnEvent` | `FurnaceBurnEvent` |  |
| `FurnaceSmeltEvent` | `FurnaceSmeltEvent` |  |
| `InventoryClickEvent` | `InventoryClickEvent` |  |
| `InventoryCloseEvent` | `InventoryCloseEvent` |  |
| `InventoryCreativeEvent` | `PlayerCreativeSlotEvent` |  |
| `InventoryDragEvent` | `InventoryDragEvent` |  |
| `InventoryOpenEvent` | `InventoryOpenEvent` |  |
| `ItemSpawnEvent` | `EntitySpawnEvent` | item entities |
| `PlayerAttemptPickupItemEvent` | `PlayerPickupItemEvent` |  |
| `PlayerChangedWorldEvent` | `PlayerChangedWorldEvent` |  |
| `PlayerCommandPreprocessEvent` | `PlayerCommandEvent` |  |
| `PlayerDeathEvent` | `PlayerDeathEvent` |  |
| `PlayerDropItemEvent` | `PlayerDropItemEvent` |  |
| `PlayerGameModeChangeEvent` | `PlayerGameModeChangeEvent` |  |
| `PlayerInteractEntityEvent` | `PlayerInteractEntityEvent` |  |
| `PlayerInteractEvent` | `PlayerInteractEvent`, `PlayerInteractBlockEvent` |  |
| `PlayerItemConsumeEvent` | `PlayerConsumeEvent` |  |
| `PlayerItemHeldEvent` | `PlayerItemHeldEvent` |  |
| `PlayerJoinEvent` | `PlayerJoinEvent` |  |
| `PlayerLevelChangeEvent` | `PlayerLevelChangeEvent` |  |
| `PlayerLoginEvent` | `PlayerPreLoginEvent` | Cancel and KickMessage; deprecated in Paper |
| `PlayerMoveEvent` | `PlayerMoveEvent` |  |
| `PlayerPickupItemEvent` | `PlayerPickupItemEvent` | deprecated in Paper |
| `PlayerPortalEvent` | `PlayerPortalEvent` |  |
| `PlayerPreLoginEvent` | `PlayerPreLoginEvent` | deprecated in Paper |
| `PlayerQuitEvent` | `PlayerQuitEvent` |  |
| `PlayerResourcePackStatusEvent` | `PlayerResourcePackStatusEvent` |  |
| `PlayerRespawnEvent` | `PlayerRespawnEvent` |  |
| `PlayerSwapHandItemsEvent` | `PlayerSwapHandsEvent` |  |
| `PlayerTeleportEvent` | `PlayerTeleportEvent` |  |
| `PlayerToggleFlightEvent` | `PlayerToggleFlightEvent` |  |
| `PlayerToggleSneakEvent` | `PlayerToggleSneakEvent` |  |
| `PlayerToggleSprintEvent` | `PlayerToggleSprintEvent` |  |
| `PluginDisableEvent` | `PluginDisableEvent` |  |
| `PluginEnableEvent` | `PluginEnableEvent` |  |
| `PortalCreateEvent` | `PortalCreateEvent` |  |
| `PrepareAnvilEvent` | `PrepareAnvilEvent` |  |
| `PrepareItemCraftEvent` | `PrepareCraftEvent` |  |
| `ServerListPingEvent` | `ServerListPingEvent` |  |
| `ServerLoadEvent` | `ServerStartEvent` |  |
| `ServerTickEndEvent` | `TickEndEvent` |  |
| `ServerTickStartEvent` | `TickStartEvent` |  |
| `SpawnerSpawnEvent` | `EntitySpawnEvent` | Reason = Spawner |

## Partly covered

| Paper event | Ramjet event | What is missing |
|---|---|---|
| `AsyncChatDecorateEvent` | `PlayerChatEvent` | PlayerChat.Decoration is the signature-neutral equivalent (planned next) |
| `AsyncChatEvent` | `PlayerChatEvent` | PlayerChat; the recipient set cannot be edited per message (FilterKind/FilterBits only) (planned next) |
| `AsyncPlayerChatEvent` | `PlayerChatEvent` | PlayerChat; Paper's recipient set cannot be edited per message (FilterKind/FilterBits only); deprecated in Paper (planned next) |
| `ChatEvent` | `PlayerChatEvent` | Deprecated synchronous chat event; same gap as AsyncChatEvent (planned next) |
| `ClockTimeSkipEvent` | `TimeSkipEvent` | TimeSkip cannot be cancelled (not planned yet) |
| `EntityCombustByBlockEvent` | `EntityCombustEvent` | EntityCombust has an entity source only; no block source (planned next) |
| `EntityDeathEvent` | `EntityDeathEvent` | EntityDeath is not cancellable (Paper's is) and has no drop list or experience to change (planned next) |
| `EntityPickupItemEvent` | `PlayerPickupItemEvent` | PlayerPickupItem covers players; mobs, villagers and other entities picking up items have no event (planned next) |
| `PaperServerListPingEvent` | `ServerListPingEvent` | ServerListPing: MOTD, counts, hide players; no player sample, favicon or version text (not planned yet) |
| `PlayerChatEvent` | `PlayerChatEvent` | Deprecated synchronous chat event; same gap as AsyncPlayerChat (planned next) |
| `PlayerConnectionCloseEvent` | `PlayerQuitEvent` | PlayerQuit fires only for players that joined; connections closed during login or configuration fire nothing (not planned yet) |
| `PlayerInteractAtEntityEvent` | `PlayerInteractEntityEvent` | PlayerInteractEntity has no click position on the entity (planned next) |
| `PlayerKickEvent` | `PlayerKickEvent` | PlayerKick fires only for kicks by a command or a plugin; kicks the server decides itself (idle timeout, flying, invalid chat) fire nothing (planned next) |
| `PlayerPostRespawnEvent` | `PlayerRespawnEvent` | PlayerRespawn fires before the respawn and carries the destination; no event after it (not planned yet) |
| `ThunderChangeEvent` | `ThunderChangeEvent` | ThunderChange is cancellable and gives the cause; it fires before the thunder flips |
| `TimeSkipEvent` | `TimeSkipEvent` | TimeSkip cannot be cancelled (not planned yet) |
| `TrialSpawnerSpawnEvent` | `EntitySpawnEvent` | EntitySpawn has no trial spawner reason (planned next) |
| `WeatherChangeEvent` | `RainChangeEvent` | RainChange is cancellable and gives the cause; it fires before the rain flips. WeatherChange still reports the visible change |

## What is planned

The missing events by area. **Next** is what we add first; events that hook the same mechanic land together. New events are cancellable where Paper's are and the vanilla code allows it.

### Explosions and projectiles

- **Next** (16): `AreaEffectCloudApplyEvent`, `BlockExplodeEvent`, `EntityExplodeEvent`, `EntityShootBowEvent`, `ExpBottleEvent`, `ExplosionPrimeEvent`, `FireworkExplodeEvent`, `LingeringPotionSplashEvent`, `PlayerEggThrowEvent`, `PlayerLaunchProjectileEvent`, `PlayerPickupArrowEvent`, `PlayerRiptideEvent`, `PotionSplashEvent`, `ProjectileHitEvent`, `ProjectileLaunchEvent`, `TNTPrimeEvent (Bukkit)`
- **Not planned yet** (7): `ArrowBodyCountChangeEvent`, `EntityIgniteEvent`, `EntityLoadCrossbowEvent`, `PlayerReadyArrowEvent`, `TargetHitEvent`, `ThrownEggHatchEvent`, `WaterBottleSplashEvent`

### Blocks

- **Next** (13): `BlockDestroyEvent`, `BlockDispenseArmorEvent`, `BlockDispenseEvent`, `BlockFertilizeEvent`, `BlockFromToEvent`, `BlockPhysicsEvent`, `BlockPreDispenseEvent`, `CauldronLevelChangeEvent`, `EntityBlockFormEvent`, `FluidLevelChangeEvent`, `LeavesDecayEvent`, `MoistureChangeEvent`, `StructureGrowEvent`
- **Later** (4): `BeaconEffectEvent`, `BellRingEvent`, `BlockReceiveGameEvent`, `SpongeAbsorbEvent`
- **Not planned yet** (13): `BeaconActivatedEvent`, `BeaconDeactivatedEvent`, `BellResonateEvent`, `BlockBreakBlockEvent`, `BlockDispenseLootEvent`, `BlockFailedDispenseEvent`, `BlockLockCheckEvent`, `BlockShearEntityEvent`, `DragonEggFormEvent`, `NotePlayEvent`, `SculkBloomEvent`, `VaultChangeStateEvent`, `VaultDisplayItemEvent`

### Entities

- **Next** (39): `CreeperPowerEvent`, `EntityBreakDoorEvent`, `EntityBreedEvent`, `EntityChangeBlockEvent`, `EntityCombustByBlockEvent`, `EntityDeathEvent`, `EntityDropItemEvent`, `EntityFertilizeEggEvent`, `EntityInteractEvent`, `EntityKnockbackByEntityEvent (Paper)`, `EntityKnockbackByEntityEvent (Bukkit)`, `EntityKnockbackEvent (Paper)`, `EntityKnockbackEvent (Bukkit)`, `EntityMoveEvent`, `EntityPickupItemEvent`, `EntityPoseChangeEvent`, `EntityPotionEffectEvent`, `EntityPushedByEntityAttackEvent`, `EntityRegainHealthEvent`, `EntityResurrectEvent`, `EntityTameEvent`, `EntityTargetEvent`, `EntityTargetLivingEntityEvent`, `EntityToggleGlideEvent`, `EntityToggleSwimEvent`, `EntityTransformEvent`, `EntityUnleashEvent`, `EntityZapEvent`, `ExperienceOrbMergeEvent`, `ItemDespawnEvent`, `ItemMergeEvent`, `PlayerLeashEntityEvent`, `PlayerNaturallySpawnCreaturesEvent`, `PlayerUnleashEntityEvent`, `PreCreatureSpawnEvent`, `PreSpawnerSpawnEvent`, `SheepDyeWoolEvent`, `SlimeSplitEvent`, `TrialSpawnerSpawnEvent`
- **Later** (12): `EntityBreakByEntityEvent`, `EntityBreakEvent`, `EntityJumpEvent`, `EntityLungeEvent`, `EntityPathfindEvent`, `EntityPortalEnterEvent`, `EntityPortalEvent`, `EntityPortalExitEvent`, `EntityRemoveEvent`, `EntityTeleportEvent`, `ItemTransportingEntityValidateTargetEvent`, `SlimePathfindEvent`
- **Not planned yet** (51): `BatToggleSleepEvent`, `CreeperIgniteEvent`, `ElderGuardianAppearanceEvent`, `EnderDragonChangePhaseEvent`, `EnderDragonFireballHitEvent`, `EnderDragonFlameEvent`, `EnderDragonShootFireballEvent`, `EndermanAttackPlayerEvent`, `EndermanEscapeEvent`, `EntityAddToWorldEvent`, `EntityAirChangeEvent`, `EntityAttemptSmashAttackEvent`, `EntityAttemptSpinAttackEvent`, `EntityCollideWithEntityEvent`, `EntityCompostItemEvent`, `EntityConstructEvent`, `EntityDamageItemEvent`, `EntityDyeEvent`, `EntityEffectTickEvent`, `EntityEnterBlockEvent`, `EntityEnterLoveModeEvent`, `EntityEquipmentChangedEvent`, `EntityInsideBlockEvent`, `EntityPortalReadyEvent`, `EntityRemoveFromWorldEvent`, `EntitySpellCastEvent`, `EntityTeleportEndGatewayEvent`, `ItemTransportingEntitySortEvent`, `PhantomPreSpawnEvent`, `PigZombieAngerEvent`, `PufferFishStateChangeEvent`, `SheepRegrowWoolEvent`, `ShulkerDuplicateEvent`, `SkeletonHorseTrapEvent`, `SlimeChangeDirectionEvent`, `SlimeSwimEvent`, `SlimeTargetLivingEntityEvent`, `SlimeWanderEvent`, `StriderTemperatureChangeEvent`, `SulfurCubeSwallowItemEvent`, `TameableDeathMessageEvent`, `TurtleGoHomeEvent`, `TurtleLayEggEvent`, `TurtleStartDiggingEvent`, `VillagerAcquireTradeEvent`, `VillagerCareerChangeEvent`, `VillagerReplenishTradeEvent`, `WardenAngerChangeEvent`, `WitchConsumePotionEvent`, `WitchReadyPotionEvent`, `WitchThrowPotionEvent`

### Players

- **Next** (39): `BlockCanBuildEvent`, `BlockDropItemEvent`, `BlockExpEvent`, `BlockMultiPlaceEvent`, `EntityExhaustionEvent`, `EntityPlaceEvent`, `FoodLevelChangeEvent`, `HangingBreakByEntityEvent`, `HangingBreakEvent`, `HangingPlaceEvent`, `PlayerAdvancementDoneEvent`, `PlayerArmSwingEvent`, `PlayerArmorChangeEvent`, `PlayerArmorStandManipulateEvent`, `PlayerBedEnterEvent`, `PlayerBedLeaveEvent`, `PlayerBucketEmptyEvent`, `PlayerBucketEntityEvent`, `PlayerBucketFillEvent`, `PlayerEditBookEvent`, `PlayerExpChangeEvent`, `PlayerFishEvent`, `PlayerInteractAtEntityEvent`, `PlayerItemBreakEvent`, `PlayerItemDamageEvent`, `PlayerItemMendEvent`, `PlayerJumpEvent`, `PlayerKickEvent`, `PlayerLocaleChangeEvent`, `PlayerOpenSignEvent`, `PlayerPickupExperienceEvent`, `PlayerRecipeBookClickEvent (Paper)`, `PlayerSetSpawnEvent`, `PlayerShearEntityEvent`, `PlayerStartSpectatingEntityEvent`, `PlayerStopSpectatingEntityEvent`, `PlayerTakeLecternBookEvent`, `PlayerVelocityEvent`, `SignChangeEvent`
- **Later** (2): `PlayerFlowerPotManipulateEvent`, `PlayerShearBlockEvent`
- **Not planned yet** (41): `AsyncPlayerSpawnLocationEvent`, `BlockBreakProgressUpdateEvent`, `BlockDamageAbortEvent`, `FishHookStateChangeEvent`, `PlayerAdvancementCriterionGrantEvent`, `PlayerBedFailEnterEvent`, `PlayerChangedMainHandEvent`, `PlayerClientLoadedWorldEvent`, `PlayerClientOptionsChangeEvent`, `PlayerDeepSleepEvent`, `PlayerElytraBoostEvent`, `PlayerExpCooldownChangeEvent`, `PlayerFailMoveEvent`, `PlayerHarvestBlockEvent`, `PlayerHideEntityEvent`, `PlayerInputEvent`, `PlayerInsertLecternBookEvent`, `PlayerInventorySlotChangeEvent`, `PlayerItemCooldownEvent`, `PlayerItemFrameChangeEvent`, `PlayerItemGroupCooldownEvent`, `PlayerLecternPageChangeEvent`, `PlayerMapFilledEvent`, `PlayerNameEntityEvent`, `PlayerPickBlockEvent`, `PlayerPickEntityEvent`, `PlayerPostRespawnEvent`, `PlayerRecipeBookSettingsChangeEvent`, `PlayerRecipeDiscoverEvent`, `PlayerShieldDisableEvent`, `PlayerShowEntityEvent`, `PlayerSignCommandPreprocessEvent`, `PlayerStatisticIncrementEvent`, `PlayerStopUsingItemEvent`, `PlayerSwapWithEquipmentSlotEvent`, `PlayerTeleportEndGatewayEvent`, `PlayerToggleEntityAgeLockEvent`, `PlayerTrackEntityEvent`, `PlayerUntrackEntityEvent`, `PrePlayerAttackEntityEvent`, `UncheckedSignChangeEvent`

### Vehicles and mounts

- **Next** (12): `EntityDismountEvent`, `EntityMountEvent`, `HorseJumpEvent`, `VehicleBlockCollisionEvent`, `VehicleCreateEvent`, `VehicleDamageEvent`, `VehicleDestroyEvent`, `VehicleEnterEvent`, `VehicleEntityCollisionEvent`, `VehicleExitEvent`, `VehicleMoveEvent`, `VehicleUpdateEvent`
- **Not planned yet** (1): `EntityToggleSitEvent`

### Inventories, crafting and trading

- **Next** (14): `AnvilDamagedEvent`, `BlockCookEvent`, `BrewEvent`, `BrewingStartEvent`, `CampfireStartEvent`, `FurnaceExtractEvent`, `FurnaceStartSmeltEvent`, `InventoryMoveItemEvent`, `InventoryPickupItemEvent`, `PlayerStonecutterRecipeSelectEvent`, `PlayerTradeEvent`, `PrepareSmithingEvent`, `SmithItemEvent`, `TradeSelectEvent`
- **Later** (2): `PiglinBarterEvent`, `PlayerPurchaseEvent`
- **Not planned yet** (11): `BrewingStandFuelEvent`, `CartographyItemEvent`, `CompostItemEvent`, `CrafterCraftEvent`, `HopperInventorySearchEvent`, `ItemCraftedEvent`, `PlayerBundleItemSelectEvent`, `PlayerChangeBeaconEffectEvent`, `PlayerLoomPatternSelectEvent`, `PrepareHopperTransferEvent`, `PrepareItemEnchantEvent`

### World and weather

- **Covered** (8): `LightningStrikeEvent`, `SpawnChangeEvent`, `ThunderChangeEvent`, `WeatherChangeEvent` (through `RainChange`), `WorldInitEvent`, `WorldLoadEvent`, `WorldSaveEvent`, `WorldUnloadEvent`
- **Later** (8): `AsyncStructureSpawnEvent`, `ChunkPopulateEvent`, `EntitiesLoadEvent`, `EntitiesUnloadEvent`, `GenericGameEvent`, `RaidFinishEvent`, `RaidTriggerEvent`, `StructuresLocateEvent`
- **Not planned yet** (13): `AsyncStructureGenerateEvent`, `ClockTimeSkipEvent`, `LootGenerateEvent`, `LootableInventoryReplenishEvent`, `MapInitializeEvent`, `RaidSpawnWaveEvent`, `RaidStopEvent`, `TimeSkipEvent`, `WorldBorderBoundsChangeEvent`, `WorldBorderBoundsChangeFinishEvent`, `WorldBorderCenterChangeEvent`, `WorldDifficultyChangeEvent`, `WorldGameRuleChangeEvent`

### Chat

- **Next** (5): `AsyncChatDecorateEvent`, `AsyncChatEvent`, `AsyncPlayerChatEvent`, `ChatEvent`, `PlayerChatEvent`
- **Not planned yet** (1): `AsyncChatCommandDecorateEvent`

### Server, commands and connections

- **Next** (7): `AsyncPlayerSendCommandsEvent`, `AsyncTabCompleteEvent`, `PlayerCommandSendEvent`, `PlayerServerFullCheckEvent`, `ServerCommandEvent`, `TabCompleteEvent`, `UnknownCommandEvent`
- **Later** (2): `AsyncPlayerConnectionConfigureEvent`, `RemoteServerCommandEvent`
- **Not planned yet** (23): `AsyncPlayerSendSuggestionsEvent`, `BroadcastMessageEvent`, `ClientTickEndEvent`, `FillProfileEvent`, `GS4QueryEvent`, `LookupProfileEvent`, `PaperServerListPingEvent`, `PlayerChunkLoadEvent`, `PlayerChunkUnloadEvent`, `PlayerCodeOfConductSendEvent`, `PlayerConnectionCloseEvent`, `PlayerConnectionInitialConfigureEvent`, `PlayerConnectionReconfigureEvent`, `PlayerConnectionValidateLoginEvent`, `PlayerLinksSendEvent`, `PlayerRegisterChannelEvent`, `PlayerUnregisterChannelEvent`, `PreFillProfileEvent`, `PreLookupProfileEvent`, `ProfileWhitelistVerifyEvent`, `ServerResourcesReloadedEvent`, `WhitelistStateUpdateEvent`, `WhitelistToggleEvent`

## Not applicable

| Paper event | Why |
|---|---|
| `AsyncPlayerChatPreviewEvent` | Deprecated; chat preview no longer exists in the protocol |
| `AsyncServerDataFixerRemoveBlockEntityEvent` | Ramjet has no datafixer for old worlds |
| `BellRevealRaiderEvent` | Deprecated; superseded by BellResonate |
| `CommandRegisteredEvent` | Deprecated; Bukkit-to-Brigadier command sync does not exist (all commands are Brigadier nodes) |
| `EntityCreatePortalEvent` | Deprecated; superseded by PortalCreate, which Ramjet has |
| `IllegalPacketEvent` | Deprecated, never fired by Paper |
| `InventoryBlockStartEvent` | Bukkit base class of FurnaceStartSmelt / BrewingStart / CampfireStart |
| `InventoryEvent` | Bukkit base class, never fired by itself |
| `PigZapEvent` | Deprecated; superseded by EntityZap |
| `PlayerAnimationEvent` | Deprecated; superseded by PlayerArmSwing |
| `PlayerAttackEntityCooldownResetEvent` | Deprecated; superseded by PrePlayerAttackEntity |
| `PlayerBucketFishEvent` | Deprecated; superseded by PlayerBucketEntity |
| `PlayerChatTabCompleteEvent` | Deprecated; chat tab completion no longer exists in the protocol |
| `PlayerHandshakeEvent` | BungeeCord / Velocity handshake spoofing; not applicable to Ramjet login |
| `PlayerRecipeBookClickEvent (Bukkit)` | Deprecated; superseded by the Paper event of the same name |
| `PlayerSignOpenEvent` | Deprecated; superseded by PlayerOpenSign |
| `PlayerSpawnChangeEvent` | Deprecated; superseded by PlayerSetSpawn |
| `PlayerSpawnLocationEvent` | Deprecated; superseded by AsyncPlayerSpawnLocation |
| `PlayerUseUnknownEntityEvent` | Paper implementation detail: clicks on entity ids the server does not know |
| `PrepareGrindstoneEvent` | Deprecated; superseded by the Paper PrepareResultEvent |
| `PrepareInventoryResultEvent` | Deprecated base class of the Prepare* events |
| `PrepareResultEvent` | Paper base class of the Prepare* events, not fired by itself |
| `ProjectileCollideEvent` | Deprecated; superseded by ProjectileHitEvent |
| `RegionizedServerInitEvent` | Folia only |
| `ServerExceptionEvent` | Plugins run in an isolated worker process; their exceptions are logged by the host, not delivered to other plugins |
| `ServiceRegisterEvent` | Bukkit ServicesManager; Ramjet's Services.Changed callback replaces it |
| `ServiceUnregisterEvent` | Bukkit ServicesManager; Ramjet's Services.Changed callback replaces it |
| `TNTPrimeEvent (Paper)` | Deprecated; superseded by TNTPrimeEvent in org.bukkit.event.block |

## Events Paper does not have

Ramjet events without a Paper counterpart: `PacketReceiveEvent` (and `PacketSendEvent`, reserved) for typed packets, `PlayerInteractBlockEvent`, `PrivateMessageEvent`, `PermissionChangedEvent` and `PermissionsReloadedEvent`, `CommandsRegisteredEvent`, `MenuClickEvent` and `MenuCloseEvent` for plugin GUIs, `CustomItemUseEvent`, `CustomBlockPlaceEvent`, `CustomBlockBreakEvent` and `CustomBlockInteractEvent` for the content layer, and `NpcInteractEvent`. Combat tag start and end and a pre-quit event, which the first-party PvP plugin needs, are being added.

## Until an event exists

- Poll in a [system](../concepts/systems.md) or a scheduled task, comparing against the state you saved on the previous tick. State that Ramjet exposes as a component (health, food, effects, position) is cheap to compare.
- Events that come from a client packet can be observed with `PacketReceiveEvent`.
- Ask for the event in an issue on the SDK repository, with the plugin that needs it; the plugins we looked at decide the order.
