# Administration

Who may join, who may run the operator commands, and who is kept out: the operator list, the whitelist and the ban lists. The four lists are the server's own files (`ops.json`, `whitelist.json`, `banned-players.json`, `banned-ips.json`), so a change made by a plugin is the same change `/op`, `/whitelist` or `/ban` makes, and the server enforces it the same way.

## Login order

A player who connects is checked in this order, and the first refusal is the one they see:

1. Their own ban (`banned-players.json`), with its reason and expiry.
2. The whitelist, when it is on. Operators are on it whether or not they are listed.
3. The address ban (`banned-ips.json`).
4. The player limit. An operator with `bypassesPlayerLimit` may join a full server.

## Operators

`Server.Operators` reads and changes the operator list. An operator has a permission level (1 moderator, 2 gamemaster, 3 admin, 4 owner) that decides which commands the player sees. The level is what `/op` grants: pass one to choose it, or leave it out for the server's `op-permission-level`.

```csharp
// An offline player: the server knows the uuid of every name it has seen.
Uuid? id = OfflinePlayers.Find("Notch");
if (id is { } uuid)
{
    Server.Operators.Add(uuid, "Notch", level: 2);   // gamemaster
}

// An online player: the uuid is on the Player.
Server.Operators.Add(player.Uuid, player.Name);       // the server's op-permission-level

OperatorEntry? entry = Server.Operators.Check(player.Uuid);
foreach (OperatorEntry op in Server.Operators.All())
    Log.Info($"{op.Name}: level {op.Level}");

Server.Operators.Remove(player.Uuid);                 // the same as /deop
```

The rules are the commands' rules:

* A change of an online player's level sends that player the new permission level and the new command tree, at once.
* Removing an operator kicks the unlisted players when enforcement and the whitelist are both on.
* `Add` with the entry already there changes nothing and returns `false`. A new operator beyond `access.max_operators` is refused. The name must be 1 to 16 characters of `A-Z`, `a-z`, `0-9` and `_`; a bad name or a level outside 1 to 4 is an error.
* `bypassesPlayerLimit: true` lets an operator join a full server. Leave it out (`null`) and an existing operator keeps its flag; a new one starts without it.

## Whitelist

`Server.Whitelist` is the list of players who may join while the whitelist is on, and two switches: `Enabled` (`white-list`) and `Enforced` (`enforce-whitelist`).

```csharp
Server.Whitelist.Enabled = true;     // only listed players and operators may join
Server.Whitelist.Enforced = true;    // unlisted players already online are kicked, now and after each change

Server.Whitelist.Add(player.Uuid, player.Name);
bool listed = Server.Whitelist.Contains(player.Uuid);
Server.Whitelist.Remove(player.Uuid);   // kicks that player when enforcement is on (and the whitelist is on)
Server.Whitelist.Reload();              // read whitelist.json again after the file changed outside the server
```

The switches are written to `server.properties`, as `/whitelist on` and `/whitelist off` are. Turning enforcement on kicks the unlisted players at once, but only when the whitelist is on: with the whitelist off there is nothing to enforce. Adding a player kicks nobody.

Enforcement looks at the list alone: with `Enforced` on, an operator who is not on the list is kicked too. The login is the only place an operator is let in without being listed.

## Bans

Bans go through `IBanList`, the service the server registers. Ask for it with `Services`, so a moderation plugin that replaces the built-in list is used by everyone who asks.

```csharp
var bans = Services.Get<IBanList>();

// Ban for a day, with a reason shown on the disconnect screen.
bans.AddPlayer(player.Uuid, player.Name, "griefing", DateTimeOffset.UtcNow.AddDays(1), source: "AntiGrief");
player.Kick("You are banned for griefing.");     // a ban does not kick: the plugin does

bans.AddAddress("203.0.113.7", reason: "botnet", expires: null, source: "AntiGrief");
BanEntry? ban = bans.Check(player.Uuid, null);   // the player's ban; pass the login's address to check that too
```

Ban writes reach the file within a few seconds, not at once, as with the server's own `/ban`. A ban ends when its expiry passes; `expires: null` is forever.

## What is saved, and when

Operators and the two whitelist switches are written when the change is made. A plugin's whitelist entries and bans are written a few seconds later, and again at each world save; `Reload()` writes the pending entries before it reads the file back. A write that fails stays pending and is retried. A plugin never has to save these lists; it only has to make the change. Every change a plugin makes is logged with the plugin's id, and a new operator or whitelist entry beyond the list's quota (`access.max_operators`, `access.max_whitelist_entries`, `access.max_whitelist_bytes`) is refused.

## Offline players

Every call that takes a uuid works for a player who is not online. To find one from a name, use `OfflinePlayers.Find(name)`, which knows every name the server has seen. `OfflinePlayers.Get(uuid)` gives the first join, the last time they were seen and the saved position.
