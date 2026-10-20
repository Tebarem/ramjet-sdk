# Permissions

Permission nodes, wildcards, defaults, groups, the provider interface, and how vanilla ops map onto them.

## Nodes

A permission is a dotted string: `homes.use`, `homes.set`, `homes.limit.unlimited`. You register the nodes your plugin checks, with a default and a description, so admins can discover them:

```csharp
Permissions.Register("homes.use",   PermissionDefault.True, "Use /home");
Permissions.Register("homes.set",   PermissionDefault.True, "Use /sethome");
Permissions.Register("homes.admin", PermissionDefault.Op,   "Teleport to other players' homes");
```

(or `Permissions.Register(new Permission(node, default, description))`, or declare them in the `permissions` array of [`plugin.json`](../reference/plugin-json.md) so they exist before your plugin loads). Nodes you register are removed again when your plugin unloads.

Check one in code, or let a command do it for you with `.Permission("homes.use")`:

```csharp
if (!player.HasPermission("homes.set")) { player.SendMessage("No permission."); return; }
// or, for a player who may be offline:
bool allowed = Permissions.Has(uuid, "homes.admin");
// the raw tri-state the backend holds, without the defaults:
PermissionValue v = PermissionManager.GetExplicit(uuid, "homes.admin");   // True | False | Unset
```

## Declared in `plugin.json`, in code, or both

A node can be declared in the `permissions` array of [`plugin.json`](../reference/plugin-json.md) (for a source plugin: `[assembly: McPermission(...)]`), with `Permissions.Register` in `OnEnable`, or both. The rules:

- **Manifest first, code second, no error.** The manifest's nodes are registered when the plugin is loaded, before your plugin class is created. If `OnEnable` then registers the same node, the code's registration **replaces** the manifest's: its default and description are the ones in force. Declare each node in one place; the manifest is enough unless the default depends on something you only know in code.
- **A node belongs to one plugin at a time.** If a *different* loaded plugin has already declared the node, the second declaration fails with `permission node 'x' is already declared by plugin 'Name'`: from the manifest this stops the plugin from loading, from code it is an exception in `OnEnable` that has the same effect. The cause is in the log and in `Plugins.Problems`; the first declaration keeps its meaning. Start your nodes with your plugin's id (`homes.use`) so they do not collide. A hot reload of your own plugin takes over its nodes without this error.
- **Both kinds go away together.** Nodes from the manifest and from code are removed when the plugin unloads or is hot-reloaded, and declared again by the new version.
- **Declaring is not required to check or grant.** `HasPermission("x")` on a node nobody declared is allowed, and `permissions.json` can grant any node. An undeclared node that nobody granted is denied, and the node does not appear in listings for admins.

## Defaults

| Default | Meaning |
|---|---|
| `PermissionDefault.True` | everybody |
| `PermissionDefault.False` | nobody, unless granted |
| `PermissionDefault.Op` | vanilla permission level 2 or higher |
| `PermissionDefault.OpLevel(n)` | vanilla level `n` or higher (1 = bypass spawn protection, 2 = gamemaster, 3 = admin, 4 = owner) |
| `PermissionDefault.NotOp` | only non-operators |

In `plugin.json` and `permissions.json` the same values are written `"true"`, `"false"`, `"op"`, `"op:3"` and `"not_op"`.

A node that nobody registered and nobody granted resolves to `false`. The console always has every permission.

## Wildcards and precedence

A grant of `homes.*` matches every node under `homes.`, `*` matches everything. When several entries match, **the most specific wins**: an exact node beats the longest matching wildcard, which beats a shorter one, and `*` comes last. A `false` value is a denial; at the same specificity it beats a grant. In `permissions.json` a key written `"-node"` means the same as `"node": false`.

Entries are looked at in layers: first the player's own entries, then his groups by weight (highest first, with a group's parents after it), then the implicit `default` group everybody belongs to. Within one layer the most specific node wins.

## Groups and per-player overrides

The built-in provider has **groups**: named, weighted, with parents (inheritance), a set of nodes with true/false values and optional prefix/suffix metadata. A **per-player override** (by UUID) can add nodes and change group membership. Everything is stored in `permissions.json` in the server's folder: a readable, hand-editable file, written atomically when it changes and on shutdown. A file with malformed JSON does not stop the server: the provider starts empty, so the registered defaults apply, and the problem is logged until the file is fixed and reloaded.

Plugins can manage the built-in backend through `Permissions.BuiltIn` (a `FilePermissionProvider`): `CreateGroup`, `SetGroupPermission`, `SetPlayerPermission`, `AddToGroup`, `RemoveFromGroup`, `SetGroupParents`, `SetGroupMeta`, `GetPrefix` / `GetSuffix`, `Reload` and `Save`.

## Vanilla ops

Operators keep working exactly as before: the op level of a player (`ops.json`) is the *default value source* for `Op` and `OpLevel` defaults, and **every vanilla command has a node `minecraft.command.<name>`** whose default is `OpLevel(required level)`. When any provider returns an explicit value for such a node it overrides the level check for that command, so `/gamemode` can be granted to a level-0 player or denied to an op. With no explicit values, behaviour is exactly vanilla.

## Replacing the backend

`Permissions.SetProvider(provider)` plugs in another source of truth, for example a database-backed or LuckPerms-style plugin. The interface is:

```csharp
public interface IPermissionProvider
{
    string Name { get; }
    PermissionValue Get(Uuid player, string node);        // True, False, or Unset when you have no opinion
    IReadOnlyCollection<string> Groups(Uuid player);      // group names, for display and admin commands
    event Action<Uuid?>? Changed;                         // raise when permissions changed (null: a group or everything)
}
```

Wildcards and groups are the provider's business: `Get` returns an explicit value or `Unset`, and then the node's registered default decides. Raise `Changed` when something changes so the server re-sends the command trees. The provider is removed when its plugin unloads and the server falls back to the built-in file provider.

## Events

`PermissionChangedEvent` (a player, or `AllPlayers` for a group or everything; consumers re-send command trees or update UI) and `PermissionsReloadedEvent`.

## Admin commands

The sample plugin [`PermissionAdmin`](../../samples/PermissionAdmin/) provides `/perm user <player> set|unset|check ...`, `/perm group ...` and `/perm reload` on top of this API, so a replacement backend can reuse or replace it.

See the [homes guide](../guides/homes-plugin.md) for permissions in a complete plugin.
