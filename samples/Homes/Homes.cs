using System.Text.Json;
using System.Text.Json.Serialization;

namespace Homes;

/// <summary>Settings of the Homes plugin (<c>plugins/homes/config.json</c>, written with these defaults on first run).</summary>
public sealed class HomesConfig
{
    /// <summary>How many homes a player may set (players with <c>homes.unlimited</c> are not limited).</summary>
    public int MaxHomes { get; set; } = 3;

    /// <summary>Seconds a player must wait between two teleports (players with <c>homes.cooldown.bypass</c> do not wait).</summary>
    public int CooldownSeconds { get; set; } = 10;

    /// <summary>The sound played to the player after teleporting; empty for none.</summary>
    public string TeleportSound { get; set; } = "minecraft:entity.enderman.teleport";
}

/// <summary>One saved home.</summary>
public sealed class SavedHome
{
    /// <summary>The dimension key.</summary>
    public string World { get; set; } = "minecraft:overworld";
    /// <summary>X.</summary>
    public double X { get; set; }
    /// <summary>Y.</summary>
    public double Y { get; set; }
    /// <summary>Z.</summary>
    public double Z { get; set; }
    /// <summary>Yaw.</summary>
    public float Yaw { get; set; }
    /// <summary>Pitch.</summary>
    public float Pitch { get; set; }
}

/// <summary>The saved homes of every player (<c>homes.json</c>): profile id to home name to home.</summary>
public sealed class HomesData
{
    /// <summary>Homes per player.</summary>
    public Dictionary<string, Dictionary<string, SavedHome>> Players { get; set; } = [];
}

[JsonSerializable(typeof(HomesConfig))]
[JsonSerializable(typeof(HomesData))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class HomesJson : JsonSerializerContext;

/// <summary>Sample: <c>/sethome</c>, <c>/home</c>, <c>/delhome</c>, <c>/homes</c> with a typed config, permission nodes declared in <c>plugin.json</c>, a data file in the plugin's
/// data folder, tab completion of the player's own home names, a cooldown, cross-dimension teleporting and feedback with sound and titles.</summary>
[PluginInfo("Homes", Version = "1.0.0", Author = "Ramjet", Description = "Set homes and teleport back to them")]
public sealed class HomesPlugin : Plugin
{
    private Config<HomesConfig> _config = null!;
    private HomesData _data = new();
    private readonly Dictionary<Uuid, ulong> _lastTeleport = [];

    protected override void OnEnable()
    {
        _config = Config.Load(HomesJson.Default.HomesConfig);
        if (Config.ReadText("homes.json") is { } json)
        {
            try
            {
                _data = JsonSerializer.Deserialize(json, HomesJson.Default.HomesData) ?? new HomesData();
                // System.Text.Json builds ordinal dictionaries: home names compare without regard to case, as /home Base and /home base should
                foreach (var key in _data.Players.Keys.ToList())
                    _data.Players[key] = new Dictionary<string, SavedHome>(_data.Players[key], StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException e) { Log.Error("homes.json is invalid, starting with no homes (the file is kept)", e); }
        }

        Commands.Register(Command.Literal("sethome").Permission("homes.use")
            .Executes(ctx => SetHome(ctx, "home"))
            .Then(Command.Argument("name", Arg.Word()).Suggests(OwnHomes).Executes(ctx => SetHome(ctx, ctx.GetString("name")))));

        Commands.Register(Command.Literal("home").Permission("homes.use")
            .Executes(ctx => GoHome(ctx, "home"))
            .Then(Command.Argument("name", Arg.Word()).Suggests(OwnHomes).Executes(ctx => GoHome(ctx, ctx.GetString("name")))),
            description: "Teleport to one of your homes");

        Commands.Register(Command.Literal("delhome").Permission("homes.use")
            .Then(Command.Argument("name", Arg.Word()).Suggests(OwnHomes).Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                var homes = HomesOf(player, create: false);
                if (homes is null || !homes.Remove(ctx.GetString("name"))) throw new CommandFailure($"You have no home called '{ctx.GetString("name")}'");
                Save();
                ctx.Reply($"Deleted home '{ctx.GetString("name")}'");
                return 1;
            })));

        Commands.Register(Command.Literal("homes").Permission("homes.use").Executes(ctx =>
        {
            var player = ctx.RequirePlayer();
            var homes = HomesOf(player, create: false);
            if (homes is null || homes.Count == 0) throw new CommandFailure("You have no homes. Use /sethome <name>");
            foreach (var (name, home) in homes.OrderBy(h => h.Key))
                ctx.Reply(Component.Text($"{name}: ").Color(TextColor.Gold).Append(Component.Text($"{home.X:0} {home.Y:0} {home.Z:0} ({home.World})").Color(TextColor.Gray)));
            return homes.Count;
        }));
    }

    private IEnumerable<string> OwnHomes(CommandContext ctx)
        => ctx.Player is { } p && HomesOf(p, create: false) is { } homes ? homes.Keys.Order() : [];

    private Dictionary<string, SavedHome>? HomesOf(Player player, bool create)
    {
        string key = player.Uuid.ToString();
        if (_data.Players.TryGetValue(key, out var homes)) return homes;
        return create ? _data.Players[key] = new Dictionary<string, SavedHome>(StringComparer.OrdinalIgnoreCase) : null;
    }

    private void Save() => Config.WriteText("homes.json", JsonSerializer.Serialize(_data, HomesJson.Default.HomesData));

    private int SetHome(CommandContext ctx, string name)
    {
        var player = ctx.RequirePlayer();
        var homes = HomesOf(player, create: true)!;
        bool exists = homes.ContainsKey(name);
        if (!exists && homes.Count >= _config.Value.MaxHomes && !player.HasPermission("homes.unlimited"))
            throw new CommandFailure($"You can have at most {_config.Value.MaxHomes} homes. Delete one with /delhome <name>");
        var at = player.Location;
        homes[name] = new SavedHome { World = at.World.Key, X = at.X, Y = at.Y, Z = at.Z, Yaw = at.Yaw, Pitch = at.Pitch };
        Save();
        ctx.Reply(Component.Text(exists ? "Moved home " : "Set home ").Color(TextColor.Green).Append(Component.Text(name).Color(TextColor.Yellow)));
        return 1;
    }

    private int GoHome(CommandContext ctx, string name)
    {
        var player = ctx.RequirePlayer();
        var homes = HomesOf(player, create: false);
        if (homes is null || !homes.TryGetValue(name, out var home))
            throw new CommandFailure(name == "home" ? "You have no home yet. Use /sethome" : $"You have no home called '{name}'");

        ulong now = Server.CurrentTick;
        if (!player.HasPermission("homes.cooldown.bypass") && _lastTeleport.TryGetValue(player.Uuid, out ulong last))
        {
            long waitTicks = _config.Value.CooldownSeconds * 20L - (long)(now - last);
            if (waitTicks > 0) throw new CommandFailure($"Wait {(waitTicks + 19) / 20} more seconds before teleporting again");
        }

        var world = Server.GetWorld(home.World) ?? player.World;
        if (!player.Teleport(new Location(world, home.X, home.Y, home.Z, home.Yaw, home.Pitch)))
            throw new CommandFailure("The teleport was cancelled");
        _lastTeleport[player.Uuid] = now;
        if (_config.Value.TeleportSound.Length > 0) player.PlaySound(_config.Value.TeleportSound, SoundCategory.Players);
        player.ShowTitle(Component.Text("Home").Color(TextColor.Green), Component.Text(name).Color(TextColor.Yellow), TitleTimes.Default);
        return 1;
    }
}
