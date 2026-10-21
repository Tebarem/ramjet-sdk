# Mc.Scripting.Api: public types and members

Signatures of every public type of the plugin API (namespace `Mc.Scripting`) except the event structs, which are in events.md.json. Do not edit by hand.

## ApiVersion (struct, plugins)

```csharp
public readonly struct ApiVersion : IComparable<ApiVersion>, IEquatable<ApiVersion>
```

The version of the plugin API (semantic: a minor increase is additive, a major increase is breaking).

- `public ApiVersion(int Major, int Minor)`: The version of the plugin API (semantic: a minor increase is additive, a major increase is breaking).
- `public int Major { get; init; }`: Breaking changes.
- `public int Minor { get; init; }`: Additive changes.
- `public static ApiVersion Current { get; }`: The API version this server provides.
- `public int CompareTo(ApiVersion other)`
- `public bool Satisfies(ApiVersion required)`: Whether a plugin that needs `required` can run on this server version.
- `public override string ToString()`
- `public static bool TryParse(string? text, out ApiVersion version)`: Parses "1.0", "1" or "1.x".

## ArmorSlot (enum, other)

```csharp
public enum ArmorSlot : byte
```

A worn armor slot (PlayerArmorChange.Slot).

- `Feet` = 0: Boots.
- `Legs` = 1: Leggings.
- `Chest` = 2: Chestplate.
- `Head` = 3: Helmet.

## AttributeOperation (enum, other)

```csharp
public enum AttributeOperation : int
```

How an attribute modifier combines with the attribute (vanilla `AttributeModifier.Operation`).

- `AddValue` = 0: Adds the amount to the base value.
- `AddMultipliedBase` = 1: Adds the amount times the base value.
- `AddMultipliedTotal` = 2: Multiplies the total by (1 + amount), after the other two operations.

## AttributeValue (struct, other)

```csharp
public readonly struct AttributeValue : IEquatable<AttributeValue>
```

The base and the modified value of an attribute of a living entity.

- `public AttributeValue(double Base, double Value)`: The base and the modified value of an attribute of a living entity.
- `public double Base { get; init; }`: The base value (what `LivingEntity.SetAttributeBase` writes).
- `public double Value { get; init; }`: The value with every modifier applied.

## BedEnterResult (enum, other)

```csharp
public enum BedEnterResult : byte
```

The outcome of a bed use as the server found it (PlayerBedEnter.Result).

- `Ok` = 0: The player can sleep.
- `TooFarAway` = 1: The bed is out of reach.
- `Obstructed` = 2: Something blocks the space above the bed.
- `NotSafe` = 3: Monsters are near the bed.
- `NotPossibleHere` = 4: The dimension does not let the player sleep here.
- `Other` = 5: Any other reason.

## BedUseResult (enum, other)

```csharp
public enum BedUseResult : byte
```

What a plugin decides for a bed use (PlayerBedEnter.UseBed).

- `Default` = 0: Keep the outcome the server found.
- `Allow` = 1: Sleep even when the server found a problem.
- `Deny` = 2: Stay awake.

## Belowname (class, display)

```csharp
public sealed class Belowname
```

Text (and optionally a number) shown under a player's name, per viewer. It uses the engine's below-name objective, which is defined while a player has one. Reach it as `plugin.Belowname`. Call from the tick thread. What a plugin sets is removed when the plugin unloads.

- `public void Clear(Player viewer, Player target)`: Removes the text under one player's name for one viewer.
- `public void Set(Player viewer, Player target, Component text, int? number)`: Shows text under one player's name for one viewer, with a number when given (hidden when null).

## BlockFace (enum, worlds)

```csharp
public enum BlockFace : byte
```

A block face / direction (vanilla Direction order).

- `Down` = 0: Negative Y.
- `Up` = 1: Positive Y.
- `North` = 2: Negative Z.
- `South` = 3: Positive Z.
- `West` = 4: Negative X.
- `East` = 5: Positive X.

## BlockPos (struct, primitives)

```csharp
public struct BlockPos : IEquatable<BlockPos>
```

Integer block coordinates.

- `public BlockPos(int x, int y, int z)`: Creates a block position.
- `public int X;`: Components.
- `public int Y;`: Components.
- `public int Z;`: Components.
- `public bool Equals(BlockPos o)`: Equality.
- `public override bool Equals(object? obj)`: Equality.
- `public override int GetHashCode()`: Hash code.
- `public override string ToString()`: Text form.

## BossBar (class, display)

```csharp
public sealed class BossBar : IDisposable, IPluginResource
```

A boss bar at the top of the screen, shown to the players who were added as viewers. Create it with `BossBars.Create`; it disappears from every viewer when the plugin unloads (or when `BossBar.Remove` is called). Bars are not saved with the world.

- `public BossBarColor Color { get; set; }`: The colour.
- `public BossBarFlags Flags { get; set; }`: Darkening, music and fog.
- `public bool IsActive { get; }`: Whether the bar still exists (false after `BossBar.Remove` or the plugin unloading).
- `public string Key { get; }`: The full key of the bar ("pluginid:name"); also its id in `/bossbar`.
- `public float Progress { get; set; }`: How full the bar is, 0..1.
- `public BossBarStyle Style { get; set; }`: The segmentation.
- `public Component Title { get; set; }`: The text above the bar.
- `public IReadOnlyCollection<Uuid> Viewers { get; }`: The profile ids of the players who see (or will see, when they join) the bar.
- `public bool Visible { get; set; }`: Whether the viewers currently see the bar (hiding keeps the viewer list).
- `public BossBar AddViewer(Player player)`: Shows the bar to a player until `BossBar.RemoveViewer` or until the player leaves the server.
- `public BossBar AddViewer(Uuid player)`: Shows the bar to a player (by profile id, also an offline one: the bar appears when he joins).
- `public void Dispose()`
- `public void Remove()`: Removes the bar from every viewer and deletes it.
- `public BossBar RemoveViewer(Player player)`: Hides the bar from a player.
- `public BossBar RemoveViewer(Uuid player)`: Hides the bar from a player (by profile id).

## BossBarColor (enum, display)

```csharp
public enum BossBarColor : int
```

The colour of a boss bar.

- `Pink` = 0: Pink.
- `Blue` = 1: Blue.
- `Red` = 2: Red.
- `Green` = 3: Green.
- `Yellow` = 4: Yellow.
- `Purple` = 5: Purple.
- `White` = 6: White.

## BossBarFlags (enum, display)

```csharp
public enum BossBarFlags : int
```

Extra behaviour of a boss bar.

- `None` = 0: Nothing special.
- `DarkenScreen` = 1: Darkens the sky.
- `PlayMusic` = 2: Plays the boss music.
- `CreateFog` = 4: Creates fog.

## BossBarStyle (enum, display)

```csharp
public enum BossBarStyle : int
```

How a boss bar is divided.

- `Solid` = 0: A plain bar.
- `Segmented6` = 1: Six segments.
- `Segmented10` = 2: Ten segments.
- `Segmented12` = 3: Twelve segments.
- `Segmented20` = 4: Twenty segments.

## BossBars (class, display)

```csharp
public sealed class BossBars
```

Creates boss bars owned by a plugin (`plugin.BossBars`).

- `public BossBar Create(string name, Component title, BossBarColor color = BossBarColor.Pink, BossBarStyle style = BossBarStyle.Solid, float progress = 1f)`: Creates a boss bar with nobody watching; add viewers to show it.

## CauldronChangeReason (enum, other)

```csharp
public enum CauldronChangeReason : byte
```

Why a cauldron's contents or level change (CauldronLevelChange).

- `BucketFill` = 0: A bucket takes the fluid out of the cauldron.
- `BucketEmpty` = 1: A bucket pours its fluid into the cauldron.
- `BottleFill` = 2: A glass bottle takes water out of a water cauldron.
- `BottleEmpty` = 3: A water bottle pours its water into the cauldron.
- `BannerWash` = 4: A banner loses its top pattern in a water cauldron.
- `ArmorWash` = 5: Dyed armour or wolf armour loses its colour in a water cauldron.
- `ShulkerWash` = 6: A dyed shulker box loses its colour in a water cauldron.
- `NaturalFill` = 7: Rain, snow or a dripstone fills the cauldron (no player).
- `Extinguish` = 8: Reserved: a burning entity is put out in a water cauldron. Not raised yet (that mechanic is not ported).
- `Unknown` = 9: Any other change.

## ChannelRegistry (class, chat)

```csharp
public sealed class ChannelRegistry
```

