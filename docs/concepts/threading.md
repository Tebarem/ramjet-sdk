# The threading model

The tick thread owns the world. What runs where, what is thread-safe, and how to do async work.

## One rule

**All plugin code runs on the server's tick thread, and the game-facing plugin API must be called from it.** Event handlers, command handlers, systems, scheduled tasks, `OnEnable` and `OnDisable` are all invoked there, in the middle of the server's own tick work. There is no per-plugin thread and no lock to take: while your handler runs, nothing else in the server runs.

```
 IO threads                         Tick thread (this is where your code runs)            Worker pool
 recv / decrypt / decompress  ─▶    1. drain inbound packets                              chunk load + generation
                                    2. systems, events, scheduler  ◀── your handlers      lighting
 encrypt / write batches      ◀─    3. build and flush outbound packets                   chunk serialisation
```

The consequences are the same as in any single-threaded game loop:

- **A slow handler delays the tick for every player.** A tick has 50 ms. The server's own work uses most of that on a busy server; keep handlers in the microsecond range.
- **Never block** the tick thread: no `Thread.Sleep`, no `Task.Wait()` / `.Result`, no synchronous HTTP or disk reads of unbounded size.
- **No synchronisation is needed** for state that only your handlers touch: they never run concurrently.

Calling a game-facing API from another thread throws `WrongThreadException` instead of corrupting server state. `Server.IsTickThread` tells you whether you are on the tick thread.

## What is thread-safe

By contract these members may be called from any thread:

- `Scheduler.Post`, `Scheduler.RunSync`, `Scheduler.RunAsync`, `Scheduler.ToTick` and `Scheduler.ToAsync` (but `RunLater` and `RunEvery` are for the tick thread)
- `Logger` methods (`Log.Info`, `Log.Error`, ...)
- `Server.IsTickThread` and `Server.CurrentTick`
- reading `Config<T>.Value`
- `Services.TryGet`
- completing a login deferral (`LoginDeferral.Allow` / `Deny`)

Everything else (`Events`, `Commands`, `Player`, `World`, `Entity`, `Server.Players`, `Systems`, ...) must be called on the tick thread.

```csharp
// Do slow work off-thread, then come back to apply the result.
Events.On((ref PlayerJoinEvent e) =>
{
    string name = e.Username.ToString();              // copy: the string is only valid during the dispatch
    Player player = e.Player;
    _ = Task.Run(async () =>
    {
        string profile = await Http.Client.GetStringAsync($"https://example.org/profile/{name}");
        Scheduler.Post(() =>                            // back on the tick thread
        {
            if (player.IsOnline) Log.Info($"loaded profile of {name}: {profile.Length} bytes");
        });
    });
});
```

The same thing as an `async` method, with the thread hops spelled out:

```csharp
private async Task LoadProfile(Player player, string name)
{
    await Scheduler.ToAsync();                          // leave the tick thread
    string profile = await Http.Client.GetStringAsync($"https://example.org/profile/{name}");
    await Scheduler.ToTick();                           // back on the tick thread; dropped if the plugin unloaded meanwhile
    if (player.IsOnline) player.SendMessage($"Loaded {profile.Length} bytes");
}
```

Rules of thumb for async work:

1. **Copy everything you need before leaving the tick thread**: strings (`Utf8String.ToString()`), numbers, handles. Event structs and their borrowed fields are invalid after the handler returns.
2. **Re-validate on return**: the player may have left (`Player.IsOnline`), the entity may be dead (`Entity.IsAlive`).
3. **Do not touch the API from `Task.Run`, `async` continuations, `System.Threading.Timer` or `FileSystemWatcher` callbacks.** An `await` resumes on the thread pool unless you hop back with `Scheduler.Post` or `await Scheduler.ToTick()`.
4. Avoid `async void` handlers: an exception on a pool thread has no plugin to be attributed to.
5. Use `Http.Client` (or `new HttpClient(Http.Handler)`) for web requests rather than your own `HttpClient`; do not dispose either, they are shared. The server limits what plugins may reach (for example, it refuses requests to private and loopback addresses) and applies its own timeouts.

If a posted task or scheduled callback throws, the exception is logged with your plugin's name and the server carries on. A task posted by a plugin that has unloaded in the meantime is dropped.

## Systems and queries

Systems you register from C# run on the tick thread in the server's pipeline phases (see [systems](systems.md)). The server can run its own built-in systems on worker threads, but managed systems are not multithreaded: it is safe to write to your own state in a system and in an event handler without locks.

## What about the server's other threads?

The IO threads and the chunk worker pool never call into plugins. Results that come back from workers (a chunk finished generating) are integrated by the tick thread in the `AsyncIntegration` phase, so by the time you see a chunk or a block, it is in a consistent state.
