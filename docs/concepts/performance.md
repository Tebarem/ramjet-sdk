# Performance rules

Allocation-free hot paths, what each call costs, and why the rules exist.

The server is built to carry far more players per process than a JVM server, and the plugin layer is built not to give that back. Most of the cost of a plugin is **what your handlers allocate**, not the cost of calling them.

## Why allocation is the thing to avoid

- A tick has **50 ms**, and the tick thread does *all* the work: networking glue, entities, blocks, your handlers. There is no spare thread to absorb a pause.
- .NET uses a **generational GC**. Short-lived garbage is cheap to collect, but every allocation on a path that runs thousands of times per tick (movement, damage, block updates, chat of a busy server) adds up to collections whose pauses land **in the tick**. Concurrent GC is on, which helps, but a gen0 collection still stops the world briefly.
- Events are designed to be free: they are blittable structs created by the firing code on its own stack and passed by `ref`; with no listener the fire costs a bit test. You keep that only if your handler does not allocate either.

## The rules

1. **Hot-path handlers must not allocate.** Hot = anything that fires per tick, per move packet, per block or per entity: `PlayerMove`, `TickStart/End`, `EntityDamage`, `BlockPlace/Break` on a busy world, systems, scheduled tasks that repeat often.
2. **No LINQ, no `string` building, no `$"..."` interpolation, no `params` calls, no boxing, no new lambdas** on those paths. A lambda that captures variables allocates a closure each time it is created: create delegates once (in `OnEnable`, or as `static` lambdas) and reuse them.
3. **Read `Utf8String` / `NativeBytes` in place.** Compare bytes (`EqualsAscii("..."u8)`, `Bytes.StartsWith(...)`) and only call `ToString()` when you really need a managed string (and then once, not per event).
4. **Allocate once, reuse**: `static readonly` tables, pooled arrays (`ArrayPool<T>`), `Span<T>` / `stackalloc` for small temporaries, `StringBuilder` fields cleared between uses.
5. **Use queries for bulk work.** A per-entity `Get`/`Set` in a loop crosses the managed/native boundary each time (about 3-7 ns); iterate query chunks instead (about 2.7 ns per entity).
6. **Subscribe to what you need.** `PlayerMoveEvent` fires per move packet; do not subscribe to it for a feature that needs it rarely. Unsubscribe (`Subscription.Dispose`) when a feature is turned off. `Server.PlayerMoveThreshold` raises the distance a player has to move before the event fires again, if the whole server can live with coarser movement events.
7. **Check cheaply, then work**: `if (!HasWork) return;` first; `Events.HasSubscribers<T>()` before building an event that costs something; compare the cheap field before the expensive one.
8. **Do heavy work off the tick thread** and come back with `Scheduler.Post` (see [threading](threading.md)); never block.
9. **Do not allocate in `foreach` over interfaces**: iterate arrays and `List<T>` (struct enumerators), not `IEnumerable<T>`.

```csharp
// Bad on a hot path: allocates a string, a closure and an iterator on every move
Events.On((ref PlayerMoveEvent e) =>
{
    var nearby = _zones.Where(z => z.Contains(e.To)).ToList();
    Log.Debug($"{e.Player} moved to {e.To}");
});

// Good: no allocation, no work unless the player is inside a zone
private static readonly Zone[] s_zones = ...;
private void OnMove(ref PlayerMoveEvent e)
{
    foreach (ref readonly var z in s_zones.AsSpan())     // Span enumeration: no allocation
        if (z.Contains(e.To)) { /* ... */ }
}
// in OnEnable: Events.On<PlayerMoveEvent>(OnMove);     // one delegate, created once
```

(`Events.On(OnMove)` with a method group allocates its delegate once, at registration.)

## What things cost

Approximate figures, measured on a development machine with .NET 10:

| Path | Cost |
|---|---|
| fire an event, no subscribers | about 0.4 ns |
| fire an event into one C# handler (ref struct, mutate a field) | about 10-12 ns |
| C# call into the server (e.g. `EntityAlive`) | about 3.4 ns |
| C# `entity.Get<T>()` | about 6.5 ns |
| C# chunk query pass, 10 000 entities, one component | about 2.7 ns per entity |

The numbers are for the plumbing. They say a handler that does nothing is free, and that what you do inside it is what you pay.

## Measure your own plugin

- To check a handler for allocations in a test or while developing, wrap it:

  ```csharp
  long before = GC.GetAllocatedBytesForCurrentThread();
  Handle(ref e);
  long allocated = GC.GetAllocatedBytesForCurrentThread() - before;   // should be 0
  ```
- `Server.TickTimes(n)` returns the server's own busy time of the last `n` ticks (the same figures `/tick query` reports); compare with and without your plugin. `Server.PluginMemoryBytes` is the memory of the process your plugins run in.
- A plugin that takes a noticeable share of the tick budget shows up in the server's per-system timings: wrap heavy logic in a system so it appears there by name.
