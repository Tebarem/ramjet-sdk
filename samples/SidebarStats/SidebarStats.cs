namespace SidebarStats;

/// <summary>Sample: a personal sidebar for every player with live lines (health, food, latency, position, kills and deaths), each refreshed once a second; the players are spread over
/// the twenty ticks of that second (a slice every tick) so that a big server does not rebuild every sidebar in one tick.
/// <c>/stats</c> toggles it. Each player has his own <see cref="Sidebar"/>, so the lines differ per player; the sidebar objects are owned by the plugin and disappear when it unloads.</summary>
[PluginInfo("SidebarStats", Version = "1.0.0", Author = "Ramjet", Description = "Live personal sidebar scoreboard")]
public sealed class SidebarStatsPlugin : Plugin
{
    private readonly Dictionary<Entity, Sidebar> _sidebars = [];
    private readonly HashSet<Uuid> _hidden = [];
    private readonly Dictionary<Uuid, (int Kills, int Deaths)> _counts = [];

    protected override void OnEnable()
    {
        foreach (var player in Server.Players) Attach(player);

        Events.On((ref PlayerJoinEvent e) => Attach(e.Player));
        Events.On((ref PlayerQuitEvent e) =>
        {
            if (_sidebars.Remove(e.Player.Entity, out var sidebar)) sidebar.Remove();
        });
        Events.On((ref PlayerDeathEvent e) =>
        {
            Count(e.Player.Uuid, deaths: 1);
            var killer = new Player(e.Killer);
            if (killer.IsOnline && killer != e.Player) Count(killer.Uuid, kills: 1);
        });

        Commands.Register(Command.Literal("stats").Executes(ctx =>
        {
            var player = ctx.RequirePlayer();
            if (!_hidden.Add(player.Uuid))
            {
                _hidden.Remove(player.Uuid);
                Attach(player);
                ctx.Reply("Stats sidebar on");
            }
            else
            {
                if (_sidebars.TryGetValue(player.Entity, out var sidebar)) sidebar.Hide(player);
                ctx.Reply("Stats sidebar off");
            }
            return 1;
        }));

        Scheduler.RunEvery(1, Refresh);
    }

    private void Count(Uuid player, int kills = 0, int deaths = 0)
    {
        var (k, d) = _counts.GetValueOrDefault(player);
        _counts[player] = (k + kills, d + deaths);
    }

    private void Attach(Player player)
    {
        if (_hidden.Contains(player.Uuid)) return;
        if (!_sidebars.TryGetValue(player.Entity, out var sidebar))
            _sidebars[player.Entity] = sidebar = Sidebars.Create(Component.Text("Stats").Color(TextColor.Gold).Bold());
        Fill(sidebar, player);
        sidebar.Show(player);
    }

    // Every tick the players whose slot (entity index modulo 20) is the tick's slot: one twentieth of the sidebars, each of them once a second
    private void Refresh()
    {
        ulong slot = Server.CurrentTick % 20;
        foreach (var (entity, sidebar) in _sidebars)
        {
            if ((entity.Id & 0xFFFFFFFF) % 20 != slot) continue;
            var player = new Player(entity);
            if (player.IsOnline && sidebar.IsShownTo(player)) Fill(sidebar, player);
        }
    }

    private void Fill(Sidebar sidebar, Player player)
    {
        var (kills, deaths) = _counts.GetValueOrDefault(player.Uuid);
        var at = player.Position;
        sidebar.SetLines(
        [
            Line("Health", $"{player.Health:0.#}/{player.MaxHealth:0}", TextColor.Red),
            Line("Food", player.FoodLevel.ToString(), TextColor.Gold),
            Line("Ping", $"{player.Latency} ms", TextColor.Green),
            Line("XYZ", $"{at.X:0} {at.Y:0} {at.Z:0}", TextColor.Aqua),
            Component.Empty,
            Line("Kills", kills.ToString(), TextColor.White),
            Line("Deaths", deaths.ToString(), TextColor.White),
        ]);
    }

    private static Component Line(string label, string value, TextColor color)
        => Component.Text(label + ": ").Color(TextColor.Gray).Append(Component.Text(value).Color(color));
}
