using Mc.Scripting;
using Mc.Scripting.Events;

namespace HelloPlugin;

/// <summary>Per-player tick counter (plain data, stored in flecs).</summary>
public struct JoinTicks
{
    public long Ticks;
}

/// <summary>Sample plugin: events (cancellation and mutation), a component, a per-tick system,
/// a command and the scheduler.</summary>
[PluginInfo("HelloPlugin", Version = "1.0.0", Author = "Ramjet")]
public sealed class HelloPlugin : Plugin
{
    private Query<JoinTicks> _players = null!;
    private int _chatSeen;

    protected override void OnEnable()
    {
        Ecs.RegisterComponent<JoinTicks>("hello:join_ticks");
        _players = Ecs.Query<JoinTicks>();

        Events.On((ref PlayerJoinEvent e) =>
        {
            Log.Info($"welcome {e.Username.ToString()}!");
            e.Player.Set(new JoinTicks());
        });

        // Cancel chat containing "spam". High priority = runs late, after Normal handlers.
        Events.On((ref PlayerChatEvent e) =>
        {
            _chatSeen++;
            if (e.Message.Contains("spam", StringComparison.OrdinalIgnoreCase)) e.Cancelled = true;
        }, EventPriority.High);

        // Halve damage dealt by entities.
        Events.On((ref EntityDamageEvent e) =>
        {
            if (!e.Attacker.IsNull) e.Amount *= 0.5f;
        });

        // Count ticks for every player that joined.
        Systems.Add("hello:count_ticks", Phase.Tick, _ =>
        {
            foreach (var chunk in _players)
            {
                var ticks = chunk.Field<JoinTicks>(0);
                for (int i = 0; i < ticks.Length; i++) ticks[i].Ticks++;
            }
        });

        Commands.Register("hello", ctx =>
        {
            ctx.Reply($"Hello, {(ctx.Args.Length > 0 ? ctx.Args[0] : "world")}! (chat seen: {_chatSeen})");
            ctx.Result = ctx.Args.Length;
        }, "Says hello");

        Scheduler.RunEvery(20 * 60, () => Log.Info("still alive"));
    }

    protected override void OnDisable() => Log.Info("goodbye");
}
