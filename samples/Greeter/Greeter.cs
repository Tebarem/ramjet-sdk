using System.Text.Json.Serialization;

namespace Greeter;

/// <summary>The settings of the Greeter plugin. The defaults below are written to <c>plugins/greeter/config.json</c> the first time the plugin runs; edit the file and
/// run <c>/greeter reload</c> to apply changes. Colours use legacy <c>&amp;</c> codes.</summary>
public sealed class GreeterConfig
{
    /// <summary>The line everybody sees when a player joins ({player} is the name).</summary>
    public string JoinMessage { get; set; } = "&8[&a+&8] &e{player}";

    /// <summary>The line everybody sees when a player leaves.</summary>
    public string QuitMessage { get; set; } = "&8[&c-&8] &7{player}";

    /// <summary>A private message sent to the joining player; empty to disable.</summary>
    public string Welcome { get; set; } = "&6Welcome to the server, &e{player}&6! Have fun.";

    /// <summary>The server list description (two lines separated by \n); empty keeps the server's own.</summary>
    public string Motd { get; set; } = "&a&lA C# powered server\n&7plugins are the whole ecosystem";
}

[JsonSerializable(typeof(GreeterConfig))]
internal sealed partial class GreeterJson : JsonSerializerContext;

/// <summary>Sample: greets players on join with a coloured broadcast and a private message, says goodbye on quit and
/// customises the server list entry. Everything is configurable in <c>config.json</c> (typed <see cref="Config{T}"/>, defaults written on first run, live reload).</summary>
[PluginInfo("Greeter", Version = "1.1.0", Author = "Ramjet", Description = "Join/quit messages and a server list MOTD")]
public sealed class GreeterPlugin : Plugin
{
    private Config<GreeterConfig> _config = null!;

    protected override void OnEnable()
    {
        _config = Config.Load(GreeterJson.Default.GreeterConfig);
        _config.Watch = true;  // edits of config.json are picked up without a command

        Commands.Register(Command.Literal("greeter")
            .Then(Command.Literal("reload").Permission("greeter.reload").Executes(ctx =>
            {
                bool ok = _config.Reload();
                ctx.Reply(ok ? "Greeter configuration reloaded" : "config.json is invalid, see the server log (the previous values stay active)");
                return ok ? 1 : 0;
            })));

        Events.On((ref PlayerJoinEvent e) =>
        {
            string name = e.Username.ToString();
            var config = _config.Value;
            // the message everybody sees (replaces "<name> joined the game")
            if (config.JoinMessage.Length == 0) e.SetJoinMessage(Component.Empty);
            else e.SetJoinMessage(Component.Legacy(config.JoinMessage.Replace("{player}", name)));
            // a private welcome, sent right away
            if (config.Welcome.Length > 0) e.Player.SendMessage(Component.Legacy(config.Welcome.Replace("{player}", name)));
        });

        Events.On((ref PlayerQuitEvent e) =>
        {
            var config = _config.Value;
            e.SetQuitMessage(config.QuitMessage.Length == 0 ? Component.Empty : Component.Legacy(config.QuitMessage.Replace("{player}", e.Username.ToString())));
        });

        Events.On((ref ServerListPingEvent e) =>
        {
            if (_config.Value.Motd.Length > 0) e.SetMotd(Component.Legacy(_config.Value.Motd.Replace("\\n", "\n")));
            Log.Debug($"ping from {e.Address.ToString()}");
        });
    }
}
