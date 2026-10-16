# Systems and tick phases

Per-tick systems in the server's pipeline, ordering against the server's own systems, replacing and disabling vanilla behaviour.

A **system** is a function the server calls once per tick, in a chosen phase, with the tick's delta time in seconds. Systems are how you run per-tick logic over many entities (with a [query](entities-and-ecs.md#queries)), and how you hook, reorder, replace or switch off the server's own behaviour. Scripted systems live in the **server's own system registry**: they run in the same pipelines as the built-in systems, are ordered against them by name, and show up with timings in the server's system list.

```csharp
private Query<Mana> _mana = null!;

protected override void OnEnable()
{
    Ecs.RegisterComponent<Mana>("mymana:mana");
    _mana = Ecs.Query<Mana>();

    Systems.Add("regen", Phase.Tick, dt =>
    {
        foreach (var chunk in _mana)
        {
            var mana = chunk.Field<Mana>(0);
            for (int i = 0; i < mana.Length; i++) mana[i].Value += 1;
        }
    });
}
```

## Phases

`Phase` values, in the order they run each tick:

| Phase | What happens there |
|---|---|
| `NetworkReceive` | drain inbound packets, apply them to entities |
| `PreTick` | timers, scheduled tasks, input integration |
| `Tick` | game logic: entities, blocks ... (the default for `Add`) |
| `PostTick` | removals, collision resolution, derived state |
| `Tracking` | spatial index update, view diffs |
| `NetworkSend` | build and encode outbound packets |
| `AsyncIntegration` | integrate worker results (chunk loads, lighting) |

Deferred changes (entity creation, component add/set/remove) made in one phase are merged at the phase boundary and are visible to every **later** phase of the same tick. A `Tick` system sees an entity that a built-in `NetworkReceive` system created in the same tick; the other way round only from the next phase on.

## Ordering against other systems

```csharp
Systems.Register(Phase.Tick, "regen", dt => ...);                   // end of the phase (same as Add)
Systems.After("entities:tick", "after_entities", dt => ...);        // right behind a built-in or scripted system
Systems.Before("entities:tick", "before_entities", dt => ...);
var h = Systems.Replace("mobs:ai", "my_ai", dt => ...);             // disables mobs:ai, runs in its place
h.Dispose();                                                         // ... and restores mobs:ai
Systems.SetEnabled("mobs:spawner", false);                          // re-enabled when the plugin unloads
```

The built-in systems you can anchor on have names such as `entities:tick`, `mobs:ai`, `mobs:spawner`, `inventory:items`, `xp:orbs`, `vehicles:tick`, `village:tick` and `dragon:tick`. They belong to the server and can change between server versions, so anchoring on one ties your plugin to the server version you tested; `SetEnabled` tells you with `false` when a name does not exist.

- **Names** are the registry's names and are unique. Yours are namespaced by the plugin: `Add("regen", ...)` in a plugin called `Hello` registers `Hello:regen` (a name that already starts with `Hello:` is kept). Registering a name a built-in system or another plugin holds fails (throws, error logged). `SystemHandle.Name` returns the effective name.
- **Anchors** for `After`/`Before`/`Replace` are full registry names (not namespaced), and the new system takes the anchor's phase.
- **`Replace` is reversible by design**: the original is disabled and yours runs right after it. Dispose the handle or unload the plugin and the original comes back. Two plugins replacing or disabling the same system do not fight: it is re-enabled only when the last holder goes.
- **Registry changes made from inside a system** (`Register`, `Dispose`, `SetEnabled`) are queued and applied right after the tick; the handle is valid immediately.
- **`SetEnabled(name, bool)`** works on built-in and scripted systems, returns false for an unknown name, and is undone for a system your plugin disabled when it unloads.

## Hot reload and system names

A reloaded plugin registers its successor **before** the predecessor is released. For that moment the new system runs as `Hello:regen#2`, and is renamed back to `Hello:regen` (at the same position) once the old one is gone.

## Systems versus events versus the scheduler

| You want | Use |
|---|---|
| react to something that happened | an [event](events.md) |
| run logic every tick over many entities | a system and a query |
| run something later or periodically, not per entity | the [scheduler](scheduler.md) |
| change how vanilla behaves | `Replace`, `Before`, `After` or `SetEnabled` on a built-in system |

Managed systems are not multithreaded: they run on the tick thread, one after another.