Plugin messaging channels (Paper's Messenger, sendPluginMessage). `ChannelRegistry.On` listens on a channel: the messages that the clients send on it arrive in the handler. The engine tells the clients which channels are listened on (minecraft:register). Send with {{cref:M:Mc.Scripting.Player.SendPluginMessage(System.String,System.ReadOnlySpan{System.Byte})}}.

Refused: the minecraft:* channels (a client's own registrations are read through `Player.Channels`), velocity:* and bungeecord:*. Messages arrive in the tick window, in order per player, within the budgets of scripting.channels (a message beyond them is dropped). A listener ends with its plugin (unload or reload). Tick thread only. The worker delivers the messages; the in-process runtime does not.

- `public ChannelSubscription On(string channel, PluginMessageHandler handler)`: Listens on a channel. The first listener of a channel makes the engine announce it to the clients (minecraft:register).

## ChannelSubscription (class, chat)

```csharp
public sealed class ChannelSubscription : IDisposable, IPluginResource
```

A listener on a plugin messaging channel (`ChannelRegistry.On`). Dispose it to stop listening.

- `public string Channel { get; }`: The channel the subscription listens on.
- `public bool IsActive { get; }`: Whether the subscription still listens (false after Dispose).
- `public void Dispose()`: Stops listening. When it was the last listener of its channel, the engine stops announcing the channel to the clients.

## Chat (class, chat)

```csharp
public static class Chat
```

Chat lines that no player signed: the disguised chat the console's `/say` sends. The chat type picks the vanilla format and the name and message are shown in it. Use it for lines that are not a player's own words (a notice in a staff channel, a relayed message). The line is unsigned and has no signature marker, so it is never shown as a player's verified chat; for a player's own words use the chat event instead.

- `public static int SendTo(IEnumerable<Player> recipients, ChatKind kind, Component name, Component? target, Component message)`: Sends an unsigned chat line to some players. The line is formatted from `kind` and `name` (the sender's name as the line shows it) and `message`. The message kinds (`ChatKind.MsgCommandIncoming`, `ChatKind.MsgCommandOutgoing`, `ChatKind.TeamMsgCommandIncoming`, `ChatKind.TeamMsgCommandOutgoing`) need `target` (the other side's name, as the vanilla format shows it); without one the call returns -1. Players who do not accept chat are skipped.

## ChatFilterKind (enum, chat)

```csharp
public enum ChatFilterKind : byte
```

How the recipients who filter text see a chat message (vanilla FilterMask).

- `PassThrough` = 0: The text as typed.
- `FullyFiltered` = 1: Filtering recipients get nothing of the message.
- `PartiallyFiltered` = 2: Filtering recipients see the text with the characters of filter_bits hidden.

## ChatKind (enum, chat)

```csharp
public enum ChatKind : byte
```

The chat type of a line: the vanilla minecraft:chat_type entries the server uses.

- `Chat` = 0: A player's chat line (minecraft:chat).
- `EmoteCommand` = 1: The /me line (minecraft:emote_command).
- `SayCommand` = 2: The /say line (minecraft:say_command).
- `MsgCommandIncoming` = 3: A private message received (minecraft:msg_command_incoming). Needs a target.
- `MsgCommandOutgoing` = 4: A private message sent (minecraft:msg_command_outgoing). Needs a target.
- `TeamMsgCommandIncoming` = 5: A team message received (minecraft:team_msg_command_incoming). Needs a target.
- `TeamMsgCommandOutgoing` = 6: A team message sent (minecraft:team_msg_command_outgoing). Needs a target.

## ChatMute (class, chat)

```csharp
public static class ChatMute
```

Chat mutes. The engine keeps the mute table itself (it survives plugin reloads and server restarts) and refuses a muted player's chat lines, `/say`, `/me` and private messages before a plugin sees them, so a plugin does not check mutes on the message path. Commands and other actions are not affected. The reason is what the engine's mute notice (`chat.mute_notice`) can show the player. Thread: tick thread.

- `public static bool Clear(Uuid player)`: Lifts a player's chat mute.
- `public static ChatMuteEntry? Get(Uuid player)`: The player's active chat mute, null when the player is not muted (an expired mute is not active).
- `public static IReadOnlyList<ChatMuteEntry> List()`: All active chat mutes, in profile id order: a snapshot, read from the engine in pages when this is called.
- `public static bool Set(Uuid player, DateTimeOffset? expires, string? reason)`: Mutes a player's chat. A mute already in force is replaced by this one; the same mute again changes nothing.

## ChatMuteEntry (class, chat)

```csharp
public sealed class ChatMuteEntry : IEquatable<ChatMuteEntry>
```

An active chat mute: the muted player, when the mute ends and the reason it was given.

- `public ChatMuteEntry(Uuid Player, DateTimeOffset? Expires, string? Reason)`: An active chat mute: the muted player, when the mute ends and the reason it was given.
- `public DateTimeOffset? Expires { get; init; }`: When the mute ends; null = forever.
- `public Uuid Player { get; init; }`: The muted player's profile id.
- `public string? Reason { get; init; }`: The reason the mute was given; null when none.

## ChatRoute (class, chat)

```csharp
public static class ChatRoute
```

Restricts who gets a chat message while its handler runs. Signed chat stays signed: a restricted line is sent to the listed players only, as the engine's own private messages are.

- `public static IReadOnlyList<Player> Audience()`: The audience of the running handler: the players who would get the line now. Without a restriction that is every player who accepts chat (a `PlayerChatEvent` line); after {{cref:M:Mc.Scripting.ChatRoute.Restrict(System.Collections.Generic.IEnumerable{Mc.Scripting.Player})}} it is the restricted list. Offline profiles are left out.
- `public static int Restrict(IEnumerable<Player> recipients)`: Restricts the recipients of the chat message of the running handler. Only the listed players get the line. Works in a `PlayerChatEvent` or a `PrivateMessageEvent` handler; the restriction applies to that one message only and ends when the dispatch is over. A second call replaces the list. An empty list means nobody gets the line. Offline handles are left out.

## ClickType (enum, inventories)

```csharp
public enum ClickType : byte
```

How an inventory slot was clicked (vanilla ContainerInput folded with the button).

- `Left` = 0: Left click.
- `ShiftLeft` = 1: Shift + left click (quick move).
- `Right` = 2: Right click.
- `ShiftRight` = 3: Shift + right click (quick move).
- `Middle` = 4: Middle click (clone, creative).
- `NumberKey` = 5: A hotbar number key (swap).
- `DoubleClick` = 6: Double click (collect to cursor).
- `Drop` = 7: Drop key over a slot.
- `ControlDrop` = 8: Control + drop key over a slot (whole stack).
- `WindowBorderLeft` = 9: Left click outside the window (drops the cursor stack).
- `WindowBorderRight` = 10: Right click outside the window (drops one).
- `SwapOffhand` = 11: Swap with the off hand (F key).
- `Unknown` = 12: Anything else.
- `Drag` = 13: The end of a drag over several slots (only delivered to MenuClick, which has no InventoryDrag counterpart for locked menus).

## Combat (class, other)

```csharp
public static class Combat
```

The combat tag: an allowed PvP hit tags both sides for the policy's tag time (native, no worker round trip per hit). The tag ends by expiry, death, logout or `Combat.Untag`. Events: `CombatTagStart` and `CombatTagEnd`. All members must be called on the tick thread.

- `public static Player? Opponent(Player player)`: The player who tagged this one, or null when the player is not tagged.
- `public static TimeSpan Remaining(Player player)`: The time of combat tag left (zero when the player is not tagged).
- `public static bool Tag(Player player, TimeSpan duration, Player? opponent = null)`: Tags a player against an opponent (or no one) for `duration`. A tag already on refreshes its time.
- `public static bool Untag(Player player)`: Ends the player's combat tag (reason untag). Returns false when the player was not tagged.

## CombatTagEndReason (enum, other)

```csharp
public enum CombatTagEndReason : byte
```

Why a combat tag ended (CombatTagEnd).

- `Expired` = 0: The tag time ran out.
- `Death` = 1: The player died.
- `Logout` = 2: The player left the server.
- `Untag` = 3: A plugin cleared the tag (Combat.Untag).

## CommandResult (struct, commands)

```csharp
public readonly struct CommandResult : IEquatable<CommandResult>
```

The outcome of `Server.DispatchCommand` and `Server.DispatchCommand`.

- `public CommandResult(bool Success, int Result, string? Error)`: The outcome of `Server.DispatchCommand` and `Server.DispatchCommand`.
- `public string? Error { get; init; }`: The message for the failure, in English, as the sender would see it; null on success.
- `public int Result { get; init; }`: The command's result as vanilla's dispatcher reports it (the value its executor returns); 0 on failure.
- `public bool Success { get; init; }`: True when the command ran without failing. A vanilla command that matched no executor, a syntax error, or a plugin command that failed (a `CommandFailure` or an exception) is not a success.

## Arg (class, commands)

```csharp
public static class Arg
```

Factories for the argument types of the vanilla command engine. Values are read in the executor with the typed getters of `CommandContext` (`GetInt`, `GetPlayer`, `GetBlockPos`, ...); types without a typed getter are read as text with `CommandContext.GetText`.

- `public static ArgType Angle()`: A yaw angle, absolute or relative (`90`, `~-10`).
- `public static ArgType BlockPos()`: A block position (`1 64 1`, `~ ~1 ~`, `^ ^ ^2`).
- `public static ArgType BlockPredicate()`: A block predicate (state, tag, block entity data).
- `public static ArgType BlockState()`: A block state such as `minecraft:oak_stairs[facing=north]` (read with `CommandContext.GetBlockState`).
- `public static ArgType Bool()`: true / false.
- `public static ArgType Color()`: A named chat colour (`red`, `gold`, ...) or `reset`.
- `public static ArgType ColumnPos()`: An x/z column position.
- `public static ArgType Component()`: A text component in JSON/SNBT form (read with `CommandContext.GetText`).
- `public static ArgType ContextFloatProvider()`: A float provider as the loot / worldgen data writes it (a constant, `{type:"minecraft:uniform",min_inclusive:0,max_exclusive:1}`, ...).
- `public static ArgType ContextIntProvider()`: An int provider as the loot / worldgen data writes it.
- `public static ArgType Dialog()`: A dialog: an id of the `dialog` registry or an inline definition.
- `public static ArgType Dimension()`: A dimension key.
- `public static ArgType Double(double min = -1.7976931348623157E+308, double max = 1.7976931348623157E+308)`: A double-precision number, optionally limited.
- `public static ArgType Entities()`: Any number of entities.
- `public static ArgType Entity()`: Exactly one entity.
- `public static ArgType EntityAnchor()`: An entity anchor: `feet` or `eyes`.
- `public static ArgType Feature()`: A configured worldgen feature (an id of the `worldgen/configured_feature` registry).
- `public static ArgType Float(float min = -3.4028235E+38f, float max = 3.4028235E+38f)`: A single-precision number, optionally limited.
- `public static ArgType FloatRange()`: An inclusive decimal range.
- `public static ArgType Function()`: A data pack function id or tag.
- `public static ArgType GameMode()`: A game mode name.
- `public static ArgType GameProfile()`: A player profile (also offline players: names and uuids).
- `public static ArgType GreedyString()`: The rest of the line.
- `public static ArgType Heightmap()`: A heightmap name (`world_surface`, `motion_blocking`, ...).
- `public static ArgType HexColor()`: A colour as `#RRGGBB`.
- `public static ArgType Int(int min = -2147483648, int max = 2147483647)`: A 32-bit integer, optionally limited.
- `public static ArgType IntRange()`: An inclusive integer range (`1..5`, `..3`).
- `public static ArgType Item()`: An item with components, e.g. `minecraft:diamond_sword[damage=5]`.
- `public static ArgType ItemPredicate()`: An item predicate.
- `public static ArgType ItemSlot()`: One inventory slot name (`container.5`, `weapon.mainhand`).
- `public static ArgType ItemSlots()`: A group of inventory slots (`container.*`).
- `public static ArgType Long(long min = -9223372036854775808, long max = 9223372036854775807)`: A 64-bit integer, optionally limited.
- `public static ArgType LootModifier()`: A loot modifier.
- `public static ArgType LootPredicate()`: A loot predicate.
- `public static ArgType LootTable()`: A loot table.
- `public static ArgType Message()`: A chat message that may contain selectors.
- `public static ArgType NbtCompound()`: An SNBT compound.
- `public static ArgType NbtPath()`: An NBT path.
- `public static ArgType NbtTag()`: Any SNBT tag.
- `public static ArgType Objective()`: A scoreboard objective name.
- `public static ArgType ObjectiveCriteria()`: A scoreboard criterion (`dummy`, `health`, `minecraft.mined:minecraft.stone`).
- `public static ArgType Operation()`: A scoreboard operation (`+=`, `%=`, `><`).
- `public static ArgType Particle()`: A particle with its parameters (`minecraft:dust{color:[1,0,0],scale:1}`).
- `public static ArgType Player()`: One online player (name, uuid or a selector that matches exactly one player).
- `public static ArgType Players()`: Any number of online players (names or selectors such as `@a`).
- `public static ArgType Resource(string registry)`: An entry of a registry ("minecraft:enchantment", "minecraft:sound_event", ...), suggested by the engine.
- `public static ArgType ResourceKey(string registry)`: A registry key (not the entry), e.g. `Arg.ResourceKey("minecraft:worldgen/biome")`.
- `public static ArgType ResourceLocation()`: A resource location, `namespace:path`.
- `public static ArgType ResourceOrTag(string registry)`: An entry of a registry or a `#tag` of it.
- `public static ArgType ResourceOrTagKey(string registry)`: A registry key or a `#tag` of keys.
- `public static ArgType ResourceSelector(string registry)`: A selector over the entries of a registry (`*`, `minecraft:*`, tags).
- `public static ArgType Rotation()`: A rotation (yaw pitch).
- `public static ArgType ScoreHolder(bool multiple = false)`: A score holder (names, selectors, `*`).
- `public static ArgType ScoreboardSlot()`: A scoreboard display slot (`sidebar`, `list`, `below_name`, `sidebar.team.red`).
- `public static ArgType SlotSource()`: A slot source of the item modifier data (`minecraft:slot_source`).
- `public static ArgType String()`: A word or a quoted phrase.
- `public static ArgType Style()`: A text style in JSON/SNBT form.
- `public static ArgType SwingAnimation()`: A swing animation type (`none`, `stab`, `whack`).
- `public static ArgType Swizzle()`: A set of axes (`xz`, `xyz`).
- `public static ArgType Team()`: A team name.
- `public static ArgType TemplateMirror()`: A structure mirror (`none`, `left_right`, `front_back`).
- `public static ArgType TemplateRotation()`: A structure rotation (`none`, `clockwise_90`, `180`, `counterclockwise_90`).
- `public static ArgType Time(int minTicks = 0)`: A duration such as `10s`, `5d`, `20t` (parsed to ticks).
- `public static ArgType Uuid()`: A uuid (read with `CommandContext.GetText`).
- `public static ArgType Vec2()`: A 2D position.
- `public static ArgType Vec3()`: A position with decimals.
- `public static ArgType Word()`: A single word (letters, digits, `_ - . +`).

## ArgType (class, commands)

```csharp
public sealed class ArgType
```

The type of a command argument: what the client's command tree shows (parser id + properties) and what the server parses. Create with the `Arg` factories.

- `public string RegistryName { get; }`: The vanilla argument type id, e.g. "brigadier:integer" or "minecraft:entity".
- `public override string ToString()`

## AsyncSuggestionProvider (delegate, commands)

```csharp
public delegate Task<IReadOnlyList<string>> AsyncSuggestionProvider(SuggestionContext context, string typed)
```

An asynchronous suggestion provider (the list is produced off the tick thread, e.g. from a database).

## Command (class, commands)

```csharp
public static class Command
```

Entry point for building command trees: `Command.Literal("home").Then(Command.Argument("name", Arg.Word()).Executes(...))`.

- `public static CommandNode Argument(string name, ArgType type)`: A typed argument.
- `public static CommandNode Literal(string name)`: A literal word.

## CommandContext (class, commands)

```csharp
public sealed class CommandContext
```

The parsed invocation of a command: the source and the argument values. Valid only while the executor / provider runs.

- `public string[] Args { get; }`: The words after the command name (flat-command style access: split on spaces).
- `public string Input { get; }`: The whole command line without the leading slash.
- `public string Name { get; }`: The name of the executing command node chain's root literal (the word the player typed).
- `public Player? Player { get; }`: The sending player, null for the console.
- `public string RawArgs { get; }`: The text after the command name.
- `public int Result { get; set; }`: The command result the executor of an `Action`-style handler returns (default 1). Set it to change the number `/execute store` sees.
- `public Entity Sender { get; }`: The executing entity (shortcut for `Source.Entity`; `Entity.Null` for the console).
- `public CommandSource Source { get; }`: Who runs the command.
- `public DetachedCommand Detach()`: Copies what a command needs after its executor returned: who ran it, the input, the permission level and the position. Use it for deferred work (`Scheduler.RunLater`, a task, a callback): the context itself must not be used after the executor returned.
- `public BlockPos GetBlockPos(string name)`: The block position of a `Arg.BlockPos` argument (relative coordinates resolved against the source).
- `public BlockState GetBlockState(string name)`: The block state of a `Arg.BlockState` argument.
- `public bool GetBool(string name)`: The value of a boolean argument.
- `public double GetDouble(string name)`: The value of a double argument.
- `public IReadOnlyList<Entity> GetEntities(string name)`: The entities a selector argument matches (an empty list when none).
- `public float GetFloat(string name)`: The value of a float argument.
- `public int GetInt(string name)`: The value of an integer argument.
- `public long GetLong(string name)`: The value of a long argument.
- `public Player GetPlayer(string name)`: The single player a `Arg.Player` argument names. Throws `CommandFailure` when none matched.
- `public IReadOnlyList<Player> GetPlayers(string name)`: The players a selector argument matches.
- `public string GetString(string name)`: The value of a word / phrase / greedy string argument.
- `public string GetText(string name)`: The text of any argument as typed (block states, items, components, nbt, uuids, resource ids, ...).
- `public Vec3d GetVec3(string name)`: The position of a `Arg.Vec3` argument (relative coordinates resolved against the source).
- `public bool Has(string name)`: Whether an optional argument was given.
- `public void Reply(string message)`: Sends a plain message to whoever ran the command.
- `public void Reply(Component message)`: Sends a rich message to whoever ran the command.
- `public Player RequirePlayer()`: The sending player, or throws `CommandFailure` ("Only players can use this command") for the console.

## CommandExecutor (delegate, commands)

```csharp
public delegate int CommandExecutor(CommandContext context)
```

A command executor: runs the command and returns its result (vanilla semantics: the number `/execute store` sees; 1 = success).

## CommandFailure (class, commands)

```csharp
public sealed class CommandFailure : Exception
```

Thrown by an executor to fail the command with a message (red text, no stack trace in the log). The command result is 0.

- `public CommandFailure(string message)`: Creates a failure with a plain message.
- `public CommandFailure(string translationKey, string message)`: Creates a failure with a translation key (client-translated) and an English fallback message.
- `public string? TranslationKey { get; }`: The translation key, if any.

## CommandHandler (delegate, commands)

```csharp
public delegate void CommandHandler(CommandContext context)
```

A flat command body (`Commands.Register("name", ctx => ...)`): reads `CommandContext.Args` and may set `CommandContext.Result`.

## CommandNode (class, commands)

```csharp
public sealed class CommandNode
```

A node of a command tree under construction: a literal word or a typed argument, with children, an executor, requirements and suggestions. Build trees with `Command.Literal` / `Command.Argument` and register them with `CommandRegistry.Register`.

- `public bool IsLiteral { get; }`: Whether this is a literal word.
- `public string Name { get; }`: The literal word or the argument name.
- `public ArgType? Type { get; }`: The argument type, null for a literal.
- `public CommandNode Executes(CommandExecutor executor)`: Runs `executor` when the command line ends at this node.
- `public CommandNode Executes(Action<CommandContext> action)`: Runs `action` when the command line ends at this node; the result is `CommandContext.Result` (default 1).
- `public CommandNode OpLevel(int level)`: Requires a vanilla operator level (0-4) for this node.
- `public CommandNode Permission(string node)`: Requires a permission node (see `Mc.Scripting.Permissions`) to see and use this node. Players without it do not get the node in their client's tree. The console always passes.
- `public CommandNode Requires(Func<CommandSource, bool> requirement)`: Requires an arbitrary condition of the command source. A node with a condition is encoded per player in the client's command tree.
- `public CommandNode Suggests(SuggestionProvider provider)`: Provides tab-completion suggestions for this argument (server-side, on the tick thread).
- `public CommandNode Suggests(Func<CommandContext, IEnumerable<string>> provider)`: Provides suggestions from a list computed on the tick thread.
- `public CommandNode SuggestsAsync(AsyncSuggestionProvider provider)`: Provides suggestions asynchronously (the client waits until the task completes).
- `public CommandNode Then(CommandNode child)`: Adds a child node.

## CommandRegistry (class, commands)

```csharp
public sealed class CommandRegistry
```

Command registration, scoped to a plugin (`plugin.Commands`). Plugin commands are nodes of the server's real command tree: they appear in the client's command tree with argument types and suggestions, and run through the same dispatcher as vanilla commands. Everything registered here is removed again when the plugin unloads (the clients' trees are refreshed once per tick).

Two forms. The tree form `Register(root, aliases, description)` takes a literal built with `Command.Literal` and `Command.Argument`: typed arguments, suggestions, permissions and aliases, shown in the client's command tree. The flat form `Register(name, handler, description)` is a shortcut for a `name` literal followed by a greedy text argument; the handler reads `CommandContext.Args`. Plugin commands are removed when the plugin unloads. See [Commands](/concepts/commands/).

- `public RegisteredCommand Register(CommandNode root, string[]? aliases = null, string description = "")`: Registers a command tree.
- `public RegisteredCommand Register(string name, CommandHandler handler, string description = "")`: Registers a flat command: the word `name` followed by anything. The handler reads `CommandContext.Args`. Prefer `CommandRegistry.Register` for typed arguments, suggestions and permissions.

```csharp
Commands.Register("hello", ctx =>
{
    ctx.Reply($"Hello, {(ctx.Args.Length > 0 ? ctx.Args[0] : "world")}!");
    ctx.Result = ctx.Args.Length;
}, "Says hello");
```

```csharp
Commands.Register(Command.Literal("greet").Permission("hello.greet")
    .Then(Command.Argument("name", Arg.Word())
        .Suggests(ctx => new[] { "world", "steve" })
        .Executes(ctx =>
        {
            ctx.Reply($"Hello, {ctx.GetString("name")}!");
            return 1;
        })),
    description: "Greets someone");
```

## CommandSource (class, commands)

```csharp
public sealed class CommandSource
```

Who runs a command: a player or the console, with a permission level, a position and a dimension.

- `public Entity Entity { get; }`: The executing entity (`Entity.Null` for the console and command blocks).
- `public bool IsConsole { get; }`: Whether the command runs from the server console.
- `public int PermissionLevel { get; }`: The vanilla permission level of the source (0-4).
- `public Player? Player { get; }`: The executing player, null when the console or another entity runs the command.
- `public Vec3d Position { get; }`: The position the command runs at (the entity's position for players; the origin for the console).
- `public World World { get; }`: The dimension the command runs in.
- `public void Failure(string message)`: Sends a red failure message to the source (the command is not aborted).
- `public void Success(string message, bool broadcastToOps = false)`: Sends feedback to the source (and, like vanilla, to operators when `broadcastToOps` is set).

## DetachedCommand (class, commands)

```csharp
public sealed class DetachedCommand
```

What a command needed, copied out of a `CommandContext` so that it can be used after the executor returned (`CommandContext.Detach`).

- `public string Input { get; }`: The whole command line without the leading slash.
- `public string Name { get; }`: The command word.
- `public int PermissionLevel { get; }`: The vanilla permission level of the source when the command ran.
- `public Player? Player { get; }`: The executing player, null for the console.
- `public Vec3d Position { get; }`: The position the command ran at.
- `public Entity Sender { get; }`: The executing entity (`Entity.Null` for the console).
- `public void Reply(string message)`: Sends a plain message to whoever ran the command (nothing happens when the player left; call on the tick thread).
- `public void Reply(Component message)`: Sends a rich message to whoever ran the command.

## RegisteredCommand (class, commands)

```csharp
public sealed class RegisteredCommand : IDisposable, IPluginResource
```

A command registered through `CommandRegistry`. Dispose to unregister (plugin unload does it automatically).

- `public IReadOnlyList<string> Names { get; }`: The names the command answers to (the main name first, then the aliases).
- `public void Dispose()`: Unregisters the command. Calling it again does nothing.

## SuggestionContext (class, commands)

```csharp
public sealed class SuggestionContext
```

What an asynchronous suggestion provider gets: a copy of the request (the native command context does not outlive the call that started the provider).

- `public string Input { get; }`: The command line typed so far (without the leading slash).
- `public int PermissionLevel { get; }`: The permission level of the asker.
- `public Player? Player { get; }`: The player asking, null for the console.
- `public Entity Sender { get; }`: The player (or entity) asking; `Entity.Null` for the console.
- `public string Typed { get; }`: The text typed so far for the argument being completed.

## SuggestionProvider (delegate, commands)

```csharp
public delegate void SuggestionProvider(CommandContext context, SuggestionsBuilder builder)
```

A suggestion provider: fills the builder with completions for the argument being typed.

## SuggestionsBuilder (class, commands)

```csharp
public sealed class SuggestionsBuilder
```

Collects the suggestions of one argument.

- `public string Remaining { get; }`: The text typed so far for the argument.
- `public void Suggest(int value)`: Offers an integer completion.
- `public void Suggest(string text, string? tooltip = null)`: Offers a completion. Like vanilla's `matchesSubStr`, completions that do not match `SuggestionsBuilder.Remaining` are dropped: the typed text must be a case-insensitive prefix of the completion or of a part of it after a `.`, `_` or `/`.

## Component<T> (class, ecs)

```csharp
public static class Component<T> where T : unmanaged
```

Per-type component id cache. Set by `Ecs.RegisterComponent`.

The cache is one slot per CLR type for the whole process, so a component type must be owned by a single plugin (declare your own struct; do not register a shared type such as `Vec3d` under different keys from different plugins). Registering a type that is already bound to another key throws. The binding is cleared when the registering plugin unloads. You rarely use this type directly: `Ecs.RegisterComponent<T>(key)` fills it, and `Entity.Get<T>()` and friends read the cached id from it.

- `public static ulong Id { get; }`: ECS component id, 0 if not registered.
- `public static string? Key { get; }`: The key the component type is registered under, null if unregistered.

## Config<T> (class, config)

```csharp
public sealed class Config<T> : IDisposable, IPluginResource where T : class, new()
```

A typed, JSON-backed configuration file of a plugin (`plugins/<id>/config.json` by default). Defaults (the values of a freshly constructed `T`, including property initialisers) are written on first run; the file is read with comments and trailing commas allowed. Describe `T` with System.Text.Json source generation so loading needs no reflection.

- `public string Path { get; }`: The path of the file.
- `public T Value { get; }`: The current values: an immutable snapshot replaced as a whole on `Config.Reload` (safe to read from any thread).
- `public bool Watch { get; set; }`: Whether the file is watched: edits on disk are picked up (debounced) and reported through `Config.Changed`.
- `public void Dispose()`
- `public bool Reload()`: Re-reads the file. A parse error keeps the previous value, logs the file and line, and never overwrites the file.
- `public void Save()`: Writes the current value to the file (pretty-printed). Use it after changing a copy of `Config.Value` with `Config.Update`.
- `public void Update(T value)`: Replaces the value (and saves it).
- `public event Action<T>? Changed;`: Raised after a reload that produced a new value (also when the file changed on disk with `Config.Watch` on), on the tick thread.

```csharp
public sealed class HomesConfig { public int MaxHomes { get; set; } = 3; public int CooldownSeconds { get; set; } = 5; }
[JsonSerializable(typeof(HomesConfig))] partial class Json : JsonSerializerContext;
var config = Config.Load(Json.Default.HomesConfig);   // config.Value.MaxHomes

```

## PluginConfig (class, config)

```csharp
public sealed class PluginConfig
```

Plugin-scoped access to configuration files (`plugin.Config`) and the data folder.

- `public string DataFolder { get; }`: The plugin's data folder (`plugins/<id>/`), created on first access. Put every file the plugin writes here.
- `public Config<T> Load<T>(JsonTypeInfo<T> typeInfo, string fileName = "config.json") where T : class, new()`: Loads (and creates, with defaults, when missing) a JSON config file in the data folder.
- `public string? ReadText(string fileName)`: Reads a text file of the data folder, null when it does not exist.
- `public void WriteText(string fileName, string text)`: Writes a text file of the data folder atomically (temp file + rename).

## BlockBreakContext (class, content)

```csharp
public sealed class BlockBreakContext
```

What a custom block's break handler gets. Valid during the handler only.

- `public CustomBlock Block { get; }`: The block definition.
- `public bool Cancelled { get; set; }`: Keeps the block (nothing breaks).
- `public bool DropItems { get; set; }`: Whether the drops of the block are spawned (default true; nothing drops in creative mode anyway).
- `public Player Player { get; }`: The player breaking it.
- `public BlockPos Position { get; }`: The position.
- `public ItemStack Tool { get; }`: The tool in the player's hand.
- `public World World { get; }`: The dimension.

## BlockInteractContext (class, content)

```csharp
public sealed class BlockInteractContext
```

What a custom block's interact handler gets. Valid during the handler only.

- `public CustomBlock Block { get; }`: The block definition.
- `public bool Cancelled { get; set; }`: Stops the item use after the block (placing a block against it, the held item's own use).
- `public BlockFace Face { get; }`: The clicked face.
- `public Hand Hand { get; }`: The hand.
- `public ItemStack Item { get; }`: The stack in that hand.
- `public Player Player { get; }`: The player.
- `public BlockPos Position { get; }`: The position of the block.
- `public World World { get; }`: The dimension.

## BlockPlaceContext (class, content)

```csharp
public sealed class BlockPlaceContext
```

What a custom block's place handler gets. Valid during the handler only.

- `public BlockPos Against { get; }`: The block that was clicked.
- `public CustomBlock Block { get; }`: The block definition.
- `public bool Cancelled { get; set; }`: Refuses the placement.
- `public Hand Hand { get; }`: The hand.
- `public Player Player { get; }`: The player.
- `public BlockPos Position { get; }`: Where the block is placed.
- `public World World { get; }`: The dimension.

## BlockRegistry (class, content)

```csharp
public sealed class BlockRegistry
```

Registers custom blocks.

- `public IReadOnlyList<CustomBlock> All { get; }`: Every registered block (a snapshot).
- `public CustomBlock? At(World world, BlockPos pos)`: The custom block a world position holds.
- `public CustomBlock? Get(string id)`: The registered block with this id.
- `public CustomBlock? Of(BlockState state)`: The custom block a block state stands for.
- `public void Register(CustomBlock block)`: Registers a block for the plugin whose code is running: picks its block state (the same one after a restart), marks it custom in the engine and starts listening for its breaking and clicking. Released when the plugin unloads.

## Content (class, content)

```csharp
public static class Content
```

Entry point of the custom content API: `Content.Items` and `Content.Blocks` (and `ResourcePackBuilder`, `ResourcePackHost`).

- `public static BlockRegistry Blocks { get; }`: Custom blocks.
- `public static ItemRegistry Items { get; }`: Custom items.

## CustomBlock (class, content)

```csharp
public sealed class CustomBlock
```

A custom block: a state of a vanilla block that vanilla play seldom produces (and never turns a block into: the engine guards it), with its own hardness, drops and behaviour. The world stores a plain vanilla block state (so a server without the plugin shows a note block, never a corrupt chunk); a resource pack gives that state its own model (`ResourcePackBuilder.AddCustomBlock`).

**Which state.** Custom blocks live in the note block's states with a mob head instrument (the ones nobody gets without a head above the block), 175 of them, handed out in registration order and remembered in the file named by `content.state_file` in Ramjet.toml, so the same id gets the same state after a restart. `CustomBlock.BaseBlock` names the family: only `minecraft:note_block` exists in this version. While a block is registered the vanilla note block behaviour (tuning, notes, the instrument following the block below) is suppressed for its state. **Mining.** The server decides how long a block takes to break, from `CustomBlock.HardnessValue`; the client predicts it from the note block (hardness 0.8), so a block that takes much longer than that appears to break on the client and comes back (the server refuses to finish it early): keep custom hardness near 0.8 or lower for smooth mining.

- `public CustomBlock(string id, string baseBlock = "minecraft:note_block")`: Creates a definition.
- `public string BaseBlock { get; }`: The vanilla block whose states host it.
- `public float HardnessValue { get; }`: How long it takes to mine (vanilla hardness; negative = unbreakable).
- `public string Id { get; }`: The id.
- `public string? ModelId { get; }`: The model id of the block in the resource pack (`assets/myplugin/models/block/ruby_ore.json` is `"myplugin:block/ruby_ore"`), or null.
- `public BlockState State { get; }`: The block state that stands for the block in the world (valid after registration).
- `public CustomBlock Drops(Func<BlockBreakContext, IEnumerable<ItemStack>> drops)`: Sets what the block drops when a player breaks it (nothing drops in creative mode, and nothing by default).
- `public CustomBlock DropsItem(CustomItem item, int count = 1)`: The block drops this item (a registered custom item or any stack) when broken.
- `public CustomBlock Hardness(float hardness)`: Sets the hardness.
- `public CustomBlock Model(string modelId)`: Sets the model the resource pack gives the block (used by `ResourcePackBuilder.AddCustomBlock`).
- `public CustomBlock OnBreak(Action<BlockBreakContext> handler)`: Sets what happens when a player breaks the block (before the drops; cancel the context to keep the block).
- `public CustomBlock OnInteract(Action<BlockInteractContext> handler)`: Sets what happens when a player right-clicks the block (sneaking with an item in hand skips it, like vanilla blocks).
- `public CustomBlock OnPlace(Action<BlockPlaceContext> handler)`: Sets what happens when a player places the block with a custom item (`CustomItem.Places`); cancel the context to refuse.
- `public bool Place(World world, BlockPos pos)`: Puts the block into the world (registered blocks only).

## CustomItem (class, content)

```csharp
public sealed class CustomItem
```

A custom item: a vanilla item the client already knows (the base item decides the id the client sees and the behaviour class) carrying data components that make it a different item: `custom_data` (the id, found again with `ItemStack.CustomId`), `item_model` (the look, from a resource pack), a name, lore, a stack size and a rarity. No client mod is needed. Register it with `ItemRegistry.Register`, hand out stacks with `CustomItem.CreateStack`.

Choose an inert base item (`minecraft:paper`, `minecraft:stick`, `minecraft:amethyst_shard`): the vanilla behaviour of the base item still happens unless a use handler cancels it. Stacks keep their id when the plugin is unloaded; they behave like the base item until it is back.

- `public CustomItem(string id, string baseItem)`: Creates a definition.
- `public string BaseItem { get; }`: The vanilla base item.
- `public int CooldownTicks { get; }`: Ticks a player waits between two uses (0 for none).
- `public Component? DisplayName { get; }`: The display name.
- `public bool? HasGlint { get; }`: The enchantment glint (null: the item's own).
- `public string Id { get; }`: The id.
- `public IReadOnlyList<Component> LoreLines { get; }`: The lore lines.
- `public int MaxStackSize { get; }`: The maximum stack size, 0 for the base item's own.
- `public string? ModelId { get; }`: The item model id (an item definition of the resource pack), or null for the base item's own.
- `public string? PlacesBlock { get; }`: The id of the custom block the item places when used on a block, or null.
- `public ItemRarity? Rarity { get; }`: The rarity, null for the base item's own.
- `public Action<ItemUseContext>? UseHandler { get; }`: The handler called when a player uses the item in the air or on a block.
- `public Action<ItemUseContext>? UseOnHandler { get; }`: The handler called when a player uses the item on a block.
- `public CustomItem Cooldown(int ticks)`: Makes a player wait between two uses: further uses are ignored (cancelled) during the cooldown.
- `public ItemStack CreateStack(int count = 1)`: A stack of the item with every component of the definition.
- `public CustomItem Glint(bool glint = true)`: Forces the enchantment glint on or off.
- `public CustomItem Lore(params Component[] lines)`: Sets the lore lines.
- `public bool Matches(ItemStack stack)`: Whether a stack is an instance of this item (its `ItemStack.CustomId` is the id).
- `public CustomItem MaxStack(int max)`: Sets the maximum stack size.
- `public CustomItem Model(string modelId)`: Sets the item model (`minecraft:item_model`).
- `public CustomItem Name(Component name)`: Sets the display name (`minecraft:item_name`: not italic, replaced by an anvil rename).
- `public CustomItem OnUse(Action<ItemUseContext> handler)`: Sets what happens when a player uses the item (right click in the air or on a block). Cancel the context to stop the vanilla behaviour of the base item.
- `public CustomItem OnUseOn(Action<ItemUseContext> handler)`: Sets what happens when a player uses the item on a block (after {{cref:M:Mc.Scripting.Content.CustomItem.OnUse(System.Action{Mc.Scripting.Content.ItemUseContext})}}).
- `public CustomItem Places(CustomBlock block)`: Makes the item place a custom block against the clicked face (and use up one item).
- `public CustomItem WithRarity(ItemRarity rarity)`: Sets the rarity.

## ItemRarity (enum, content)

```csharp
public enum ItemRarity : int
```

How rare an item is (the colour of its name).

- `Common` = 0: White.
- `Uncommon` = 1: Yellow.
- `Rare` = 2: Aqua.
- `Epic` = 3: Light purple.

## ItemRegistry (class, content)

```csharp
public sealed class ItemRegistry
```

Registers custom items.

- `public IReadOnlyList<CustomItem> All { get; }`: Every registered item (a snapshot).
- `public ItemStack Create(string id, int count = 1)`: A stack of a registered item.
- `public CustomItem? Get(string id)`: The registered item with this id.
- `public CustomItem? Of(ItemStack stack)`: The definition of a stack that is a registered custom item.
- `public void Register(CustomItem item)`: Registers an item for the plugin whose code is running (released when the plugin unloads).

## ItemUseContext (class, content)

```csharp
public sealed class ItemUseContext
```

What a custom item's use handler gets. Valid during the handler only.

- `public bool Cancelled { get; set; }`: Stops the vanilla behaviour of the base item and of the clicked block.
- `public CustomItem Definition { get; }`: The definition of the item.
- `public BlockFace Face { get; }`: The clicked face (only when `ItemUseContext.HasBlock`).
- `public Hand Hand { get; }`: The hand holding the item.
- `public bool HasBlock { get; }`: Whether the click was on a block.
- `public ItemStack Item { get; }`: The stack that was used.
- `public Player Player { get; }`: The player.
- `public BlockPos Position { get; }`: The clicked block (only when `ItemUseContext.HasBlock`).
- `public World World { get; }`: The dimension the player is in.
- `public void Consume(int count = 1)`: Uses up items of the stack that was used (nothing happens in creative mode).

## PackImage (class, content)

```csharp
public static class PackImage
```

Small generated textures for packs and tests: 8-bit RGBA PNG files made without any imaging library.

- `public static byte[] Checker(uint a, uint b, int size = 16, int cell = 4)`: A PNG of a two colour checker board.
- `public static byte[] Create(int width, int height, Func<int, int, uint> pixel)`: A PNG from a pixel function.
- `public static byte[] Solid(uint argb, int size = 16)`: A PNG of one colour.

## ResourcePackBuilder (class, content)

```csharp
public sealed class ResourcePackBuilder
```

Builds the resource pack zip that goes with custom content: item definitions, models, textures, language files and the block state remap of custom blocks. The output is deterministic (sorted entries, fixed timestamps), so the same input gives the same SHA-1 and clients keep their cached copy until something changes.

- `public string Name { get; }`: The pack name.
- `public ResourcePackBuilder AddCustomBlock(CustomBlock block, byte[] png)`: Gives the block state of a registered custom block a cube model with one texture. Writes the block state file of the note block with every variant: the custom ones point at the new model, all others keep the vanilla model.
- `public ResourcePackBuilder AddCustomBlockModel(CustomBlock block, string modelId)`: Points the block state of a registered custom block at a model of the pack (written by `ResourcePackBuilder.AddModel` or `ResourcePackBuilder.AddFile`).
- `public ResourcePackBuilder AddFile(string path, byte[] content)`: Adds (or replaces) a file of the pack.
- `public ResourcePackBuilder AddFile(string path, string text)`: Adds (or replaces) a text file of the pack.
- `public ResourcePackBuilder AddItem(string itemModelId, byte[] png)`: Adds a flat item: the item definition, a `item/generated` model and the texture, all named after the item model id.
- `public ResourcePackBuilder AddItem(CustomItem item, byte[] png)`: Adds a flat item for a custom item (its `CustomItem.ModelId` names the model; set it with `CustomItem.Model(id)`).
- `public ResourcePackBuilder AddItemDefinition(string itemModelId, string json)`: Adds an item definition (`assets/<namespace>/items/<path>.json`): what a stack with this `item_model` looks like.
- `public ResourcePackBuilder AddLang(string locale, IReadOnlyDictionary<string, string> entries, string? namespaceId = null)`: Adds a language file (`assets/<namespace>/lang/<locale>.json`).
- `public ResourcePackBuilder AddModel(string modelId, string json)`: Adds a model (`assets/<namespace>/models/<path>.json`).
- `public ResourcePackBuilder AddTexture(string textureId, byte[] png)`: Adds a texture (`assets/<namespace>/textures/<path>.png`).
- `public ResourcePackFile Build()`: Builds the zip and its SHA-1.
- `public ResourcePackBuilder Description(string description)`: Sets the description shown in the pack list.
- `public ResourcePackBuilder PackFormat(int format)`: Sets the resource pack format number written to `pack.mcmeta` (97 for 26.3).
- `public static ResourcePackBuilder Create(string name)`: Starts a pack.

```csharp
var pack = ResourcePackBuilder.Create("myplugin")
    .Description("My plugin")
    .AddItem(ruby, PackImage.Solid(0xFFE0115F))
    .AddCustomBlock(rubyOre, PackImage.Checker(0xFF8B0A3C, 0xFFE0115F))
    .AddLang("en_us", new Dictionary<string, string> { ["item.myplugin.ruby"] = "Ruby" })
    .Build();
pack.SendTo(player);   // hosted by ResourcePackHost (content.pack_host_port), or pack.ToResourcePack(yourUrl)

```

## ResourcePackFile (class, content)

```csharp
public sealed class ResourcePackFile
```

A built resource pack: the zip, its SHA-1 and the id clients know it by.

- `public byte[] Bytes { get; }`: The zip.
- `public Uuid Id { get; }`: The pack id (derived from the name: a pack of the same name replaces the older one on the client).
- `public string Name { get; }`: The pack name.
- `public string Sha1 { get; }`: The SHA-1 of the zip (40 lower-case hex characters).
- `public int Size { get; }`: The size of the zip in bytes.
- `public ResourcePack Publish(bool required = false, Component? prompt = null)`: Hosts the zip with `ResourcePackHost` and returns the description a client needs.
- `public void SendTo(Player player, bool required = false, Component? prompt = null)`: Hosts the pack (see `ResourcePackFile.Publish`) and asks a player to download it.
- `public ResourcePack ToResourcePack(string url, bool required = false, Component? prompt = null)`: The pack as the packet a client needs, for a URL you host yourself.
- `public void WriteTo(string path)`: Writes the zip to a file.

## ResourcePackHost (class, content)

```csharp
public static class ResourcePackHost
```

The server's built-in web server for resource packs: it answers `GET` / `HEAD` of `/<sha1>.zip` for the packs that were published, and keeps nothing of the plugins that published them (a pack is removed when its plugin unloads). It listens on 127.0.0.1 unless told otherwise.

Plugins run in a sandboxed worker process that denies sockets, so the server cannot be started from a plugin: `ResourcePackHost.Start` throws `NotSupportedException`, and so does `ResourcePackHost.Publish` when `content.pack_host_port` is set (the server is started from that setting only). Host the zip on a web server you run and use `ResourcePackFile.ToResourcePack` with its URL instead.

- `public static string? BaseUrl { get; }`: The base URL the clients use (`http://host:port`), or null while the server is off.
- `public static bool IsRunning { get; }`: Whether the server is listening.
- `public static int Port { get; }`: The port the server listens on, 0 while it is off.
- `public static string Publish(ResourcePackFile pack)`: Publishes a pack and returns the URL it is downloadable from. The pack stays available until the plugin that published it unloads (or `ResourcePackHost.Unpublish`).
- `public static string Start(string bind = "127.0.0.1", int port = 0, string? publicUrl = null)`: Starts the server (does nothing when it already runs). Not available to plugins: the worker's sandbox denies sockets.
- `public static void Stop()`: Takes back the packs the calling plugin published, and stops the server when it was started by code (`ResourcePackHost.Start`) and no published pack of any plugin is left. Packs of other plugins stay available; a server started by the configuration keeps running.
- `public static void Unpublish(ResourcePackFile pack)`: Stops handing out a pack the calling plugin published (it stays available while another plugin publishes the same pack).

## DamageCause (enum, events)

```csharp
public enum DamageCause : byte
```

Why an entity took damage (derived from the vanilla damage type).

- `Other` = 0: Any damage type without a dedicated cause.
- `Contact` = 1: Cactus, stalagmite, sweet berry bush.
- `EntityAttack` = 2: Melee attack by an entity.
- `Projectile` = 3: Arrow, trident, fireball, thrown item, spit, wind charge ...
- `Suffocation` = 4: Inside a wall.
- `Fall` = 5: Fall damage.
- `Fire` = 6: Standing in fire.
- `FireTick` = 7: Burning.
- `Lava` = 8: Lava.
- `Drowning` = 9: Out of air.
- `Explosion` = 10: An explosion.
- `Void` = 11: Below the world.
- `Lightning` = 12: Lightning bolt.
- `Starvation` = 13: Starvation.
- `Magic` = 14: Magic, poison, instant damage.
- `Wither` = 15: The wither effect or a wither.
- `FallingBlock` = 16: A falling block or anvil.
- `Thorns` = 17: Thorns enchantment.
- `FlyIntoWall` = 18: Elytra kinetic damage.
- `HotFloor` = 19: Magma block.
- `Cramming` = 20: Entity cramming.
- `Dryout` = 21: Water animals out of water.
- `Freeze` = 22: Powder snow.
- `Kill` = 23: /kill or the generic kill damage.
- `Border` = 24: Outside the world border.
- `Campfire` = 25: Campfire.
- `MaceSmash` = 26: Mace smash attack.
- `Custom` = 27: Damage dealt by a plugin.

## DisplaySlot (enum, display)

```csharp
public enum DisplaySlot : int
```

Where an objective is displayed.

- `List` = 0: The player list (Tab), next to each name.
- `Sidebar` = 1: The sidebar at the right of the screen.
- `BelowName` = 2: Below the name tags of players.

## DragType (enum, inventories)

```csharp
public enum DragType : byte
```

Kind of inventory drag.

- `Even` = 0: Left drag: split the cursor stack evenly.
- `Single` = 1: Right drag: one item per slot.
- `Clone` = 2: Middle drag (creative).

## Ecs (class, ecs)

```csharp
public sealed class Ecs
```

ECS access scoped to a plugin (`plugin.Ecs`): custom components, queries and raw entities. Entities are not owned (they survive plugin reloads); queries are, and are destroyed on unload. Game worlds (dimensions) are `World`.

Game worlds (dimensions) are a different type, `Mc.Scripting.Worlds.World`, reached through `Server.Worlds` and `Server.Overworld`.

- `public Entity GetEntity(ulong id)`: Wraps an entity id.
- `public Query<T1> Query<T1>() where T1 : unmanaged`: Creates a cached query over one component type.
- `public Query<T1, T2> Query<T1, T2>() where T1 : unmanaged where T2 : unmanaged`: Creates a cached query over two component types.
- `public Query<T1, T2, T3> Query<T1, T2, T3>() where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged`: Creates a cached query over three component types.
- `public ulong RegisterComponent<T>(string key) where T : unmanaged`: Registers (or re-binds) a plain-data component backed by a flecs component. Keys are global ("myplugin:health"); registering the same key with the same layout (field types and offsets) after a hot reload keeps existing component values. A changed layout drops the values, but is refused while another plugin is also bound to the key. Engine components can be bound by their registered name as long as the layout matches.
- `public Entity Spawn()`: Creates an empty ECS entity (not a game entity: it has no position and is never sent to clients).

```csharp
public struct Mana { public int Value; }

private Query<Mana> _mana = null!;

protected override void OnEnable()
{
    Ecs.RegisterComponent<Mana>("hello:mana");
    _mana = Ecs.Query<Mana>();
    Systems.Add("regen", Phase.Tick, _ =>
    {
        foreach (var chunk in _mana)
            foreach (ref var m in chunk.Field<Mana>(0))
                m.Value++;
    });
}
```

## Edit (class, other)

```csharp
public static class Edit
```

Bulk region edits run by the engine in slices of the tick budget (for WorldEdit-style plugins). A plugin builds the request (the WEJB binary format of the engine's edit module) and submits it; the engine validates every field, applies the job, and records what it changed so the owner can undo it. Nothing here touches blocks one at a time: a request is one call, whatever its size. Tick thread only. The owner is always a player: the engine keeps history and clipboards per player.

- `public static bool Cancel(Player player, long job)`: Cancels a queued or running job the player owns. Changes already made stay recorded, so undo restores them.
- `public static void ClearHistory(Player player)`: Drops the player's undo and redo records.
- `public static EditClipboardInfo? ClipboardInfo(Player player, ulong handle)`: A clipboard the player holds.
- `public static EditJobInfo? GetJob(Player player, long job)`: The state of a job the player owns.
- `public static EditHistory History(Player player)`: The player's undo and redo stacks.
- `public static EditMemory Memory()`: Engine memory held by bulk edits, for all players (for memory reports).
- `public static long Redo(Player player, int steps = 1)`: Queues a redo of up to `steps` of the player's undone edits.
- `public static bool ReleaseClipboard(Player player, ulong handle)`: Releases a clipboard the player holds. True when it was released.
- `public static long Start(Player player, byte[] request)`: Queues a request for a player. The request is validated in full before anything changes.
- `public static long Undo(Player player, int steps = 1)`: Queues an undo of up to `steps` of the player's most recent edits.

## EditClipboardInfo (struct, other)

```csharp
public readonly struct EditClipboardInfo : IEquatable<EditClipboardInfo>
```

A clipboard the player holds: its size, the copy origin relative to its minimum corner, and its engine memory.

- `public EditClipboardInfo(int SizeX, int SizeY, int SizeZ, int OriginX, int OriginY, int OriginZ, ulong Bytes)`: A clipboard the player holds: its size, the copy origin relative to its minimum corner, and its engine memory.
- `public ulong Bytes { get; init; }`
- `public int OriginX { get; init; }`
- `public int OriginY { get; init; }`
- `public int OriginZ { get; init; }`
- `public int SizeX { get; init; }`
- `public int SizeY { get; init; }`
- `public int SizeZ { get; init; }`

## EditHistory (struct, other)

```csharp
public readonly struct EditHistory : IEquatable<EditHistory>
```

The undo and redo stacks of a player.

- `public EditHistory(uint UndoSteps, uint RedoSteps, ulong UndoBytes, ulong RedoBytes)`: The undo and redo stacks of a player.
- `public ulong RedoBytes { get; init; }`
- `public uint RedoSteps { get; init; }`
- `public ulong UndoBytes { get; init; }`
- `public uint UndoSteps { get; init; }`

## EditJobInfo (struct, other)

```csharp
public readonly struct EditJobInfo : IEquatable<EditJobInfo>
```

The state of one edit job, as the engine reports it.

- `public EditJobInfo(EditJobState State, ulong Changed, uint ElapsedTicks, ulong Clipboard, string Message)`: The state of one edit job, as the engine reports it.
- `public ulong Changed { get; init; }`: Cells whose state changed.
- `public ulong Clipboard { get; init; }`: The clipboard handle of a finished copy or cut (0 otherwise).
- `public uint ElapsedTicks { get; init; }`: Ticks the job has been alive.
- `public string Message { get; init; }`: A short note: empty on success.
- `public EditJobState State { get; init; }`: The job's state.

## EditJobState (enum, other)

```csharp
public enum EditJobState : int
```

What a bulk edit job is at the engine's job system's state.

- `Queued` = 0: Waiting for the owner's running job, or for a slot.
- `Running` = 1: Being applied in slices on the tick thread.
- `Done` = 2: Finished; every cell in the region was handled (some columns may have been skipped, see the message).
- `Failed` = 3: Stopped by a limit or an error; the changes made so far are recorded and can be undone.
- `Cancelled` = 4: Cancelled by its owner (or the owner left); the changes made so far are recorded and can be undone.

## EditMemory (struct, other)

```csharp
public readonly struct EditMemory : IEquatable<EditMemory>
```

Engine memory held by bulk edits, for every player together.

- `public EditMemory(ulong HistoryBytes, ulong ClipboardBytes)`: Engine memory held by bulk edits, for every player together.
- `public ulong ClipboardBytes { get; init; }`
- `public ulong HistoryBytes { get; init; }`

## EffectAction (enum, other)

```csharp
public enum EffectAction : byte
```

What happens to a potion effect (EntityPotionEffect).

- `Added` = 0: The entity did not have the effect; it is added.
- `Changed` = 1: The entity had the effect and it gets a new amplifier or duration.
- `Removed` = 2: The effect is removed: it ran out, the entity died, a command or a plugin removed it.
- `Cleared` = 3: Every effect of the entity is cleared at once (one event for the batch, not one Removed each).

## EffectCause (enum, other)

```csharp
public enum EffectCause : byte
```

What caused a potion effect change (EntityPotionEffect). Unknown when the call site does not say.

- `Unknown` = 0: No cause reported by the call site.
- `Plugin` = 1: A plugin added or removed the effect through the host.
- `Command` = 2: A /effect command.
- `PotionDrink` = 3: The entity drank a potion.
- `PotionSplash` = 4: A splash or lingering potion hit the entity.
- `AreaEffectCloud` = 5: An area effect cloud applies the effect.
- `Food` = 6: A food item with an effect (golden apple, suspicious stew ...).
- `Milk` = 7: The entity drank milk and its effects are cleared.
- `Arrow` = 8: A tipped arrow hit the entity.
- `Attack` = 9: A mob's attack applies the effect (a spider's poison, a wither skeleton's wither).
- `Beacon` = 10: A beacon gives the effect to the players in its range.
- `Conduit` = 11: A conduit gives its power to the players around it.
- `Totem` = 12: A totem of undying saved the entity: its death effects. A cancel is ignored for the removals it makes.
- `Expiration` = 13: The effect ran out. Cancel is ignored.
- `Death` = 14: The entity died and its effects are cleared. Cancel is ignored.
- `Axolotl` = 15: An axolotl gives its regeneration or mining fatigue.
- `Conversion` = 16: A mob converts (zombie villager cure, drowned, zombified piglin ...) and its effects change.
- `Dolphin` = 17: A dolphin gives the dolphin's grace.
- `Illusion` = 18: An illusioner casts blindness or invisibility.
- `SpiderSpawn` = 19: A spider spawns with its effect (a spider jockey).
- `TurtleHelmet` = 20: A turtle helmet gives water breathing.
- `VillagerTrade` = 21: A villager gives regeneration after a trade.
- `Warden` = 22: A warden's sonic boom gives darkness.
- `WitherRose` = 23: A wither rose gives wither when touched.
- `Raid` = 24: A raid omen or the hero of the village (raids). Ramjet extra, not in Paper.
- `TrialSpawner` = 25: A trial spawner gives its trial omen. Ramjet extra, not in Paper.
- `Block` = 26: A block gives the effect (a potent sulfur block). Ramjet extra, not in Paper.
- `Nautilus` = 27: A nautilus gives its rider an effect. Ramjet extra, not in Paper.

## Entity (struct, ecs)

```csharp
public readonly struct Entity : IEquatable<Entity>
```

Handle to an ECS entity (flecs id). Blittable: usable directly in events and components.

Refs returned by `Entity.Get` point into ECS storage and are invalidated by structural changes (adding/removing components, destroying entities). Don't keep them. A handle stays valid as a value forever; whether the entity behind it is alive is a separate question (`IsAlive`). Operations on a dead handle fail (`Get<T>` throws, `Has<T>` is false).

- `public Entity(ulong id)`: Wraps an entity id.
- `public readonly ulong Id;`: The flecs entity id (index and generation); 0 is the null entity.
- `public bool IsAlive { get; }`: Whether the entity exists. A destroyed entity's id goes stale (the generation no longer matches).
- `public bool IsGameEntity { get; }`: Whether this is an entity of the game world (a player, mob, item, orb, projectile ...) as opposed to a bare ECS entity.
- `public bool IsNull { get; }`: Whether this is the null entity.
- `public Location Location { get; }`: The location of a game entity.
- `public string TypeKey { get; }`: The entity type key of a game entity (`"minecraft:zombie"`, `"minecraft:item"`, `"minecraft:player"`), empty for a stale handle or an entity that is not a game entity (one created with `Entity.Spawn`).
- `public static Entity Null { get; }`: The null entity.
- `public LivingEntity? AsLiving()`: The entity as a living entity (a player, a mob, ...), or null when it is not living: a stale handle, a bare ECS entity, an item or a painting.
- `public bool Despawn()`: Removes a game entity from the world without any death logic (no drops, no death event): items, orbs, mobs, projectiles. Players cannot be removed (use `Player.Kick`).
- `public void Destroy()`: Destroys an entity created with `Entity.Spawn`. Does nothing for the null entity or a stale handle. Entities of the game (players, mobs, items, anything an event or a handle hands out) are not the plugin's to destroy: the call is ignored with a warning (use the game APIs, e.g. `Player.Kick`).
- `public bool Equals(Entity other)`: Same entity id.
- `public override bool Equals(object? obj)`: Same entity id.
- `public ref T Get<T>() where T : unmanaged`: Reference to component `T`; throws if the entity lacks it. For a component the game registered itself (bound by its engine name) the reference points at a COPY: engine components are read only for plugins, writing through it changes nothing.
- `public override int GetHashCode()`: Hash of the id.
- `public NbtCompound? GetNbt()`: The data of a game entity as `/data get entity` shows it (position, motion, health, name, item, ...).
- `public ref T GetOrAdd<T>() where T : unmanaged`: Reference to the component, adding it (zero-initialised) when missing.
- `public bool Has<T>() where T : unmanaged`: Whether the entity has component `T`.
- `public bool Kill()`: Kills a living entity the way the `/kill` command does (generic kill damage: death events, drops and the death animation); other entities are removed.
- `public Entity Remove<T>() where T : unmanaged`: Removes a component.
- `public Entity Set<T>(in T value) where T : unmanaged`: Sets (adds or overwrites) a component value.
- `public bool SetGlowingFor(Player viewer, bool glowing)`: Makes this entity (a player or a mob) glow for one viewer: the viewer sees the glowing outline on it, the other players do not. The colour is the vanilla white (no team colour). The glow ends with `glowing = false`, with the entity, or with the plugin.
- `public bool SetNbt(NbtCompound data, bool merge = true)`: Changes the data of a game entity like `/data merge entity`. Players cannot be edited this way (use the player API).
- `public bool Teleport(Location location)`: Moves a game entity. Players go through `Player.Teleport` (with its event); mobs, items and the like move silently, within their dimension.
- `public override string ToString()`: Debug text.
- `public bool TryGet<T>(out T value) where T : unmanaged`: Copies the component if present.
- `public bool TryGetLocation(out Location location)`: Where a game entity is.
- `public ref T TryGetRef<T>() where T : unmanaged`: Pointer-style access: a null ref (check with Unsafe.IsNullRef) when absent.
- `public static Entity Spawn()`: Creates an empty ECS entity (not a game entity).
- `public static bool operator !=(Entity a, Entity b)`: Different entity id.
- `public static bool operator ==(Entity a, Entity b)`: Same entity id.

```csharp
public struct Mana { public int Value; }

// in OnEnable:
Ecs.RegisterComponent<Mana>("hello:mana");

// in an event handler, where e is a PlayerJoinEvent:
Entity player = e.Player;                             // a Player converts to its Entity
player.Set(new Mana { Value = 10 });                  // add or overwrite
if (player.Has<Mana>()) player.Get<Mana>().Value--;   // in-place edit through a ref
if (player.TryGet<Mana>(out var copy)) Log.Info($"mana {copy.Value}");
player.Remove<Mana>();
```

## EntityPose (enum, other)

```csharp
public enum EntityPose : byte
```

The pose of a player (EntityPoseChange, the physics pose).

- `Standing` = 0: Standing.
- `Crouching` = 1: Sneaking.
- `Swimming` = 2: Swimming.
- `FallFlying` = 3: Gliding with an elytra.
- `SpinAttack` = 4: A riptide spin attack.
- `Sleeping` = 5: In a bed.

## EntityRemoveCause (enum, other)

```csharp
public enum EntityRemoveCause : byte
```

Why an entity is removed (EntityRemove).

- `Unknown` = 0: No removal site named a cause (a pickup, a merge or another path without one).
- `Despawn` = 1: Natural despawn: far from every player, or the random despawn check.
- `Death` = 2: The death animation of a mob ended.
- `Killed` = 3: Killed by a command (/kill) or Entity.kill.
- `Plugin` = 4: Removed by a plugin.
- `Unloaded` = 5: Its chunk unloaded: the entity is saved with the chunk.
- `ChangedDimension` = 6: It left through a portal: a copy in the destination dimension replaces it.

## EquipmentSlot (enum, other)

```csharp
public enum EquipmentSlot : byte
```

A slot of a living entity's equipment (the values are the inventory's EquipSlot numbers).

- `MainHand` = 0: The main hand.
- `Feet` = 1: Boots.
- `Legs` = 2: Leggings.
- `Chest` = 3: Chestplate.
- `Head` = 4: Helmet.
- `OffHand` = 5: The off hand.
- `Body` = 6: The body slot (horse and wolf armour).
- `Saddle` = 7: The saddle.

## EventBus (class, events)

```csharp
public static class EventBus
```

Owner-less event firing helpers.

- `public static bool Fire<T>(ref T ev) where T : struct, IEvent`: Fires an event through the native bus; returns false if it was cancelled.
- `public static bool HasSubscribers<T>() where T : struct, IEvent`: Whether anybody (native or managed) listens to `T`.

## EventHeader (struct, events)

```csharp
public struct EventHeader
```

First field of every event struct (McEventHeader).

The first field of every event struct. Handlers use `Cancelled`; the other fields are managed by the runtime.

- `public byte Flags;`: Flag bits (bit 0 = cancellable).
- `public ushort Id;`: Event type id.
- `public uint Size;`: Size of the full event struct.
- `public bool Cancelled { get; set; }`: Whether a handler cancelled the event (ignored if not cancellable).
- `public bool IsCancellable { get; }`: Whether the event type can be cancelled.
- `public static EventHeader For<T>() where T : struct, IEvent`: Creates the header for event type `T`.

## EventListener<T> (delegate, events)

```csharp
public delegate void EventListener<T>(ref T ev) where T : struct, IEvent
```

Event handler; gets the event by reference (mutable fields and Cancelled are writable).

## EventPriority (enum, events)

```csharp
public enum EventPriority : int
```

Lower runs first. Any int is allowed; these are conventional values.

- `Lowest` = -200: Runs first.
- `Low` = -100: Runs early.
- `Normal` = 0: Default.
- `High` = 100: Runs late.
- `Highest` = 200: Runs last before monitors.
- `Monitor` = 1000: Observe the final outcome; must not modify the event.

## EventRegistry (class, events)

```csharp
public sealed class EventRegistry
```

Event subscription and firing, scoped to a plugin (everything is released on unload).

- `public bool Fire<T>(ref T ev) where T : struct, IEvent`: Fires an event through the native bus (native and managed handlers, by priority). Returns false if it was cancelled.
- `public bool HasSubscribers<T>() where T : struct, IEvent`: Whether anybody (native or managed) listens to `T`.
- `public Subscription On<T>(EventListener<T> listener, EventPriority priority = EventPriority.Normal, bool receiveCancelled = false) where T : struct, IEvent`: Subscribes `listener` to events of type `T`.

```csharp
// Block spam (High runs after Normal, so other plugins' edits are already applied)
Events.On((ref PlayerChatEvent e) =>
{
    if (e.Message.Contains("spam", StringComparison.OrdinalIgnoreCase)) e.Cancelled = true;
}, EventPriority.High);

// Rewrite a mutable field
Events.On((ref EntityDamageEvent e) => e.Amount *= 0.5f);

// Observe the final outcome without changing it
Events.On((ref PlayerChatEvent e) => Log.Info(e.Cancelled ? "chat was blocked" : "chat delivered"),
          EventPriority.Monitor, receiveCancelled: true);
```

## EventType<T> (class, events)

```csharp
public static class EventType<T> where T : struct, IEvent
```

Resolved runtime id per event type.

- `public static readonly ushort Id;`: The runtime event id.

## LoginDeferral (class, events)

```csharp
public sealed class LoginDeferral : IDisposable, IPluginResource
```

A login held by a plugin (see `PlayerPreLoginEvent.Defer`). Completing it is thread-safe and happens once.

- `public bool IsCompleted { get; }`: Whether the deferral was already completed.
- `public static Component UnloadDenyMessage { get; set; }`: What a player sees when the plugin holding his login unloaded before it answered (the safe default: a check that never finished is not a pass).
- `public void Allow()`: Lets the login continue. Callable from any thread.
- `public void Deny(Component? reason = null)`: Refuses the login. Callable from any thread.
- `public void Dispose()`: The plugin unloaded with the deferral pending: the login is refused with `LoginDeferral.UnloadDenyMessage`. A check that never answered (a ban list lookup) must not turn into a pass; the player just reconnects. Complete the deferral yourself with `LoginDeferral.Allow` to decide otherwise.

## ExhaustionReason (enum, other)

```csharp
public enum ExhaustionReason : byte
```

What caused food exhaustion (PlayerExhaustion).

- `Attack` = 0: The player attacked.
- `BlockMined` = 1: The player broke a block.
- `Crouch` = 2: Crouching (not fired by Ramjet yet).
- `Damaged` = 3: The player took damage.
- `Jump` = 4: The player jumped.
- `JumpSprint` = 5: The player jumped while sprinting.
- `Regen` = 6: Natural regeneration.
- `Sprint` = 7: The player sprinted.
- `Swim` = 8: The player swam.
- `WalkOnWater` = 9: The player walked on water.
- `WalkUnderwater` = 10: The player walked under water.
- `Unknown` = 11: Any other source (a hunger effect, a plugin).

## ExplosionResult (enum, other)

```csharp
public enum ExplosionResult : byte
```

What an explosion does to the blocks around it (EntityExplode, BlockExplode).

- `Keep` = 0: No block is destroyed.
- `Destroy` = 1: Blocks are destroyed and always drop their items (TNT, without drop decay).
- `DestroyWithDecay` = 2: Blocks are destroyed; their drops survive with the chance of the yield (creepers, ghasts, withers).
- `TriggerBlock` = 3: Blocks are triggered, not destroyed (the wind burst). Reserved: no explosion event uses it yet.

## Facing (enum, worlds)

```csharp
public enum Facing : int
```

Values of the protocol enum Facing (the wire carries the ordinal).

- `Down` = 0: Facing down (ordinal 0).
- `Up` = 1: Facing up (ordinal 1).
- `North` = 2: Facing north (ordinal 2).
- `South` = 3: Facing south (ordinal 3).
- `West` = 4: Facing west (ordinal 4).
- `East` = 5: Facing east (ordinal 5).

## FishState (enum, other)

```csharp
public enum FishState : byte
```

The state of a fishing bobber (PlayerFish).

- `Fishing` = 0: The rod was cast; the bobber does not exist yet.
- `CaughtFish` = 1: A loot stack was caught (once per stack).
- `CaughtEntity` = 2: The hook pulled an entity in.
- `InGround` = 3: The bobber rests on the ground.
- `FailedAttempt` = 4: The fish got away: the bite window ran out.
- `ReelIn` = 5: The rod was reeled in with nothing on the hook.
- `Bite` = 6: A fish bit the hook.
- `Lured` = 7: The fish were lured to the bobber.

## GameMode (enum, players)

```csharp
public enum GameMode : byte
```

Vanilla game mode (the numbers are the protocol ids).

- `Survival` = 0: Survival mode.
- `Creative` = 1: Creative mode.
- `Adventure` = 2: Adventure mode.
- `Spectator` = 3: Spectator mode.

## GameModeChangeCause (enum, events)

```csharp
public enum GameModeChangeCause : byte
```

Why a game mode changed.

- `Unknown` = 0: Unknown cause.
- `Command` = 1: /gamemode or /defaultgamemode.
- `Plugin` = 2: A plugin set the game mode.
- `DefaultGameMode` = 3: The server default game mode was applied.

## GrowSpecies (enum, other)

```csharp
public enum GrowSpecies : byte
```

What a sapling or a mushroom grows (StructureGrow). Mega variants count as their tree.

- `Oak` = 0: Oak (also fancy oak).
- `Spruce` = 1: Spruce, mega spruce and mega pine.
- `Birch` = 2: Birch.
- `Jungle` = 3: Jungle tree and mega jungle tree.
- `Acacia` = 4: Acacia.
- `DarkOak` = 5: Dark oak (2x2).
- `PaleOak` = 6: Pale oak (2x2).
- `Cherry` = 7: Cherry.
- `Mangrove` = 8: Mangrove and tall mangrove.
- `Azalea` = 9: Azalea tree.
- `Poplar` = 10: Red, orange and yellow poplar.
- `RedMushroom` = 11: Huge red mushroom.
- `BrownMushroom` = 12: Huge brown mushroom.
- `CrimsonFungus` = 13: Crimson fungus.
- `WarpedFungus` = 14: Warped fungus.
- `Other` = 15: A growth that is not one of the above.

## Hand (enum, players)

```csharp
public enum Hand : byte
```

The hand an action was performed with.

- `MainHand` = 0: The main hand.
- `OffHand` = 1: The off hand.

## HangingBreakCause (enum, other)

```csharp
public enum HangingBreakCause : byte
```

Why a hanging entity (item frame, glow item frame, painting) is about to break (HangingBreak, EntityBreak).

- `Entity` = 0: An entity hit it and no explosion was involved: remover is the attacker, or the owner of the projectile that hit it.
- `Explosion` = 1: An explosion damaged it or pushed it off its wall (remover is the entity behind the explosion, if any).
- `Obstruction` = 2: A block now fills the space it hangs in.
- `Physics` = 3: The wall behind it is gone.
- `Default` = 4: Damage with no entity behind it (fire, lava, a block).

## Hologram (class, visuals)

```csharp
public sealed class Hologram : IDisposable, IPluginResource
```

Text that floats in the world. The players see it (it is an entity on their side only: the server has none), and it is not saved with the world. Create it with `Holograms.Create`. It disappears for every viewer when the plugin unloads or when `Hologram.Remove` is called. Holograms are shown to everybody until `Hologram.HideFromAll` or `Hologram.ShowTo`.

- `public bool IsActive { get; }`: Whether the hologram still exists.
- `public HologramOptions Options { get; set; }`: The look of the hologram. Assign a new `HologramOptions` to change it.
- `public Vec3d Position { get; }`: Where it is.
- `public Component Text { get; set; }`: The text (a component; a newline makes a line break).
- `public void Dispose()`
- `public Hologram HideFrom(Player player)`: Hides the hologram from one player (while it is shown to everybody).
- `public Hologram HideFromAll()`: Hides the hologram from everybody (clears the list of players it is shown to or hidden from).
- `public Hologram Move(Vec3d position)`: Moves the hologram inside its world.
- `public void Remove()`: Removes the hologram from every viewer and deletes it.
- `public Hologram ShowTo(Player player)`: Shows the hologram to one player (after `Hologram.HideFromAll`), or hides it from one player while it is shown to everybody.
- `public Hologram ShowToAll()`: Shows the hologram to everybody again (clears the list of players it is shown to or hidden from).

## HologramAlign (enum, visuals)

```csharp
public enum HologramAlign : int
```

How the lines of a hologram are aligned.

- `Center` = 0: Centred.
- `Left` = 8: Left aligned.
- `Right` = 16: Right aligned.

## HologramBillboard (enum, visuals)

```csharp
public enum HologramBillboard : int
```

How a hologram turns towards the players.

- `Fixed` = 0: Fixed in the world.
- `Vertical` = 1: Turns around the vertical axis.
- `Horizontal` = 2: Turns around the horizontal axis.
- `Center` = 3: Always faces the player.

## HologramOptions (class, visuals)

```csharp
public sealed class HologramOptions
```

The look of a hologram (the fields of the client's text display; the defaults are vanilla's).

- `public HologramOptions()`
- `public HologramAlign Align { get; set; }`: The alignment of the lines.
- `public int Background { get; set; }`: The background colour, ARGB (0x40000000 is vanilla's translucent black).
- `public HologramBillboard Billboard { get; set; }`: How the text turns towards the players.
- `public bool DefaultBackground { get; set; }`: Uses the client's default background instead of `HologramOptions.Background`.
- `public int LineWidth { get; set; }`: The wrap width in pixels (1..1024).
- `public float Scale { get; set; }`: The uniform scale (0 .. 64].
- `public bool SeeThrough { get; set; }`: The text shows through blocks.
- `public bool Shadow { get; set; }`: Draws a shadow under the text.
- `public float ShadowRadius { get; set; }`: The shadow radius (0..16).
- `public float ShadowStrength { get; set; }`: The shadow strength (0..1).
- `public int? TextOpacity { get; set; }`: The text opacity 0..255; null for the default.
- `public float ViewRange { get; set; }`: The view distance multiplier (0..64).

## Holograms (class, visuals)

```csharp
public sealed class Holograms
```

Creates holograms for this plugin. Everything made here is removed when the plugin unloads.

- `public Hologram Create(World world, Vec3d position, Component text, HologramOptions? options = null)`: Creates a hologram that everybody sees.
- `public Hologram Create(World world, Vec3d position, IEnumerable<string> lines, HologramOptions? options = null)`: Creates a hologram from plain lines (one per line of text).

## Http (class, server)

```csharp
public static class Http
```

HTTP(S) for plugins. In the plugin worker the requests are performed by the server: its HTTP client resolves each name, refuses loopback, private, link-local and metadata addresses, limits request and response sizes and the rate, and enforces the timeouts (see the server's scripting.http.* settings in ramjet.toml). In an in-process host (development only) requests go out directly. Use `Http.Client`, or `new HttpClient(Http.Handler)` for your own timeouts and headers. Do not dispose either: they are shared by every plugin.

- `public static HttpClient Client { get; }`: A shared client over `Http.Handler`. Set the time limit per request with a `CancellationToken`; the server applies its own total timeout as well. Do not dispose it.
- `public static HttpMessageHandler Handler { get; }`: The message handler behind every request. Share it; do not dispose it.

## IEvent (interface, events)

```csharp
public interface IEvent
```

An event type. Catalogue events are `ref struct`s: a handler gets the event by reference and can neither store it nor capture it in a closure or an async method, so the borrowed native data it carries (strings, byte buffers) cannot outlive the dispatch. Plugin-defined events are ordinary blittable structs that start with an `EventHeader` field (or ref structs of the same layout).

Catalogue events are the engine's built-in events (fixed ids). Plugins can declare their own events: implement `IEvent.Key` (e.g. "myplugin:thing") and leave `IEvent.CatalogueId` at 0; an id is assigned at runtime.

- `static virtual ushort CatalogueId { get; }`: Catalogue id for built-in events, 0 for plugin-defined events.
- `static virtual bool IsCancellable { get; }`: Whether handlers can cancel the event.
- `static abstract string Key { get; }`: Registry key ("plugin:name" for plugin events).

```csharp
public struct HomeSetEvent : IEvent
{
    public EventHeader Header;      // must come first
    public Entity Player;
    public BlockPos Position;

    public static string Key => "homes:set";
    public static bool IsCancellable => true;
}

// fire it from your plugin
var ev = new HomeSetEvent { Player = p, Position = pos };
if (Events.Fire(ref ev)) { /* nobody cancelled */ }
```

## IPlugin (interface, plugins)

```csharp
public interface IPlugin
```

Plugin entry point. One public, non-abstract type with a parameterless constructor per plugin assembly. Most plugins derive from `Plugin` instead.

- `void Disable()`: Called when the plugin is unloaded; registrations are released afterwards.
- `void Enable(PluginContext context)`: Called once after load; register everything here.

## IPluginResource (interface, plugins)

```csharp
public interface IPluginResource : IDisposable
```

Something owned by a plugin that must be released when it unloads.

Implement it for your own objects and call `PluginContext.Track` so they are disposed on unload.

## IgniteCause (enum, events)

```csharp
public enum IgniteCause : byte
```

What set a block on fire.

- `FlintAndSteel` = 0: A player used flint and steel.
- `Lava` = 1: Lava next to a flammable block.
- `Lightning` = 2: A lightning strike.
- `Spread` = 3: Fire spreading from another fire.
- `FireCharge` = 4: A fire charge.
- `Explosion` = 5: An explosion.
- `Arrow` = 6: A flaming arrow.
- `Plugin` = 7: A plugin.
- `Other` = 8: Anything else.

## InteractAction (enum, events)

```csharp
public enum InteractAction : byte
```

What a player clicked.

- `LeftClickAir` = 0: Left click without a block target.
- `LeftClickBlock` = 1: Left click (start digging) on a block.
- `RightClickAir` = 2: Right click without a block target.
- `RightClickBlock` = 3: Right click on a block.

## InteractionHand (enum, players)

```csharp
public enum InteractionHand : int
```

Values of the protocol enum InteractionHand (the wire carries the ordinal).

- `MainHand` = 0: InteractionHand main_hand (ordinal 0).
- `OffHand` = 1: InteractionHand off_hand (ordinal 1).

## BlockInventory (class, inventories)

```csharp
public sealed class BlockInventory : EngineInventory
```

The inventory of a block entity by position: chests, trapped chests, barrels, furnaces, blast furnaces, smokers, hoppers, droppers, dispensers, brewing stands and shulker boxes. A double chest is two inventories of 27 slots (one per half). The handle holds a position, not the block entity: it is empty when the block goes away.

- `public BlockPos Position { get; }`: The block position.
- `public World World { get; }`: The dimension.
- `public static BlockInventory? At(World world, BlockPos pos)`: The inventory of the block entity at a position.

## EngineInventory (class, inventories)

```csharp
public abstract class EngineInventory : IInventory
```

Shared implementation of the inventories the engine owns: one native call per operation, the inventory is addressed by (kind, world, reference) every time, so a handle never keeps anything alive and a block that was destroyed meanwhile simply reads as an empty inventory.

- `public IEnumerable<ItemStack> Contents { get; }`
- `public bool Exists { get; }`: Whether the inventory still exists (a destroyed block entity or a closed player has none).
- `public virtual int Size { get; }`
- `public virtual int AddItem(ItemStack stack)`
- `public virtual void Clear()`
- `public bool Contains(ItemStack stack)`: Whether the inventory holds at least `stack`.Count items of its type.
- `public int Count(ItemStack stack)`: How many items of this type (and components) the inventory holds in total.
- `public virtual ItemStack Get(int slot)`
- `public virtual int RemoveItem(ItemStack stack)`
- `public virtual void Set(int slot, ItemStack stack)`

## IInventory (interface, inventories)

```csharp
public interface IInventory
```

A list of item slots: a player's inventory, a chest, a plugin menu. Every read returns a copy of the stack in the slot and every write replaces the stack: no object aliases the engine's storage, so a stack can be kept and a slot can change under it without harm. Slots count from 0. Tick thread only.

- `IEnumerable<ItemStack> Contents { get; }`: Every slot in order (a snapshot).
- `int Size { get; }`: The number of slots.
- `int AddItem(ItemStack stack)`: Adds a stack, merging into existing stacks first and then filling free slots.
- `void Clear()`: Empties every slot.
- `ItemStack Get(int slot)`: Reads a slot.
- `int RemoveItem(ItemStack stack)`: Removes items of the stack's type (and components) from the slots.
- `void Set(int slot, ItemStack stack)`: Replaces a slot.

## ItemStack (struct, inventories)

```csharp
public readonly struct ItemStack : IEquatable<ItemStack>
```

An immutable item stack value: an item type, a count and data components. Every `With...` method returns a new stack; the value can be kept, compared and put into inventories, menus and the world (`IInventory.Set`, {{cref:M:Mc.Scripting.Inventories.Menu.Slot(System.Int32,Mc.Scripting.Inventories.ItemStack,System.Action{Mc.Scripting.Inventories.MenuClick})}}, `World.DropItem`).

Components are addressed by their registry key (`"minecraft:custom_name"`) and carry NBT values. The ones the server converts are given in a plugin friendly form: text components (`custom_name`, `item_name`, `lore` lines) as JSON strings (what `Component.ToJson` makes), integers (`damage`, `max_damage`, `max_stack_size`, `repair_cost`, `map_id`, `enchantable`, `dyed_color`), `rarity` as `"common"` .. `"epic"`, `enchantment_glint_override` as a byte, `unbreakable` / `glider` as an empty compound, `custom_data` as a compound, `item_model` and `tooltip_style` as identifier strings, `custom_model_data` as `{floats, flags, strings, colors}`, `enchantments` and `stored_enchantments` as `{"minecraft:sharpness": 3}`, `tooltip_display` as `{hide_tooltip, hidden_components}`, `use_cooldown` as `{seconds, group?}` and `food` as `{nutrition, saturation, can_always_eat}`. Every other component the server knows is read as an opaque byte array holding its network encoding: it survives copying a stack but cannot be edited. A component key starting with `!` removes a component of the item's prototype (`ItemStack.WithoutDefault`). A stack that no inventory accepts (unknown item, a count above the stack size, a component whose value does not convert) is refused by the call that receives it with an `ArgumentException`.

- `public ItemStack(string type, int count = 1)`: Creates a stack.
- `public IEnumerable<string> ComponentKeys { get; }`: The keys of the components added to the stack (and `!key` entries for removed defaults).
- `public int Count { get; }`: The stack size (0 for an empty stack).
- `public NbtCompound? CustomData { get; }`: The plugin data of the stack (`minecraft:custom_data`), or null.
- `public string? CustomId { get; }`: The id of the custom item this stack is (`custom_data.mc.id`, written by the content layer), or null for an ordinary stack.
- `public bool IsEmpty { get; }`: Whether the stack holds nothing.
- `public int MaxStackSize { get; }`: The largest size a stack like this can have (the item's own, or its `max_stack_size` component). Asks the server.
- `public string Type { get; }`: The item key (`"minecraft:air"` for an empty stack).
- `public static ItemStack Empty { get; }`: No item.
- `public bool Equals(ItemStack other)`
- `public override bool Equals(object? obj)`
- `public NbtTag? GetComponent(string key)`: Reads a data component.
- `public override int GetHashCode()`
- `public bool HasComponent(string key)`: Whether the stack carries a component that was added to it.
- `public bool IsIn(string tag)`: Whether the item of this stack is in an item tag (`Items`): `stack.IsIn("#minecraft:swords")`. The components do not matter.
- `public NbtCompound ToNbt()`: The stack in its NBT form (`{id:"minecraft:stick",count:2,components:{...}}`), the form the server exchanges.
- `public override string ToString()`
- `public ItemStack WithComponent(string key, NbtTag value)`: The same stack with a data component set.
- `public ItemStack WithCount(int count)`: The same stack with another size.
- `public ItemStack WithCustomData(NbtCompound data)`: The same stack with plugin data (`minecraft:custom_data`) replaced.
- `public ItemStack WithCustomId(string customId)`: The same stack marked as a custom item (`custom_data.mc.id`); other plugin data is kept.
- `public ItemStack WithCustomModelData(float value)`: The same stack with one float of `minecraft:custom_model_data` (what `range_dispatch` model definitions of a resource pack read).
- `public ItemStack WithDamage(int damage)`: The same stack with durability damage (`minecraft:damage`).
- `public ItemStack WithEnchantment(string enchantment, int level)`: The same stack with an enchantment level (`minecraft:enchantments`).
- `public ItemStack WithItemModel(string modelId)`: The same stack with another model (`minecraft:item_model`): the id of an item definition of a resource pack (`assets/<namespace>/items/<path>.json`).
- `public ItemStack WithItemName(Component name)`: The same stack with an item name (`minecraft:item_name`: the base name of the item, not italic, replaced by a rename in an anvil).
- `public ItemStack WithLore(params Component[] lines)`: The same stack with lore lines (`minecraft:lore`).
- `public ItemStack WithMaxStackSize(int max)`: The same stack with another maximum stack size (`minecraft:max_stack_size`, 1 to 99).
- `public ItemStack WithName(Component name)`: The same stack with a custom name (`minecraft:custom_name`: shown in italics like a renamed item).
- `public ItemStack WithUnbreakable()`: The same stack that cannot take durability damage (`minecraft:unbreakable`).
- `public ItemStack WithoutComponent(string key)`: The same stack without a component that was added to it (the item's own default of it, if it has one, stays).
- `public ItemStack WithoutDefault(string key)`: The same stack without the default value of a component of its item (`minecraft:max_damage` of a tool, for example).
- `public static ItemStack FromNbt(NbtCompound nbt)`: Reads a stack from its NBT form.
- `public static bool IsSimilar(ItemStack a, ItemStack b)`: Whether two stacks are the same item with the same components, whatever their sizes (the test of merging stacks).
- `public static bool operator !=(ItemStack a, ItemStack b)`: Inequality.
- `public static bool operator ==(ItemStack a, ItemStack b)`: Equality.

## ItemView (struct, inventories)

```csharp
public readonly struct ItemView
```

A read-only view of an item stack inside an event: the item and the count. Blittable.

Valid only during the dispatch it came from. `ItemView.ToStack` turns it into a full `ItemStack` (components, names, lore) that can be kept.

- `public int Count { get; }`: Stack size (0 when empty).
- `public string? CustomId { get; }`: The id of the custom item this is (`ItemStack.CustomId`), or null. Costs nothing for a stack without components.
- `public bool HasComponents { get; }`: Whether the engine has the data components of this stack (false for plain stacks).
- `public bool IsEmpty { get; }`: Whether the stack is empty (no item, air or count 0).
- `public int ItemId { get; }`: Item registry id (0 for air / nothing).
- `public string Key { get; }`: The item key, e.g. "minecraft:diamond_sword" (empty for an empty stack).
- `public ItemStack ToStack()`: A copy of the stack with all its data components (names, lore, custom data ...) that stays valid after the event. A view of a stack without components needs no call to the engine.
- `public override string ToString()`

## Menu (class, inventories)

```csharp
public sealed class Menu : IDisposable, IInventory, IPluginResource
```

A custom GUI: a container with icons and click handlers that the client shows like a chest, a hopper or a dispenser. The menu is owned by the plugin and closed for every viewer when the plugin unloads or reloads.

**A menu is locked.** The engine keeps it a display: a click that could take an icon out or put an item in is denied (and the whole menu resynced to the client) unless a handler allows it with `MenuClick.Allow`; drags over its slots, shift clicks, double clicks, number keys, the off hand key and the drop key cannot move items out of it or into it; the creative clone click gives no copy of an icon. Whatever a handler decides, the server is authoritative: it only performs what vanilla would perform for the click, and a cancelled click changes nothing. Create menus with `Menus.Chest` (`plugin.Menus`) or the static factories, which belong to the plugin whose code is running (OnEnable, an event handler, a command, a scheduled task); items in the slots are ordinary stacks, so the menu doubles as a small shared inventory (`Menu.Get`, `Menu.Set`).

- `public IEnumerable<ItemStack> Contents { get; }`
- `public bool IsDisposed { get; }`: Whether the menu was disposed (or its plugin unloaded).
- `public bool LockPlayerInventory { get; set; }`: Whether every click in the menu is denied unless allowed, also the ones that only touch the player's own inventory (false by default: the player can rearrange his own slots).
- `public int Rows { get; }`: The number of rows (1 for a hopper, 3 for a dispenser).
- `public int Size { get; }`: The number of slots of the menu itself (the player's own slots are not counted).
- `public Component Title { get; set; }`: The title shown above the slots; changing it re-opens the screen of the viewers with the same contents.
- `public MenuType Type { get; }`: The vanilla menu type.
- `public IReadOnlySet<Player> Viewers { get; }`: The players who have the menu open (a snapshot).
- `public int AddItem(ItemStack stack)`
- `public void Clear()`
- `public int Close()`: Closes the menu for every viewer.
- `public bool Close(Player player)`: Closes the menu for one viewer (his cursor stack goes back to him).
- `public void Dispose()`: Closes the menu for everybody and frees it (what unloading the plugin does).
- `public Menu FillEmpty(ItemStack icon)`: Sets the icon of every empty slot (a border or a background).
- `public ItemStack Get(int slot)`
- `public ItemStack Icon(int slot)`: The icon the plugin set for a slot with {{cref:M:Mc.Scripting.Inventories.Menu.Slot(System.Int32,Mc.Scripting.Inventories.ItemStack,System.Action{Mc.Scripting.Inventories.MenuClick})}} (the stack in the slot now is `Menu.Get`).
- `public bool IsOpenFor(Player player)`: Whether a player has this menu open.
- `public Menu OnClick(Action<MenuClick> handler)`: Called for every click in the menu (the menu's own slots and the player's), before the handler of the slot.
- `public Menu OnClose(Action<Player> handler)`: Sets what happens when a player closes the menu (or it is closed for him).
- `public Menu OnClose(Action<Player, MenuCloseReason> handler)`: Sets what happens when a player closes the menu, with the reason.
- `public bool Open(Player player)`: Opens the menu for a player, closing whatever container he has open. `InventoryOpenEvent` (kind `Custom`) fires first and can cancel it.
- `public void Refresh()`: Sends the changed slots to the viewers now. (They are synced every tick anyway; this is for code that needs the packets within the current tick.)
- `public int RemoveItem(ItemStack stack)`
- `public void Set(int slot, ItemStack stack)`
- `public Menu Slot(int slot, ItemStack icon, Action<MenuClick>? onClick = null)`: Sets the icon and the click handler of a slot.
- `public static Menu Chest(int rows, Component title)`: Creates a menu of 9-wide rows for the plugin whose code is running.
- `public static Menu Dispenser(Component title)`: Creates a dispenser-style menu (3 x 3 slots) for the plugin whose code is running.
- `public static Menu Hopper(Component title)`: Creates a hopper menu (5 slots) for the plugin whose code is running.

## MenuClick (class, inventories)

```csharp
public sealed class MenuClick
```

A click in a `MenuClick.Menu`. A menu is locked: a click that could move items out of the menu or into it (a slot of the menu, shift click, double click, number key, the off hand key, the drop key, a middle click) is denied and the client is resynced unless the handler calls `MenuClick.Allow`; `MenuClick.Cancel` always denies. Clicks that only touch the player's own inventory slots pass unless a handler cancels them (or the menu locks the player inventory too). The object is valid during the handler only.

- `public bool Allowed { get; }`: Whether the click will be processed (set by `MenuClick.Allow`; false by default for a click on the menu's own slots).
- `public int Button { get; }`: The raw button of the click packet.
- `public bool Cancelled { get; set; }`: Whether the click is cancelled for good (`MenuClick.Cancel` or setting this).
- `public ItemStack Clicked { get; }`: The stack in the clicked slot (before the click).
- `public ItemStack Cursor { get; }`: The stack the player holds on the cursor (before the click).
- `public bool IsTopInventory { get; }`: Whether the click is on one of the menu's own slots.
- `public Menu Menu { get; }`: The menu.
- `public Player Player { get; }`: The player who clicked.
- `public int PlayerSlot { get; }`: For a click on the player's own slots: the slot in `PlayerInventory` numbering; otherwise -1.
- `public int Slot { get; }`: The clicked slot in the menu's numbering: 0 to `Menu.Size` - 1 are the menu's own slots, then the player's 27 main and 9 hotbar slots; -999 outside the window.
- `public MenuClickType Type { get; }`: How the player clicked.
- `public void Allow()`: Lets the click change items like it would in a chest (take the stack out of the menu, put one in, shift click ...). The server only does what vanilla does.
- `public void Cancel()`: Denies the click: nothing changes and the client is resynced (also for clicks that would pass by default).
- `public void Close()`: Closes the menu for the player after the click.
- `public void SetClicked(ItemStack stack)`: Replaces the stack in the clicked slot (a slot of the menu or one of the player's).
- `public void SetCursor(ItemStack stack)`: Replaces the stack on the player's cursor (the client is resynced after the click).

## MenuClickType (enum, inventories)

```csharp
public enum MenuClickType : int
```

The kind of a click in a menu.

- `Left` = 0: A plain left click.
- `Right` = 1: A plain right click.
- `ShiftClick` = 2: Shift plus click.
- `NumberKey` = 3: A number key swap.
- `DoubleClick` = 4: A double click.
- `Drop` = 5: A drop key.
- `Drag` = 6: The end of a drag over the player's own slots (`MenuClick.Slot` is the first dragged slot; a drag over one of the menu's own slots is denied by the engine before any handler sees it).
- `Middle` = 7: A middle click (the creative clone click).
- `SwapOffhand` = 8: The off hand key over a slot.
- `ControlDrop` = 9: Control plus the drop key (the whole stack).
- `Outside` = 10: A click outside the window (drops the cursor stack).
- `Unknown` = 11: Anything else.

## MenuType (enum, inventories)

```csharp
public enum MenuType : int
```

The vanilla menu type a `Menu` uses (what the client draws).

- `Chest` = 0: A chest: 1 to 6 rows of 9 slots.
- `Hopper` = 1: A hopper: 5 slots in a row.
- `Dispenser` = 2: A dispenser or dropper: 3 x 3 slots.

## Menus (class, inventories)

```csharp
public sealed class Menus
```

Creates menus owned by a plugin (`plugin.Menus`).

- `public Menu Chest(int rows, Component title)`: Creates a chest-style menu of 9-wide rows.
- `public Menu Create(MenuType type, Component title, int rows = 3)`: Creates a menu.
- `public Menu Dispenser(Component title)`: Creates a dispenser-style menu (3 x 3 slots).
- `public Menu Hopper(Component title)`: Creates a hopper menu (5 slots).

## PlayerInventory (class, inventories)

```csharp
public sealed class PlayerInventory : EngineInventory
```

The inventory of a player: 41 slots numbered like the game's own inventory container, plus the cursor. Slots 0-8 are the hotbar, 9-35 the main inventory, 36 to 39 the armor (boots, leggings, chestplate, helmet) and 40 the off hand. A stale player (one who left) has no slots: reads are empty and writes are refused.

- `public const int BootsSlot = 36;`: The boots slot.
- `public const int ChestplateSlot = 38;`: The chestplate slot.
- `public const int FirstMainSlot = 9;`: First slot of the main inventory (below the hotbar).
- `public const int HelmetSlot = 39;`: The helmet slot.
- `public const int LeggingsSlot = 37;`: The leggings slot.
- `public const int OffHandSlot = 40;`: The off hand slot.
- `public const int SlotCount = 41;`: Number of slots.
- `public ItemStack Boots { get; set; }`: The boots.
- `public ItemStack Chestplate { get; set; }`: The chestplate.
- `public ItemStack Cursor { get; set; }`: The stack the player carries on the cursor while a container or his inventory screen is open. Setting it resyncs the client.
- `public ItemStack Helmet { get; set; }`: The helmet.
- `public ItemStack Leggings { get; set; }`: The leggings.
- `public ItemStack MainHand { get; set; }`: The stack in the main hand (the selected hotbar slot).
- `public ItemStack OffHand { get; set; }`: The stack in the off hand.
- `public Player Player { get; }`: The player.
- `public int SelectedSlot { get; set; }`: The selected hotbar slot (0-8); setting it tells the client.
- `public override int Size { get; }`
- `public int Give(ItemStack stack)`: Gives a stack to the player: what does not fit into the inventory is dropped at his feet (like picking up with a full inventory).
- `public ItemStack Hotbar(int index)`: The stack in a hotbar slot.
- `public static PlayerInventory Of(Player player)`: The inventory of a player.

## InventoryKind (enum, inventories)

```csharp
public enum InventoryKind : byte
```

The kind of container a player has open.

- `Chest` = 0: Chest / ender chest / generic rows menu.
- `Dispenser` = 1: Dispenser or dropper (3x3).
- `Furnace` = 2: Furnace.
- `BlastFurnace` = 3: Blast furnace.
- `Smoker` = 4: Smoker.
- `Workbench` = 5: Crafting table.
- `Enchanting` = 6: Enchanting table.
- `Brewing` = 7: Brewing stand.
- `Hopper` = 8: Hopper.
- `Anvil` = 9: Anvil.
- `Grindstone` = 10: Grindstone.
- `Stonecutter` = 11: Stonecutter.
- `Smithing` = 12: Smithing table.
- `Loom` = 13: Loom.
- `Cartography` = 14: Cartography table.
- `Beacon` = 15: Beacon.
- `ShulkerBox` = 16: Shulker box.
- `Barrel` = 17: Barrel.
- `Lectern` = 18: Lectern.
- `Merchant` = 19: Villager / wandering trader trades.
- `PlayerInventory` = 20: The player's own inventory.
- `Custom` = 21: A plugin-created menu.
- `Unknown` = 22: Anything else.

## ItemFrameChangeAction (enum, other)

```csharp
public enum ItemFrameChangeAction : byte
```

What a player does to an item frame (PlayerItemFrameChange).

- `Place` = 0: Puts an item into the empty frame.
- `Remove` = 1: Takes the item out of the frame (a punch).
- `Rotate` = 2: Rotates the framed item (a use of the frame).

## KickCause (enum, other)

```csharp
public enum KickCause : byte
```

Why a player is being kicked (PlayerKick).

- `Command` = 0: A /kick command.
- `Plugin` = 1: A plugin kicked the player through the host.
- `Idle` = 2: The player idled too long (player-idle-timeout). Cancel keeps the player.
- `Flying` = 3: The player floated too long. Cancel keeps the player.
- `InvalidMovement` = 4: The player's movement was rejected as invalid. Cancel is ignored.
- `Other` = 5: Any other server-decided kick. Cancel is ignored.

## KnockbackCause (enum, other)

```csharp
public enum KnockbackCause : byte
```

What knocked a living entity back (EntityKnockback).

- `Unknown` = 0: No cause reported by the call site.
- `Attack` = 1: A melee, stab or extra attack knockback.
- `Effect` = 2: A knockback that comes from an effect (LivingEntity.knockback with comesFromEffect).
- `Push` = 3: A push by a mob's ability (a ravager's roar).
- `Explosion` = 4: An explosion pushes the entity.
- `ShieldBlock` = 5: A shield or blocking item pushes back the attacker.
- `SweepAttack` = 6: The sweep attack of a sword hits the entity.
- `Damage` = 7: The damage of a hit pushes the entity (the generic damage knockback).

## LightningCause (enum, other)

```csharp
public enum LightningCause : byte
```

Why a lightning bolt is made (LightningStrike).

- `Command` = 0: /summon lightning_bolt, execute summon.
- `Custom` = 1: A plugin spawned it.
- `Trident` = 2: Channeling of a thrown trident that hits a block (the player is the one who threw it).
- `Trap` = 3: A skeleton trap (the visual bolt of a skeleton horse).
- `Weather` = 4: A thunder storm strike.
- `Unknown` = 5: Not known.
- `Enchantment` = 6: Channeling as a post-attack effect of a melee hit (the player is the one who hit).

## LivingEntity (struct, other)

```csharp
public readonly struct LivingEntity : IEquatable<LivingEntity>
```

A handle to a living entity: a player, a mob or another entity with health (vanilla `LivingEntity`). Blittable, like `LivingEntity.Entity`.

Get one with `Entity.AsLiving` (null when the entity is not living) or `Player.AsLiving`. A handle that goes stale (the entity died and was removed) reads defaults and changes nothing. Members must be called on the tick thread. Health, absorption and attribute values are in vanilla units (a heart is 2).

- `public LivingEntity(Entity entity)`: Wraps an entity. Prefer `Entity.AsLiving`, which checks that the entity is living.
- `public readonly Entity Entity;`: The entity this handle names.
- `public double Absorption { get; set; }`: The golden hearts of absorption, in half hearts. Setting it is not clamped to the `minecraft:max_absorption` attribute (Paper semantics).
- `public Component? CustomName { get; set; }`: The custom name of a mob (the name tag), null when it has none. Setting null clears it. Mobs only; ignored for other entities.
- `public bool CustomNameVisible { get; set; }`: Whether the custom name of a mob is shown above its head at all times (vanilla `CustomNameVisible`). Mobs only.
- `public bool Glowing { get; set; }`: Whether the entity glows (the outline in the team colour, white without a team) for every player online. The glow follows players who join or leave. The server keeps at most 64 plugin glows and the visuals table is shared: when there is no room, setting it does nothing, and the getter reads false until the glow reaches every viewer. Any living entity that the clients track.
- `public double Health { get; set; }`: The health. Setting clamps it to 0 .. `LivingEntity.MaxHealth` like vanilla; 0 kills the entity through the normal death handling.
- `public bool Invisible { get; set; }`: Whether the entity is invisible to the clients (the shared flag 0x20). Reads true while the flag is sent, which includes the case where the invisibility effect is active. Setting it changes only the plugin's own share: clearing it does not end the effect. Any living entity.
- `public bool Invulnerable { get; set; }`: Whether the entity takes no damage (vanilla `Invulnerable`). Any living entity. Hits whose damage type is tagged as bypassing invulnerability (such as falling into the void) and hits from a creative-mode player still land, as vanilla. It is separate from a player's creative invulnerability (the abilities), which also refuses damage on its own. A mob keeps the flag in its saved data (the NBT `Invulnerable` tag); a player's flag is runtime state only: it is not written to the player's save and does not survive a dimension change or a respawn.
- `public bool IsDead { get; }`: Whether the entity is dead or dying. False for a handle that is not living.
- `public bool IsLiving { get; }`: Whether the entity is still alive and living.
- `public bool IsNone { get; }`: Whether this is the "no living entity" handle.
- `public double MaxHealth { get; }`: The maximum health (the `minecraft:max_health` attribute, 20 by default). Read only: change it with attribute modifiers.
- `public bool NoAi { get; set; }`: Whether a mob's artificial intelligence is off: it does not move, look or attack on its own (vanilla `NoAI`). Mobs only; ignored for players and other entities.
- `public IReadOnlyList<Entity> Passengers { get; }`: The entities riding this one, in the order of the vehicle's passenger list (players first, like vanilla). A snapshot.
- `public bool Persistent { get; set; }`: Whether a mob stays in the world when no player is near (vanilla `PersistenceRequired`): it is not despawned. Mobs only; ignored for players and other entities.
- `public bool Silent { get; set; }`: Whether the entity makes no sound of its own (vanilla `Silent`): no ambient, hurt or death sounds. Any living entity.
- `public Entity? Target { get; set; }`: The entity a mob attacks, null when it has no target (or the entity is not a mob, or the target is gone). Setting null clears it; setting a target that is not living is ignored. A new target takes effect in the mob's next AI tick, through the same path its goals use (so a mob that refuses the target keeps its old one). The mob's goals may change the target again afterwards.
- `public Entity? Vehicle { get; }`: The entity this one rides (a mob, a player, a boat), or null when it rides nothing or the handle is stale.
- `public static LivingEntity None { get; }`: The "no living entity" handle.
- `public bool AddAttributeModifier(string key, string id, double amount, AttributeOperation operation = AttributeOperation.AddValue)`: Adds an attribute modifier with an identifier. Remove it again with `LivingEntity.RemoveAttributeModifier`; the identifier is what makes it unique on this attribute (a second modifier with the same id is refused, like `/attribute modifier add`).
- `public bool AddEffect(PotionEffect effect)`: Adds a potion effect (vanilla `LivingEntity.addEffect`): a stronger or longer effect of the same kind replaces the current one. The same as `Player.AddEffect` for every living entity.
- `public bool Damage(double amount, string damageType = "minecraft:generic", Entity? attacker = null)`: Hurts the entity through the damage pipeline (`Combat.hurt`): armour, enchantments, invulnerability, the damage cooldown, death and the death events all apply, as they do for an attack of the same kind.
- `public bool Dismount()`: Stops this entity riding, as `/ride <target> dismount` does: it lands at the top of the vehicle, and cannot board again for a short while.
- `public int EjectPassengers()`: Ejects every entity riding this one, as vanilla `Entity.ejectPassengers`.
- `public bool Equals(LivingEntity other)`: Same entity id.
- `public override bool Equals(object? obj)`: Same entity id.
- `public AttributeValue? GetAttribute(string key)`: An attribute of the entity: its base value and its value with the modifiers (`minecraft:max_health`, `minecraft:movement_speed`, `minecraft:attack_damage`, ...). Only attributes the entity has are readable (a mob has the ones of its type).
- `public double? GetAttributeModifier(string key, string id)`: The amount of an attribute modifier by identifier.
- `public double GetDropChance(EquipmentSlot slot)`: The chance (0 .. 1, above 1 keeps the item) that a mob drops the item of an equipment slot when it dies. 0.085 by default.
- `public PotionEffect? GetEffect(string effect)`: The active effect of a kind, null when the entity does not have it.
- `public IReadOnlyList<PotionEffect> GetEffects()`: Every active potion effect of the entity, with its duration, amplifier and display flags.
- `public ItemStack GetEquipment(EquipmentSlot slot)`: The item in an equipment slot (a copy; empty for an empty slot or a handle that is not living).
- `public override int GetHashCode()`: Hash of the entity id.
- `public bool HasEffect(string effect)`: Whether the entity has a potion effect.
- `public bool Heal(double amount)`: Heals the entity like `LivingEntity.heal`: adds health, clamped to the maximum. Nothing happens to a dead entity.
- `public bool Mount(Entity vehicle)`: Makes this entity ride a vehicle, as `/ride <target> mount <vehicle>` does (forced: the vehicle's willingness is not asked). Refused when this entity already rides something, the vehicle is a player, the vehicle is this entity or one of its vehicles (a cycle), or they are in different dimensions.
- `public bool RemoveAttributeModifier(string key, string id)`: Removes an attribute modifier by identifier.
- `public bool RemoveEffect(string effect)`: Removes a potion effect.
- `public bool SetAttributeBase(string key, double value)`: Sets the base value of an attribute, like `/attribute <target> <attribute> base set`. The entity is refreshed: a lower maximum health clamps the health.
- `public bool SetDropChance(EquipmentSlot slot, double chance)`: Sets the drop chance of a mob's equipment slot. Players ignore it (their items are always theirs to keep).
- `public bool SetEquipment(EquipmentSlot slot, in ItemStack stack)`: Puts an item into an equipment slot, like `/item replace entity <target> <slot>`. A mob's armour slot takes only items of that slot; an empty stack clears the slot. The change is sent to the clients.
- `public override string ToString()`: Debug text.
- `public static bool operator !=(LivingEntity a, LivingEntity b)`: Different entity id.
- `public static bool operator ==(LivingEntity a, LivingEntity b)`: Same entity id.

## LocateSource (enum, other)

```csharp
public enum LocateSource : byte
```

Who asked for a structure search (StructuresLocate).

- `Command` = 0: The /locate structure command.
- `EyeOfEnder` = 1: A thrown eye of ender looking for its stronghold.
- `ExplorerMap` = 2: An exploration map (loot or a cartographer trade).
- `Dolphin` = 3: A dolphin looking for buried treasure.

## Location (struct, primitives)

```csharp
public readonly struct Location : IEquatable<Location>
```

A position with a rotation in a world.

- `public Location(World World, Vec3d Position, float Yaw = 0f, float Pitch = 0f)`: A position with a rotation in a world.
- `public Location(World world, double x, double y, double z, float yaw = 0f, float pitch = 0f)`: Creates a location from coordinates.
- `public BlockPos BlockPos { get; }`: The block containing the position.
- `public float Pitch { get; init; }`: Pitch in degrees (-90 = up, 90 = down).
- `public Vec3d Position { get; init; }`: Position in blocks.
- `public World World { get; init; }`: The dimension.
- `public double X { get; }`: X coordinate.
- `public double Y { get; }`: Y coordinate.
- `public float Yaw { get; init; }`: Yaw in degrees (0 = south, 90 = west).
- `public double Z { get; }`: Z coordinate.

## Log (class, server)

```csharp
public static class Log
```

Global logger (prefix "[scripting]"). Plugins should prefer their own `Logger`.

- `public static void Error(string message)`: Writes an error.
- `public static void Error(string message, Exception e)`: Writes an error with the exception.
- `public static void Info(string message)`: Writes an info message with the "[scripting]" prefix.
- `public static void Warn(string message)`: Writes a warning.

## LogLevel (enum, server)

```csharp
public enum LogLevel : int
```

Log severity.

- `Trace` = 0: Very detailed diagnostics.
- `Debug` = 1: Debug output.
- `Info` = 2: Normal operation.
- `Warn` = 3: Something unexpected but harmless.
- `Error` = 4: An error.

## Logger (class, server)

```csharp
public sealed class Logger
```

Writes to the server log (native logger), prefixed with the plugin name. A plugin's own logger (`Plugin.Log`) also writes to the plugin's log file, `logs/plugins/<plugin-id>/latest.log`.

- `public Logger(string name)`: Creates a logger that prefixes `[name]`.
- `public void Debug(string message)`: Writes a debug message.
- `public void Error(string message)`: Writes an error.
- `public void Error(string message, Exception exception)`: Writes an error with the exception and its stack trace.
- `public void Info(string message)`: Writes an info message.
- `public void Trace(string message)`: Writes a trace message.
- `public void Warn(string message)`: Writes a warning.
- `public void Write(LogLevel level, string message)`: Writes a message at `level`.

## LootContext (class, other)

```csharp
public sealed class LootContext
```

What a loot table is rolled with: the world, and the parameters the table's conditions and functions read (the vanilla `LootParams`).

Only the parameters the plugin API can name are here. Set the ones the table and its `LootContext.Type` need; a table that needs one that is not set makes `LootTables.Generate` throw an `ArgumentException` naming it. Entities are handles: one that has died when the roll starts is an error too. Killer also counts as the direct attacker (the melee case, as `/loot kill` rolls it). The damage type of a mob roll is what the kill was (`"minecraft:player_attack"`, `"minecraft:mob_attack"`, `"minecraft:magic"`).

- `public LootContext(World world)`: Creates a context for a roll in a world. The world's dimension decides the location conditions.
- `public BlockState? BlockState { get; set; }`: The block state the loot comes from (vanilla `BLOCK_STATE`): the broken or the interacted block.
- `public string? DamageType { get; set; }`: The damage type of the death or hit (vanilla `DAMAGE_SOURCE`), e.g. `"minecraft:player_attack"`; a name without a namespace is in `minecraft`.
- `public float? ExplosionRadius { get; set; }`: The radius of the explosion the loot comes from (vanilla `EXPLOSION_RADIUS`), what the explosion decay condition reads.
- `public Entity? Killer { get; set; }`: The attacker (vanilla `ATTACKING_ENTITY` and `DIRECT_ATTACKING_ENTITY`), the one that gets the kill.
- `public float Luck { get; set; }`: The luck of the rolling player (vanilla `LootParams.getLuck`, the Luck attribute, fishing luck); 0 by default.
- `public Location? Origin { get; set; }`: Where the loot comes from (vanilla `ORIGIN`): the block or the entity's position. Only the position is read, the world must be `LootContext.World`.
- `public Player? Player { get; set; }`: The player that last damaged the entity (vanilla `LAST_DAMAGE_PLAYER`), what `killed_by_player` conditions read. Must be a player.
- `public long? Seed { get; set; }`: The seed of this roll. Null (the default) uses the table's random sequence, which the server advances; a seed makes the roll repeatable. 0 is the same as null.
- `public Entity? This { get; set; }`: The entity the loot belongs to (vanilla `THIS_ENTITY`): the mob that died, the chest's opener, the fishing player.
- `public ItemStack? Tool { get; set; }`: The tool used (vanilla `TOOL`); `ItemStack.Empty` for none.
- `public LootContextType Type { get; set; }`: The context type (the vanilla parameter set); `LootContextType.Auto` (the default) takes the table's own type.
- `public World World { get; }`: The world the loot is rolled in.

## LootContextType (enum, other)

```csharp
public enum LootContextType : int
```

The vanilla loot context type (a `LootContextParamSet`): which parameters a roll is given. `LootContextType.Auto` takes the type the table itself declares (its `"type"` key; a table without one takes no particular parameter).

- `Auto` = 0: The type the table declares.
- `Chest` = 1: Chests and other containers (`minecraft:chest`): an origin, optionally a this entity.
- `Command` = 2: Commands (`minecraft:command`): an origin, optionally a this entity.
- `Selector` = 3: Selectors (`minecraft:selector`): an origin and a this entity.
- `Block` = 4: Block drops (`minecraft:block`): a block state, an origin and a tool.
- `Entity` = 5: Mob drops (`minecraft:entity`): a this entity, an origin and a damage type; the attacker and the player are optional.
- `Equipment` = 6: Equipment (`minecraft:equipment`): an origin and a this entity.
- `Fishing` = 7: Fishing (`minecraft:fishing`): an origin and a tool, optionally a this entity.
- `Gift` = 8: Gifts (`minecraft:gift`): an origin and a this entity.
- `Barter` = 9: Piglin bartering (`minecraft:barter`): a this entity.
- `Vault` = 10: Vaults (`minecraft:vault`): an origin, optionally a this entity and a tool.
- `Archaeology` = 11: Archaeology brushing (`minecraft:archaeology`): an origin, a this entity and a tool.
- `AdvancementReward` = 12: Advancement rewards (`minecraft:advancement_reward`): an origin and a this entity.
- `AdvancementEntity` = 13: Advancement entity conditions (`minecraft:advancement_entity`): an origin and a this entity.

## LootTables (class, other)

```csharp
public static class LootTables
```

Loot tables of the datapack: check that one exists, list them, roll one with a `LootContext`, and fill a container with one.

A roll checks the context the way vanilla checks a `LootParams`: every parameter the context type requires must be given, and nothing the type does not allow. The table's own type is checked too, so a table that needs an origin is not rolled without one. Rolls use the server's random source (or the context's `LootContext.Seed`), so a seeded roll is repeatable. Stacks are split to their maximum size, empty ones are dropped. Ids are `"namespace:path"`, the namespace may be left out for `minecraft`. Loot is a tick thread API.

- `public static bool Exists(string key)`: Whether the datapack has a loot table with this id.
- `public static bool FillContainer(World world, BlockPos pos, string key, long? seed = null)`: Sets the loot table of a container (a chest, trapped chest, barrel or shulker box), the `LootTable` tag vanilla keeps in a container's NBT: the contents are rolled when the container is first opened, replacing what it held.
- `public static IReadOnlyList<ItemStack> Generate(string key, LootContext context)`: Rolls a loot table with a context and returns what it drops.
- `public static IReadOnlyList<string> Keys()`: Every loot table id of the datapack (`"minecraft:chests/simple_dungeon"`, `"minecraft:blocks/stone"`, ...).

## McPermissionAttribute (class, permissions)

```csharp
public sealed class McPermissionAttribute : Attribute
```

Declares a permission node of a source plugin (the `permissions` array of `plugin.json`): `[assembly: McPermission("homes.use", Default = "true", Description = "Use /home")]`. Like `McPluginAttribute` it is read from the syntax before compiling, so the arguments must be string literals.

- `public McPermissionAttribute(string node)`: Declares a permission node of a source plugin (the `permissions` array of `plugin.json`): `[assembly: McPermission("homes.use", Default = "true", Description = "Use /home")]`. Like `McPluginAttribute` it is read from the syntax before compiling, so the arguments must be string literals.
- `public string? Default { get; init; }`: Who has it by default: `true`, `false`, `op`, `op:N` or `not_op` (default `op`).
- `public string? Description { get; init; }`: What the permission allows.
- `public string Node { get; }`: The permission node.

## McPluginAttribute (class, plugins)

```csharp
public sealed class McPluginAttribute : Attribute
```

The manifest of a **source plugin** (a `.cs` file or folder the server compiles at startup), written as an assembly attribute at the top of one of its files: `[assembly: McPlugin("homes", version: "1.2.0", depends: ["economy >=1.0"])]`. It carries the same information as `plugin.json` (a `plugin.json` next to the sources takes precedence). The server reads it from the syntax of the file **before** compiling, to compute the load order, so every argument must be a literal: string literals, `null`, and arrays or collection expressions of string literals (no constants, no `nameof`, no interpolation).

The attribute is ignored on a compiled plugin (it has a `plugin.json` or `PluginInfoAttribute`).

- `public McPluginAttribute(string id, string? version = null, string? name = null, string[]? depends = null, string[]? softDepends = null, string[]? loadBefore = null, string? description = null, string[]? authors = null, string? website = null, string? api = null, string? main = null)`: Declares the plugin.
- `public string? Api { get; }`: The API version.
- `public string[]? Authors { get; }`: The authors.
- `public string[]? Depends { get; }`: The hard dependencies.
- `public string? Description { get; }`: The description.
- `public string Id { get; }`: The plugin id.
- `public string[]? LoadBefore { get; }`: The plugins that load after this one.
- `public string? Main { get; }`: The plugin class.
- `public string? Name { get; }`: The display name.
- `public string[]? SoftDepends { get; }`: The soft dependencies.
- `public string? Version { get; }`: The plugin version.
- `public string? Website { get; }`: The website.

## MenuCloseReason (enum, inventories)

```csharp
public enum MenuCloseReason : byte
```

Why a plugin menu was closed for a player.

- `Player` = 0: The player closed it (Escape, the inventory key) or the client sent a close.
- `Plugin` = 1: A plugin called Menu.Close.
- `Unload` = 2: The plugin that owns the menu unloaded or reloaded.
- `Quit` = 3: The player left the server.
- `Replaced` = 4: Another container was opened for the player.
- `Invalid` = 5: The server closed it (death, dimension change, the menu no longer valid).

## MoveInitiator (enum, other)

```csharp
public enum MoveInitiator : byte
```

What moved an item between inventories without a player (InventoryMoveItem).

- `Hopper` = 0: A hopper pushed the item out of its inventory or pulled it in.
- `Dropper` = 1: A dropper put the item into the container in front of it.
- `HopperMinecart` = 2: A hopper minecart pulled the item out of the container above it.

## NametagCollision (enum, display)

```csharp
public enum NametagCollision : int
```

Who a player collides with (bumps into).

- `Always` = 0: Collides with everybody.
- `Never` = 1: Collides with nobody.
- `PushOtherTeams` = 2: Pushes the players of other teams only.
- `PushOwnTeam` = 3: Pushes the players of its own team only.

## NametagStyle (class, display)

```csharp
public sealed class NametagStyle
```

The look of a nametag for one viewer (see `Nametags.Set`).

- `public NametagStyle()`
- `public NametagCollision Collision { get; init; }`: Who the player collides with.
- `public int? Colour { get; init; }`: The legacy colour of the name as an index 0 to 15 (the §0 to §f colour codes, NOT RGB), or null for none. A value outside 0..15 throws `ArgumentOutOfRangeException` when the style is applied.
- `public bool FriendlyFire { get; init; }`: Whether the players of the team hit each other.
- `public Component? Prefix { get; init; }`: The text before the name, or null for none.
- `public bool SeeInvisibles { get; init; }`: Whether the players of the team see the invisible players of the team.
- `public Component? Suffix { get; init; }`: The text after the name, or null for none. The prefix and the suffix together are at most 8000 bytes.
- `public NametagVisibility Visibility { get; init; }`: Who sees the nametag.

## NametagVisibility (enum, display)

```csharp
public enum NametagVisibility : int
```

Who sees a nametag.

- `Always` = 0: Everybody sees it.
- `Never` = 1: Nobody sees it.
- `HideForOtherTeams` = 2: Only the players of the same team see it.
- `HideForOwnTeam` = 3: Only the players of other teams see it.

## Nametags (class, display)

```csharp
public sealed class Nametags
```

Nametag styles per viewer: a prefix and a suffix around a player's name, a team colour, and the rules for seeing and bumping the team's players. Each style is a team of the client; the players with the same style share it. Reach it as `plugin.Nametags`. Call from the tick thread. What a plugin sets is removed when the plugin unloads.

- `public void Clear(Player viewer, Player target)`: Puts one player's nametag back to the server's own for one viewer.
- `public void Set(Player viewer, Player target, NametagStyle style)`: Gives one player a nametag style as one viewer sees him. A player has one style per viewer; a new style replaces the old one.

## NativeBytes (struct, primitives)

```csharp
public readonly ref struct NativeBytes
```

Borrowed byte buffer (McBytes). Only valid during the dispatch that provided it.

- `public NativeBytes(byte* data, long size)`: Wraps native memory.
- `public int Length { get; }`: Length in bytes.
- `public ReadOnlySpan<byte> Span { get; }`: The bytes (borrowed).
- `public byte[] ToArray()`: Copies the bytes.

## NbtByte (class, nbt)

```csharp
public sealed class NbtByte : NbtValueTag
```

Signed 8 bit integer (also the game's boolean).

- `public NbtByte(sbyte value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public sbyte Value { get; set; }`: The value.
- `public override NbtTag Clone()`

## NbtByteArray (class, nbt)

```csharp
public sealed class NbtByteArray : NbtTag
```

An array of bytes.

- `public NbtByteArray(sbyte[] value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public sbyte[] Value { get; set; }`: The values (not copied).
- `public override NbtTag Clone()`

## NbtCompound (class, nbt)

```csharp
public sealed class NbtCompound : NbtTag, IEnumerable, IEnumerable<KeyValuePair<string, NbtTag>>
```

A compound: named tags (keys are unique; the enumeration order is the insertion order).

- `public NbtCompound()`
- `public int Count { get; }`: Number of entries.
- `public IEnumerable<string> Keys { get; }`: The keys.
- `public override NbtType Type { get; }`
- `public NbtTag this[string key] { get; set; }`: Gets or sets an entry.
- `public void Clear()`: Removes every entry.
- `public override NbtTag Clone()`
- `public bool Contains(string key)`: Whether a key exists.
- `public T? Get<T>(string key) where T : NbtTag`: Gets an entry of a given type.
- `public bool GetBool(string key, bool fallback = false)`: Reads a boolean (a non-zero numeric entry).
- `public NbtCompound? GetCompound(string key)`: Reads a compound entry.
- `public double GetDouble(string key, double fallback = 0)`: Reads a numeric entry as a double.
- `public IEnumerator<KeyValuePair<string, NbtTag>> GetEnumerator()`
- `public int GetInt(string key, int fallback = 0)`: Reads a numeric entry as an int.
- `public NbtList? GetList(string key)`: Reads a list entry.
- `public long GetLong(string key, long fallback = 0)`: Reads a numeric entry as a long (any integer or float type; floats are truncated).
- `public string GetString(string key, string fallback = "")`: Reads a string entry.
- `public NbtCompound Merge(NbtCompound other)`: Deep-merges another compound into this one: nested compounds merge, everything else is copied (like `/data merge`).
- `public NbtCompound Put(string key, NbtTag tag)`: Adds or replaces an entry.
- `public NbtCompound PutBool(string key, bool value)`: Sets a byte entry (the game's boolean).
- `public NbtCompound PutByte(string key, sbyte value)`: Sets a byte entry.
- `public NbtCompound PutDouble(string key, double value)`: Sets a double entry.
- `public NbtCompound PutFloat(string key, float value)`: Sets a float entry.
- `public NbtCompound PutInt(string key, int value)`: Sets an int entry.
- `public NbtCompound PutLong(string key, long value)`: Sets a long entry.
- `public NbtCompound PutShort(string key, short value)`: Sets a short entry.
- `public NbtCompound PutString(string key, string value)`: Sets a string entry.
- `public bool Remove(string key)`: Removes an entry.
- `public bool TryGet(string key, out NbtTag tag)`: Gets an entry.

## NbtDouble (class, nbt)

```csharp
public sealed class NbtDouble : NbtValueTag
```

64 bit float.

- `public NbtDouble(double value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public double Value { get; set; }`: The value.
- `public override NbtTag Clone()`

## NbtFloat (class, nbt)

```csharp
public sealed class NbtFloat : NbtValueTag
```

32 bit float.

- `public NbtFloat(float value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public float Value { get; set; }`: The value.
- `public override NbtTag Clone()`

## NbtInt (class, nbt)

```csharp
public sealed class NbtInt : NbtValueTag
```

Signed 32 bit integer.

- `public NbtInt(int value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public int Value { get; set; }`: The value.
- `public override NbtTag Clone()`

## NbtIntArray (class, nbt)

```csharp
public sealed class NbtIntArray : NbtTag
```

An array of 32 bit integers.

- `public NbtIntArray(int[] value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public int[] Value { get; set; }`: The values (not copied).
- `public override NbtTag Clone()`

## NbtIo (class, nbt)

```csharp
public static class NbtIo
```

Binary NBT (the file form of the game: type byte, name, payload; big endian; strings in Java's modified UTF-8). Reading enforces a nesting depth of 512 like the game.

- `public const long DefaultQuota = 33554432;`: How much memory (in the accounting of the game's NbtAccounter: about 48 bytes per tag, plus strings and arrays) a read may build: 32 MiB. A few bytes of input can describe a very large tree (a list of a million empty compounds is one megabyte of data and a hundred megabytes of objects); the quota stops it.
- `public static NbtTag Read(ReadOnlySpan<byte> data)`: Parses a named root tag (the name is ignored).
- `public static NbtTag Read(ReadOnlySpan<byte> data, long quota)`: Parses a named root tag with an explicit memory quota.
- `public static byte[] Write(NbtTag tag)`: Serialises a tag as a named root with an empty name.

## NbtList (class, nbt)

```csharp
public sealed class NbtList : NbtTag, ICollection<NbtTag>, IEnumerable, IEnumerable<NbtTag>, IList<NbtTag>
```

A list of tags of one type.

- `public NbtList()`: Creates an empty list.
- `public NbtList(IEnumerable<NbtTag> items)`: Creates a list with items.
- `public int Count { get; }`
- `public NbtType ElementType { get; }`: The type of the items (End while the list is empty).
- `public bool IsReadOnly { get; }`
- `public override NbtType Type { get; }`
- `public NbtTag this[int index] { get; set; }`
- `public void Add(NbtTag item)`
- `public void Clear()`
- `public override NbtTag Clone()`
- `public bool Contains(NbtTag item)`
- `public void CopyTo(NbtTag[] array, int arrayIndex)`
- `public IEnumerator<NbtTag> GetEnumerator()`
- `public int IndexOf(NbtTag item)`
- `public void Insert(int index, NbtTag item)`
- `public bool Remove(NbtTag item)`
- `public void RemoveAt(int index)`

## NbtLong (class, nbt)

```csharp
public sealed class NbtLong : NbtValueTag
```

Signed 64 bit integer.

- `public NbtLong(long value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public long Value { get; set; }`: The value.
- `public override NbtTag Clone()`

## NbtLongArray (class, nbt)

```csharp
public sealed class NbtLongArray : NbtTag
```

An array of 64 bit integers.

- `public NbtLongArray(long[] value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public long[] Value { get; set; }`: The values (not copied).
- `public override NbtTag Clone()`

## NbtShort (class, nbt)

```csharp
public sealed class NbtShort : NbtValueTag
```

Signed 16 bit integer.

- `public NbtShort(short value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public short Value { get; set; }`: The value.
- `public override NbtTag Clone()`

## NbtString (class, nbt)

```csharp
public sealed class NbtString : NbtValueTag
```

A string.

- `public NbtString(string value)`: Creates the tag.
- `public override NbtType Type { get; }`
- `public string Value { get; set; }`: The value.
- `public override NbtTag Clone()`

## NbtTag (class, nbt)

```csharp
public abstract class NbtTag
```

An NBT value (the data format of block entities, entities, item components and level data). Tags are mutable managed objects: they are copies of the engine's data, so changing one changes nothing in the world until it is handed back (for example to `World.SetBlockEntityData`). Use `NbtTag.Clone` for a deep copy.

- `protected NbtTag()`
- `public abstract NbtType Type { get; }`: The type of this tag.
- `public abstract NbtTag Clone()`: A deep copy.
- `public byte[] ToBytes()`: Serialises the tag to the binary file form (named root with an empty name, big endian).
- `public override string ToString()`: The tag as stringified NBT (what `/data get` prints), compact, compound keys sorted.
- `public static bool DeepEquals(NbtTag? a, NbtTag? b)`: Whether two tags have the same type and contents (compounds compare by keys, not by order).
- `public static NbtTag FromBytes(ReadOnlySpan<byte> data)`: Parses the binary file form.
- `public static NbtTag Parse(string snbt)`: Parses stringified NBT (`{Health:20.0f,Tags:["a"]}`), with the grammar of the game (typed suffixes, quoted strings, `[B;1,2]` arrays ...).
- `public static NbtCompound ParseCompound(string snbt)`: Parses stringified NBT that must be a compound.
- `public static implicit operator NbtTag(sbyte v)`: An 8 bit integer tag.
- `public static implicit operator NbtTag(short v)`: A 16 bit integer tag.
- `public static implicit operator NbtTag(int v)`: A 32 bit integer tag.
- `public static implicit operator NbtTag(long v)`: A 64 bit integer tag.
- `public static implicit operator NbtTag(float v)`: A float tag.
- `public static implicit operator NbtTag(double v)`: A double tag.
- `public static implicit operator NbtTag(string v)`: A string tag.
- `public static implicit operator NbtTag(bool v)`: A byte tag holding 1 or 0 (the game stores booleans as bytes).

## NbtType (enum, nbt)

```csharp
public enum NbtType : byte
```

The type of an NBT tag (the id byte of the binary format).

- `End` = 0: End marker (never a value).
- `Byte` = 1: Signed 8 bit integer.
- `Short` = 2: Signed 16 bit integer.
- `Int` = 3: Signed 32 bit integer.
- `Long` = 4: Signed 64 bit integer.
- `Float` = 5: 32 bit float.
- `Double` = 6: 64 bit float.
- `ByteArray` = 7: Array of bytes.
- `String` = 8: UTF-8 string.
- `List` = 9: List of tags of one type.
- `Compound` = 10: Named tags.
- `IntArray` = 11: Array of 32 bit integers.
- `LongArray` = 12: Array of 64 bit integers.

## NbtValueTag (class, nbt)

```csharp
public abstract class NbtValueTag : NbtTag
```

A tag with a single numeric or text value.

- `protected NbtValueTag()`

## Snbt (class, nbt)

```csharp
public static class Snbt
```

Stringified NBT (the text form of `/data` and `/summon`), through the game's own parser and printer.

- `public static string Format(NbtTag tag)`: Formats a tag as SNBT.
- `public static NbtTag Parse(string text)`: Parses SNBT.

## Npc (class, visuals)

```csharp
public sealed class Npc : IDisposable, IPluginResource
```

A fake player that stands in the world: the players see it with a name, a skin, equipment and an arm that swings, and a click on it fires {{cref:M:Mc.Scripting.Npcs.OnInteract(System.Action{Mc.Scripting.NpcInteraction})}}. Nothing of it exists on the server (it has no physics and no AI). Create it with `Npcs.Create`; it disappears for every viewer when the plugin unloads or when `Npc.Remove` is called.

- `public ulong Handle { get; }`: The handle the engine gives a click on this NPC (see `NpcInteraction.Handle`).
- `public bool IsActive { get; }`: Whether the NPC still exists.
- `public string Name { get; }`: The profile name above its head.
- `public float Pitch { get; }`: The vertical look, degrees.
- `public Vec3d Position { get; }`: Where it stands.
- `public float Yaw { get; }`: The horizontal look, degrees.
- `public void Dispose()`
- `public Npc HideFrom(Player player)`: Hides the NPC from one player (while it is shown to everybody).
- `public Npc HideFromAll()`: Hides the NPC from everybody.
- `public Npc Look(float yaw, float pitch)`: Turns the NPC where it stands.
- `public Npc Move(Vec3d position, float yaw, float pitch)`: Moves the NPC (within its world) and turns it.
- `public void Remove()`: Removes the NPC from every viewer (and from the tab list) and deletes it.
- `public Npc SetEquipment(NpcSlot slot, ItemStack item)`: Sets what a slot of the NPC holds or wears (an empty stack clears it).
- `public Npc SetPose(NpcPose pose)`: Changes the pose (standing or sneaking).
- `public Npc ShowTo(Player player)`: Shows the NPC to one player (after `Npc.HideFromAll`), or hides it from one player while it is shown to everybody.
- `public Npc ShowToAll()`: Shows the NPC to everybody again.
- `public Npc Swing(bool offhand = false)`: Swings an arm.

## NpcClick (enum, visuals)

```csharp
public enum NpcClick : byte
```

What a player did to an NPC.

- `Attack` = 0: A left click (an attack).
- `Interact` = 1: A right click (an interaction).

## NpcInteraction (struct, visuals)

```csharp
public readonly struct NpcInteraction
```

A click on an NPC ({{cref:M:Mc.Scripting.Npcs.OnInteract(System.Action{Mc.Scripting.NpcInteraction})}}).

- `public NpcClick Action { get; }`: A left click (an attack) or a right click (an interaction).
- `public Hand Hand { get; }`: The hand of the click (a left click is the main hand).
- `public ulong Handle { get; }`: The handle of the NPC (the engine's; `NpcInteraction.Npc` of it is `NpcInteraction.Npc`.Handle).
- `public Npc? Npc { get; }`: The NPC, if it still exists.
- `public Player Player { get; }`: The player who clicked.
- `public bool Sneaking { get; }`: Whether the player was sneaking.

## NpcPose (enum, visuals)

```csharp
public enum NpcPose : int
```

The pose of an NPC.

- `Standing` = 0: Standing.
- `Crouching` = 5: Sneaking.

## NpcSkin (class, visuals)

```csharp
public sealed class NpcSkin
```

The skin of an NPC: a texture (the value and signature of a "textures" profile property), or the skin of a player whose profile id is known.

- `public Uuid CopyOf { get; }`: The profile id whose skin is copied (default when the texture is given).
- `public string? Signature { get; }`: The texture signature, when there is one.
- `public string Value { get; }`: The texture value (base64), empty when the skin is copied.
- `public static NpcSkin FromTexture(string value, string? signature = null)`: A skin from a texture property (at most 8192 bytes of value and 1024 of signature).
- `public static NpcSkin OfPlayer(Player player)`: The skin of a player.
- `public static NpcSkin OfProfile(Uuid profile)`: The skin of a profile id (an online player's skin is copied; an offline one gives the default skin).

## NpcSlot (enum, visuals)

```csharp
public enum NpcSlot : int
```

The equipment slots of an NPC.

- `MainHand` = 0: The main hand.
- `OffHand` = 1: The off hand.
- `Feet` = 2: The boots.
- `Legs` = 3: The leggings.
- `Chest` = 4: The chestplate.
- `Head` = 5: The helmet.

## Npcs (class, visuals)

```csharp
public sealed class Npcs
```

Creates NPCs for this plugin and listens to the clicks on them. Everything made here is removed when the plugin unloads.

- `public Npc Create(World world, Vec3d position, string name, NpcSkin? skin = null, bool showInTab = false, float yaw = 0f, float pitch = 0f)`: Creates an NPC that everybody sees.
- `public Subscription OnInteract(Action<NpcInteraction> handler)`: Calls `handler` when a player clicks an NPC of this plugin's server (any NPC: check `NpcInteraction.Npc`).

## Objective (class, display)

```csharp
public sealed class Objective
```

An objective of the server scoreboard (the one `/scoreboard` edits and the world saves): scores per holder name, shown in a display slot. For content that differs per player use `Sidebar` instead.

- `public string Name { get; }`: The objective's name (what commands use).
- `public int AddScore(string holder, int amount)`: Adds to the score of a holder (a missing score counts as 0).
- `public int? GetScore(string holder)`: The score of a holder (a player name, or any text for a fake entry), null when none is set.
- `public bool ResetScore(string holder)`: Removes the score of a holder.
- `public void SetScore(string holder, int value)`: Sets the score of a holder.
- `public void SetTitle(Component title)`: Sets the title shown above the sidebar.

## OfflinePlayers (class, players)

```csharp
public static class OfflinePlayers
```

Players who are not online, looked up by name or uuid: the server's name cache (usercache.json, vanilla's), the first join and last seen times, and the saved player data. Reads only; a plugin that changes an offline player's data does it through the owner's own store. Thread: tick thread (the reads go to the engine).

- `public static Uuid? Find(string name)`: The uuid of a player name (case-insensitive) that the server remembers; null when it does not know the name.
- `public static OfflineProfile? Get(Uuid uuid)`: The profile of a uuid the server knows, online or not (online players have their newer state in the player's own handle).

## OfflineProfile (class, players)

```csharp
public sealed class OfflineProfile : IEquatable<OfflineProfile>
```

What the server knows about a player who is not online: the name, the first join and the last time seen (both from the server's records; null when unknown), and the position in the saved player data (null when there is no saved data).

- `public OfflineProfile(Uuid Id, string Name, DateTimeOffset? FirstJoin, DateTimeOffset? LastSeen, string? Dimension, Vec3d? Position, float Yaw, float Pitch)`: What the server knows about a player who is not online: the name, the first join and the last time seen (both from the server's records; null when unknown), and the position in the saved player data (null when there is no saved data).
- `public string? Dimension { get; init; }`: The dimension key ("minecraft:overworld") the player was saved in; null without saved data.
- `public DateTimeOffset? FirstJoin { get; init; }`: When the player first joined (null when unknown).
- `public Uuid Id { get; init; }`: The uuid.
- `public DateTimeOffset? LastSeen { get; init; }`: When the player was last saved or left (null when unknown).
- `public string Name { get; init; }`: The newest name the server has for the uuid.
- `public float Pitch { get; init; }`: The saved pitch; 0 without saved data.
- `public Vec3d? Position { get; init; }`: The saved feet position; null without saved data.
- `public float Yaw { get; init; }`: The saved yaw; 0 without saved data.

## PacketBase (class, packets)

```csharp
public abstract class PacketBase
```

Base of every typed packet (the generated classes derive from it). The wire form is the packet's fields in declaration order; the engine decodes and validates them. Plugins read and write the public fields.

- `protected PacketBase()`
- `public abstract int PacketIndex { get; }`: The packet's index in the engine's catalogue (its class has the constant Index).

## PacketEvent<T> (class, packets)

```csharp
public sealed class PacketEvent<T> where T : PacketBase
```

One packet handed to a handler. In intercept mode the handler may change `PacketEvent.Packet` and call `PacketEvent.Cancel`.

- `public bool Cancelled { get; }`: True once a handler cancelled the packet.
- `public T Packet { get; }`: The packet. Changing its fields changes what goes on (the engine validates the new values like any other).
- `public Player Player { get; }`: The player the packet is from (serverbound) or to (clientbound).
- `public void Cancel()`: Stops the packet: a serverbound packet's handler does not run, a clientbound packet is not sent. Only in intercept mode; an observer's cancel is ignored.

## PacketMode (enum, packets)

```csharp
public enum PacketMode : int
```

How a packet handler is called (see `Packets.On`,Mc.Scripting.PacketMode,Mc.Scripting.EventPriority)}}).

- `Observe` = 0: A copy of the packet after the fact: the handler cannot change or cancel it, and nothing waits for it.
- `Intercept` = 1: The handler runs before the packet goes on and may cancel it or change its fields. The engine waits for the answer within a small budget; when none comes in time the packet goes on unchanged.

## PacketSendStatus (enum, packets)

```csharp
public enum PacketSendStatus : int
```

What `Packets.Send` did with the packet.

- `Sent` = 0: The packet is in the player's stream, in order with the engine's packets of the tick.
- `Refused` = 1: The packet was refused: a value out of range, an unknown registry id, an entity id this plugin may not name, or a packet the engine does not send to players.
- `NotInPlay` = 2: The player is not in play (nothing was sent).
- `OverBudget` = 3: The player's (or the server's) byte budget for plugin packets of this tick is spent (nothing was sent).
- `NotAvailable` = 4: The engine does not carry packets here (the in-process runtime, a test fixture): nothing was sent.

## Packets (class, packets)

```csharp
public static class Packets
```

Typed packets: send a packet to a player, and observe, change or cancel the packets players send and the ones sent to them. The engine encodes every packet with its own codecs, so nothing malformed reaches a client.

- `public static int AllocateEntityId()`: A new entity id for a fake entity (an AddEntity of this plugin). The ids are reserved for plugins, so they never collide with a real entity. Release it with `Packets.ReleaseEntityId` when the client should forget it. 0 when none is available.
- `public static IDisposable On<T>(Action<PacketEvent<T>> handler, PacketMode mode = PacketMode.Observe, EventPriority priority = EventPriority.Normal) where T : PacketBase, new()`: Subscribes to one packet type. In `PacketMode.Observe` the handler sees a copy of each packet; in `PacketMode.Intercept` it may cancel the packet or change its fields. Handlers run in priority order (lowest first). Returns the subscription: dispose it to unsubscribe (also done when the plugin unloads).
- `public static bool ReleaseEntityId(int id)`: Gives back an id from `Packets.AllocateEntityId`. False when this plugin does not hold it.
- `public static PacketSendStatus Send(Player player, PacketBase packet)`: Sends a packet to a player. The packet is encoded by the engine, after the engine checks its values (ranges, registry ids, entity ids: an entity a plugin creates needs `Packets.AllocateEntityId`). It is written in order with the engine's own packets of the tick.
- `public static PacketSendStatus SendPacket(Player player, PacketBase packet)`: Shorthand for `Packets.Send`: `player.SendPacket(new Packets.Clientbound.AddEntity { ... })`.

## Particle (class, players)

```csharp
public static class Particle
```

Helpers for the textual particle descriptions `Player.SpawnParticle` takes (the same syntax as the `/particle` command).

- `public static string Block(BlockState state)`: Block breaking particles of a block state.
- `public static string Dust(float red, float green, float blue, float scale = 1f)`: A coloured dust particle.
- `public static string Of(string key)`: A particle by its registry key (`minecraft:flame`, `happy_villager`, ...).

## FilePermissionProvider (class, permissions)

```csharp
public sealed class FilePermissionProvider : IPermissionProvider
```

A file-based backend: groups (weighted, inheriting, with prefix and suffix) and per-player overrides, stored in a JSON file. The built-in backend (`PermissionManager.BuiltIn`) is `permissions.json` in the server directory; a provider created with another path reads and writes only that file. Resolution order: the player's own entries, then his groups by weight (highest first, parents after their child), then the implicit "default" group everybody belongs to. Within one layer the most specific node wins: an exact entry beats `a.b.*`, a longer wildcard beats a shorter one, `*` comes last; a `false` value is a denial. A key written as `"-node"` in the file means the same as `"node": false`.

- `public FilePermissionProvider(string path)`: Opens (and creates, when missing) a permission file at `path`. A file that cannot be read (malformed JSON) does not stop the server: the provider starts empty (the registered defaults apply), logs the problem once and leaves the file alone so that it can be fixed and reloaded with `FilePermissionProvider.Reload`. The file is written by this provider itself, so it works for any path the plugin can write; the server's own permissions.json is `PermissionManager.BuiltIn`.
- `public IReadOnlyList<string> GroupNames { get; }`: The group names.
- `public string? LoadError { get; }`: Why the file could not be read (null when it was, or since it was fixed and reloaded).
- `public string Name { get; }`
- `public void AddToGroup(Uuid player, string group, string? name = null)`: Adds the player to a group.
- `public void CreateGroup(string name, int weight = 0)`: Creates a group (no-op when it exists).
- `public bool DeleteGroup(string name)`: Deletes a group and removes it from every player and parent list.
- `public PermissionValue Get(Uuid player, string node)`
- `public string GetPrefix(Uuid player)`: The prefix of the player's highest-weight group that has one (empty if none).
- `public string GetSuffix(Uuid player)`: The suffix of the player's highest-weight group that has one (empty if none).
- `public IReadOnlyDictionary<string, bool> GroupEntries(string group)`: The explicit entries of a group, for admin tools.
- `public IReadOnlyCollection<string> Groups(Uuid player)`
- `public IReadOnlyDictionary<string, bool> PlayerEntries(Uuid player)`: The explicit entries of a player (without groups), for admin tools.
- `public void Reload()`: Re-reads the file (a missing file is created with a "default" and an "admin" group).
- `public bool RemoveFromGroup(Uuid player, string group)`: Removes the player from a group.
- `public void Save()`: Writes the file if something changed, atomically (temp file, flushed, then renamed over the old one). The built-in backend's file is written by the engine (a host call: the worker's sandbox refuses writes to the server folder), and the engine refuses a write that comes too soon after the last one (its rate limit): then the changes stay pending and the next call (the runtime's flush, a second later) tries again. A provider on another path writes that file itself and never touches permissions.json. Never throws; a refusal or a failed write is logged once per streak and retried.
- `public void SetGroupMeta(string group, string prefix, string suffix)`: Sets prefix and suffix text of a group (metadata for chat plugins).
- `public void SetGroupParents(string group, IEnumerable<string> parents)`: Sets the groups a group inherits from.
- `public void SetGroupPermission(string group, string node, bool? value)`: Sets (true/false) or removes (null) a permission entry of a group.
- `public void SetPlayerPermission(Uuid player, string node, bool? value, string? name = null)`: Sets (true/false) or removes (null) a permission entry of a player.
- `public event Action<Uuid?>? Changed;`

## IPermissionProvider (interface, permissions)

```csharp
public interface IPermissionProvider
```

A permission backend: answers "does this player have this node?" with an explicit value or no opinion. Implement it to replace the built-in file-based backend (for example with a database or a LuckPerms-style system) and install it with `PermissionManager.SetProvider`.

- `string Name { get; }`: A short name for logs and admin tools.
- `PermissionValue Get(Uuid player, string node)`: The explicit value of `node` for the player: wildcards (`a.b.*`, `*`) and groups are the provider's business. Return `PermissionValue.Unset` when nothing matches, so the node's registered default decides.
- `IReadOnlyCollection<string> Groups(Uuid player)`: The names of the groups the player belongs to (for display and admin commands).
- `public event Action<Uuid?>? Changed;`: Raised when permissions changed (the player's id, or null when a group or everything changed): the server refreshes the command trees.

## NativePermissionTable (class, permissions)

```csharp
public static class NativePermissionTable
```

Feeds the engine's permission table: declares node defaults, sets and changes the rules of a player, claims the table, and reads its statistics. The engine validates every message as a whole; a refused message changes nothing (the status is the host call's: 0 applied, -1 malformed, -2 oversize, -3 refused, -4 version gap, -5 unknown kind). While a claim is held the engine answers the permission requirements of commands from the table; the rules describe a player's permissions, and a player whose rules were never sent gets the defaults.

- `public const byte Any = 255;`: The wildcard of a context position.
- `public const int MaxMessageBytes = 262144;`: The largest message the engine accepts.
- `public const int MaxRulesPerMessage = 512;`: Rules in one message (a larger subject is sent with delta messages).
- `public static int Apply(byte[] message)`: Applies a message built by one of the functions below.
- `public static byte[] BeginSnapshot()`: Starts a full resynchronisation (after a worker start): every subject sent before `NativePermissionTable.EndSnapshot` is kept, the others are dropped at the end. Until then the last state keeps answering.
- `public static bool Claim(bool on)`: Claims (true) or releases (false) the table. Releasing returns command requirements to the built-in backend.
- `public static bool Declare(Permission permission)`: Declares a node's defaults for its five operator levels (what the node answers for a subject without a rule). Called by the server for every registered permission node; a plugin does not need it for nodes it registers through `PermissionManager.Register`.
- `public static byte[] DeltaSubject(Uuid subject, uint baseVersion, uint version, IReadOnlyList<ValueTuple<bool, NativeRule>> changes)`: Upserts or removes rules of a subject whose version is `baseVersion`; the engine refuses a delta on any other version (-4) and then the subject needs {{cref:M:Mc.Scripting.Permissions.NativePermissionTable.SetSubject(Mc.Scripting.Uuid,System.UInt32,System.Byte,System.Byte,System.Collections.Generic.IReadOnlyList{Mc.Scripting.Permissions.NativeRule})}}.
- `public static byte[] DropSubject(Uuid subject)`: Drops a subject's state (a player who left; the engine frees it).
- `public static byte[] EndSnapshot()`: Ends a full resynchronisation (see `NativePermissionTable.BeginSnapshot`).
- `public static byte[] SetSlots(Uuid subject, byte slot0, byte slot1)`: Sets a subject's plugin context slot values without touching its rules.
- `public static byte[] SetSubject(Uuid subject, uint version, byte slot0, byte slot1, IReadOnlyList<NativeRule> rules)`: Replaces every rule of a subject (at most `NativePermissionTable.MaxRulesPerMessage` of them).
- `public static NativeTableStats? Stats()`: The table's counters (the engine's `perm_stats`).
- `public static bool WriteBuiltin(string json)`: Writes the built-in backend's file (permissions.json) through the engine: the worker's own sandbox refuses the write.

## NativeRule (struct, permissions)

```csharp
public readonly struct NativeRule : IEquatable<NativeRule>
```

A rule of the engine's permission table: a node or pattern over a context pattern, with a layer and a value.

- `public NativeRule(string Node, bool Allow, ushort Layer = 0, byte[]? Context = null)`: A rule of the engine's permission table: a node or pattern over a context pattern, with a layer and a value.
- `public bool Allow { get; init; }`: True grants the node, false denies it.
- `public byte[]? Context { get; init; }`: The six context positions (world, dimension, gamemode, server, slot 0, slot 1); `NativePermissionTable.Any` matches every value. Null means any in every position.
- `public ushort Layer { get; init; }`: Lower layers win: 0 the player's own rules, then the groups by weight (highest first), then the default group.
- `public string Node { get; init; }`: The node or a pattern: "homes.use", "homes.*", "*".

## NativeTableStats (struct, permissions)

```csharp
public readonly struct NativeTableStats : IEquatable<NativeTableStats>
```

The engine's permission table counters.

- `public NativeTableStats(ulong Subjects, ulong Rules, ulong Nodes, ulong Applied, ulong Rejected, ulong Generation, int LastStatus, bool Claimed)`: The engine's permission table counters.
- `public ulong Applied { get; init; }`: Messages applied.
- `public bool Claimed { get; init; }`: Whether the table is claimed.
- `public ulong Generation { get; init; }`: Snapshot generations begun.
- `public int LastStatus { get; init; }`: Status of the last message.
- `public ulong Nodes { get; init; }`: Interned names.
- `public ulong Rejected { get; init; }`: Messages refused.
- `public ulong Rules { get; init; }`: Rules over all subjects.
- `public ulong Subjects { get; init; }`: Subjects with state.

## Permission (class, permissions)

```csharp
public sealed class Permission : IEquatable<Permission>
```

A declared permission node: its name, default and description.

- `public Permission(string Node, PermissionDefault Default, string Description = "")`: A declared permission node: its name, default and description.
- `public PermissionDefault Default { get; init; }`: The value when nothing else decides.
- `public string Description { get; init; }`: What the permission allows (shown by admin tools).
- `public string Node { get; init; }`: The dotted node, e.g. "homes.use".

## PermissionDefault (struct, permissions)

```csharp
public readonly struct PermissionDefault : IEquatable<PermissionDefault>
```

What a permission node is worth when no group or player entry decides it.

- `public static PermissionDefault False { get; }`: Nobody has it (until granted).
- `public static PermissionDefault NotOp { get; }`: Everybody who is not an operator (level below 2).
- `public static PermissionDefault Op { get; }`: Operators of vanilla permission level 2 and up (the "gamemaster" level: command blocks and most admin commands).
- `public static PermissionDefault True { get; }`: Everybody has it.
- `public bool Evaluate(int permissionLevel)`: Evaluates the default for a player of the given vanilla permission level.
- `public override string ToString()`: The name used in `plugin.json` and `permissions.json`: "true", "false", "op", "op:3", "not_op".
- `public static PermissionDefault OpLevel(int level)`: Operators of at least vanilla level `level` (1 = bypass spawn protection, 2 = gamemaster, 3 = admin, 4 = owner).
- `public static bool TryParse(string? text, out PermissionDefault value)`: Parses the text form (see `PermissionDefault.ToString`).

## PermissionManager (class, permissions)

```csharp
public static class PermissionManager
```

The server's permission system: declared nodes with defaults, the active backend (`IPermissionProvider`) and the question "does this player have this node?". The default backend is a `FilePermissionProvider` on `permissions.json`; a plugin may replace it.

Resolution: the provider's explicit value (wildcards, groups and player overrides are its business); if it has no opinion, the node's registered default decides (`PermissionDefault.Op` and friends are evaluated against the player's vanilla operator level); an unregistered node without an entry is denied, except `minecraft.command.<name>` which defaults to the vanilla operator level of that command. The console always has every permission. Everything here runs on the tick thread.

- `public static FilePermissionProvider BuiltIn { get; }`: The built-in file backend (created on first use at `permissions.json` in the server directory).
- `public static IPermissionProvider Provider { get; }`: The active backend.
- `public static IReadOnlyList<Permission> Registered { get; }`: A snapshot of the declared nodes.
- `public static PermissionValue GetExplicit(Uuid player, string node)`: The tri-state value the backend holds for the player, without applying defaults.
- `public static bool Has(Player player, string node)`: Whether the player has the permission.
- `public static bool Has(Uuid player, string node)`: Whether the player (online or not) has the permission. The operator level is looked up in the server's ops list.
- `public static bool Has(Entity entity, string node)`: Whether the entity has the permission (`Entity.Null` is the console and has everything).
- `public static bool IsRegistered(string node)`: Whether a node was declared.
- `public static void Register(Permission permission)`: Declares a permission node (the plugin-scoped `plugin.Permissions.Register` unregisters it on unload).
- `public static void Reload()`: Reloads the built-in backend's file and fires `PermissionsReloaded`.
- `public static void SetProvider(IPermissionProvider provider)`: Replaces the backend. The previous one is restored when the owning plugin unloads (use the plugin-scoped overload from plugins).

## PermissionValue (enum, permissions)

```csharp
public enum PermissionValue : int
```

The three-valued answer of a permission provider.

- `Unset` = 0: The provider has no opinion; the node's default decides.
- `True` = 1: Explicitly granted.
- `False` = 2: Explicitly denied (beats a grant of the same specificity).

## PluginPermissions (class, permissions)

```csharp
public sealed class PluginPermissions
```

Permissions as seen by one plugin (`plugin.Permissions`): declare the nodes it uses, query players, and optionally replace the backend. Everything declared here is removed when the plugin unloads or hot-reloads.

- `public FilePermissionProvider BuiltIn { get; }`: The built-in file backend (groups and player overrides stored in `permissions.json`).
- `public bool Has(Player player, string node)`: Whether the player has the permission.
- `public bool Has(Uuid player, string node)`: Whether the player (online or not) has the permission.
- `public void Register(Permission permission)`: Declares a permission node.
- `public void Register(string node, PermissionDefault default, string description = "")`: Declares a permission node with its default and a description.
- `public void SetProvider(IPermissionProvider provider)`: Replaces the permission backend for as long as this plugin is loaded (a LuckPerms-style plugin does this in `OnEnable`).

## Phase (enum, systems)

```csharp
public enum Phase : int
```

Server tick phases in execution order (the engine's phases). Deferred changes (entity creation, component add/set/remove) made in one phase are visible to every later phase of the same tick, e.g. an entity a native NetworkReceive system creates is seen by Tick systems.

- `NetworkReceive` = 0: Drain inbound packets, apply them to entities.
- `PreTick` = 1: Timers, scheduled tasks, input integration.
- `Tick` = 2: Game logic (entities, blocks, ...).
- `PostTick` = 3: Removals, collision resolution, derived state.
- `Tracking` = 4: Spatial index update, view diffs.
- `NetworkSend` = 5: Build and encode outbound packets.
- `AsyncIntegration` = 6: Integrate worker results (chunk loads, lighting).

## Player (struct, players)

```csharp
public readonly struct Player : IEquatable<Player>
```

A handle to a connected player. Blittable (usable in events and ECS components).

A handle stays valid only while the player is online: after he leaves it goes stale. Stale handles fail safe: reads return defaults, actions do nothing (see `Player.IsOnline`). A player who rejoins gets a new handle. All members must be called on the tick thread.

- `public Player(Entity entity)`: Wraps an entity that is a player.
- `public readonly Entity Entity;`: The underlying ECS entity.
- `public double Absorption { get; set; }`: The golden hearts of absorption in half hearts. Setting it is not clamped to the `minecraft:max_absorption` attribute (Paper semantics).
- `public IReadOnlyList<string> ActiveEffects { get; }`: The keys of the effects the player has.
- `public string Address { get; }`: The client's address as the server sees it ("127.0.0.1:50321"), empty for a stale handle.
- `public int Air { get; set; }`: The remaining air in ticks (300 is full; below 0 the player drowns).
- `public bool AllowFlight { get; set; }`: Whether the player may fly (creative mode sets it automatically).
- `public IReadOnlyList<string> Channels { get; }`: The channels the player's client registered (minecraft:register), in the order it sent them. Empty for a stale handle.
- `public Component? DisplayName { get; set; }`: The name the server shows for this player where vanilla uses Player.getDisplayName: the join and leave lines, the chat sender, death and advancement messages (the team format applies to it). Null when none is set, and then the profile `Player.Name` is shown. Set it to null to clear it. Not persisted: it is cleared when the player leaves, so a plugin that keeps nicknames sets it again on join. The profile name stays the key of commands, the scoreboard and the click and insertion text of chat. Stale handles: reads give null, writes do nothing.
- `public double Exhaustion { get; set; }`: The exhaustion, 0..40: at 4 the saturation (or food level) drops by one.
- `public int ExperienceLevel { get; set; }`: The experience level shown in the HUD. Setting resets the progress within the level.
- `public double ExperienceProgress { get; set; }`: The progress to the next level, 0..1 (the green bar). Stored exactly as a float: no rounding to whole points.
- `public int FireTicks { get; set; }`: The ticks the player will keep burning (0 when not on fire). Setting 0 extinguishes.
- `public float FlySpeed { get; set; }`: The flying speed, 0..1 (default 0.05).
- `public int FoodLevel { get; set; }`: The food level, 0..20 (the hunger bar; below 7 sprinting stops, at 0 the player starves).
- `public GameMode GameMode { get; set; }`: The player's game mode. Setting it fires `PlayerGameModeChangeEvent` (a handler may cancel it).
- `public double Health { get; set; }`: Health in half hearts (20 is full). Setting is clamped to 0..`Player.MaxHealth`; 0 kills the player through the normal death handling.
- `public ItemStack HeldItem { get; }`: The stack in the player's main hand (empty for a stale handle).
- `public PlayerInventory Inventory { get; }`: The player's inventory: slots, hotbar, armor, off hand and the cursor (`PlayerInventory`). A stale handle (a player who left) gives an inventory with no slots.
- `public bool Invulnerable { get; set; }`: Whether the player takes no damage (creative mode sets it automatically).
- `public bool IsFlying { get; set; }`: Whether the player is flying (creative or spectator flight, or `Player.AllowFlight`). Setting it only works while `Player.AllowFlight` is true.
- `public bool IsGliding { get; }`: Whether the player is gliding with elytra.
- `public bool IsNone { get; }`: Whether this is the "no player" handle.
- `public bool IsOnGround { get; }`: Whether the player is on the ground.
- `public bool IsOnline { get; }`: Whether the player is connected and in the play state.
- `public bool IsSneaking { get; }`: Whether the player is sneaking (crouching).
- `public bool IsSprinting { get; }`: Whether the player is sprinting.
- `public bool IsSwimming { get; }`: Whether the player is swimming.
- `public int Latency { get; }`: The round trip time to the client in milliseconds (-1 for a stale handle).
- `public string Locale { get; }`: The client's language, e.g. "en_us".
- `public Location Location { get; }`: Position and rotation in the player's world.
- `public double MaxHealth { get; }`: The maximum health (the attribute value, 20 by default). Read only: change it with attribute modifiers.
- `public string Name { get; }`: The player's name, empty for a stale handle.
- `public int PermissionLevel { get; }`: The vanilla operator level of the player (0-4, from ops.json); 0 for an offline handle.
- `public Vec3d Position { get; }`: The player's position (feet), `default` for a stale handle.
- `public PvpFlags? PvpPolicyFlags { get; }`: The PvP flags last written with `Player.SetPvpPolicy`, or null when the player has no policy (or is not online).
- `public double Saturation { get; set; }`: The saturation, 0..`Player.FoodLevel`: the buffer that is used up before the food level drops.
- `public int TotalExperience { get; }`: The total experience points ever collected (drives the score on the death screen). Read only; use `Player.GiveExperience`.
- `public Uuid Uuid { get; }`: The player's profile id, `default` for a stale handle.
- `public float WalkSpeed { get; set; }`: The walking speed, 0..1 (default 0.1).
- `public World World { get; }`: The dimension the player is in (the overworld for a stale handle).
- `public static Player None { get; }`: The "no player" handle.
- `public bool AddEffect(PotionEffect effect)`: Adds a status effect (a stronger or longer effect of the same kind replaces the current one like in vanilla).
- `public LivingEntity AsLiving()`: The player as a living entity: health, absorption, potion effects, attributes, damage and healing. Every player is living; a stale player gives a handle whose members read defaults.
- `public bool ClearEffects()`: Removes every status effect.
- `public bool ClearFakeBlock(BlockPos position)`: Shows the world's real block at a position again.
- `public void ClearFakeBlocks()`: Ends every fake block of this player.
- `public void ClearTitle(bool resetTimes = false)`: Hides the current title and subtitle.
- `public bool Equals(Player other)`
- `public override bool Equals(object? obj)`
- `public ref T Get<T>() where T : unmanaged`: Reference to a plugin component on the player's entity; throws if the player lacks it. The reference points into ECS storage and dies with the next structural change: re-fetch instead of keeping it.
- `public PotionEffect? GetEffect(string effect)`: The active effect of a kind, null when the player does not have it.
- `public override int GetHashCode()`
- `public void GiveExperience(int points)`: Gives experience points (negative takes them away), levelling up as needed, like `/xp add`.
- `public void GiveExperienceLevels(int levels)`: Gives whole experience levels (negative takes them away).
- `public bool Has<T>() where T : unmanaged`: Whether the player has a plugin component.
- `public bool HasPermission(string node)`: Whether the player has a permission node (groups, per-player entries, wildcards and node defaults are resolved by the permission backend).
- `public void Kick(Component reason)`: Disconnects the player. Fires `PlayerKickEvent` first (a handler may cancel it).
- `public bool ListensOn(string channel)`: Whether the player's client registered the channel (minecraft:register), that is, whether it listens on it. False for a stale handle.
- `public void PlaySound(string sound, SoundCategory category = SoundCategory.Master, float volume = 1f, float pitch = 1f)`: Plays a sound to this player only, at his position.
- `public void PlaySound(string sound, Vec3d position, SoundCategory category = SoundCategory.Master, float volume = 1f, float pitch = 1f)`: Plays a sound to this player only, coming from a position.
- `public Player Remove<T>() where T : unmanaged`: Removes a plugin component from the player.
- `public void RemoveAllResourcePacks()`: Removes every resource pack the server pushed.
- `public bool RemoveEffect(string effect)`: Removes a status effect.
- `public void RemoveResourcePack(Uuid id)`: Removes a resource pack from the client.
- `public void SendActionBar(Component message)`: Shows a message in the action bar above the hotbar. No-op for a stale handle.
- `public void SendMessage(Component message)`: Sends a chat (system) message to the player. No-op for a stale handle.
- `public bool SendPluginMessage(string channel, ReadOnlySpan<byte> data)`: Sends a plugin message to the player on a channel (a custom payload of the play state). Vanilla clients ignore channels they did not register; clients with a mod or a proxy that listens on it receive the bytes.
- `public void SendResourcePack(ResourcePack pack)`: Asks the client to download and apply a resource pack. The answer arrives as `PlayerResourcePackStatusEvent` events.
- `public Player Set<T>(in T value) where T : unmanaged`: Adds or overwrites a plugin component on the player. The component disappears when the player leaves.
- `public bool SetGlowingFor(Player viewer, bool glowing)`: Makes this player glow for one viewer: the viewer sees the outline (for him only, the others do not change). See `Entity.SetGlowingFor`.
- `public void SetListName(Component? name)`: Changes the name everybody sees for this player in the tab list (colours, prefixes). Null restores the profile name. The name is kept until the player leaves.
- `public void SetPvpPolicy(PvpFlags flags, int tagTicks)`: Writes the PvP policy of this player: the native gate decides every hit on the player from it. Refresh it when the state changes (toggle, newbie or respawn protection, world). `tagTicks` is the combat tag an allowed hit gives both sides (0: none). A player without a policy keeps the vanilla rules. Stale handles, flags this server does not know and a tag time outside 0 .. 1728000 ticks (one day) do nothing.
- `public void SetTabList(Component? header, Component? footer)`: Sets the text above and below the player list (Tab). Null clears a part.
- `public bool ShowBlock(BlockPos position, BlockState state)`: Shows a block state at a position to this player only. The world keeps its real block; the server still collides with the real one, and the client predicts collisions against what it sees. The fake block is re-sent with its chunk and after real changes of its position, and dropped when the player changes world or leaves.
- `public void ShowSubtitle(Component subtitle)`: Shows only a subtitle (it appears together with the next title; vanilla shows nothing for a subtitle without a title).
- `public void ShowTitle(Component title, Component? subtitle = null, TitleTimes? times = null)`: Shows a title (and optionally a subtitle) in the middle of the screen. No-op for a stale handle.
- `public bool SpawnParticle(string particle, Vec3d position, int count = 1, Vec3d offset = default, float speed = 0f, bool force = false)`: Shows a particle effect to this player only.
- `public void StopSound(string? sound = null, SoundCategory? category = null)`: Stops sounds this player is hearing.
- `public bool Teleport(Location location)`: Teleports the player to a location, also into another dimension. Fires `PlayerTeleportEvent` (cause `TeleportCause.Plugin`); a handler may change the destination or cancel it.
- `public bool Teleport(Vec3d position)`: Teleports the player within his dimension, keeping his rotation.
- `public override string ToString()`
- `public void Transfer(string host, int port = 25565)`: Sends the client to another server (it must accept transfers: `accepts-transfers=true` there).
- `public bool TryGet<T>(out T value) where T : unmanaged`: Copies a plugin component of the player if present.
- `public static bool operator !=(Player a, Player b)`: Inequality.
- `public static bool operator ==(Player a, Player b)`: Equality (same entity id).
- `public static implicit operator Entity(Player p)`: A player is the entity it wraps.

## PlayerAction (enum, events)

```csharp
public enum PlayerAction : int
```

Values of the protocol enum PlayerAction (the wire carries the ordinal).

- `StartDestroyBlock` = 0: PlayerAction start_destroy_block (ordinal 0).
- `ChangeDestroyDirection` = 1: PlayerAction change_destroy_direction (ordinal 1).
- `AbortDestroyBlock` = 2: PlayerAction abort_destroy_block (ordinal 2).
- `StopDestroyBlock` = 3: PlayerAction stop_destroy_block (ordinal 3).
- `DropAllItems` = 4: PlayerAction drop_all_items (ordinal 4).
- `DropItem` = 5: PlayerAction drop_item (ordinal 5).
- `ReleaseUseItem` = 6: PlayerAction release_use_item (ordinal 6).
- `SwapItemWithOffhand` = 7: PlayerAction swap_item_with_offhand (ordinal 7).
- `Stab` = 8: PlayerAction stab (ordinal 8).

## PlayerSnapshot (struct, players)

```csharp
public readonly struct PlayerSnapshot : IEquatable<PlayerSnapshot>
```

One online player as the engine reports him (see `Server.PlayersSnapshot`).

- `public PlayerSnapshot(ulong EntityId, Guid Uuid, int World, int GameMode, int Ping, bool Listed, bool AllowsListing, bool HasDisplayName, string Name)`: One online player as the engine reports him (see `Server.PlayersSnapshot`).
- `public bool AllowsListing { get; init; }`: Whether he allows being listed (the server list setting of his client).
- `public ulong EntityId { get; init; }`: The entity handle; `PlayerSnapshot.Player` wraps it.
- `public int GameMode { get; init; }`: The game mode, 0 survival, 1 creative, 2 adventure, 3 spectator.
- `public bool HasDisplayName { get; init; }`: Whether he has a display name in the tab list.
- `public bool Listed { get; init; }`: Whether he is listed in the tab list.
- `public string Name { get; init; }`: His name.
- `public int Ping { get; init; }`: The latency in milliseconds.
- `public Player Player { get; }`: The player handle (valid while he is online).
- `public Guid Uuid { get; init; }`: The profile id.
- `public int World { get; init; }`: The index of his dimension, -1 when unknown (see `Server.Worlds`).

## Plugin (class, plugins)

```csharp
public abstract class Plugin : IPlugin
```

Convenience base class exposing the plugin's scoped services.

- `protected Plugin()`: Plugins have a parameterless constructor (implicit). Do not do work in it; use `OnEnable`.
- `public BossBars BossBars { get; }`: Boss bars of this plugin (removed from every viewer when the plugin unloads).
- `public ChannelRegistry Channels { get; }`: Plugin messaging channels: listen on a channel (`ChannelRegistry.On`).
- `public ChunkTickets ChunkTickets { get; }`: Chunk tickets of this plugin: chunks it keeps loaded (released when the plugin unloads).
- `public CommandRegistry Commands { get; }`: Commands of this plugin.
- `public PluginConfig Config { get; }`: Configuration files and the data folder of this plugin.
- `public PluginContext Context { get; }`: The plugin's scoped services.
- `public string DataFolder { get; }`: The plugin's data folder (`plugins/<id>/`), created on first access.
- `public Ecs Ecs { get; }`: ECS access (components, queries).
- `public EventRegistry Events { get; }`: Event subscriptions of this plugin.
- `public Holograms Holograms { get; }`: Holograms: text that floats in the world for the players (removed when the plugin unloads).
- `public PluginMetadata Info { get; }`: What the plugin declares about itself (id, version, dependencies, ...).
- `public Logger Log { get; }`: The plugin's logger.
- `public Menus Menus { get; }`: Custom GUIs of this plugin (closed for every viewer when the plugin unloads).
- `public Npcs Npcs { get; }`: NPCs: fake players with a skin, equipment and click events (removed when the plugin unloads).
- `public PluginPermissions Permissions { get; }`: Permission nodes of this plugin and permission queries.
- `public Scheduler Scheduler { get; }`: Scheduled tasks of this plugin.
- `public ServiceManager Services { get; }`: Plugin-to-plugin services: register an implementation, look up another plugin's.
- `public Sidebars Sidebars { get; }`: Per-player sidebar scoreboards of this plugin (hidden when the plugin unloads).
- `public SystemRegistry Systems { get; }`: ECS systems of this plugin.
- `protected virtual void OnDisable()`: Optional cleanup; registrations are released automatically afterwards.
- `protected abstract void OnEnable()`: Register events, commands and systems here.

```csharp
using Mc.Scripting;

[PluginInfo("Hello", Version = "1.0.0", Author = "you")]
public sealed class Hello : Plugin
{
    protected override void OnEnable()
    {
        Events.On((ref PlayerJoinEvent e) => Log.Info($"{e.Username.ToString()} joined"));
    }

    protected override void OnDisable() => Log.Info("bye");
}
```

## PluginContext (class, plugins)

```csharp
public sealed class PluginContext
```

Per-plugin services. Everything registered through it is released automatically when the plugin unloads or hot-reloads.

Plugins normally use the same members through the `Plugin` base class properties (`Events`, `Commands`, ...). Use the context directly when your code lives outside the plugin class: pass it to helper classes so everything they register is released with the plugin.

- `public string AssemblyPath { get; }`: Path of the plugin assembly.
- `public BossBars BossBars { get; }`: Boss bars.
- `public ChannelRegistry Channels { get; }`: Plugin messaging channels of this plugin (`ChannelRegistry`).
- `public ChunkTickets ChunkTickets { get; }`: Chunk tickets.
- `public CommandRegistry Commands { get; }`: Commands.
- `public PluginConfig Config { get; }`: Configuration files and the data folder.
- `public Ecs Ecs { get; }`: ECS access.
- `public EventRegistry Events { get; }`: Event subscriptions.
- `public Holograms Holograms { get; }`: Holograms (text displays).
- `public int Id { get; }`: Stable plugin id (kept across hot reloads).
- `public PluginMetadata Info { get; }`: What the plugin declares about itself (`plugin.json` or `PluginInfoAttribute`).
- `public bool IsUnloaded { get; }`: Whether the plugin was unloaded.
- `public Logger Log { get; }`: The plugin's logger.
- `public Menus Menus { get; }`: Custom GUIs (menus) owned by this plugin.
- `public string Name { get; }`: The plugin name.
- `public Npcs Npcs { get; }`: NPCs (fake players).
- `public PluginPermissions Permissions { get; }`: Permissions.
- `public Scheduler Scheduler { get; }`: Scheduler.
- `public ServiceManager Services { get; }`: Plugin-to-plugin services.
- `public Sidebars Sidebars { get; }`: Sidebars.
- `public SystemRegistry Systems { get; }`: Systems.
- `public string Version { get; }`: The plugin version.
- `public void Track(IPluginResource resource)`: Registers an arbitrary resource to be disposed on unload.

## PluginDependency (struct, plugins)

```csharp
public readonly struct PluginDependency : IEquatable<PluginDependency>
```

A dependency declaration of a plugin: the id of the other plugin and an optional version constraint.

- `public PluginDependency(string Id, string? Constraint)`: A dependency declaration of a plugin: the id of the other plugin and an optional version constraint.
- `public string? Constraint { get; init; }`: A version constraint such as ">=1.2", "^1.2", "=1.2.3" (null = any version).
- `public string Id { get; init; }`: The other plugin's id.
- `public bool Accepts(string version)`: Whether `version` satisfies the constraint.
- `public override string ToString()`
- `public static PluginDependency Parse(string text)`: Parses "economy" or "economy >=1.2".

## PluginInfoAttribute (class, plugins)

```csharp
public sealed class PluginInfoAttribute : Attribute
```

Optional metadata for a plugin type.

- `public PluginInfoAttribute(string name)`: Optional metadata for a plugin type.
- `public string? Author { get; init; }`: Author.
- `public string? Description { get; init; }`: Description.
- `public string Name { get; }`: Display name.
- `public string Version { get; init; }`: Version text.

```csharp
[PluginInfo("Homes", Version = "1.2.0", Author = "you", Description = "Set and teleport to homes")]
public sealed class HomesPlugin : Plugin
{
    protected override void OnEnable() { /* ... */ }
}
```

## PluginMessageHandler (delegate, plugins)

```csharp
public delegate void PluginMessageHandler(Player player, ReadOnlySpan<byte> data)
```

Receives a message a client sent on a channel the plugin listens on. `data` is valid for the call only.

## PluginMetadata (class, plugins)

```csharp
public sealed class PluginMetadata
```

What a plugin declares about itself: from `plugin.json`, or from `PluginInfoAttribute` when there is no manifest.

- `public PluginMetadata(string id, string name, string version)`: Creates metadata (the runtime does this; plugins read it from `PluginContext.Info`).
- `public ApiVersion Api { get; init; }`: The plugin API version the plugin was written for.
- `public bool ApiDeclared { get; init; }`: Whether the manifest named an API version (otherwise 1.0 was assumed).
- `public IReadOnlyList<string> Authors { get; init; }`: The author names.
- `public IReadOnlyList<PluginDependency> Depends { get; init; }`: Plugins that must be loaded (and enabled) before this one; a missing one prevents this plugin from loading.
- `public string Description { get; init; }`: A one-line description.
- `public string Id { get; }`: Unique id: lower case letters, digits, '-' and '_' (1-32 characters). Also the data folder name.
- `public IReadOnlyList<string> LoadBefore { get; init; }`: Plugins that should be loaded after this one (the reverse of a soft dependency).
- `public string? Main { get; init; }`: The full name of the plugin class when the assembly has several candidates (default: the single public `IPlugin`).
- `public string Name { get; }`: Display name (used as the log prefix).
- `public IReadOnlyList<PluginDependency> SoftDepends { get; init; }`: Plugins that are loaded before this one when present.
- `public string Source { get; init; }`: The path of the manifest or assembly this was read from.
- `public string Version { get; }`: Plugin version.
- `public string Website { get; init; }`: A website or repository url.
- `public static bool IsValidId(string id)`: Whether `id` is a valid plugin id.

## Plugins (class, plugins)

```csharp
public static class Plugins
```

Read-only view of the loaded plugins: for admin tools (a `/plugins` command), for plugins that adapt to an optional other plugin, and for diagnosing why a plugin did not load.

- `public static IReadOnlyList<PluginMetadata> All { get; }`: The loaded (enabled) plugins in load order: dependencies before their dependents.
- `public static IReadOnlyList<string> Problems { get; }`: The problems found while discovering and loading plugins (bad manifests, missing or outdated dependencies, dependency cycles, an unsupported API version, failures in `OnEnable`), one readable sentence each, newest scan last.
- `public static PluginMetadata? Get(string id)`: The metadata of a loaded plugin, null when no plugin with this id is loaded.
- `public static bool IsLoaded(string id)`: Whether a plugin is loaded.

## PortalCreateReason (enum, events)

```csharp
public enum PortalCreateReason : byte
```

Why a portal is being created.

- `Fire` = 0: Lit with fire.
- `NetherPair` = 1: The destination portal of a nether portal trip.
- `Plugin` = 2: A plugin.

## PotionEffect (struct, players)

```csharp
public readonly struct PotionEffect : IEquatable<PotionEffect>
```

A status effect to apply to a player (a potion effect in Bukkit terms).

- `public PotionEffect(string Effect, int DurationTicks, int Amplifier = 0, bool Ambient = false, bool ShowParticles = true, bool ShowIcon = true)`: A status effect to apply to a player (a potion effect in Bukkit terms).
- `public const int Infinite = -1;`: The duration of an effect that never runs out.
- `public bool Ambient { get; init; }`: Whether it comes from a beacon (translucent particles).
- `public int Amplifier { get; init; }`: The level minus one: 0 is level I, 1 is level II, up to 255.
- `public int DurationTicks { get; init; }`: How long it lasts in ticks (20 per second), or `PotionEffect.Infinite`.
- `public string Effect { get; init; }`: The effect key, e.g. `minecraft:speed` (the namespace may be left out).
- `public bool IsInfinite { get; }`: Whether the effect never runs out.
- `public bool ShowIcon { get; init; }`: Whether the effect shows in the player's HUD.
- `public bool ShowParticles { get; init; }`: Whether the effect shows particles around the player.
- `public static PotionEffect For(string effect, TimeSpan duration, int amplifier = 0)`: Creates an effect lasting a number of seconds.

## PresenceExtensions (class, display)

```csharp
public static class PresenceExtensions
```

The presence API of a plugin: `plugin.Tablist`, `plugin.Nametags` and `plugin.Belowname`, on the plugin base class and on the plugin context. Everything a plugin sets through them is restored and removed when the plugin unloads.

- `public static Belowname get_Belowname(Plugin plugin)`
- `public static Belowname get_Belowname(PluginContext context)`
- `public static Nametags get_Nametags(Plugin plugin)`
- `public static Nametags get_Nametags(PluginContext context)`
- `public static Tablist get_Tablist(Plugin plugin)`
- `public static Tablist get_Tablist(PluginContext context)`

## PvpFlags (enum, other)

```csharp
public enum PvpFlags : uint
```

The bits of a player's PvP policy (`Player.SetPvpPolicy`). Without a policy a player fights by the vanilla rules.

- `None` = 0: No PvP flag: the player can neither be hit by nor hit another player through the gate.
- `PvpOn` = 1: The player has PvP switched on. A hit between two players counts only when both have it.
- `MayAttackPlayers` = 2: The player may attack other players (a newbie may not).
- `ShieldFromPlayers` = 4: The player takes no damage from other players (newbie or respawn protection).
- `ShieldFromMobs` = 8: The player takes no damage from mobs and other non-player attackers.
- `ShieldFromEnvironment` = 16: The player takes no environmental damage (fire, lava, fall, drowning ...).
- `WorldPvpOff` = 32: PvP is off in the player's world (checked on both sides of a hit).

## Query (class, ecs)

```csharp
public abstract class Query : IDisposable, IPluginResource
```

A cached ECS query. Iterate with `foreach (var chunk in query)` for the fastest path (SoA spans per archetype), or use `Each` for convenience.

Iterate `foreach (var chunk in query)` for the fastest path (one `QueryChunk` per archetype, with a `Span<T>` per component), or call `Each` on the typed query. Structural changes made while iterating (adding or removing components, destroying entities) are deferred until the iteration ends.

- `public int Count()`: Number of matching entities (iterates).
- `public void Dispose()`: Destroys the query.
- `public QueryEnumerator GetEnumerator()`: Starts an iteration over the matching archetype chunks.

## QueryAction<T1> (delegate, ecs)

```csharp
public delegate void QueryAction<T1>(Entity entity, ref T1? c1)
```

Callback for `Each` over one component.

## QueryAction<T1, T2> (delegate, ecs)

```csharp
public delegate void QueryAction<T1, T2>(Entity entity, ref T1? c1, ref T2? c2)
```

Callback for `Each` over two components.

## QueryAction<T1, T2, T3> (delegate, ecs)

```csharp
public delegate void QueryAction<T1, T2, T3>(Entity entity, ref T1? c1, ref T2? c2, ref T3? c3)
```

Callback for `Each` over three components.

## QueryChunk (struct, ecs)

```csharp
public readonly ref struct QueryChunk
```

One archetype's worth of query results: entity ids and one column per query term.

- `public int Count { get; }`: Number of entities in the chunk.
- `public ReadOnlySpan<Entity> Entities { get; }`: The entity ids of the chunk.
- `public Span<T> Field<T>(int index) where T : unmanaged`: Column for query term `index` (the order of the query's type arguments).

## QueryEnumerator (struct, ecs)

```csharp
public ref struct QueryEnumerator
```

Iterates archetype chunks. Disposing early releases the native iterator.

- `public QueryChunk Current { get; }`: The current chunk.
- `public void Dispose()`: Releases the native iterator when abandoning the loop early.
- `public bool MoveNext()`: Advances to the next chunk.

## Query<T1> (class, ecs)

```csharp
public sealed class Query<T1> : Query where T1 : unmanaged
```

A query over one component type.

- `public void Each(QueryAction<T1> action)`: Calls `action` for every matching entity.

```csharp
// _mana is a Query<Mana> created in OnEnable with Ecs.Query<Mana>()
_mana.Each((Entity entity, ref Mana m) => m.Value++);
```

## Query<T1, T2> (class, ecs)

```csharp
public sealed class Query<T1, T2> : Query where T1 : unmanaged where T2 : unmanaged
```

A query over two component types.

- `public void Each(QueryAction<T1, T2> action)`: Calls `action` for every matching entity.

## Query<T1, T2, T3> (class, ecs)

```csharp
public sealed class Query<T1, T2, T3> : Query where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
```

A query over three component types.

- `public void Each(QueryAction<T1, T2, T3> action)`: Calls `action` for every matching entity.

## RegainReason (enum, other)

```csharp
public enum RegainReason : byte
```

Why an entity regains health (EntityRegainHealth). Custom is the default of every heal without a reason.

- `Custom` = 0: Any heal without a more specific reason: a plugin, an iron golem's repair, an animal's or a mount's regeneration.
- `Regen` = 1: Natural regeneration of a player with a full food bar, and the peaceful rule (fast regeneration, one heal every 20 ticks).
- `Satiated` = 2: Natural regeneration from saturation (the food bar is full, the saturation is spent).
- `Eating` = 3: A tamed or bred animal or a mount heals from the food it is given.
- `EnderCrystal` = 4: An end crystal heals the ender dragon.
- `Magic` = 5: The instant health effect (a healing potion or splash).
- `MagicRegen` = 6: The regeneration effect, one heal per tick it applies.
- `WitherSpawn` = 7: The wither heals while it is summoned (the spawn sequence).
- `Wither` = 8: A wither skull hit heals the wither.

## ResourcePack (class, players)

```csharp
public sealed class ResourcePack : IEquatable<ResourcePack>
```

A resource pack the server asks clients to download.

- `public ResourcePack(Uuid Id, string Url, string? Sha1 = null, bool Required = false, Component? Prompt = null)`: A resource pack the server asks clients to download.
- `public Uuid Id { get; init; }`: The pack id (clients keep one pack per id: pushing the same id replaces it).
- `public Component? Prompt { get; init; }`: Text shown in the download prompt, null for the default.
- `public bool Required { get; init; }`: Whether the client is disconnected when it declines the pack.
- `public string? Sha1 { get; init; }`: The SHA-1 hash of the zip as 40 hex characters; null skips the check (clients then re-download on every push).
- `public string Url { get; init; }`: The download address (http or https).
- `public static ResourcePack FromUrl(string url, string? sha1 = null, bool required = false, Component? prompt = null)`: Creates a pack whose id is derived from its url, so pushing the same url twice replaces the first push.

## ResourcePackStatus (enum, players)

```csharp
public enum ResourcePackStatus : byte
```

Client report about a pushed resource pack (protocol order).

- `SuccessfullyLoaded` = 0: The pack was applied.
- `Declined` = 1: The player declined it.
- `FailedDownload` = 2: The download failed.
- `Accepted` = 3: The player accepted it (download starts).
- `Downloaded` = 4: Downloaded, not applied yet.
- `InvalidUrl` = 5: The url was invalid.
- `FailedReload` = 6: Applying the pack failed.
- `Discarded` = 7: The pack was discarded.

## RespawnReason (enum, events)

```csharp
public enum RespawnReason : byte
```

Why a player is respawning.

- `Death` = 0: The player died.
- `EndExit` = 1: Returning from the end after the credits.

## ScheduledTask (class, scheduler)

```csharp
public sealed class ScheduledTask : IDisposable, IPluginResource
```

A scheduled task. Dispose/Cancel to stop it.

- `public bool IsCancelled { get; }`: Whether the task was cancelled or finished.
- `public void Cancel()`: Stops the task.
- `public void Dispose()`: Same as `ScheduledTask.Cancel`.

## Scheduler (class, scheduler)

```csharp
public sealed class Scheduler
```

Tick-based scheduling (20 ticks = 1 second) and the bridge between the tick thread and worker threads. Scheduled tasks run on the tick thread and are cancelled when the plugin unloads.

`Scheduler.RunLater`/`Scheduler.RunEvery` must be called on the tick thread. `Scheduler.Post`, `Scheduler.RunSync`, `Scheduler.RunAsync` and the awaiters are thread-safe.

- `public Task Delay(long ticks)`: Awaitable that resumes on the tick thread after `ticks` ticks.
- `public TickAwaitable EnsureTick()`: Awaitable that resumes on the tick thread: `await Scheduler.EnsureTick();`. Already on the tick thread it continues synchronously (no hop); otherwise it resumes at the next tick boundary, like `Scheduler.ToTick`. If the plugin unloads first the continuation is dropped.
- `public void Post(Action action)`: Thread-safe: queue `action` to run on the tick thread next tick (fire and forget).
- `public Task RunAsync(Action action)`: Runs `action` on the .NET thread pool. Must not touch game APIs (use `Scheduler.RunSync` to hop back).
- `public Task<T?> RunAsync<T>(Func<T?> func)`: Runs `func` on the .NET thread pool.
- `public ScheduledTask RunEvery(long periodTicks, Action action, long initialDelayTicks = 0)`: Runs `action` every `periodTicks` ticks, starting after `initialDelayTicks` (at least 1).
- `public ScheduledTask RunLater(long delayTicks, Action action)`: Runs `action` after `delayTicks` ticks on the tick thread.
- `public Task RunSync(Action action)`: Thread-safe: runs `action` on the tick thread at the next tick boundary (FIFO).
- `public Task<T?> RunSync<T>(Func<T?> func)`: Thread-safe: runs `func` on the tick thread at the next tick boundary and returns its result.
- `public TickAwaitable ToAsync()`: Awaitable that always resumes on a thread-pool thread: `await Scheduler.ToAsync();` (leave the tick thread before blocking work).
- `public TickAwaitable ToTick()`: Awaitable that always resumes on the tick thread at the next tick boundary, even when already there: `await Scheduler.ToTick();` (a wait loop such as `while (!loaded) await Scheduler.ToTick();` needs this). Use `Scheduler.EnsureTick` to skip the hop when already on the tick thread. If the plugin unloads first the continuation is dropped.

```csharp
Scheduler.RunLater(20, () => Server.Broadcast("one second later"));

ScheduledTask autosave = Scheduler.RunEvery(20 * 60, SaveAll);   // every minute
// autosave.Cancel();

// from a thread-pool task: fetch off the tick thread, then apply on it
_ = Task.Run(async () =>
{
    string json = await Http.Client.GetStringAsync(url);
    Scheduler.Post(() => Apply(json));     // back on the tick thread
});
```

## ScoreFormat (enum, display)

```csharp
public enum ScoreFormat : int
```

How a score is shown next to its holder.

- `None` = 0: The number is shown.
- `Blank` = 1: The number is hidden.
- `Fixed` = 2: A fixed text is shown instead of the number.

## Scoreboard (class, display)

```csharp
public static class Scoreboard
```

The server's scoreboard (shared by all players, saved with the world, edited by `/scoreboard`): objectives with scores per holder name and their display slots. Every change reaches every player's client. Use `Sidebar` for per-player content.

- `public static Objective CreateObjective(string name, Component? title = null, string criteria = "dummy")`: Creates an objective. Objectives are part of the world: they stay until removed (they are not owned by the plugin).
- `public static Objective? GetObjective(string name)`: An existing objective by name; null when there is none.
- `public static bool RemoveObjective(Objective objective)`: Deletes an objective and all its scores.
- `public static void SetDisplay(DisplaySlot slot, Objective? objective)`: Shows an objective in a display slot (null clears the slot).

## Server (class, server)

```csharp
public static class Server
```

Server-wide services: the tick clock, the online players, the worlds and broadcast messages.

Everything here except `Server.IsTickThread` and `Server.CurrentTick` must be called on the tick thread.

- `public static long CommittedBytes { get; }`: The server's private commit in bytes, -1 when unknown.
- `public static ulong CurrentTick { get; }`: Current server tick (as reported by the engine; thread-safe).
- `public static bool IsTickThread { get; }`: Whether the calling thread is the server tick thread (thread-safe, ~1 ns).
- `public static long MemoryBytes { get; }`: The server's RAM in bytes as Task Manager's Memory column shows it (private working set; Linux: resident minus shared), -1 when the platform cannot tell. Plugins run in a separate worker process, so `Environment.WorkingSet` is the worker's, not the server's.
- `public static IOperatorList Operators { get; }`: The operator list (ops.json): the players with a permission level, online or not. Thread: tick thread.
- `public static World Overworld { get; }`: The overworld.
- `public static int PlayerCount { get; }`: Number of players in the play state.
- `public static double PlayerMoveThreshold { get; set; }`: How far a player has to move (blocks) before `PlayerMoveEvent` fires again (a turn of one degree also counts). 0, the default, fires for every change of position or rotation (about 20 events a second per moving player): raise it when your handler only cares about coarse movement.
- `public static IReadOnlyList<Player> Players { get; }`: A snapshot of the online players (in the play state).
- `public static long PluginMemoryBytes { get; }`: The RAM of the process the plugins run in (the worker; in-process plugins: the server), measured like `Server.MemoryBytes`: private working set (Linux: resident minus shared). One system call, no allocation, any thread; -1 when unknown. Prefer it to `Process.GetCurrentProcess()`, which snapshots every process of the machine.
- `public static IWhitelist Whitelist { get; }`: The whitelist (whitelist.json) and its switches (white-list, enforce-whitelist). Thread: tick thread.
- `public static long WorkingSetBytes { get; }`: The server's working set in bytes (resident, shared pages included), -1 when unknown.
- `public static IReadOnlyList<World> Worlds { get; }`: The live worlds of the server: the vanilla dimensions (overworld first), then the runtime worlds. A new list on every call.
- `public static void Broadcast(string message)`: Sends a plain-text system message to everybody (and the console).
- `public static void Broadcast(Component message)`: Sends a rich text message to every online player and logs its plain text on the console.
- `public static Task<World> CreateWorldAsync(WorldOptions options)`: Makes a runtime world (Bukkit's `WorldCreator`): saved (anvil or slime storage) or temporary, normal, flat or void terrain, empty or a copy of a `WorldOptions.Template`. The world is built at the start of the next tick; the task completes then, on the tick thread, so the code after `await` can use the world at once. Move players in with `player.Teleport(new Location(world, x, y, z))`.
- `public static CommandResult DispatchCommand(string line)`: Runs a command line as the console: operator level 4, with the feedback in the server log. The line is the command as a player types it, with or without the leading slash, and it goes through the server's command dispatcher, so vanilla and plugin commands both run.
- `public static CommandResult DispatchCommand(Player player, string line)`: Runs a command line as a player: the player's own permissions decide what runs, and the feedback goes to the player's chat, as when the player types it. The line is limited to 256 characters (a chat line), with or without the leading slash. See `Server.DispatchCommand` for the rules; a player who is offline fails at once. Inside an event handler, a system, a scheduler phase or another plugin's call it fails with a message and runs nothing: use `Server.DispatchCommandAsync` there.
- `public static Task<CommandResult> DispatchCommandAsync(string line)`: Queues a command line to run as the console and returns a task for its outcome. Unlike `Server.DispatchCommand` it works for plugin commands too: the line runs at the end of the next tick, when the plugin worker is not in a call.
- `public static Task<CommandResult> DispatchCommandAsync(Player player, string line)`: Queues a command line to run as a player, with that player's permissions and feedback. See `Server.DispatchCommandAsync`.
- `public static Player? GetPlayer(string name)`: Finds an online player by name (case-insensitive).
- `public static Player? GetPlayer(Uuid uuid)`: Finds an online player by uuid.
- `public static World? GetWorld(string key)`: Finds a live world by key ("minecraft:the_nether", "ramjet:arena"). A name without a namespace is looked up as `minecraft:` first, then as `ramjet:` (the namespace of runtime worlds made without one): "the_nether", "arena".
- `public static Task<World> LoadWorldAsync(string name)`: Loads a saved runtime world (made by `Server.CreateWorldAsync` or `/world create`, not temporary) at the start of the next tick. The task completes then, on the tick thread. Admins can have worlds loaded at startup with `[worlds] load` in ramjet.toml.
- `public static IReadOnlyList<PlayerSnapshot> PlayersSnapshot()`: A snapshot of every online player with the data the engine keeps about him: the handle, the profile id, the dimension, the game mode, the ping and the tab list flags. One pass over the players, read in pages from the engine; use `Server.Players` when only the handles are needed.
- `public static void SendMessage(Entity target, string message)`: Sends a system chat message to `target` (Entity.Null = console).
- `public static TickTimes TickTimes(int ticks = 20)`: The engine's own tick time over the last `ticks` ticks (1 to 6000), as `/tick query` reports it: the busy time of each tick. Prefer it to timing your own systems: plugins run in a separate worker process, and a Stopwatch there also counts the round trips to the engine. Values are -1 when the engine cannot tell.
- `public static Task UnloadWorldAsync(World world, bool save = true, bool movePlayers = false)`: Unloads a runtime world at the start of the next tick (Bukkit's `unloadWorld`). The vanilla dimensions stay loaded. Handles of the world are invalid afterwards (`World.IsValid` is false).

## ServiceUnavailableException (class, services)

```csharp
public sealed class ServiceUnavailableException : InvalidOperationException
```

A plugin-to-plugin service that was registered is no longer available (its provider unloaded).

- `public ServiceUnavailableException(Type serviceType)`: Creates the exception.

## Affix (class, services)

```csharp
public sealed class Affix : IEquatable<Affix>
```

One prefix or suffix of a player: its text, its weight (the highest weight comes first) and the group or plugin that supplied it.

- `public Affix(string Text, int Weight, string Group)`: One prefix or suffix of a player: its text, its weight (the highest weight comes first) and the group or plugin that supplied it.
- `public string Group { get; init; }`: Where it came from (a permission group, or a plugin's own name).
- `public string Text { get; init; }`: The text, as the provider stores it (the chat and tab plugins decorate it).
- `public int Weight { get; init; }`: The rank among the affixes of the same kind; a higher weight comes first.

## AffixKind (enum, services)

```csharp
public enum AffixKind : int
```

Whether an affix goes before the player's name (a prefix) or after it (a suffix).

- `Prefix` = 0: Before the name, as in "[Admin] Steve".
- `Suffix` = 1: After the name, as in "Steve [AFK]".

## BanEntry (class, services)

```csharp
public sealed class BanEntry : IEquatable<BanEntry>
```

One ban: a player (uuid and the name it was banned under) or an address. `BanEntry.Expires` null means forever.

- `public BanEntry(Uuid? Player, string? Address, string? Name, DateTimeOffset Created, DateTimeOffset? Expires, string Source, string? Reason)`: One ban: a player (uuid and the name it was banned under) or an address. `BanEntry.Expires` null means forever.
- `public string? Address { get; init; }`: The banned address without port; null for a player ban.
- `public DateTimeOffset Created { get; init; }`: When the ban was written.
- `public DateTimeOffset? Expires { get; init; }`: When it ends; null = forever.
- `public string? Name { get; init; }`: The name the player was banned under; null for an address ban.
- `public Uuid? Player { get; init; }`: The banned player; null for an address ban.
- `public string? Reason { get; init; }`: The reason shown on the disconnect screen; null for the default text.
- `public string Source { get; init; }`: Who wrote it ("(Unknown)" when nobody said).

## Currency (class, services)

```csharp
public sealed class Currency : IEquatable<Currency>
```

A currency. Amounts are long minor units (cents when `Currency.FractionDigits` is 2): no floating point on any path.

- `public Currency(string Name, string Symbol, int FractionDigits, string PluralName)`: A currency. Amounts are long minor units (cents when `Currency.FractionDigits` is 2): no floating point on any path.
- `public int FractionDigits { get; init; }`: Minor units per major unit as a power of ten, 0 to 9 (checked when the currency is made).
- `public string Name { get; init; }`: The name ("dollar").
- `public string PluralName { get; init; }`: The plural name ("dollars").
- `public string Symbol { get; init; }`: The symbol written before the amount ("$").
- `public string Format(long minor)`: The amount as text: the symbol, the major units and the minor units after a point ("$12.05"). Negative amounts start with a minus sign.

## EconomyResult (enum, services)

```csharp
public enum EconomyResult : int
```

The outcome of a money operation.

- `Ok` = 0: Done.
- `Unknown` = 1: The account does not exist (the player never had one, or the provider does not create it on demand).
- `NotEnough` = 2: The account lacks the money.
- `OverMax` = 3: The deposit would go over the provider's maximum balance.
- `Refused` = 4: Refused: a zero or negative amount, a frozen account, or another rule of the provider.

## IBanList (interface, services)

```csharp
public interface IBanList
```

The server's ban lists: the vanilla files banned-players.json and banned-ips.json. The engine's login check reads the same files, so a ban written here, by /ban or by another plugin is enforced the same way; a plugin that replaces this service (a moderation plugin) must write the same files or keep them as its only store. A write does not kick an online player: the caller does that (`Player.Kick`). Thread: tick thread.

- `bool AddAddress(string address, string? reason, DateTimeOffset? expires, string source)`: Bans an address (the bare IP; a port is dropped).
- `bool AddPlayer(Uuid player, string name, string? reason, DateTimeOffset? expires, string source)`: Bans a player. An identical ban that is still in force is kept as it is.
- `IReadOnlyList<BanEntry> Addresses()`: The address bans that are still in force.
- `BanEntry? Check(Uuid player, string? address)`: The ban that refuses a login of this player or address (the player ban first), null when none.
- `IReadOnlyList<BanEntry> Players()`: The player bans that are still in force.
- `bool RemoveAddress(string address)`: Removes an address ban.
- `bool RemovePlayer(Uuid player)`: Removes a player's ban.

## IEconomy (interface, services)

```csharp
public interface IEconomy
```

An economy: balances of players (online or offline, by uuid), the transfers between them, and the currency. Provided by an economy plugin (Essentials is the first-party default, at a lower priority than any real economy). Without a provider, money rules must do nothing rather than invent money: check `ServiceManager.TryGet` for null. Thread: tick thread.

- `Currency Currency { get; }`: The currency of the economy.
- `long Balance(Uuid player)`: The balance in minor units; 0 for an unknown account.
- `EconomyResult Deposit(Uuid player, long minor, string reason)`: Adds money to an account.
- `bool Has(Uuid player, long minor)`: Whether the account holds at least the amount.
- `bool HasAccount(Uuid player)`: Whether the uuid has an account.
- `EconomyResult Transfer(Uuid from, Uuid to, long minor, string reason)`: Moves money from one account to another as one step: both or neither.
- `EconomyResult Withdraw(Uuid player, long minor, string reason)`: Takes money from an account, refusing to go below zero.
- `public event Action<Uuid?>? Changed;`: Raised when an account's balance changed (tick thread). A null uuid means many accounts may have changed.

## IOperatorList (interface, services)

```csharp
public interface IOperatorList
```

The server's operator list: the vanilla file ops.json, the list /op and /deop change. Operators get their permission level and the command tree of that level, and bypass the whitelist. The changes are written at once. A change for an online player resends its permission level and command tree, as /op does; a removal also kicks the unlisted players when enforce-whitelist and the whitelist are on, as /deop does. Works for players who are offline: the uuid is the key, and `OfflinePlayers.Find` resolves a name. Each change is logged with the plugin that made it. Thread: tick thread.

- `bool Add(Uuid player, string name, int? level = null, bool? bypassesPlayerLimit = null)`: Makes a player an operator, or changes an operator's level and bypass flag. An identical entry is kept as it is.
- `IReadOnlyList<OperatorEntry> All()`: Every operator, in uuid order.
- `OperatorEntry? Check(Uuid player)`: The operator entry of a uuid, null when it is not an operator.
- `bool Remove(Uuid player)`: Removes an operator (as /deop does).

## IPlaceholders (interface, services)

```csharp
public interface IPlaceholders
```

The placeholder registry: text templates such as "Online: %server_online%" are rendered through it, and any plugin can add its own placeholders (the chat format, the tab list, nametags, kits, the messages of other plugins). Process-wide: the built-in instance is registered with `ServiceManager`, so every plugin in the same process sees the same names. Rendering is pure managed code (no engine call of its own): a value that reads engine state pays for that read, which is why each placeholder declares how long its value may be reused. Thread: tick thread (the values are evaluated where the text is rendered).

- `void Invalidate(string id)`: Drops every cached value of a placeholder so the next render evaluates it again.
- `bool IsRegistered(string id)`: Whether an id is registered.
- `IPluginResource Register(string id, PlaceholderScope scope, TimeSpan cacheFor, PlaceholderValue value)`: Adds a placeholder. The id is lowercase letters, digits and underscores ("charter_prefix"); "%charter_prefix%" renders it. A registration with the same id replaces the earlier one (a hot-reload successor). Disposing the result (or unloading the owning plugin, when it registered from a plugin's enable or event handler) removes it.
- `string Resolve(Player subject, string text)`: Renders a template for a subject. "%%" is a literal percent sign; a token that is not a registered placeholder is left as written. Text without a percent sign comes back unchanged.

## IPlayerMeta (interface, services)

```csharp
public interface IPlayerMeta
```

Player meta and affixes: the prefix, suffix and typed values that a permission provider (or any plugin) supplies for a player, read by the chat, tab and nametag plugins. Registered with `ServiceManager`; the highest-priority provider answers. Thread: tick thread.

Ramjet's first-party permissions plugin implements it; without one, a built-in provider answers with no meta and no affixes. Contexts (a world, a server) are not part of this contract yet: a provider that needs them offers its own interface.

- `IReadOnlyList<Affix> Affixes(Uuid player, AffixKind kind)`: The prefixes or suffixes of a player, highest weight first.
- `bool TryGet<T>(Uuid player, MetaKey<T?> key, out T? value)`: The value of a meta key for a player, from the provider's highest layer that has it.
- `public event Action<Uuid?>? Changed;`: Raised when a player's meta or affixes changed (tick thread). A null uuid means every player may have changed.

## IPrivateMessages (interface, services)

```csharp
public interface IPrivateMessages
```

The private-message service: the delivery, formatting, ignore and mute rules of `/msg` and `/r`. Registered by the chat plugin when it is installed; the command owner (Essentials) hands each message here and delivers a plain message itself when no provider is registered. Thread: tick thread.

- `PrivateMessageResult Send(Player sender, Player recipient, string message)`: Sends a private message between two online players. The provider formats it and shows it to both ends.

## IWhitelist (interface, services)

```csharp
public interface IWhitelist
```

The server's whitelist: the vanilla file whitelist.json, and the two switches that enforce it (white-list and enforce-whitelist in server.properties). A player who is not on the list (and is not an operator) cannot join while `IWhitelist.Enabled` is on. With `IWhitelist.Enforced` on, the online players who are not on the list are kicked, operators included, as the list or a switch makes them unlisted; this is vanilla's kickUnlistedPlayers, which looks at the list only. Offline players work: the uuid is the key, and `OfflinePlayers.Find` resolves a name. Adding a player and the switches are written at once to the server's files; a script's additions and removals are written a few seconds later, with the ban lists (a plugin never saves them itself). Each change is logged with the plugin that made it. Thread: tick thread.

- `bool Enabled { get; set; }`: Whether the whitelist is in force (white-list in server.properties). Setting it writes server.properties, as /whitelist on and off do; turning it on kicks the unlisted players when `IWhitelist.Enforced` is on.
- `bool Enforced { get; set; }`: Whether unlisted online players are kicked (enforce-whitelist in server.properties), operators included. Turning it on kicks them at once when `IWhitelist.Enabled` is on.
- `bool Add(Uuid player, string name)`: Adds a player to the list (kicks nobody). The write is deferred a few seconds, with the ban lists.
- `IReadOnlyList<WhitelistEntry> All()`: Every player on the list, in uuid order.
- `bool Contains(Uuid player)`: Whether a uuid is on the list. Operators are not counted here: they are let in by the login check.
- `void Reload()`: Writes the pending additions and removals, then reads whitelist.json again (as /whitelist reload does), then kicks the unlisted online players when `IWhitelist.Enforced` is on. A file that cannot be read leaves the list as it was.
- `bool Remove(Uuid player)`: Removes a player from the list (the write is deferred like `IWhitelist.Add`), then kicks the online players who are no longer listed when `IWhitelist.Enforced` and `IWhitelist.Enabled` are on (as /whitelist remove does).

## MetaKey<T> (class, services)

```csharp
public sealed class MetaKey<T>
```

A typed key of player meta, declared by the plugin that owns the value (`new MetaKey<int>("charter:level")`). Only string, int, bool and TimeSpan are allowed: a value of another type is refused by the provider, so readers never parse text.

- `public MetaKey(string name)`: Creates a key. Throws for a value type other than string, int, bool or TimeSpan.
- `public string Name { get; }`: The key.
- `public override string ToString()`

## OperatorEntry (class, services)

```csharp
public sealed class OperatorEntry : IEquatable<OperatorEntry>
```

One operator: a player with a permission level (the vanilla file ops.json).

- `public OperatorEntry(Uuid Player, string Name, int Level, bool BypassesPlayerLimit)`: One operator: a player with a permission level (the vanilla file ops.json).
- `public bool BypassesPlayerLimit { get; init; }`: Whether the operator may join a full server.
- `public int Level { get; init; }`: The permission level, 1 to 4: 1 moderator, 2 gamemaster, 3 admin, 4 owner (what the command tree and /op use).
- `public string Name { get; init; }`: The name the operator was added under.
- `public Uuid Player { get; init; }`: The operator's uuid.

## PlaceholderScope (enum, services)

```csharp
public enum PlaceholderScope : int
```

Whether a placeholder has one value for everybody (`PlaceholderScope.Global`) or one value per player.

- `Global` = 0: One value for everybody; the subject passed to the value is `Player.None`.
- `Player` = 1: One value per subject player.

## PlaceholderValue (delegate, services)

```csharp
public delegate string PlaceholderValue(Player subject, string[] args)
```

Computes a placeholder's value for a subject from the arguments written after its name: "%id:arg1:arg2%" passes ["arg1", "arg2"]. Throwing leaves the last good value in place (or the token as written) and logs the error once a minute.

## PlayerMetaExtensions (class, services)

```csharp
public static class PlayerMetaExtensions
```

Helpers over `IPlayerMeta` for the plugins that only need text.

- `public static string AffixText(IPlayerMeta meta, Uuid player, AffixKind kind, string separator = " ")`: The affixes of one kind joined into one text, highest weight first.

## PrivateMessageResult (enum, services)

```csharp
public enum PrivateMessageResult : int
```

The outcome of a private message.

- `Ok` = 0: Delivered.
- `Offline` = 1: The recipient is not online.
- `Ignored` = 2: The recipient ignores the sender.
- `Muted` = 3: The sender is muted.
- `Refused` = 4: Refused by the provider's rules (empty text, the recipient refuses messages, and the like).

## ServiceManager (class, services)

```csharp
public sealed class ServiceManager
```

Plugin-to-plugin services: a plugin registers an implementation of an interface (an economy, a chat formatter, a region lookup) and other plugins look it up without referencing the provider's code. The interface type lives in the provider's assembly or in a shared one; consumers declare `depends` / `softDepends` so the runtime resolves that assembly into their load context (the types then unify). Registrations are owned by the registering plugin and removed when it unloads, which fires `ServiceManager.Changed` so consumers drop their references.

- `public IDisposable Changed<T>(Action onChanged) where T : class`: Calls `onChanged` (on the tick thread) whenever the set of providers of `T` changes: a registration, an unregistration or a provider plugin unloading. Use it to refresh a cached `ServiceManager.TryGet` result.
- `public T Get<T>() where T : class`: The highest-priority provider of `T`; throws `ServiceUnavailableException` when none is registered.
- `public IReadOnlyList<ServiceRegistration<T>> GetAll<T>() where T : class`: All registrations of `T`, best first.
- `public void Register<T>(T instance, ServicePriority priority = ServicePriority.Normal) where T : class`: Registers an implementation of `T`.
- `public T? TryGet<T>() where T : class`: The highest-priority provider of `T`, null when none is registered. Thread-safe.
- `public bool Unregister<T>() where T : class`: Removes this plugin's registration of `T`.

## ServicePriority (enum, services)

```csharp
public enum ServicePriority : int
```

How a provider ranks when several plugins register the same service type (the highest wins).

- `Lowest` = -2: Used only when nothing better is registered.
- `Low` = -1: Below normal.
- `Normal` = 0: The default.
- `High` = 1: Preferred over normal providers.
- `Highest` = 2: Preferred over everything else.

## ServiceRegistration<T> (class, services)

```csharp
public sealed class ServiceRegistration<T> : IEquatable<ServiceRegistration<T>> where T : class
```

One registered service: the implementation, who provided it and how it ranks.

- `public ServiceRegistration(T Instance, string Provider, ServicePriority Priority)`: One registered service: the implementation, who provided it and how it ranks.
- `public T Instance { get; init; }`: The implementation.
- `public ServicePriority Priority { get; init; }`: Its rank.
- `public string Provider { get; init; }`: The id of the registering plugin.

## WhitelistEntry (class, services)

```csharp
public sealed class WhitelistEntry : IEquatable<WhitelistEntry>
```

One player on the whitelist (the vanilla file whitelist.json).

- `public WhitelistEntry(Uuid Player, string Name)`: One player on the whitelist (the vanilla file whitelist.json).
- `public string Name { get; init; }`: The name the player was added under.
- `public Uuid Player { get; init; }`: The player's uuid.

## Sidebar (class, display)

```csharp
public sealed class Sidebar : IDisposable, IPluginResource
```

A sidebar scoreboard (the list of lines at the right of the screen) shown to chosen players. Each sidebar is a private objective sent only to its viewers, so different players can see different content at the same time (create one sidebar per player for personal lines). Lines are rich text, line 0 is the top; vanilla shows at most 15. Create it with `Sidebars.Create`; it disappears from the viewers when the plugin unloads.

- `public const int MaxLines = 15;`: The number of lines the client can show.
- `public int LineCount { get; }`: The number of lines currently set.
- `public Component Title { get; set; }`: The title above the lines.
- `public IReadOnlyList<Player> Viewers { get; }`: The players who currently see this sidebar (those who left are dropped).
- `public void Clear()`: Removes every line.
- `public void Dispose()`
- `public void Hide(Player player)`: Hides the sidebar from a player.
- `public bool IsShownTo(Player player)`: Whether the player sees this sidebar.
- `public void Remove()`: Hides the sidebar from everybody and releases it.
- `public void SetLine(int index, Component? text)`: Sets one line (0 is the top). A null text removes the line.
- `public void SetLines(IReadOnlyList<Component> lines)`: Replaces all lines: the first text becomes line 0, lines beyond the given ones are removed.
- `public void Show(Player player)`: Shows the sidebar to a player (replacing any sidebar of the server scoreboard on his screen).

## Sidebars (class, display)

```csharp
public sealed class Sidebars
```

Creates sidebars owned by a plugin (`plugin.Sidebars`).

- `public Sidebar Create(Component title)`: Creates a sidebar nobody sees yet.

## SignOpenCause (enum, other)

```csharp
public enum SignOpenCause : byte
```

What opens the text editor of a sign (PlayerOpenSign).

- `Interact` = 0: The player right-clicked the sign with an empty hand.
- `Place` = 1: The player placed the sign; the editor opens at once.

## SoundCategory (enum, players)

```csharp
public enum SoundCategory : int
```

Where a sound is mixed in the client's volume sliders.

- `Master` = 0: The master volume only.
- `Music` = 1: Music.
- `Records` = 2: Jukebox and note blocks.
- `Weather` = 3: Weather.
- `Blocks` = 4: Blocks.
- `Hostile` = 5: Hostile creatures.
- `Neutral` = 6: Friendly creatures.
- `Players` = 7: Players.
- `Ambient` = 8: Ambient and cave sounds.
- `Voice` = 9: Voice (narration).
- `Ui` = 10: User interface sounds.

## SpawnReason (enum, events)

```csharp
public enum SpawnReason : byte
```

Why an entity is being added to the world.

- `Default` = 0: Unknown / not classified.
- `Natural` = 1: Natural mob spawning.
- `ChunkGeneration` = 2: Created with a freshly generated chunk.
- `Breeding` = 3: A baby from breeding.
- `SpawnEgg` = 4: A spawn egg.
- `Spawner` = 5: A mob spawner.
- `Command` = 6: /summon.
- `Plugin` = 7: Spawned by a plugin.
- `Structure` = 8: Spawned by a structure.
- `Reinforcements` = 9: Zombie reinforcements.
- `Lightning` = 10: Lightning (skeleton trap, zombified piglin ...).
- `SlimeSplit` = 11: A slime split.
- `Mount` = 12: A jockey / mount.
- `Dispenser` = 13: Spawned by a dispenser.
- `Drop` = 14: Created by a game mechanic: item drops, experience orbs, falling blocks, projectiles.
- `Portal` = 15: Spawned by a portal.
- `TrialSpawner` = 16: Spawned by a trial spawner (its mobs ignore the light level and are persistent).

## SpawnSetCause (enum, other)

```csharp
public enum SpawnSetCause : byte
```

Why a player's spawn point is set (PlayerSetSpawn.Cause).

- `Bed` = 0: A bed was used.
- `RespawnAnchor` = 1: A respawn anchor was charged.
- `Command` = 2: A command set the point.

## Subscription (class, events)

```csharp
public sealed class Subscription : IDisposable, IPluginResource
```

An active event subscription. Dispose to unsubscribe.

- `public bool IsActive { get; }`: Whether the subscription is still active.
- `public void Dispose()`: Unsubscribes the listener.

## SystemCallback (delegate, systems)

```csharp
public delegate void SystemCallback(float deltaTime)
```

Per-tick system callback; receives the tick's delta time in seconds.

## SystemHandle (class, systems)

```csharp
public sealed class SystemHandle : IDisposable, IPluginResource
```

A registered system. Dispose to remove it from the pipeline.

- `public string Name { get; }`: The system's full registry name, `"{plugin name}:{name}"`: the name passed to `SystemRegistry.Add` with the plugin prefix (unless it already had one). While a hot-reload predecessor still holds that name it is the temporary "name#2" form, renamed back to the plain name once the predecessor is released. Pass either this full name or the short name you registered with to `SystemRegistry.After`, `SystemRegistry.Before`, `SystemRegistry.Replace` and `SystemRegistry.SetEnabled`.
- `public void Dispose()`: Removes the system from the pipeline.

## SystemPlacement (enum, systems)

```csharp
public enum SystemPlacement : int
```

Where a system goes relative to the engine's registry.

- `Append` = 0: End of the phase.
- `After` = 1: Right after the system named by the anchor (same phase).
- `Before` = 2: Right before the system named by the anchor (same phase).
- `Replace` = 3: Takes the anchor's place: the anchor is disabled and this runs right after it. Disposing the handle (or unloading the plugin) re-enables the original.

## SystemRegistry (class, systems)

```csharp
public sealed class SystemRegistry
```

Registers ECS systems in the engine's tick pipeline, scoped to a plugin (released on unload).

- `public SystemHandle Add(string name, Phase phase, SystemCallback callback)`: Adds a system that runs once per tick at the end of `phase` on the tick thread. The registry name is `"{plugin name}:{name}"` (see `SystemRegistry.Register`); the handle's `SystemHandle.Name` reports it.
- `public SystemHandle After(string anchor, string name, SystemCallback callback)`: Registers `name` to run right after the system `anchor`. The anchor is a short name this plugin registered or a full registry name (`SystemHandle.Name`), e.g. `"vanilla:physics"` for an engine system.
- `public SystemHandle Before(string anchor, string name, SystemCallback callback)`: Registers `name` to run right before the system `anchor`. The anchor is a short name this plugin registered or a full registry name (`SystemHandle.Name`).
- `public SystemHandle Register(Phase phase, string name, SystemCallback callback, SystemPlacement placement = SystemPlacement.Append, string? anchor = null)`: Registers a system in the engine's registry. The registry name is namespaced by the plugin: `"{plugin name}:{name}"` (left as is when `name` already starts with that prefix), so plugins cannot collide; a collision with a native system or with another plugin's system throws. `anchor` names an existing (native or scripted) system for After/Before/Replace (the phase is then the anchor's and `phase` is ignored). An anchor is accepted by its short name (a system this plugin registered) or its full name (see `SystemHandle.Name`). Called from inside a system, the registration takes effect when the tick ends.
- `public SystemHandle Replace(string target, string name, SystemCallback callback)`: Replaces the (native or scripted) system `target`: it is disabled and `name` runs in its place. `target` is a short name this plugin registered or a full registry name (`SystemHandle.Name`). Dispose the handle (or unload the plugin) to restore it.
- `public bool SetEnabled(string name, bool enabled)`: Enables or disables a registered system by name (native or scripted). The name is a short name this plugin registered or a full registry name (`SystemHandle.Name`). A system this plugin disabled is re-enabled when the plugin unloads. Returns false if no such system exists.

```csharp
Systems.Add("regen", Phase.Tick, dt => { /* runs every tick */ });
Systems.After("vanilla:physics", "after_physics", dt => { });
var h = Systems.Replace("vanilla:ai", "my_ai", dt => { });   // disables vanilla:ai
h.Dispose();                                                   // restores it
```

## TabEntry (class, display)

```csharp
public sealed class TabEntry
```

The fields of one player's tab entry for one viewer. A null field keeps what it was (see `Tablist.Set`).

- `public TabEntry()`
- `public Component? Display { get; init; }`: The name shown in the list, or null to keep it.
- `public GameMode? GameMode { get; init; }`: The game mode shown, or null to keep it.
- `public int? Latency { get; init; }`: The latency shown in milliseconds (0 to 60000), or null to keep it.
- `public bool? Listed { get; init; }`: Whether the player is listed, or null to keep it.
- `public int? Order { get; init; }`: The order in the list (lower first), or null to keep it.

## Tablist (class, display)

```csharp
public sealed class Tablist
```

The tab list as one viewer sees it: the name, order, latency and game mode of each player in the list, the header and footer, fake entries (extra slots with their own name and skin) and the objectives shown in the list or under the names. Every change is seen by the viewer only. What a plugin sets is restored and removed when the plugin unloads. Reach it as `plugin.Tablist`. Call from the tick thread.

- `public void AddFakeEntry(Player viewer, Guid slotId, string name, string? skinValue, string? skinSignature, int latency, int order, Component? display, bool hat)`: Adds a fake entry: an extra row of the tab list with its own name and skin, for one viewer.
- `public void Header(Player viewer, Component? header, Component? footer)`: Sets the header and footer of the tab list for one viewer. Null (or an empty component) is no header or footer.
- `public void RemoveFakeEntry(Player viewer, Guid slotId)`: Removes a fake entry added with `Tablist.AddFakeEntry`.
- `public void RemoveObjective(Player viewer, string name)`: Removes an objective of one viewer, with its scores.
- `public void ResetScore(Player viewer, string objective, string holder)`: Removes one holder's score from an objective of one viewer.
- `public void Restore(Player viewer, Player target)`: Puts back the server's own tab values of one player for one viewer (name, listed, order, latency and game mode), whoever set them.
- `public void Set(Player viewer, Player target, TabEntry entry)`: Changes how one viewer sees one player in the tab list. A field left null keeps what it was.
- `public void SetObjective(Player viewer, int slot, string name, Component title)`: Defines an objective for one viewer: slot 0 is the tab list (the value after each name), slot 2 is below the name.
- `public void SetScore(Player viewer, string objective, string holder, int score, Component? display, ScoreFormat format, Component? fixedFormat)`: Sets one holder's score on an objective of one viewer.

## Tags (class, other)

```csharp
public static class Tags
```

The datapack tags of the blocks, items, entity types and biomes: `#minecraft:logs` and the like. The tag names are given with or without the leading `#`; the keys are `"namespace:path"` (the namespace may be left out for `minecraft`).

Membership answers what the datapack says, including the tags that name other tags. An unknown tag or key is simply not a member (no exception): a tag lists nothing. Tags are read on the tick thread (the loot service is single threaded). Example: `Tags.Blocks.Contains("minecraft:logs", "minecraft:oak_log")` is true; `BlockState.IsIn("#minecraft:logs")` asks the same of a block state.

## TargetReason (enum, other)

```csharp
public enum TargetReason : byte
```

Why a mob's target changed (EntityTarget). Classified from the server state, not from the goal that chose it.

- `Unknown` = 0: Not classified.
- `ClosestPlayer` = 1: A player was chosen as the target (the nearest-player rules).
- `TargetAttackedEntity` = 2: The entity that last hurt the mob became the target (hurt-by rules).
- `ForgotTarget` = 3: The mob lost its target and the old one is still alive.
- `TargetDied` = 4: The mob lost its target because the old one died or left.
- `Custom` = 5: A plugin set the target (Mob.setTarget through the plugin API). Reported once the plugin target setter lands (batch 1: MobState::script_target).

## TeleportCause (enum, events)

```csharp
public enum TeleportCause : byte
```

Why a player is being teleported.

- `Unknown` = 0: Unknown cause.
- `Command` = 1: /tp or another command.
- `Plugin` = 2: A plugin called Teleport.
- `EnderPearl` = 3: An ender pearl.
- `ChorusFruit` = 4: A chorus fruit.
- `NetherPortal` = 5: A nether portal.
- `EndPortal` = 6: An end portal.
- `EndGateway` = 7: An end gateway.
- `Spectate` = 8: Teleporting to an entity as a spectator.
- `Respawn` = 9: Respawning.
- `Dismount` = 10: Dismounting a vehicle.
- `Enderman` = 11: An enderman teleporting (EnderMan.teleport).
- `Shulker` = 12: A shulker moving to another block (Shulker.teleportSomewhere).
- `TamedOwner` = 13: A tamed animal teleporting to its owner (TamableAnimal.tryToTeleportToOwner).
- `RandomTeleport` = 14: A mob's random teleport (LivingEntity.randomTeleport: a cat's morning gift).

## ClickAction (enum, chat)

```csharp
public enum ClickAction : int
```

What happens when a player clicks a text component.

- `OpenUrl` = 0: Opens a URL (the client asks for confirmation).
- `RunCommand` = 1: Runs a command as the player (the value includes the slash).
- `SuggestCommand` = 2: Puts text into the chat box.
- `CopyToClipboard` = 3: Copies the value to the clipboard.
- `ChangePage` = 4: Changes the page of a book.

## ClickEvent (struct, chat)

```csharp
public readonly struct ClickEvent : IEquatable<ClickEvent>
```

A click event of a text component.

- `public ClickEvent(ClickAction Action, string Value)`: A click event of a text component.
- `public ClickAction Action { get; init; }`: What happens.
- `public string Value { get; init; }`: Url / command / clipboard text / page number.
- `public static ClickEvent ChangePage(int page)`: Change the book page.
- `public static ClickEvent CopyToClipboard(string text)`: Copy text to the clipboard.
- `public static ClickEvent OpenUrl(string url)`: Open a URL.
- `public static ClickEvent RunCommand(string command)`: Run a command as the player ("/spawn").
- `public static ClickEvent SuggestCommand(string command)`: Suggest a command in the chat box.

## Component (class, ecs)

```csharp
public sealed class Component
```

An immutable rich text component (vanilla "text component"): text, translatable text, keybind, score or selector content with a style and children. Every method that "changes" a component returns a new one.

Strings convert implicitly to a literal text component (no formatting codes are interpreted, so user input is safe); use `Component.Legacy` for `&` codes and `Component.Parse` for MiniMessage-style tags.

- `public IReadOnlyList<Component> Children { get; }`: The children appended after this component's own content.
- `public string Content { get; }`: The literal text of a text component (empty for other kinds).
- `public bool IsEmpty { get; }`: Whether this is the empty literal component without style or children.
- `public TextColor TextColour { get; }`: The colour, `TextColor.None` if inherited.
- `public static Component Empty { get; }`: The empty component.
- `public Component Append(Component child)`: Appends a child component.
- `public Component Append(string text)`: Appends literal text.
- `public Component Bold(bool value = true)`: Sets bold.
- `public Component Color(TextColor color)`: Sets the colour.
- `public Component Font(string font)`: Sets the font resource key.
- `public Component Insertion(string text)`: Text inserted into the chat box when the component is shift-clicked.
- `public Component Italic(bool value = true)`: Sets italic.
- `public Component Obfuscated(bool value = true)`: Sets obfuscated (random characters).
- `public Component OnClick(ClickEvent click)`: Sets the click event.
- `public Component OnHover(HoverEvent hover)`: Sets the hover event.
- `public Component Strikethrough(bool value = true)`: Sets strikethrough.
- `public string ToJson()`: Serialises to the vanilla JSON text component form (what the native layer parses).
- `public string ToPlainText()`: The text of this component and its children without formatting (translatable components show their key).
- `public override string ToString()`
- `public Component Tooltip(Component text)`: Shortcut for a text tooltip.
- `public Component Underlined(bool value = true)`: Sets underlined.
- `public static Component FromJson(string json)`: Parses the vanilla JSON form (also a bare JSON string).
- `public static Component FromJson(Utf8String json)`: Parses the vanilla JSON form from engine memory.
- `public static Component Join(params Component[] parts)`: Concatenates components into one (an empty parent with the parts as children).
- `public static Component Join(Component separator, IEnumerable<Component> parts)`: Joins with a separator.
- `public static Component Keybind(string key)`: A component showing the client's key binding (e.g. "key.jump").
- `public static Component Legacy(string text, char prefix = '&')`: Converts text with `&` colour/format codes ("&aGreen &lbold") to a component. `&&` is a literal ampersand, `&#RRGGBB` a custom colour, `&r` resets.
- `public static Component Parse(string markup)`: Parses MiniMessage-style tags: `<red>`, `<#ff8800>`, `<bold>`, `<italic>`, `<underlined>`, `<strikethrough>`, `<obfuscated>`, `<reset>`, `<click:run_command:/cmd>`, `<hover:show_text:'text'>`, and closing tags `</red>`. Unknown tags are kept as text. Escape a literal `<` as `\<`.
- `public static Component Score(string name, string objective)`: A scoreboard score.
- `public static Component Selector(string selector)`: An entity selector (resolved when sent).
- `public static Component Text(string text)`: A literal text component.
- `public static Component Translatable(string key, params Component[] args)`: A translatable component (resolved by the client's language).
- `public static implicit operator Component(string text)`: Literal text converts implicitly.

## HoverEvent (class, chat)

```csharp
public sealed class HoverEvent
```

A hover event of a text component.

- `public static HoverEvent ShowEntity(string entityTypeKey, Uuid uuid, Component? name = null)`: Shows an entity tooltip.
- `public static HoverEvent ShowItem(string itemKey, int count = 1)`: Shows an item tooltip (item key and count; components are not part of v1).
- `public static HoverEvent ShowText(Component text)`: Shows a text tooltip.

## TextColor (struct, chat)

```csharp
public readonly struct TextColor : IEquatable<TextColor>
```

A named or RGB text colour.

- `public bool IsSet { get; }`: Whether a colour is set.
- `public int Rgb { get; }`: The colour as 0xRRGGBB (0 when unset).
- `public static TextColor Aqua { get; }`: Aqua (&b).
- `public static TextColor Black { get; }`: Black (&0).
- `public static TextColor Blue { get; }`: Blue (&9).
- `public static TextColor DarkAqua { get; }`: Dark aqua (&3).
- `public static TextColor DarkBlue { get; }`: Dark blue (&1).
- `public static TextColor DarkGray { get; }`: Dark gray (&8).
- `public static TextColor DarkGreen { get; }`: Dark green (&2).
- `public static TextColor DarkPurple { get; }`: Dark purple (&5).
- `public static TextColor DarkRed { get; }`: Dark red (&4).
- `public static TextColor Gold { get; }`: Gold (&6).
- `public static TextColor Gray { get; }`: Gray (&7).
- `public static TextColor Green { get; }`: Green (&a).
- `public static TextColor LightPurple { get; }`: Light purple (&d).
- `public static TextColor None { get; }`: No colour (inherit).
- `public static TextColor Red { get; }`: Red (&c).
- `public static TextColor White { get; }`: White (&f).
- `public static TextColor Yellow { get; }`: Yellow (&e).
- `public bool Equals(TextColor other)`
- `public override bool Equals(object? obj)`
- `public override int GetHashCode()`
- `public override string ToString()`: The vanilla serialisation: a colour name for the 16 named colours, "#RRGGBB" otherwise.
- `public static TextColor FromRgb(int rgb)`: A custom 24-bit colour.
- `public static TextColor Parse(string text)`: Parses a vanilla colour name ("red") or "#RRGGBB".
- `public static bool operator !=(TextColor a, TextColor b)`: Inequality.
- `public static bool operator ==(TextColor a, TextColor b)`: Equality.

## TickAwaitable (struct, scheduler)

```csharp
public readonly struct TickAwaitable : ICriticalNotifyCompletion, INotifyCompletion
```

The awaitable returned by `Scheduler.ToTick` / `Scheduler.ToAsync`.

- `public bool IsCompleted { get; }`: Awaiter protocol. Only `Scheduler.EnsureTick` completes synchronously (when already on the tick thread); `Scheduler.ToTick` always resumes at the next tick boundary and `Scheduler.ToAsync` always resumes on a pool thread.
- `public TickAwaitable GetAwaiter()`: Awaiter protocol.
- `public void GetResult()`: Awaiter protocol.
- `public void OnCompleted(Action continuation)`: Awaiter protocol.
- `public void UnsafeOnCompleted(Action continuation)`: Awaiter protocol.

## TickTimes (struct, server)

```csharp
public readonly struct TickTimes : IEquatable<TickTimes>
```

The engine's tick time over a window of ticks (`Server.TickTimes`): average (MSPT), max, p95 and p50 in milliseconds, the ticks per second over the window's length, and the last tick.

- `public TickTimes(double AverageMs, double MaxMs, double P95Ms, double P50Ms, double Tps, double LastMs)`: The engine's tick time over a window of ticks (`Server.TickTimes`): average (MSPT), max, p95 and p50 in milliseconds, the ticks per second over the window's length, and the last tick.
- `public double AverageMs { get; init; }`
- `public double LastMs { get; init; }`
- `public double MaxMs { get; init; }`
- `public double P50Ms { get; init; }`
- `public double P95Ms { get; init; }`
- `public double Tps { get; init; }`

## TimeSkipReason (enum, events)

```csharp
public enum TimeSkipReason : byte
```

Why the day time jumped.

- `Commands` = 0: /time set or /time add.
- `NightSkip` = 1: Players slept through the night.
- `Custom` = 2: A plugin.

## TitleTimes (struct, display)

```csharp
public readonly struct TitleTimes : IEquatable<TitleTimes>
```

The timings of a title, in ticks (20 per second).

- `public TitleTimes(int FadeIn, int Stay, int FadeOut)`: The timings of a title, in ticks (20 per second).
- `public int FadeIn { get; init; }`: Time to fade in.
- `public int FadeOut { get; init; }`: Time to fade out.
- `public int Stay { get; init; }`: Time on screen.
- `public static TitleTimes Default { get; }`: The vanilla defaults: 0.5 s in, 3.5 s on screen, 1 s out.
- `public static TitleTimes From(TimeSpan fadeIn, TimeSpan stay, TimeSpan fadeOut)`: Creates timings from durations.

## TntPrimeCause (enum, other)

```csharp
public enum TntPrimeCause : byte
```

What set a TNT block off (TntPrime).

- `Fire` = 0: Fire burned the TNT block (the fire is the priming block).
- `Redstone` = 1: Redstone power reached the TNT block.
- `Player` = 2: A player lit it with flint and steel or a fire charge.
- `Explosion` = 3: Another explosion set it off (a chain reaction).
- `Projectile` = 4: A burning projectile hit it.
- `BlockBreak` = 5: A player broke unstable TNT (the TNT is primed instead of dropped).
- `Dispenser` = 6: A dispenser lit it with flint and steel (the dispenser is the priming block).

## TransformReason (enum, other)

```csharp
public enum TransformReason : byte
```

Why a mob is transformed into another mob (EntityTransform).

- `Unknown` = 0: Anything else (a hoglin becomes a zoglin).
- `Cured` = 1: A zombie villager is cured and becomes a villager.
- `Drowned` = 2: A zombie in water becomes a drowned.
- `Infection` = 3: A zombie infects a villager, or kills one: a zombie villager.
- `Lightning` = 4: A lightning bolt struck it: a pig becomes a zombie pig, a villager a witch.
- `Metamorphosis` = 5: A tadpole grows into a frog.
- `PiglinZombified` = 6: A piglin is zombified.
- `Sheared` = 7: A mooshroom is sheared into a cow.
- `Frozen` = 8: A skeleton is frozen in powder snow and becomes a stray.

## UnleashReason (enum, other)

```csharp
public enum UnleashReason : byte
```

Why a leashed entity is unleashed (EntityUnleash).

- `HolderGone` = 0: The holder (player, knot or mob) is gone or dead.
- `PlayerUnleash` = 1: A player took the lead off or sheared the leash. Never the reason of EntityUnleash: a player action fires PlayerUnleashEntity only.
- `Distance` = 2: The leashed entity is too far from its holder.
- `Unknown` = 3: Any other reason (the dimension changed, a bucket, a copper golem statue, ...).

## Utf8String (struct, primitives)

```csharp
public readonly ref struct Utf8String
```

Borrowed UTF-8 string (McStr). Only valid during the call/dispatch that provided it: call `Utf8String.ToString` to keep a copy.

Event string fields (`Message`, `Username`, ...) are this type: borrowed UTF-8 memory owned by the engine. Only `ToString()` and `Contains()` allocate.

- `public Utf8String(byte* data, long size)`: Wraps native memory (the engine does this; plugins receive these in events).
- `public ReadOnlySpan<byte> Bytes { get; }`: The UTF-8 bytes (borrowed).
- `public bool IsEmpty { get; }`: Whether the string is empty.
- `public int Length { get; }`: Length in bytes.
- `public bool Contains(string text, StringComparison comparison = StringComparison.Ordinal)`: Whether the decoded text contains `text` (allocates).
- `public bool EqualsAscii(ReadOnlySpan<byte> utf8)`: Byte-wise equality with an ASCII/UTF-8 literal (no allocation).
- `public override string ToString()`: Copies the text into a managed string.

```csharp
Events.On((ref PlayerChatEvent e) =>
{
    if (e.Message.IsEmpty) return;                       // no allocation
    if (e.Message.EqualsAscii("!help"u8)) { /* ... */ }  // byte compare, no allocation
    string copy = e.Message.ToString();                  // allocates; safe to keep
});
```

## Uuid (struct, primitives)

```csharp
public readonly struct Uuid : IEquatable<Uuid>
```

Java-style UUID (two big-endian halves).

- `public Uuid(ulong most, ulong least)`: Creates a uuid from its two halves.
- `public readonly ulong Least;`: Least significant 64 bits.
- `public readonly ulong Most;`: Most significant 64 bits.
- `public bool Equals(Uuid other)`: Equality.
- `public override bool Equals(object? obj)`: Equality.
- `public override int GetHashCode()`: Hash code.
- `public override string ToString()`: The canonical 8-4-4-4-12 text form.
- `public static bool operator !=(Uuid a, Uuid b)`: Inequality.
- `public static bool operator ==(Uuid a, Uuid b)`: Equality.

## Vec3d (struct, primitives)

```csharp
public struct Vec3d : IEquatable<Vec3d>
```

A 3D vector of doubles (positions, velocities).

- `public Vec3d(double x, double y, double z)`: Creates a vector.
- `public double X;`: Components.
- `public double Y;`: Components.
- `public double Z;`: Components.
- `public double Length { get; }`: The length of the vector.
- `public static Vec3d Zero { get; }`: The zero vector.
- `public double DistanceTo(Vec3d other)`: The distance to another point.
- `public bool Equals(Vec3d o)`: Equality.
- `public override bool Equals(object? obj)`: Equality.
- `public override int GetHashCode()`: Hash code.
- `public override string ToString()`: Text form.
- `public static bool operator !=(Vec3d a, Vec3d b)`: Inequality.
- `public static Vec3d operator *(Vec3d a, double s)`: Scales the vector.
- `public static Vec3d operator +(Vec3d a, Vec3d b)`: Component-wise sum.
- `public static Vec3d operator -(Vec3d a, Vec3d b)`: Component-wise difference.
- `public static bool operator ==(Vec3d a, Vec3d b)`: Equality.

## VersionConstraint (class, plugins)

```csharp
public static class VersionConstraint
```

Compares dotted versions against constraints (`>=`, `>`, `=`, `<`, `<=`, `^` = same major and at least).

- `public static bool Satisfies(string? constraint, string version)`: Whether a version satisfies a constraint.

## WeatherCause (enum, other)

```csharp
public enum WeatherCause : byte
```

Why the rain or the thunder of a world changes (RainChange, ThunderChange).

- `Command` = 0: /weather.
- `Natural` = 1: The weather cycle.
- `Sleep` = 2: Everyone slept through the night: the weather is cleared.
- `Plugin` = 3: A plugin.
- `Unknown` = 4: Not known.

## WorldSaveCause (enum, other)

```csharp
public enum WorldSaveCause : byte
```

Why a world is saved (WorldSave).

- `Autosave` = 0: The periodic autosave (and the slime save of a runtime world).
- `Command` = 1: /save-all.
- `Shutdown` = 2: The server stops.
- `Plugin` = 3: A plugin (Server.Worlds save).

## BlockState (struct, worlds)

```csharp
public readonly struct BlockState : IEquatable<BlockState>
```

A block state: a block plus its properties, as the numeric id of the global palette. Blittable.

- `public BlockState(uint id)`: Wraps a state id.
- `public readonly uint Id;`: The numeric id in the vanilla block state registry (0 = air).
- `public string BlockKey { get; }`: The block key without properties ("minecraft:oak_stairs").
- `public bool IsAir { get; }`: Whether this is the air block.
- `public string Key { get; }`: The state as text, e.g. "minecraft:oak_stairs[facing=north,half=bottom,shape=straight,waterlogged=false]".
- `public static BlockState Air { get; }`: Air (id 0).
- `public bool Equals(BlockState other)`
- `public override bool Equals(object? obj)`
- `public override int GetHashCode()`
- `public bool IsIn(string tag)`: Whether the block of this state is in a block tag (`Blocks`): `state.IsIn("#minecraft:logs")`.
- `public override string ToString()`
- `public static BlockState Parse(string text)`: Parses "minecraft:stone" or "minecraft:oak_stairs[facing=north]" (missing properties take the block's defaults).
- `public static bool TryParse(string text, out BlockState state)`: Parses a block state text.
- `public static bool operator !=(BlockState a, BlockState b)`: Inequality.
- `public static bool operator ==(BlockState a, BlockState b)`: Equality.

## BlockUpdateFlags (enum, worlds)

```csharp
public enum BlockUpdateFlags : int
```

How a block change is propagated, the update flags of the game's `Level.setBlock` (combine with `|`).

- `None` = 0: Nothing: the block is written silently (no neighbour update, clients are not told). Rarely what you want.
- `Neighbors` = 1: Notify the neighbouring blocks (`neighborChanged`): redstone, doors, plants that lose their support ...
- `Clients` = 2: Send the change to the players tracking the chunk.
- `All` = 3: Neighbours and clients: the default of `World.SetBlock`.
- `NoRender` = 4: Do not re-render the block on the clients (they still get the data).
- `Immediate` = 8: Run the update on the client immediately instead of at the next frame.
- `KnownShape` = 16: The neighbour shape updates (a fence connecting to its neighbours) are skipped: the new state is taken as it is.
- `SuppressDrops` = 32: The old block does not drop its items when it is replaced (for example a flower on a block that is removed).
- `MovedByPiston` = 64: The change is a piston moving the block.
- `SkipShapeUpdateOnWire` = 128: Redstone wire: the shape updates of the wire's neighbours are skipped (the game uses it while it recalculates a wire network).
- `SkipBlockEntitySideEffects` = 256: The old block entity is replaced without its removal side effects (no contents drop).
- `SkipOnPlace` = 512: The new block's placement logic (`onPlace`) does not run.

## ChunkTicket (class, worlds)

```csharp
public sealed class ChunkTicket : IDisposable, IPluginResource
```

A hold on a chunk: while the ticket exists the chunk is kept loaded (and ticking when a player is near enough, like any chunk). Tickets are counted: the chunk unloads after the last holder released it. Create tickets with `plugin.ChunkTickets`; all of a plugin's tickets are released when it unloads.

- `public int ChunkX { get; }`: The chunk x.
- `public int ChunkZ { get; }`: The chunk z.
- `public bool IsActive { get; }`: Whether the ticket is still held.
- `public bool IsLoaded { get; }`: Whether the chunk has finished loading (loading takes a few ticks, and a chunk that has to be generated takes longer).
- `public World World { get; }`: The dimension.
- `public void Dispose()`: Same as `ChunkTicket.Release`.
- `public void Release()`: Gives the chunk back (it may unload after a grace period). Does nothing a second time.

## ChunkTickets (class, worlds)

```csharp
public sealed class ChunkTickets
```

Chunk tickets of one plugin (`plugin.ChunkTickets`): they are released automatically when the plugin unloads or reloads.

- `public const int MaxTicketsPerPlugin = 4096;`: How many chunks one plugin may hold at once (a ticket keeps a chunk in memory and ticking: a runaway loop must not load the whole world).
- `public int ActiveCount { get; }`: How many tickets this plugin currently holds.
- `public ChunkTicket Add(World world, int chunkX, int chunkZ)`: Asks for a chunk to be loaded and kept.
- `public IReadOnlyList<ChunkTicket> AddArea(World world, int centerX, int centerZ, int radius)`: Asks for the chunks of a square around a centre chunk (`radius` 0 is one chunk, 1 is 3 x 3).

## HeightMap (enum, worlds)

```csharp
public enum HeightMap : int
```

Which surface `World.HighestBlockY` looks for.

- `Surface` = 0: The highest block that is not air.
- `OceanFloor` = 1: The highest block of the sea floor: not air and not water.
- `MotionBlocking` = 2: The highest block that stops movement or holds a fluid (what rain and the sun see).
- `MotionBlockingNoLeaves` = 3: Like `HeightMap.MotionBlocking` but leaves do not count.

## WeatherType (enum, worlds)

```csharp
public enum WeatherType : int
```

The weather of a dimension.

- `Clear` = 0: No precipitation.
- `Rain` = 1: Rain (snow in cold biomes).
- `Thunder` = 2: Rain with thunderstorms and lightning.

## World (struct, worlds)

```csharp
public readonly struct World : IEquatable<World>
```

A handle to a world: a vanilla dimension (overworld, nether, end), which lives as long as the server, or a runtime world made by `/world` or `Server.CreateWorldAsync`, which lives until it is unloaded. Blittable. The members are split over several files by topic: blocks and block entities (`World.Blocks.cs`), entities and effects (`World.Entities.cs`), time, weather, spawn and chunk loading (`World.Environment.cs`), runtime worlds (`World.Runtime.cs`). Everything here must be called on the tick thread.

- `public World(int index)`: Wraps a world handle.
- `public readonly int Index;`: The world handle: the slot in the low 16 bits (overworld 0, nether 1, end 2), and for a runtime world the generation of its slot above them, so a handle kept after its world was unloaded never names the world that reuses the slot. The value `ChunkLoadEvent.Dimension` and the other events carry.
- `public WorldBorder Border { get; }`: The world border of this dimension (every dimension has one, runtime worlds included). See `WorldBorder`.
- `public long DayTime { get; set; }`: The time of day clock in ticks since the world was created (a day is 24000 ticks: 0 is sunrise, 6000 noon, 12000 sunset, 18000 midnight). Setting it moves the sun for every player at once and fires `TimeSkipEvent` (reason `Custom`).
- `public bool DayTimePaused { get; set; }`: Whether the day/night clock is stopped (`/time pause`).
- `public float DayTimeRate { get; set; }`: How fast the day/night clock runs (1 = normal, 2 = twice as fast).
- `public IReadOnlyList<Entity> Entities { get; }`: Every entity of the dimension (players, mobs, item entities, experience orbs, projectiles, falling blocks ...). A snapshot: entities that spawn or vanish afterwards are not reflected.
- `public long GameTime { get; }`: The number of ticks the dimension has been running (does not change with `/time set`; 20 per second).
- `public long GeneratedChunkCount { get; }`: How many chunks this dimension has generated from scratch since the server started (chunks loaded from disk do not count): sample it twice to get a generation rate.
- `public WorldGenerator? Generator { get; }`: The generator of a runtime world; null for a vanilla dimension.
- `public bool HasDayTime { get; }`: Whether this world has a day/night clock: the overworld, and the runtime worlds of the overworld type (each with a clock of its own); the nether, the end and their runtime copies have a fixed time.
- `public int Height { get; }`: The build height: the number of block layers (384 in the overworld). The highest block y is `MinY + Height - 1`.
- `public bool IsRaining { get; }`: Whether it is raining (also during a thunderstorm).
- `public bool IsRuntime { get; }`: Whether this is a runtime world (made by `/world create` or `Server.CreateWorldAsync`), not a vanilla dimension.
- `public bool IsTemporary { get; }`: Whether this is a temporary runtime world (in memory only, gone when it is unloaded).
- `public bool IsThundering { get; }`: Whether a thunderstorm is going on.
- `public bool IsValid { get; }`: Whether the handle names a live world of this server (false once a runtime world is unloaded).
- `public string Key { get; }`: The dimension key, e.g. "minecraft:overworld" (empty if the index is invalid).
- `public int LoadedChunkCount { get; }`: How many chunks are loaded in this dimension right now.
- `public int MaxY { get; }`: The highest block y of the dimension.
- `public int MinY { get; }`: The lowest block y of the dimension (-64 in the overworld).
- `public IReadOnlyList<Player> Players { get; }`: The players in the dimension.
- `public int SeaLevel { get; }`: The sea level (63 in the overworld).
- `public Location SpawnLocation { get; }`: The spawn point of the world (where new players appear and where players without a bed respawn) and its facing. The overworld and the runtime worlds have a spawn the game keeps and saves; the nether and the end report a placeholder.
- `public WorldStorage? Storage { get; }`: Where a saved runtime world keeps its data; null for a temporary world and for a vanilla dimension.
- `public WeatherType Weather { get; }`: The weather. The overworld and the runtime worlds of the overworld type have weather (a runtime world its own); the other worlds always report `WeatherType.Clear`.
- `public static World End { get; }`: The end.
- `public static World Nether { get; }`: The nether.
- `public static World Overworld { get; }`: The overworld.
- `public void AddDayTime(long ticks)`: Moves the day time forward (or back with a negative amount).
- `public bool DestroyBlock(BlockPos pos, bool dropItems = true, Entity breaker = default)`: Breaks a block the way the game does (`Level.destroyBlock`): the break particles and sound, optionally the loot, and the block becomes air (or its fluid). A block entity's contents drop too.
- `public Entity DropItem(BlockPos block, ItemStack stack)`: Drops a stack at the centre of a block (the way a broken block drops its loot).
- `public Entity DropItem(Vec3d position, ItemStack stack, bool randomMotion = true)`: Drops an item entity with the full stack (data components included) at a position, like a broken block's drop: a small random sideways drift and a short pickup delay. `EntitySpawnEvent` fires with reason `Plugin` and can cancel it.
- `public bool Equals(World other)`
- `public override bool Equals(object? obj)`
- `public bool Explode(Vec3d position, float power, bool setFire = false, bool breakBlocks = true, Entity source = default)`: Creates an explosion like a creeper's: it damages and pushes entities (with exposure and distance falloff), destroys blocks (dropping loot, with decay), optionally lights fires, and the clients get the particles and the sound.
- `public Task<byte[]> ExportAsync()`: The world as a blob: its blocks, block entities, entities and points of interest in one buffer, the format of slime worlds. Keep it (a file in your data folder, a database) and pass it as `WorldOptions.Template` to make copies: one arena per match. Only temporary and slime worlds can be exported. The world is saved into its storage first. The task completes on the tick thread, a tick or more later.
- `public BlockState GetBlock(BlockPos pos)`: Reads a block state. A position in an unloaded chunk or outside the build height reads as air (check `World.IsBlockLoaded` to tell); the chunk is not loaded by the read.
- `public NbtCompound? GetBlockEntityData(BlockPos pos)`: The data of the block entity at a position (a chest's items, a sign's text, a spawner ...) in the form `/data get block` shows, including the `id` and the position keys.
- `public string? GetGameRule(string name)`: A game rule's value in this world as `/gamerule` shows it: `"true"`, `"false"` or a number. A runtime world has rules of its own for what happens in it (keep_inventory, mob_griefing, random_tick_speed, advance_time ...); the vanilla dimensions share the overworld's. The rules of the server (send_command_feedback, reduced_debug_info, the command limits ...) are the same everywhere.
- `public override int GetHashCode()`
- `public BlockInventory? GetInventory(BlockPos pos)`: The inventory of the block entity at a position (`BlockInventory.At`).
- `public int HighestBlockY(int x, int z, HeightMap kind = HeightMap.MotionBlocking)`: The y of the highest block of a column (read from the chunk's height maps, so it is cheap).
- `public bool IsBlockLoaded(BlockPos pos)`: Whether the block position is inside a loaded chunk and within the build height.
- `public bool IsChunkLoaded(int chunkX, int chunkZ)`: Whether the chunk is loaded and ready (generated, lit, in memory). Blocks of other chunks read as air and cannot be changed.
- `public int PlaySound(string sound, Vec3d position, SoundCategory category = SoundCategory.Master, float volume = 1f, float pitch = 1f)`: Plays a sound at a position to every player of the dimension who is close enough to hear it (16 blocks, or farther for a volume above 1).
- `public Task SaveAsync()`: Saves a runtime world: an anvil world's chunks, entities and points of interest; a slime world's file. Nothing for a temporary world. The task completes on the tick thread, a tick or more later.
- `public bool SetBlock(BlockPos pos, BlockState state, BlockUpdateFlags flags = BlockUpdateFlags.All)`: Changes a block, like `/setblock`: the old block entity goes away, the new block's placement logic and the neighbour updates run (per `flags`), the clients are told. No player event fires (this is not a player placing the block); the engine's own block events (a fire spreading, a piston moving) are the ones the block triggers itself.
- `public bool SetBlock(BlockPos pos, string state, BlockUpdateFlags flags = BlockUpdateFlags.All)`: Changes a block to the state parsed from text (`"minecraft:oak_stairs[facing=north]"`).
- `public bool SetBlockEntityData(BlockPos pos, NbtCompound data, bool merge = true)`: Changes the data of a block entity, like `/data merge block` (or `/data modify` with the full compound). The clients receive the new data.
- `public bool SetGameRule(string name, string value)`: Sets a game rule in this world, like `/gamerule` run there (see `World.GetGameRule` for which worlds share which rules).
- `public bool SetGameRule(string name, bool value)`: Sets a boolean game rule (see `World.SetGameRule`).
- `public bool SetGameRule(string name, int value)`: Sets a numeric game rule (see `World.SetGameRule`).
- `public bool SetSpawn(BlockPos pos, float yaw = 0f, float pitch = 0f)`: Changes the spawn point (like `/setworldspawn`; the clients are told the new position of the compass and of the respawn, and it is saved with the world).
- `public bool SetWeather(WeatherType weather, int? durationTicks = null)`: Sets the weather like `/weather`. The change reaches the players immediately and fires `WeatherChangeEvent`.
- `public Entity SpawnEntity(string type, Vec3d position, NbtCompound? nbt = null, float yaw = 0f, float pitch = 0f)`: Spawns an entity of a type, like `/summon`. Mobs the engine simulates (zombies, cows, creepers ...) get their normal spawn logic: with no NBT their random attributes and equipment are rolled; with NBT the data is applied as given (`{Health:5f,CustomName:'"Bob"'}`). Item entities (`minecraft:item` with `Item:{id:"minecraft:diamond",count:3}`) and experience orbs (`minecraft:experience_orb` with `Value`) are supported too. The spawn fires `EntitySpawnEvent` (reason `Plugin`) where the entity kind has that event, and a handler may cancel it.
- `public int SpawnParticle(string particle, Vec3d position, int count = 1, Vec3d offset = default, float speed = 0f, bool force = false)`: Shows a particle effect at a position to the players of the dimension within 32 blocks (512 with `force`).
- `public override string ToString()`
- `public bool TryGetBlock(BlockPos pos, out BlockState state)`: Reads a block state, saying whether the position was readable.
- `public static bool operator !=(World a, World b)`: Inequality.
- `public static bool operator ==(World a, World b)`: Equality.

## WorldBorder (class, worlds)

```csharp
public sealed class WorldBorder
```

The world border of a dimension: the square the players are meant to stay in (vanilla's `WorldBorder`, the state `/worldborder` changes). Outside it a living entity takes damage per block beyond the safe zone, the collision of entities stops at it, and the clients draw it. Every setter goes through the same engine path as the command: the clients get the matching packets at once, the border is used by the physics from the next tick, and it is saved with the world. Get it from `World.Border`; it is a light view of the dimension's border (nothing is cached), so reading it again is cheap. This is the server's border. The client-only packets `Packets.Clientbound.SetBorderCenter`, `SetBorderSize` and `SetBorderLerpSize` change only what one client draws; they do not move this border.

Everything here must be called on the tick thread (`WrongThreadException` otherwise). Values outside the command's limits throw `ArgumentOutOfRangeException` before anything changes.

- `public const double MaxCenter = 29999984;`: The largest absolute coordinate of the centre (vanilla's absolute maximum, 29,999,984).
- `public const double MaxSize = 59999968;`: The largest size of a border in blocks across (the command's limit, 5.9999968E7).
- `public ValueTuple<double, double> Center { get; set; }`: The centre of the border (x, z) in blocks. Setting it moves the border (like `/worldborder center`); see `WorldBorder.SetCenter`.
- `public double DamagePerBlock { get; set; }`: The damage a living entity takes per block it is outside the border (beyond the `WorldBorder.SafeZone`). Default 0.2. Not negative.
- `public bool IsMoving { get; }`: Whether the border is moving to another size (a `WorldBorder.SetSize` with a lerp time, not finished yet).
- `public long LerpTicksRemaining { get; }`: The ticks left until a moving border reaches its target size (0 when it stands still).
- `public double SafeZone { get; set; }`: The distance inside the border, in blocks, where the damage does not start yet (the damage buffer of `/worldborder damage buffer`). Default 5. Not negative.
- `public double Size { get; }`: The size of the border in blocks across. While the border moves (see `WorldBorder.IsMoving`) this is the size it has reached.
- `public double TargetSize { get; }`: The size the border is moving to, or `WorldBorder.Size` when it stands still.
- `public int WarningDistance { get; set; }`: How many blocks from the border the red warning shows on the client (default 5). Not negative.
- `public int WarningTime { get; set; }`: The warning shown when the border is about to shrink onto a player, in ticks (default 300, 15 seconds). Not negative.
- `public World World { get; }`: The dimension this border belongs to.
- `public bool IsInside(BlockPos pos)`: Whether a block lies inside the border: its x and z (the block's minimum corner) are within the square, as the game decides it (x and z at or after the minimum, before the maximum).
- `public bool IsInside(Location location)`: Whether a position lies inside the border. A location in another dimension is outside this border.
- `public void SetCenter(double x, double z)`: Moves the centre of the border, like `/worldborder center`. The clients are told at once.
- `public void SetSize(double size, long lerpTimeMs = 0)`: Sets the size of the border, like `/worldborder set <size> <time>`. With `lerpTimeMs` 0 the size changes at once (the clients are told the new size); above 0 the border moves to `size` over that time, the size being interpolated every tick (the clients are told the target and the time). The time is rounded up to whole ticks (50 ms each). Setting the current size is a no-op, like the command's "nochange": nothing is sent, and a border that is moving keeps moving (the current size is the interpolated one, so the no-op also holds while a lerp runs).

## WorldGenerator (enum, worlds)

```csharp
public enum WorldGenerator : int
```

How a runtime world makes its terrain.

- `Normal` = 0: The vanilla terrain of the dimension type (overworld, nether or end noise), with its biomes and structures.
- `Flat` = 1: Superflat (overworld type only): `WorldOptions.FlatSettings`, or the classic preset (bedrock, two dirt, grass).
- `Void` = 2: Nothing (overworld type only): every chunk is empty air. Build into it, or start it from a `WorldOptions.Template`.

## WorldOptions (class, worlds)

```csharp
public sealed class WorldOptions
```

What `Server.CreateWorldAsync` makes. Only `WorldOptions.Name` is required.

- `public WorldOptions(string name)`: Options for a world named `name`.
- `public string? DimensionType { get; init; }`: The vanilla dimension type the world uses: `"minecraft:overworld"`, `"minecraft:the_nether"` or `"minecraft:the_end"` (height, sky, portals, fast lava). Null: the template's, or the overworld. Flat and void worlds use the overworld type.
- `public string? FlatSettings { get; init; }`: The superflat layers and options of a `WorldGenerator.Flat` world as vanilla's `generator-settings` JSON (`{"layers":[{"block":"minecraft:stone","height":4}],"biome":"minecraft:plains"}`). Null: the classic preset.
- `public WorldGenerator? Generator { get; init; }`: The terrain. Null: the template's generator, or `WorldGenerator.Normal`.
- `public string Name { get; }`: The world's name (see the constructor).
- `public long? Seed { get; init; }`: The generator's seed. Null: the template's, or a random one.
- `public BlockPos? Spawn { get; init; }`: The world's spawn. Null: the template's, else the top of the layers of a flat world, else 0 64 0.
- `public WorldStorage Storage { get; init; }`: Where a saved world keeps its data (default `WorldStorage.Anvil`). A world made from a `WorldOptions.Template` is temporary or slime.
- `public bool Structures { get; init; }`: Villages, strongholds and the other structures of a normal or flat world (default true).
- `public ReadOnlyMemory<byte>? Template { get; init; }`: A world blob from `World.ExportAsync` or `/world export`: the new world starts as a copy of it (its blocks, block entities, entities). Chunks the blob does not have are made by the generator (a void world: empty). The world must be `WorldOptions.Temporary` or `WorldStorage.Slime`. At most `[worlds] max_blob_mib` MiB.
- `public bool Temporary { get; init; }`: True: the world lives in memory only and is gone when it is unloaded or the server stops (one world per match). `WorldOptions.Storage` is ignored. False: the world is saved and can be loaded again with `Server.LoadWorldAsync`.

## WorldStorage (enum, worlds)

```csharp
public enum WorldStorage : int
```

Where a saved runtime world keeps its data.

- `Anvil` = 0: Region files under `<world>/dimensions/<namespace>/<name>/`, like the vanilla dimensions. Always saved when unloaded.
- `Slime` = 1: One file (`world.rjwb`) in the world's folder, written when the world is saved or unloaded, and every few minutes (`[worlds] slime_save_seconds`). In memory while the world is loaded: for small worlds (lobbies, arenas, templates).

## WrongThreadException (class, scheduler)

```csharp
public sealed class WrongThreadException : InvalidOperationException
```

A game-facing API was called from a thread other than the server tick thread.

Use `Scheduler.RunSync` to hop to the tick thread.

- `public WrongThreadException(string api)`: Creates the exception for the named API.
