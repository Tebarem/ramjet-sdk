# Events

Subscribing, priorities, cancellation, mutable fields, borrowed strings and custom events.

Events are how the server tells a plugin that something happened, and how a plugin changes the outcome. They are **synchronous**, run on the tick thread, and use **blittable structs passed by `ref`**: no boxing, no allocation, no GC pressure.

```csharp
Events.On((ref PlayerChatEvent e) =>
{
    if (e.Message.Contains("spam", StringComparison.OrdinalIgnoreCase))
        e.Cancelled = true;                       // the server drops the message
});
```

## Subscribing

`Events.On<T>(listener, priority, receiveCancelled)` returns a `Subscription`. You rarely keep it: unloading the plugin unsubscribes everything. Dispose it to unsubscribe earlier.

```csharp
Subscription sub = Events.On((ref PlayerJoinEvent e) => { ... });
sub.Dispose();                       // optional
```

The handler is a plain delegate, `EventListener<T>(ref T ev)`. There is no reflection-based listener scanning and no `@EventHandler` annotation.

## Priorities

Lower runs **first**, and the server's own handlers and yours share one ordering. The conventional values are `EventPriority`:

| Priority | Value | Use it to |
|---|---:|---|
| `Lowest` | -200 | see the event before anyone changes it |
| `Low` | -100 | |
| `Normal` | 0 | the default |
| `High` | 100 | override other plugins; act on a nearly final state |
| `Highest` | 200 | last chance to change the outcome |
| `Monitor` | 1000 | **observe** the final outcome; must not modify the event |

The order is the same as Paper's (`LOWEST` first, `MONITOR` last). Any `int` is allowed (`(EventPriority)150`).

## Cancellation

An event is either cancellable or not (`IsCancellable`, shown in the [catalogue](../reference/events.md)). On a cancellable event a handler sets `e.Cancelled = true`:

- later handlers are **skipped**, unless they subscribed with `receiveCancelled: true`;
- the code that fired the event sees the result and acts on it (`PlayerChat` cancelled: the message is dropped; `PlayerMove` cancelled: the player is teleported back);
- setting `Cancelled` on a non-cancellable event does nothing.

```csharp
// react to the final result, whoever cancelled it
Events.On((ref PlayerChatEvent e) => Log.Info(e.Cancelled ? "blocked" : "delivered"),
          EventPriority.Monitor, receiveCancelled: true);
```

## Mutable fields

Most event fields are read-only. A few are **mutable**: a handler may change them and the server reads the new value back after the dispatch (`PlayerMoveEvent.To`, `EntityDamageEvent.Amount`, `BlockBreakEvent.DropItems`...). The [catalogue](../reference/events.md) shows which fields are mutable.

```csharp
Events.On((ref EntityDamageEvent e) => e.Amount *= 0.5f);     // halve all damage
```

Text fields are changed through setter methods that copy the new value into a per-tick buffer, so nothing is leaked or kept alive:

```csharp
Events.On((ref PlayerJoinEvent e) => e.SetJoinMessage(Component.Legacy("&e" + e.Username.ToString() + " has arrived")));
Events.On((ref PlayerChatEvent e) => e.SetMessage(e.Message.ToString().Replace("darn", "****")));
```

## Borrowed strings and bytes

Text fields (`Message`, `Username`, `Command`, ...) are `Utf8String`: a view of native memory that is **only valid during the dispatch**. Reading it allocates nothing until you ask for a string:

```csharp
Events.On((ref PlayerChatEvent e) =>
{
    if (e.Message.IsEmpty) return;
    if (e.Message.EqualsAscii("!help"u8)) { /* byte compare, no allocation */ }
    string text = e.Message.ToString();      // allocates; safe to keep after the handler returns
});
```

Never store the `Utf8String`, `NativeBytes` or the `ref` event itself (it cannot be captured by a lambda: the compiler will refuse). Copy first.

## Which events exist

The [event catalogue](../reference/events.md) lists every event with its fields, which of them are mutable and whether it can be cancelled. An event is only declared when the server has something that fires it, with one exception noted in the catalogue (`PacketSendEvent` is reserved and not fired yet). Subscribe only to what you need: some events fire very often (`PlayerMoveEvent` for every accepted movement packet, `TickStartEvent` every tick).

`PlayerPreLoginEvent` is the one asynchronous event: a handler can `Defer()` the login and answer later from any thread, for example after a database lookup. The deferral is refused automatically if your plugin unloads before it answers.

## Custom events

A plugin can define its own events and fire them on the same bus, so other plugins can listen:

```csharp
public struct HomeSetEvent : IEvent
{
    public EventHeader Header;          // must be the first field
    public Entity Player;
    public int HomeCount;

    public static string Key => "homes:set";          // "plugin:name"
    public static bool IsCancellable => true;
}

// fire it
var ev = new HomeSetEvent { Player = p, HomeCount = 3 };
if (!Events.Fire(ref ev)) return;       // false: a handler cancelled it
```

- The struct must be blittable (no references or strings).
- Registering the same key with a different size or cancellability while someone listens is rejected.
- Your events and the built-in ones interleave by priority, exactly as for catalogue events.
- Use `Events.HasSubscribers<T>()` before building an event that is expensive to construct.

## Errors

An exception that escapes a handler is caught, logged with the plugin's name and stack trace, and dispatching continues with the next handler. A bug in one plugin cannot take the server down; it can only fail to do its job.

## Cost

Firing an event nobody listens to costs about a nanosecond (one bit test), and the hop into a C# handler about 10-12 ns. A handler that allocates is where the real cost is; see [performance](performance.md). Subscribe to the events you need, not to `PlayerMoveEvent` for a rarely needed feature.
