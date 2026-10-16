# A scoreboard sidebar

Show live lines in the sidebar with the Sidebar API, shared by everyone or one per player.

The sidebar is the right-hand scoreboard. In vanilla you drive it with a scoreboard objective and `/scoreboard objectives setdisplay sidebar`. The API hides that: a `Sidebar` is a **plugin-owned objective** shown **per viewer** with a blank number format, and it is removed automatically when your plugin unloads. The [SidebarStats sample](../../samples/SidebarStats/) is a complete plugin built on it.

## The API

| Member | Meaning |
|---|---|
| `Sidebars.Create(Component title)` | creates a sidebar nobody sees yet (`Sidebars` is the plugin's factory) |
| `sidebar.Title` | get/set the title |
| `sidebar.SetLine(int index, Component text)` | sets or replaces a line; line 0 is the top, `null` removes the line |
| `sidebar.SetLines(IReadOnlyList<Component>)` | replaces all lines at once |
| `sidebar.Clear()` | removes every line |
| `sidebar.Show(Player)` / `sidebar.Hide(Player)` | per-viewer visibility |
| `sidebar.IsShownTo(Player)`, `sidebar.Viewers` | who sees it |
| `sidebar.Remove()` / `Dispose()` | hides it from everybody and releases it |

Text is a `Component`: a plain `string` converts to **literal** text, so a player name can never inject formatting. For colours use `Component.Legacy("&aGreen")` or `Component.Parse("<green>Green")`, or build one with `Component.Text("Green").Color(TextColor.Green)`.

A `Sidebar` has one set of lines, shown to every player you `Show` it to. Use one shared sidebar for global information and one sidebar per player for personal information.

## A shared sidebar

Shows the player count and the server tick, refreshed once a second.

```csharp
namespace StatsBoard;

public sealed class PluginMain : Plugin
{
    private Sidebar _board = null!;

    protected override void OnEnable()
    {
        _board = Sidebars.Create(Component.Parse("<gold><bold>My Server"));

        foreach (Player p in Server.Players) _board.Show(p);        // players who are already online (after a reload)
        Events.On((ref PlayerJoinEvent e) => _board.Show(e.Player));

        Scheduler.RunEvery(20, () =>
        {
            _board.SetLines(
            [
                Component.Legacy($"&7Online: &f{Server.PlayerCount}"),
                Component.Legacy($"&7Tick: &f{Server.CurrentTick}"),
            ]);
        });
    }
}
```

## One sidebar per player

For text that differs per viewer, keep one `Sidebar` per player:

```csharp
private readonly Dictionary<Uuid, Sidebar> _boards = new();

protected override void OnEnable()
{
    Events.On((ref PlayerJoinEvent e) =>
    {
        var board = Sidebars.Create(Component.Legacy("&b" + e.Username.ToString()));
        board.Show(e.Player);
        _boards[e.Uuid] = board;
    });

    Events.On((ref PlayerQuitEvent e) =>
    {
        if (_boards.Remove(e.Uuid, out var board)) board.Remove();
    });

    Scheduler.RunEvery(20, () =>
    {
        foreach (Player p in Server.Players)
        {
            if (!_boards.TryGetValue(p.Uuid, out var board)) continue;
            board.SetLine(0, Component.Legacy($"&7Health: &c{p.Health:0.#}"));
            board.SetLine(1, Component.Legacy($"&7Level: &a{p.ExperienceLevel}"));
        }
    });
}
```

## Notes

- **Lifetime**: a sidebar is owned by your plugin; on unload or hot reload the objectives are removed from every player, so a reloaded plugin starts from a clean scoreboard and re-creates its sidebars in `OnEnable`. Players who are online at that moment need `Show` again (see the loop over `Server.Players` above).
- **Performance**: `SetLine` sends packets to the viewers of that sidebar. Only change a line when its text changed (compare with the previous string) rather than rewriting every line every tick. The SidebarStats sample spreads its players over the twenty ticks of a second so that a big server does not rebuild every sidebar in one tick.
- **Limits** come from the client: only a limited number of lines are shown (vanilla shows at most 15), and titles and line texts have a maximum length.
- **Interplay with `/scoreboard`**: objectives made by commands live in the same scoreboard module as the plugin sidebar, which uses its own objective names, so the two do not clash.
- A quitting player's handle goes stale (`IsOnline` false); removing the dictionary entry on `PlayerQuitEvent` is enough.
