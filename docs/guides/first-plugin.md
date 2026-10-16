# Your first plugin

A complete plugin with a component, three events, a system, a scheduled task and a command, built, run and hot-reloaded on your test server.

You will build **VisitTracker**: it counts how often each player joined (as a component, so the counts survive hot reloads), blocks chat messages with banned words, answers `/visits`, and logs a summary every minute.

## 1. Create the project

```bash
dotnet new ramjet-plugin -n VisitTracker
cd VisitTracker
```

(Set `RAMJET_PLUGINS` as described in [create a plugin](../getting-started/create-a-plugin.md).) Replace the generated `PluginMain.cs` with this:

```csharp
using Mc.Scripting;

namespace VisitTracker;

/// <summary>Per-player data stored in the ECS: a plain unmanaged struct. Values survive hot reloads.</summary>
public struct Visits
{
    public int Count;
    public long LastJoinTick;
    public long TicksOnline;
}

[PluginInfo("VisitTracker", Version = "1.0.0", Author = "you", Description = "Counts joins, filters chat, answers /visits")]
public sealed class PluginMain : Plugin
{
    private static readonly string[] BlockedWords = ["spam", "free-diamonds"];

    private Query<Visits> _visits = null!;
    private int _joinsSinceEnable;

    protected override void OnEnable()
    {
        // 1. A component: register the type under a global key, create a query over it once.
        Ecs.RegisterComponent<Visits>("visittracker:visits");
        _visits = Ecs.Query<Visits>();

        // 2. Join: add or update the component on the player's entity.
        Events.On((ref PlayerJoinEvent e) =>
        {
            ref var visits = ref e.Player.Entity.GetOrAdd<Visits>();   // a ref into ECS storage: use it right away
            visits.Count++;
            visits.LastJoinTick = (long)Server.CurrentTick;
            _joinsSinceEnable++;
            Log.Info($"{e.Username.ToString()} joined (visit #{visits.Count})");   // joins are rare: allocating is fine
        });

        // 3. Chat: cancel messages with a banned word. High priority runs after Normal handlers.
        Events.On((ref PlayerChatEvent e) =>
        {
            foreach (string word in BlockedWords)
            {
                if (e.Message.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    e.Cancelled = true;
                    return;
                }
            }
        }, EventPriority.High);

        // 4. A command: a real node of the server's command tree, with tab completion and a permission.
        Permissions.Register("visittracker.use", PermissionDefault.True, "Use /visits");
        Commands.Register(Command.Literal("visits").Permission("visittracker.use").Executes(ctx =>
        {
            var player = ctx.RequirePlayer();
            int count = player.TryGet<Visits>(out var visits) ? visits.Count : 0;
            ctx.Reply($"You have joined {count} time(s) since tracking started.");
            return count;                                          // the command result
        }), description: "Show how often you joined");

        // 5. A system: runs every tick over all entities that have a Visits component.
        Systems.Add("online_ticks", Phase.Tick, _ =>
        {
            foreach (var chunk in _visits)
            {
                var visits = chunk.Field<Visits>(0);
                for (int i = 0; i < visits.Length; i++) visits[i].TicksOnline++;
            }
        });

        // 6. A repeating task: once a minute (20 ticks = 1 second).
        Scheduler.RunEvery(20 * 60, () =>
            Log.Info($"{_visits.Count()} tracked players, {_joinsSinceEnable} joins since this version was enabled"));

        Log.Info("enabled");
    }

    protected override void OnDisable() => Log.Info("disabled");
}
```

## 2. Build and run

```bash
dotnet build -c Release
```

Put the plugin on your test server (automatically with `RAMJET_PLUGINS`, or by uploading it, see [build and hot reload](../getting-started/build-and-hot-reload.md)) and watch the server log:

```
[INFO] [VisitTracker] enabled
[INFO] [VisitTracker] enabled v1.0.0 (id 1)
```

Join the server with your Minecraft client to see the event handler run:

```
[INFO] [VisitTracker] YourName joined (visit #1)
```

In the game, type `/visits`: it shows up in the tab completion like any command, and answers with the count. Chat a message containing "spam": it never arrives.

## 3. What each part does

- **`Ecs.RegisterComponent<Visits>("visittracker:visits")`** binds your struct to a global key and creates the flecs component. The struct must be unmanaged (no strings or arrays) and owned by this plugin. See [entities and the ECS](../concepts/entities-and-ecs.md).
- **`e.Player.Entity.GetOrAdd<Visits>()`** returns a `ref` into storage. It is only valid until the next structural change, so it is used immediately and never stored. (`e.Player` is a `Player` handle; `.Entity` is the ECS entity behind it.)
- **`Events.On((ref PlayerChatEvent e) => ...)`**: events are structs passed by `ref`. `e.Cancelled = true` is how a handler vetoes. `EventPriority.High` runs after `Normal`.
- **`e.Message.Contains(...)`** reads the borrowed UTF-8 text of the message without allocating a string first. See [events](../concepts/events.md).
- **`Commands.Register(Command.Literal("visits")...)`** registers a Brigadier node. `ctx.Reply` answers the player, and the number the executor returns is the command result. See [commands](../concepts/commands.md).
- **`Systems.Add("online_ticks", Phase.Tick, ...)`** registers a system in the server's pipeline as `VisitTracker:online_ticks`. See [systems](../concepts/systems.md).
- **`Scheduler.RunEvery`** schedules the log line; it is cancelled automatically on unload.
- `Log` prefixes every line with the plugin name.

## 4. Hot reload it

Stay connected, then, with the server still running, change `Log.Info("enabled")` to say something else and run `dotnet build -c Release` again:

```
[INFO] hot reloading VisitTracker.dll
[INFO] [VisitTracker] enabled (v2)
[INFO] [VisitTracker] enabled v1.0.0 (id 1)
[INFO] [VisitTracker] disabled
```

When the next minute log line comes, it reads:

```
[INFO] [VisitTracker] 1 tracked players, 0 joins since this version was enabled
```

You are still tracked: the `Visits` component sits on your entity in the ECS and its layout did not change, so `/visits` still knows your count. `_joinsSinceEnable` is back to 0, because it is a field of the plugin object, which is a new instance after every reload. That difference, *state in components survives, state in fields does not*, is the most important thing to internalise about [hot reload](../concepts/hot-reload.md). (A player who disconnects loses their entity and the component with it; to keep counts across sessions, write them to disk keyed by the player's UUID, see [config and data](../concepts/config-and-data.md).)

## 5. Try next

- Make the banned words configurable: [config and data](../concepts/config-and-data.md).
- Add arguments and tab completion: [commands with arguments](commands-with-arguments.md).
- A bigger plugin with config, permissions and commands: the [homes plugin](homes-plugin.md).
- Coming from Paper? The [migration table](migrating-from-paper.md) maps every concept.
