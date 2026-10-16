using System.Text.Json.Serialization;

namespace Kits;

/// <summary>A status effect a kit grants.</summary>
public sealed class KitEffect
{
    /// <summary>The effect key, e.g. <c>minecraft:strength</c>.</summary>
    public string Effect { get; set; } = "minecraft:regeneration";
    /// <summary>How long it lasts in seconds.</summary>
    public int Seconds { get; set; } = 30;
    /// <summary>The level minus one.</summary>
    public int Amplifier { get; set; }
}

/// <summary>One kit.</summary>
public sealed class KitConfig
{
    /// <summary>Seconds a player must wait before claiming this kit again.</summary>
    public int CooldownSeconds { get; set; } = 600;
    /// <summary>Experience levels granted.</summary>
    public int ExperienceLevels { get; set; }
    /// <summary>Food level restored (0 leaves hunger alone).</summary>
    public int Food { get; set; }
    /// <summary>Status effects granted.</summary>
    public List<KitEffect> Effects { get; set; } = [];
    /// <summary>The message shown when the kit is claimed.</summary>
    public string Message { get; set; } = "Kit claimed!";
}

/// <summary>The settings of the Kits plugin: the kits by name. Item kits arrive with the inventory API; this version grants effects, experience and food.</summary>
public sealed class KitsConfig
{
    /// <summary>The kits.</summary>
    public Dictionary<string, KitConfig> Kits { get; set; } = new()
    {
        ["starter"] = new KitConfig
        {
            CooldownSeconds = 3600,
            Food = 20,
            ExperienceLevels = 3,
            Effects = [new KitEffect { Effect = "minecraft:regeneration", Seconds = 20 }, new KitEffect { Effect = "minecraft:speed", Seconds = 60 }],
            Message = "Welcome kit: food, three levels and a short boost",
        },
        ["warrior"] = new KitConfig
        {
            CooldownSeconds = 1800,
            Effects = [new KitEffect { Effect = "minecraft:strength", Seconds = 120, Amplifier = 1 }, new KitEffect { Effect = "minecraft:resistance", Seconds = 120 }],
            Message = "Warrior kit: strength and resistance for two minutes",
        },
    };
}

[JsonSerializable(typeof(KitsConfig))]
internal sealed partial class KitsJson : JsonSerializerContext;

/// <summary>Sample: <c>/kit &lt;name&gt;</c> with kits defined in the config, per-kit permission nodes declared at runtime from the config (<c>kits.kit.&lt;name&gt;</c>), cooldowns
/// measured in server ticks, tab completion that only offers the kits the player may claim, and a live config reload.</summary>
[PluginInfo("Kits", Version = "1.0.0", Author = "Ramjet", Description = "Claimable kits with cooldowns")]
public sealed class KitsPlugin : Plugin
{
    private Config<KitsConfig> _config = null!;
    private readonly Dictionary<(Uuid, string), ulong> _claimed = [];

    protected override void OnEnable()
    {
        _config = Config.Load(KitsJson.Default.KitsConfig);
        DeclarePermissions();
        _config.Changed += _ => DeclarePermissions();  // new kits get their node without a restart
        _config.Watch = true;

        Permissions.Register("kits.reload", PermissionDefault.OpLevel(3), "Reload the kits configuration");

        Commands.Register(Command.Literal("kit")
            .Then(Command.Argument("name", Arg.Word()).Suggests(ctx => ctx.Player is { } p ? Available(p) : _config.Value.Kits.Keys).Executes(Claim))
            .Then(Command.Literal("reload").Permission("kits.reload").Executes(ctx =>
            {
                ctx.Reply(_config.Reload() ? "Kits reloaded" : "kits config is invalid, see the log");
                return 1;
            })), description: "Claim a kit");

        Commands.Register(Command.Literal("kits").Executes(ctx =>
        {
            var player = ctx.RequirePlayer();
            var names = Available(player).Order().ToList();
            ctx.Reply(names.Count == 0 ? "No kits available" : "Kits: " + string.Join(", ", names));
            return names.Count;
        }));
    }

    private void DeclarePermissions()
    {
        foreach (string name in _config.Value.Kits.Keys)
            Permissions.Register($"kits.kit.{name}", PermissionDefault.True, $"Claim the {name} kit");
    }

    private IEnumerable<string> Available(Player player) => _config.Value.Kits.Keys.Where(k => player.HasPermission($"kits.kit.{k}"));

    private int Claim(CommandContext ctx)
    {
        var player = ctx.RequirePlayer();
        string name = ctx.GetString("name");
        if (!_config.Value.Kits.TryGetValue(name, out var kit)) throw new CommandFailure($"There is no kit called '{name}'");
        if (!player.HasPermission($"kits.kit.{name}")) throw new CommandFailure($"You may not use the {name} kit");

        ulong now = Server.CurrentTick;
        if (_claimed.TryGetValue((player.Uuid, name), out ulong last))
        {
            long wait = kit.CooldownSeconds * 20L - (long)(now - last);
            if (wait > 0) throw new CommandFailure($"The {name} kit is ready again in {(wait + 19) / 20} seconds");
        }
        _claimed[(player.Uuid, name)] = now;

        if (kit.Food > 0)
        {
            player.FoodLevel = Math.Max(player.FoodLevel, kit.Food);
            player.Saturation = Math.Max(player.Saturation, kit.Food);
        }
        if (kit.ExperienceLevels > 0) player.GiveExperienceLevels(kit.ExperienceLevels);
        foreach (var effect in kit.Effects)
            player.AddEffect(new PotionEffect(effect.Effect, effect.Seconds * 20, effect.Amplifier));
        player.PlaySound("minecraft:entity.player.levelup", SoundCategory.Players, 0.8f);
        player.SendActionBar(Component.Text(kit.Message).Color(TextColor.Green));
        return 1;
    }
}
