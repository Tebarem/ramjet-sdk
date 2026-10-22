# Changelog

Versions of the `Ramjet.Api` and `Ramjet.Templates` packages. While the API is in preview, a minor change can still break plugins;
breaking changes are listed under **Breaking**. `src/Ramjet.Api/Mc.Scripting.Api.surface.txt` lists the full public API, so
`git diff v<old>..v<new> -- src/Ramjet.Api/Mc.Scripting.Api.surface.txt` shows every API change between two versions.

## 1.0.0-preview.6

Fixes for the bugs found by live testing of preview.5 (thank you for the report). Needs a server updated to the same release.

### Fixed
- `World.Explode(..., source: entity)` fires `EntityExplodeEvent` naming the source, also when the source was spawned in the same tick
  (it fired `BlockExplodeEvent` before). An explosion that reaches no block still fires its event.
- `Entity.IsAlive` is false right after `Entity.Despawn()` (as Paper's `isValid()`); a second `Despawn()` returns false.
- `await Scheduler.Delay(n)` resumes on the tick thread, as documented (it resumed on a thread-pool thread, so the next API call threw
  `WrongThreadException`). It still returns a `Task`.

### Changed
- When the plugin worker process ends unexpectedly, the server log now says how (exit code, or the signal, e.g. `SIGKILL` from an
  out-of-memory kill) and shows the last lines the worker wrote to stderr (an unhandled .NET exception, for example). If you saw
  `worker dropped (worker process exited)` with nothing else, the next occurrence explains itself: please send us those lines.
- The "event not answered in time ... (worker busy)" warning names the plugin that was running and the loaded plugins.
- Documentation (as in Paper, these calls fire no event): `Player.GiveExperience` (`PlayerExpChangeEvent` is for experience orbs
  only), setting `Player.FoodLevel`, setting `PlayerInventory.SelectedSlot`, `Server.DispatchCommand` / `DispatchCommandAsync`
  (no `PlayerCommandEvent`), and `World.Explode` fires no `ExplosionPrimeEvent`. `WeatherChangeEvent` fires when the rain becomes
  visible or stops being visible, about 20 ticks after `World.SetWeather` (later when it stops); `RainChangeEvent` is the counterpart
  of Paper's `WeatherChangeEvent` and fires inside the call. `Server.UnloadWorldAsync(world, save: false)` throws for a saved
  (anvil) world, which is always saved when unloaded.

## 1.0.0-preview.5

Fixes for the bugs found by live testing of preview.4 (thank you for the detailed report). Needs a server updated to the same release.

### Fixed
- `Server.CreateWorldAsync` crashed the server on Linux.
- Changes made by a plugin now fire their events, like the same change made by a command: `Player.GameMode`, `Player.Teleport`,
  `Entity.Teleport`, `World.DestroyBlock`, `World.SetWeather`, `World.SpawnEntity` / `World.DropItem` (`EntitySpawn` with reason
  `Plugin`, `LightningStrike`), `LivingEntity.Damage` / `Heal` / `AddEffect` / `Mount` / `Kill`, `World.Explode`, `Menu.Open`. The
  handlers run before the call returns (as in Paper), so a cancel or a changed field takes effect.
- The plugin worker was sometimes restarted ("did not finish its systems of a phase within the tick budget") right after systems were
  registered or reordered (`Systems.After` / `Before` / `Replace`, including an unknown anchor) on a slow machine.
- `Entity.IsAlive` is true for every live entity, not only the ones made with `Ecs.Spawn()`.
- `Player.Absorption` / `LivingEntity.Absorption` can be set (as in Paper, the value is not limited by the `max_absorption` attribute).
- `Player.ExperienceProgress` keeps the exact value it was set to.
- Command aliases of a command that runs on its own (no arguments) work.
- `FilePermissionProvider(path).Save()` writes `path`; it no longer writes the server's own permissions file.
- The "event not answered in time" warning names the plugin.

### Added
- `Scheduler.EnsureTick()`: continues on the tick thread without waiting for the next tick when the code already runs there.
- `Systems.SetEnabled` / `After` / `Before` / `Replace` accept the short name a plugin registered (`"x:count"`) as well as the full
  name (`"<Plugin>:x:count"`).

### Changed
- `ResourcePackHost.Start` (and `Publish` with `content.pack_host_port` set) throws `NotSupportedException`: plugins run in a sandbox
  without network sockets. Host the pack yourself and use `ResourcePackFile.ToResourcePack(url)`.
- Documentation: `Server.DispatchCommand` (not inside events, systems or another plugin's call: use `DispatchCommandAsync`),
  `IInventory.AddItem` / `RemoveItem` return values, `BossBars.Create` names, `World.SaveAsync` on the vanilla dimensions,
  `NametagStyle.Colour` (a 0..15 colour index, not RGB), `ResourcePackBuilder.AddItem` (needs a model id), `Scheduler.ToTick` (always
  the next tick).

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
