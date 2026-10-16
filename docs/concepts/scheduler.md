# The scheduler

Tick-based delayed and repeating tasks, posting work back to the tick thread, and how tasks are owned and ordered.

The scheduler is tick-based: **20 ticks are one second**, and a task always runs on the tick thread. It replaces `BukkitScheduler`. Work that should not run on the tick thread goes to the thread pool (`Scheduler.RunAsync`, or `Task.Run`) and comes back with `Scheduler.Post` or `Scheduler.RunSync`, see [threading](threading.md).

```csharp
// once, 5 seconds from now
Scheduler.RunLater(100, () => Server.Broadcast("five seconds later"));

// every minute, first run after 10 ticks
ScheduledTask autosave = Scheduler.RunEvery(20 * 60, SaveAll, initialDelayTicks: 10);

autosave.Cancel();                 // or Dispose(); safe to call twice

// from another thread: run on the tick thread at the start of the next tick
Scheduler.Post(() => Apply(result));
```

| Member | Behaviour |
|---|---|
| `RunLater(delayTicks, action)` | runs once. The delay is at least 1 tick (a smaller value is raised to 1) |
| `RunEvery(periodTicks, action, initialDelayTicks = 0)` | repeats. `periodTicks` must be positive (`ArgumentOutOfRangeException` otherwise); the first run is after `max(1, initialDelayTicks)` ticks |
| `Post(action)` | **thread-safe**. Runs at the start of the next tick, FIFO, before due scheduled tasks |
| `RunSync(action)` / `RunSync(func)` | thread-safe. Like `Post`, but returns a `Task` that completes when it ran (`Task<T>` for the `Func` form) |
| `RunAsync(action)` / `RunAsync(func)` | runs on the .NET thread pool and returns a `Task`; must not touch game APIs |
| `ToTick()` / `ToAsync()` | awaitables: `await Scheduler.ToAsync();` leaves the tick thread, `await Scheduler.ToTick();` comes back at the next tick boundary |
| `Delay(ticks)` | returns a `Task` to `await` that resumes on the tick thread after the given number of ticks |
| `ScheduledTask.Cancel()` / `Dispose()` | stop it; `IsCancelled` tells |

A `ToTick` or `Delay` continuation is owned by the plugin and dropped if the plugin unloads meanwhile.

```csharp
// an async method that does slow work off the tick thread and finishes on it
private async Task LoadAsync(Player player)
{
    await Scheduler.ToAsync();
    string data = await Http.Client.GetStringAsync("https://example.org/data");
    await Scheduler.ToTick();                    // back on the tick thread
    if (player.IsOnline) player.SendMessage("Loaded " + data.Length + " bytes");
}
```

## How it runs

The scheduler is advanced once per tick. In each tick, in this order:

1. **posted** actions (those queued before the tick started), oldest first;
2. every scheduled task whose due tick has arrived, ordered by due tick and then by creation order.

A repeating task is rescheduled for `now + period` **after it ran**, so a slow task drifts later instead of piling up. If an action throws, the exception is logged with the plugin's name and the task keeps its schedule (a repeating task runs again; a one-shot is finished anyway). `RunLater` and `RunEvery` are for the tick thread; `Post`, `RunSync` and `RunAsync` are safe from any thread.

## Ownership

Tasks belong to the plugin that created them. When the plugin unloads (or hot reloads) its pending scheduled and posted tasks are discarded, so no delegate from your collectible assembly survives. A repeating task from the old version does not carry over to the new one: re-create it in `OnEnable`.

## Tips

- **Counting time**: derive durations from tick counts, not wall clocks. A lagging server stretches a tick, and your timers should stretch with it (a 5 second cooldown is `5 * 20` ticks). `Server.CurrentTick` is the server's tick counter (safe to read from any thread); use it for "now" in cooldowns.
- **Do not allocate per run** in a frequently repeating task: hoist delegates into fields, avoid LINQ and string building (see [performance](performance.md)).
- **Heavy work** (disk, network) goes to the thread pool and returns with `Post`; do not run it inside a scheduled task.
- For per-tick work over many entities prefer a [system](systems.md) with a query over a scheduled task that loops.
