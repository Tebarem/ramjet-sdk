namespace BossBarTimer;

/// <summary>Sample: <c>/timer &lt;seconds&gt; &lt;title&gt;</c> shows a countdown boss bar to every player (including those who join meanwhile), turns it red in the last ten seconds,
/// plays a tick sound and announces the end with a title. The bar belongs to the plugin: it vanishes if the plugin unloads while it runs.</summary>
[PluginInfo("BossBarTimer", Version = "1.0.0", Author = "Ramjet", Description = "Countdown timers on a boss bar")]
public sealed class BossBarTimerPlugin : Plugin
{
    private BossBar? _bar;
    private ScheduledTask? _task;
    private int _total;
    private int _left;

    protected override void OnEnable()
    {
        Permissions.Register("bossbartimer.use", PermissionDefault.Op, "Start and stop countdown timers");

        Commands.Register(Command.Literal("timer").Permission("bossbartimer.use")
            .Then(Command.Literal("stop").Executes(ctx =>
            {
                Stop();
                ctx.Reply("Timer stopped");
                return 1;
            }))
            .Then(Command.Argument("seconds", Arg.Int(1, 3600))
                .Executes(ctx => Start(ctx.GetInt("seconds"), "Time left"))
                .Then(Command.Argument("title", Arg.GreedyString()).Executes(ctx => Start(ctx.GetInt("seconds"), ctx.GetString("title"))))));

        Events.On((ref PlayerJoinEvent e) => _bar?.AddViewer(e.Player));
    }

    private int Start(int seconds, string title)
    {
        Stop();
        _total = _left = seconds;
        _bar = BossBars.Create("countdown", Component.Text(title), BossBarColor.Green, BossBarStyle.Segmented10, 1f);
        foreach (var player in Server.Players) _bar.AddViewer(player);
        _task = Scheduler.RunEvery(20, Tick, 20);
        Update(title);
        return seconds;
    }

    private void Tick()
    {
        if (_bar is null) return;
        _left--;
        if (_left <= 0)
        {
            var done = _bar.Title;
            Stop();
            foreach (var player in Server.Players)
            {
                player.ShowTitle(Component.Text("Time is up!").Color(TextColor.Red).Bold(), done, new TitleTimes(5, 40, 10));
                player.PlaySound("minecraft:entity.player.levelup", SoundCategory.Master, 1f, 0.8f);
            }
            return;
        }
        Update(null);
        if (_left <= 10)
            foreach (var player in Server.Players) player.PlaySound("minecraft:block.note_block.hat", SoundCategory.Master, 0.6f, 1.2f);
    }

    private void Update(string? title)
    {
        if (_bar is null) return;
        _bar.Progress = (float)_left / _total;
        _bar.Color = _left <= 10 ? BossBarColor.Red : _left <= _total / 2 ? BossBarColor.Yellow : BossBarColor.Green;
        if (title is not null) _bar.Title = Component.Text(title);
    }

    private void Stop()
    {
        _task?.Cancel();
        _task = null;
        _bar?.Remove();
        _bar = null;
    }
}
