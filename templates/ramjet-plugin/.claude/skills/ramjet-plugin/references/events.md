# Event catalogue

Every event of the plugin API. "Engine: fired" means the engine has a firing site for it today; "not yet" events can be subscribed to but nothing raises them. Mutable fields are the ones a handler may write.json. Do not edit by hand.

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

## Command argument types (vanilla registry)

`brigadier:bool`, `brigadier:float`, `brigadier:double`, `brigadier:integer`, `brigadier:long`, `brigadier:string`, `minecraft:entity`, `minecraft:game_profile`, `minecraft:block_pos`, `minecraft:column_pos`, `minecraft:vec3`, `minecraft:vec2`, `minecraft:block_state`, `minecraft:block_predicate`, `minecraft:item_stack`, `minecraft:item_predicate`, `minecraft:team_color`, `minecraft:hex_color`, `minecraft:component`, `minecraft:style`, `minecraft:message`, `minecraft:nbt_compound_tag`, `minecraft:nbt_tag`, `minecraft:nbt_path`, `minecraft:objective`, `minecraft:objective_criteria`, `minecraft:operation`, `minecraft:particle`, `minecraft:angle`, `minecraft:rotation`, `minecraft:scoreboard_slot`, `minecraft:score_holder`, `minecraft:swizzle`, `minecraft:team`, `minecraft:item_slot`, `minecraft:item_slots`, `minecraft:resource_location`, `minecraft:function`, `minecraft:entity_anchor`, `minecraft:int_range`, `minecraft:float_range`, `minecraft:dimension`, `minecraft:gamemode`, `minecraft:time`, `minecraft:resource_or_tag`, `minecraft:resource_or_tag_key`, `minecraft:resource`, `minecraft:resource_key`, `minecraft:resource_selector`, `minecraft:template_mirror`, `minecraft:template_rotation`, `minecraft:heightmap`, `minecraft:loot_table`, `minecraft:loot_predicate`, `minecraft:loot_modifier`, `minecraft:context_float_provider`, `minecraft:context_int_provider`, `minecraft:slot_source`, `minecraft:dialog`, `minecraft:feature`, `minecraft:swing_animation`, `minecraft:uuid`
