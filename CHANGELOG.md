# Changelog

Versions of the `Ramjet.Api` and `Ramjet.Templates` packages. While the API is in preview, a minor change can still break plugins;
breaking changes are listed under **Breaking**. `src/Ramjet.Api/Mc.Scripting.Api.surface.txt` lists the full public API, so
`git diff v<old>..v<new> -- src/Ramjet.Api/Mc.Scripting.Api.surface.txt` shows every API change between two versions.

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
