# A homes plugin with config and permissions

A complete /home /sethome /delhome /homes plugin with typed config, permissions, a data file, cooldowns and Brigadier commands.

Plugin: **Homes**. Players save named homes and teleport back to them. The finished sample, with sounds and titles added, is in [`samples/Homes`](../../samples/Homes/).

| Command | Does | Permission |
|---|---|---|
| `/sethome [name]` | saves the current position as `name` (default `home`) | `homes.set` |
| `/home [name]` | teleports to a home, with a cooldown | `homes.use` |
| `/delhome <name>` | deletes a home | `homes.set` |
| `/homes` | lists your homes | `homes.use` |

Limit: `MaxHomes` per player from config; the permission `homes.unlimited` (default: operators) lifts it.

## Manifest

```json
{
  "id": "homes",
  "name": "Homes",
  "version": "1.0.0",
  "api": "1.0",
  "main": "Homes.PluginMain",
  "authors": ["you"],
  "description": "Set and teleport to homes.",
  "permissions": [
    { "node": "homes.use",       "default": "true", "description": "Use /home and /homes" },
    { "node": "homes.set",       "default": "true", "description": "Use /sethome and /delhome" },
    { "node": "homes.unlimited", "default": "op",   "description": "No limit on the number of homes" }
  ]
}
```

Declaring the permissions in the manifest makes them exist (with defaults and descriptions) before your code runs, so a permissions plugin can list and grant them even if yours failed to load. (You can also declare them in code with `Permissions.Register(...)`.)

## Config and data types

```csharp
using System.Text.Json.Serialization;

namespace Homes;

/// <summary>plugins/homes/config.json: written with these defaults on first start, comments allowed.</summary>
public sealed class HomesConfig
{
    public int MaxHomes { get; set; } = 3;
    public int CooldownSeconds { get; set; } = 5;
}

/// <summary>One saved location (world key, coordinates and view direction).</summary>
public sealed record Home(string World, double X, double Y, double Z, float Yaw, float Pitch);

// System.Text.Json source generation: no reflection at run time
[JsonSerializable(typeof(HomesConfig))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, Home>>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class HomesJson : JsonSerializerContext { }
```

## The plugin

