using Mc.Scripting;

namespace MyPlugin;

/// <summary>PLUGIN_DESCRIPTION</summary>
[PluginInfo("MyPlugin", Version = "1.0.0", Author = "AUTHOR_NAME", Description = "PLUGIN_DESCRIPTION")]
public sealed class PluginMain : Plugin
{
    private int _joins;

    protected override void OnEnable()
    {
        Log.Info("enabled");

        // Events run synchronously on the tick thread, with the event passed by `ref`.
        // Join happens rarely, so an interpolated string is fine here. Per-tick and per-move handlers
        // must not allocate (see AGENTS.md).
        Events.On((ref PlayerJoinEvent e) =>
        {
            _joins++;
            Log.Info($"{e.Username.ToString()} joined ({_joins} joins since this plugin was enabled)");
        });

        // Cancel chat messages containing "spam". Priority High runs after Normal handlers.
        Events.On((ref PlayerChatEvent e) =>
        {
            if (e.Message.Contains("spam", StringComparison.OrdinalIgnoreCase)) e.Cancelled = true;
        }, EventPriority.High);

        // Everything registered through Events/Commands/Systems/Scheduler is released automatically on unload.
        Scheduler.RunEvery(20 * 60, () => Log.Debug("still running"));
    }

    protected override void OnDisable() => Log.Info("disabled");
}
