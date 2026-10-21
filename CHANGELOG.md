# Changelog

Versions of the `Ramjet.Api` and `Ramjet.Templates` packages. While the API is in preview, a minor change can still break plugins;
breaking changes are listed under **Breaking**. `src/Ramjet.Api/Mc.Scripting.Api.surface.txt` lists the full public API, so
`git diff v<old>..v<new> -- src/Ramjet.Api/Mc.Scripting.Api.surface.txt` shows every API change between two versions.

## 1.0.0-preview.4

A large step toward Paper's API: 155 new events (236 in all) and the APIs plugin authors asked for most.

### Added: APIs
- **Running commands:** `Server.DispatchCommand(line)` (as the console) and `Server.DispatchCommand(Player, line)` (with the player's
  permissions) return a `CommandResult` (success, result count, error). `Server.DispatchCommandAsync(...)` queues the line and runs it
  after the plugins' tick, so it can also run other plugins' commands.
- **Living entities:** `Entity.AsLiving()` / `Player.AsLiving()` give a `LivingEntity`: health, max health, absorption, heal, damage
  (by damage type and attacker), potion effects, attributes and modifiers, equipment and drop chances, silent / invulnerable /
  glowing / invisible, no AI, persistence, custom name, attack target, riding (mount, dismount, eject, passengers).
- **Administration:** `Server.Operators` (add, remove, check, list, offline players included) and `Server.Whitelist` (enabled,
  enforced, add, remove, reload).
- **World border:** `World.Border`: the server-side border (centre, size and lerp, damage, safe zone, warnings, `IsInside`).
- **Loot and tags:** `LootTables.Generate(key, LootContext)`, `Exists`, `Keys`, `FillContainer`; `Tags.Blocks | Items | EntityTypes |
  Biomes` (`Contains`, `Values`), `BlockState.IsIn`, `ItemStack.IsIn`.
- `EquipmentSlot` (`MainHand`, `Feet`, `Legs`, `Chest`, `Head`, `OffHand`, `Body`, `Saddle`) is shared by the living-entity API and the
  events.

### Added: events
Explosions and TNT, projectiles and bows, vehicles and mounts, item entities, pickups and XP, health, potion effects and knockback,
taming, breeding, transformations and sitting, mob targeting and natural spawning, mobs changing blocks, leashes, dispensers and
hoppers, furnaces, brewing, smithing, stonecutters, anvils and trading, buckets and shearing, item frames, armor stands and placed
entities, signs, books and lecterns, food, fishing and item durability, beds, spawn points and player state, worlds, weather and
lightning, block physics, fluids and growth, raids, chunks and structures, portals, teleports and entity removal, piglins, bells,
beacons and sculk, unknown commands, tab completion, the command list, the server-full check, and the server list ping (icon, version
name, shown protocol, player sample). The full list with Paper equivalents is on the
[events compared with Paper](docs/reference/events-compared-with-paper.md) page.

### Changed
- `EntityExplode` / `BlockExplode` follow Paper: a cancel protects the blocks; entities are still damaged.
- `EntityDeath` is cancellable and has mutable `DropItems` / `DropExperience`; `PlayerInteractEntity` has `ClickPos`; `PlayerKick`
  has a `Cause`; `EntityCombust` has the source block; `EntitySpawn` has the `TrialSpawner` reason; `PlayerChat` routes expose their
  audience.

### Breaking
- The constructors of `EntityDeath`, `PlayerKick`, `PlayerInteractEntity`, `EntityCombust` and `PlayerChat` gained parameters (only
  plugins that construct these events themselves are affected).

## 1.0.0-preview.3

The API the first-party plugins (chat, essentials, pvp, worldedit) use was missing from preview.2, so they did not compile against
the package. It is all public now.

### Added
- Bulk region edits run by the server in slices of the tick: `Edit.Start(player, request)` queues a set, replace, walls, faces, copy,
  cut or paste; `Edit.GetJob`, `Edit.Cancel`, `Edit.Undo`, `Edit.Redo`, `Edit.History`, `Edit.ClearHistory`, `Edit.ClipboardInfo`,
  `Edit.ReleaseClipboard` and `Edit.Memory`, with `EditJobState`, `EditJobInfo`, `EditHistory`, `EditClipboardInfo` and `EditMemory`.
  Clipboards and undo history stay in the server, per player, within fixed memory budgets.
- A native PvP gate: `Player.SetPvpPolicy(PvpFlags, tagTicks)` and `Player.PvpPolicyFlags`. A player without a policy keeps the
  vanilla rules, so a server without a PvP plugin is unchanged.
- Combat tags: `Combat.Tag`, `Combat.Untag`, `Combat.Remaining` and `Combat.Opponent`, and the events `CombatTagStartEvent` and
  `CombatTagEndEvent` (with `CombatTagEndReason`).
- `PlayerPreQuitEvent`: fired before a leaving player is removed, while its inventory and position can still be read; a handler can
  replace the quit message.
- The private-message service contract `Mc.Scripting.Services.IPrivateMessages` and `PrivateMessageResult`, so a chat plugin can
  own `/msg` delivery for a command plugin.

A plugin that uses these needs a server that has them: the server supplies the API at run time, so on an older server the plugin
fails where it first touches them.

## 1.0.0-preview.2

### Added
- Runtime worlds: `Server.CreateWorldAsync(WorldOptions)`, `Server.LoadWorldAsync(name)` and
  `Server.UnloadWorldAsync(world, save, movePlayers)`. `WorldOptions` sets the name, seed, generator (`WorldGenerator.Normal`, `Flat`,
  `Void`), flat settings, dimension type, structures, spawn, storage (`WorldStorage.Anvil`, `Slime`), a template to copy and whether
  the world is temporary.
- `World.SaveAsync()`, `World.ExportAsync()`, `World.IsRuntime`, `World.IsTemporary`, `World.Generator`, `World.Storage`.
- Game rules per world: `World.GetGameRule(name)` and `World.SetGameRule(name, bool | int | string)`.
- Sample [`Arenas`](samples/Arenas/): temporary arena worlds copied from a slime template.
- This changelog.

### Docs
- The data folder is `plugins/<id>/data/`, not `plugins/<id>/`. It is the only place a plugin can write: the plugin worker is
  sandboxed, and writes anywhere else fail with `UnauthorizedAccessException`. The old text said plain `System.IO` could write
  outside it.
- Command names are given without the leading slash, because the client adds it: `Command.Literal("set")` is `/set`, and a literal
  `"/set"` is typed as `//set`.

### Server fixes that reach plugin authors
These ship with the server, not the package; hosted servers get them on their next restart.
- Source plugins in a folder named after their id load: the server now creates and grants `plugins/<id>/data/` for them, where before
  they stayed unloaded until the folder was created by hand.
- Plugin commands with a permission or requirement no longer stay hidden from a player's command list (red, no completion) after a
  slow plugin answer: the list is recomputed on a later tick.
- `/pl` is an alias of `/plugins`.
- A failed hot reload says why in its warning (the exception and a named reason) instead of a numeric code.
- The plugin worker starts with any number of plugins (it failed with about 31 or more).
- Clients no longer crash on join near a llama that is spitting.

## 1.0.0-preview.1

First preview of `Ramjet.Api` and `Ramjet.Templates`.
