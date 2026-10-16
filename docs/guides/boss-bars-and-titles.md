# Boss bars, titles and action bars

Show a countdown boss bar, a welcome title and an action bar message.

## Titles and action bars

Titles and action bars are **fire and forget**: nothing to create, nothing to release. They are methods of `Player`.

```csharp
Events.On((ref PlayerJoinEvent e) =>
{
    Player player = e.Player;
    player.ShowTitle(Component.Parse("<gold>Welcome"),
                     Component.Parse("<gray>to the server"),
                     new TitleTimes(10, 60, 20));          // fade in, stay, fade out: ticks, 20 = 1 second

    player.SendActionBar(Component.Legacy("&aType /help to get started"));
});
```

- `ShowTitle(title, subtitle = null, times = null)`: the timings are a `TitleTimes` in ticks (`TitleTimes.Default` is the vanilla 0.5 s / 3.5 s / 1 s; `TitleTimes.From(fadeIn, stay, fadeOut)` takes `TimeSpan`s).
- `player.ShowSubtitle(subtitle)` changes only the subtitle; `player.ClearTitle()` hides a title that is showing.
- The action bar is the single line above the hotbar. Sending again replaces it; it fades by itself after a few seconds, so resend it from a repeating task if you want it to stay.
- Text is a `Component`. A plain `string` converts to literal text; use `Component.Legacy("&a...")` or `Component.Parse("<green>...")` explicitly for formatting, so user-controlled strings never inject formatting. `Component.Text("...")` with `.Color(TextColor.Gold)`, `.Bold()` and `.Append(...)` builds styled text without any markup.

## Boss bars

A `BossBar` is an **owned resource**: it is removed from every viewer when you remove or dispose it and when your plugin unloads. It maps onto vanilla's boss bars, and appears in `/bossbar` under the key `pluginid:name`.

| Member | Meaning |
|---|---|
| `BossBars.Create(name, title, color, style, progress)` | creates a bar (no viewers yet); `BossBars` is the plugin's factory |
| `bar.Progress` | 0.0 to 1.0 |
| `bar.Title`, `bar.Color`, `bar.Style` | change while it is shown |
| `bar.Flags` | `BossBarFlags.DarkenScreen`, `PlayMusic`, `CreateFog` |
| `bar.Visible` | hide and show the bar without losing its viewers |
| `bar.AddViewer(Player)` / `bar.RemoveViewer(Player)` | who sees it (also by `Uuid`: the bar appears when that player joins) |
| `bar.Remove()` / `bar.Dispose()` | removes it from all viewers and deletes it |

Colours are `BossBarColor.Pink`, `Blue`, `Red`, `Green`, `Yellow`, `Purple`, `White`; styles are `BossBarStyle.Solid`, `Segmented6`, `Segmented10`, `Segmented12`, `Segmented20`.

### A countdown

A repeating task shrinks the bar; at zero the bar is removed and a title is shown. This is the [BossBarTimer sample](../../samples/BossBarTimer/) in short:

```csharp
namespace Countdown;

public sealed class PluginMain : Plugin
{
    private BossBar? _bar;
    private ScheduledTask? _task;
    private int _total;
    private int _left;

    protected override void OnEnable()
    {
        Permissions.Register("countdown.use", PermissionDefault.Op, "Start countdowns");

        Commands.Register(Command.Literal("countdown").Permission("countdown.use")
            .Then(Command.Argument("seconds", Arg.Int(1, 3600))
                .Executes(ctx => Start(ctx.GetInt("seconds")))));

        // viewers who join later are not added automatically
        Events.On((ref PlayerJoinEvent e) => _bar?.AddViewer(e.Player));
    }

    private int Start(int seconds)
    {
        Stop();
        _total = _left = seconds;
        _bar = BossBars.Create("countdown", Component.Text($"Restart in {seconds}s"), BossBarColor.Green, BossBarStyle.Segmented10, 1f);
        foreach (Player p in Server.Players) _bar.AddViewer(p);
        _task = Scheduler.RunEvery(20, Tick, 20);                 // once a second, first run after one second
        return seconds;
    }

    private void Tick()
    {
        if (_bar is null) return;
        _left--;
        if (_left <= 0)
        {
            Stop();
            Server.Broadcast("Time is up!");
            return;
        }
        _bar.Progress = (float)_left / _total;
        _bar.Title = Component.Text($"Restart in {_left}s");
        if (_left <= 5) _bar.Color = BossBarColor.Red;
    }

    private void Stop()
    {
        _task?.Cancel();
        _bar?.Remove();
        _task = null;
        _bar = null;
    }

    protected override void OnDisable() => Stop();     // not required: unload removes the bar and cancels the task
}
```

### Notes

- **Viewers who join later** are not added automatically; subscribe to `PlayerJoinEvent` and call `AddViewer` if the bar should follow new players. A viewer who leaves is dropped by the server; `RemoveViewer` is for taking a bar away from someone who stays.
- **One bar per purpose**: boss bars stack at the top of the screen (the client shows several). Reuse one bar and change its title and progress rather than creating a bar per update.
- **Updating** `Progress` or `Title` sends a small packet to the viewers; set them only when the value changes.
- **Hot reload**: the old plugin's bars are removed from every viewer when it unloads. The new version creates its own in `OnEnable`.