```csharp
using System.Text.Json;

namespace Homes;

public sealed class PluginMain : Plugin
{
    private Config<HomesConfig> _config = null!;
    private Dictionary<string, Dictionary<string, Home>> _homes = new();   // player uuid -> name -> home
    private readonly Dictionary<Uuid, ulong> _lastTeleportTick = new();

    protected override void OnEnable()
    {
        _config = Config.Load(HomesJson.Default.HomesConfig);               // creates config.json with the defaults
        _homes = LoadHomes();

        Commands.Register(Command.Literal("sethome")
            .Permission("homes.set")
            .Executes(ctx => SetHome(ctx, "home"))
            .Then(Command.Argument("name", Arg.Word())
                .Executes(ctx => SetHome(ctx, ctx.GetString("name")))),
            description: "Save your position as a home");

        Commands.Register(Command.Literal("home")
            .Permission("homes.use")
            .Executes(ctx => GoHome(ctx, "home"))
            .Then(Command.Argument("name", Arg.Word())
                .Suggests(OwnHomeNames)                                      // tab completion of the player's own homes
                .Executes(ctx => GoHome(ctx, ctx.GetString("name")))),
            aliases: ["h"], description: "Teleport to a home");

        Commands.Register(Command.Literal("delhome")
            .Permission("homes.set")
            .Then(Command.Argument("name", Arg.Word())
                .Suggests(OwnHomeNames)
                .Executes(DeleteHome)),
            description: "Delete a home");

        Commands.Register(Command.Literal("homes")
            .Permission("homes.use")
            .Executes(ListHomes),
            description: "List your homes");

        // forget the cooldown of a player who left
        Events.On((ref PlayerQuitEvent e) => _lastTeleportTick.Remove(e.Uuid));
        Log.Info($"enabled, limit {_config.Value.MaxHomes} homes");
    }

    // ---- commands -----------------------------------------------------------------------------------------

    private int SetHome(CommandContext ctx, string name)
    {
        Player player = ctx.RequirePlayer();
        var mine = HomesOf(player);

        bool replacing = mine.ContainsKey(name);
        int limit = _config.Value.MaxHomes;
        if (!replacing && mine.Count >= limit && !player.HasPermission("homes.unlimited"))
            throw new CommandFailure($"You already have {mine.Count} homes (limit {limit}). Delete one with /delhome.");

        Location at = player.Location;
        mine[name] = new Home(at.World.Key, at.X, at.Y, at.Z, at.Yaw, at.Pitch);
        Save();
        ctx.Reply(replacing ? $"Home '{name}' moved." : $"Home '{name}' saved.");
        return mine.Count;                                                    // command result
    }

    private int GoHome(CommandContext ctx, string name)
    {
        Player player = ctx.RequirePlayer();
        if (!HomesOf(player).TryGetValue(name, out var home))
            throw new CommandFailure($"You have no home called '{name}'. Use /homes to list them.");

        ulong now = Server.CurrentTick;
        long cooldown = _config.Value.CooldownSeconds * 20L;
        if (_lastTeleportTick.TryGetValue(player.Uuid, out ulong last) && (long)(now - last) < cooldown)
            throw new CommandFailure($"Wait {(cooldown - (long)(now - last) + 19) / 20} more seconds.");

        World? world = Server.GetWorld(home.World);                          // null when that dimension does not exist
        if (world is null) throw new CommandFailure($"The world '{home.World}' is not loaded.");

        if (!player.Teleport(new Location(world.Value, home.X, home.Y, home.Z, home.Yaw, home.Pitch)))
            throw new CommandFailure("The teleport was cancelled.");          // another plugin vetoed PlayerTeleportEvent
        _lastTeleportTick[player.Uuid] = now;
        ctx.Reply($"Welcome home, '{name}'.");
        return 1;
    }

    private int DeleteHome(CommandContext ctx)
    {
        Player player = ctx.RequirePlayer();
        string name = ctx.GetString("name");
        if (!HomesOf(player).Remove(name)) throw new CommandFailure($"You have no home called '{name}'.");
        Save();
        ctx.Reply($"Home '{name}' deleted.");
        return 1;
    }

    private int ListHomes(CommandContext ctx)
    {
        Player player = ctx.RequirePlayer();
        var mine = HomesOf(player);
        ctx.Reply(mine.Count == 0 ? "You have no homes. Use /sethome." : $"Homes ({mine.Count}): {string.Join(", ", mine.Keys)}");
        return mine.Count;
    }

    // ---- data -----------------------------------------------------------------------------------------------

    private Dictionary<string, Home> HomesOf(Player player)
    {
        string key = player.Uuid.ToString();
        if (!_homes.TryGetValue(key, out var mine)) _homes[key] = mine = new Dictionary<string, Home>(StringComparer.OrdinalIgnoreCase);
        return mine;
    }

    private IEnumerable<string> OwnHomeNames(CommandContext ctx)
        => ctx.Player is { } p ? HomesOf(p).Keys : [];

    private Dictionary<string, Dictionary<string, Home>> LoadHomes()
    {
        if (Config.ReadText("homes.json") is not { } json) return new();
        try
        {
            var loaded = JsonSerializer.Deserialize(json, HomesJson.Default.DictionaryStringDictionaryStringHome) ?? new();
            // JSON objects come back with case-sensitive keys; /home Base and /home base should be the same home
            return loaded.ToDictionary(p => p.Key, p => new Dictionary<string, Home>(p.Value, StringComparer.OrdinalIgnoreCase));
        }
        catch (JsonException e)
        {
            Log.Error("homes.json is damaged, starting empty (the file is kept as homes.json.bad)", e);
            Config.WriteText("homes.json.bad", json);
            return new();
        }
    }

    // atomic write into the plugin's data folder: temp file + rename
    private void Save() =>
        Config.WriteText("homes.json", JsonSerializer.Serialize(_homes, HomesJson.Default.DictionaryStringDictionaryStringHome));
}
```

## The command tree you just built

This is the tree the server sends to the client for the four commands (arguments in angle brackets):

```
/sethome [<name>]          requires homes.set
/home [<name>]  (alias /h)  requires homes.use   <name> completes to your own homes
/delhome <name>            requires homes.set   <name> completes to your own homes
/homes                     requires homes.use
```

## How it behaves

- **Permissions**: with no permissions plugin, defaults apply: everyone can use `/home`, `/sethome`; only operators have `homes.unlimited`. A permissions backend can grant or deny any node per player or group, and the commands appear or disappear in the client's command tree accordingly.
- **Config**: edit `plugins/homes/config.json`; call `_config.Reload()` (or set `_config.Watch = true`) to pick up edits: `Config<T>` keeps a new immutable snapshot (a parse error keeps the previous values and logs the line). Values are read through `_config.Value` at the time of use, so changes apply without a restart.
- **Hot reload**: rebuild the plugin and the commands, permissions and handlers are re-registered by the new `OnEnable`; the homes are re-read from disk; the cooldown dictionary starts empty (it is not worth persisting). Nothing is lost because `Save()` writes through on every change.
- **Why `Server.CurrentTick`** for the cooldown: the timer follows the server's tick rate, so a lagging server does not let players teleport more often than intended.
- **Why `Dictionary<string, ...>` keyed by the UUID string**: JSON object keys are strings; the UUID is the stable identity of a player (names change).

## Next

- Add `/sethome` confirmation as a boss bar or title: [boss bars and titles](boss-bars-and-titles.md).
- Show homes in a sidebar: [scoreboard sidebar](scoreboard-sidebar.md).
- Expose `GetHomes(player)` to other plugins: [inter-plugin services](inter-plugin-services.md).
