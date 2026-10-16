# Entities, handles and the ECS

Why an Entity is a handle and not an object, how components, queries and systems work, and the reference-invalidation rule.

If you come from Paper, this is the biggest mental shift. In Bukkit a `Player` or `Zombie` is an object with methods and a lifetime managed by the JVM. In Ramjet the world is an **ECS** (flecs): an entity is only an id, its data lives in **components** stored in flat arrays, and behaviour lives in **systems** that run over those arrays. That layout is why the server can keep thousands of players on one process, and it shapes the plugin API.

## Entities are handles

`Entity` is a 64-bit id (`ulong Id`) wrapped in a struct. It is blittable, so it can sit in an event, a component or a collection at no cost, and **holding one never keeps anything alive**.

```csharp
Entity e = ev.Player.Entity;   // from an event: the player's entity
if (e.IsNull) return;          // default / "no entity" (e.g. no attacker for fall damage)
if (!e.IsAlive) return;        // destroyed?
e.Set(new Mana { Value = 20 });
e.Destroy();                   // an entity you created yourself; afterwards the handle is stale: IsAlive is false
```

A handle stays valid across component changes (the id never changes when you add or remove components). When the entity is destroyed the handle goes stale; operations on it fail safely (`Has<T>()` is false, `Get<T>()` throws) rather than touching the wrong entity, because ids carry a generation.

Entities of the game itself (players, mobs, items, projectiles ...) are not yours to destroy: use the game-facing calls such as `entity.Kill()` or `entity.Despawn()`. `entity.IsGameEntity` tells a game entity from a bare ECS entity you created with `Ecs.Spawn()`.

## Player, World and typed handles

For the common game objects the API gives you typed handles on top of the ECS:

- `Player` is a handle around the player's `Entity` (`player.Entity`): `Name`, `Uuid`, `Position`, `Location`, `Health`, `FoodLevel`, `SendMessage(...)`, `Teleport(...)`, `HasPermission(...)`, `Inventory` and much more. Event fields such as `PlayerJoinEvent.Player` are already `Player` values; `Server.Players` and `Server.GetPlayer(...)` give you more. A player who leaves leaves a stale handle (`player.IsOnline` is false); a rejoin gets a new entity, so an old handle never silently refers to somebody else. `Player.None` is the "no player" value.
- `World` (namespace `Mc.Scripting.Worlds`) is a handle to a dimension (`World.Overworld`, `World.Nether`, `World.End`, or `Server.GetWorld("minecraft:the_nether")`). Blocks are addressed with a `BlockPos` and read and written through the world: `world.GetBlock(pos)` returns a `BlockState` value, `world.SetBlock(pos, state)` changes it. Nothing is cached in a handle, so it never goes stale.
- **Stale handles fail safe**: reads return `default`/empty and writes are no-ops (a stale `Player.SendMessage` does nothing); nothing throws for staleness.
- The server's own components (position, health, inventory ...) are **read-only** for plugins. `Entity.Get<T>()` on one returns a copy, and writing to it changes nothing; change the game through the typed calls (`player.Health = ...`, `player.Teleport(...)`). Components you register yourself are plain read/write data, as described below.

| Paper / Bukkit | Ramjet |
|---|---|
| `Player player` object, `player.getHealth()` | `Player` handle: `player.Health` |
| `player.setMetadata("mana", ...)` / `PersistentDataContainer` | your own component: `player.Set(new Mana { ... })` |
| `UUID` lookups | `Server.GetPlayer(uuid)`; `Uuid` is also in join/quit events |
| Weak references, `player.isOnline()` | `player.IsOnline` / `entity.IsAlive` |

## Components: plain data you own

A component is an **unmanaged struct** (no references, no `string`, no arrays) that you declare and register under a global key:

```csharp
public struct Mana { public int Value; public int Max; }

protected override void OnEnable()
{
    Ecs.RegisterComponent<Mana>("mymana:mana");   // key: "plugin:name"
}
```

- The key is global and stable. Registering the same key with the **same layout** after a hot reload keeps every entity's values. A changed layout drops the values (see [hot reload](hot-reload.md)).
- **A component type must be owned by one plugin.** The API caches one id per CLR type for the whole process, so do not register a shared type (such as `Vec3d`) under different keys from different plugins; a second binding throws.
- Strings and variable data do not fit in an unmanaged struct. Keep them in a side dictionary keyed by `Entity` (clean it up when the entity dies) or write them to disk, and keep only a numeric id in the component.
- Tags are empty-ish structs: `public struct Frozen { }`, `entity.Set(new Frozen())`, `entity.Has<Frozen>()`.

Reading and writing (`Player` has the same `Get`, `TryGet`, `Has`, `Set` and `Remove`):

| Call | Meaning |
|---|---|
| `entity.Set(new T { ... })` | add or overwrite a copy |
| `entity.Has<T>()` | present? |
| `entity.TryGet<T>(out var copy)` | copy out; false when missing |
| `ref var c = ref entity.Get<T>()` | in-place access; **throws** when missing |
| `ref var c = ref entity.GetOrAdd<T>()` | in-place access, adding a zeroed component first |
| `ref var c = ref entity.TryGetRef<T>()` | a null ref when absent: test with `Unsafe.IsNullRef(ref c)` |
| `entity.Remove<T>()` | remove |

## The rule that bites: refs die on structural change

A `ref T` from `Get`, `GetOrAdd`, `TryGetRef` or a query column points **into the table that stores the entity**. It is valid only until the next *structural change touching that table*: adding or removing a component on **any** entity of the same archetype, destroying one, or creating one that lands there. The engine does these changes itself in the middle of its tick, so it can invalidate your ref between two lines of your code if you call into it.

```csharp
// WRONG: the ref may dangle after Set<Frozen> moves the entity to another table
ref var mana = ref player.Get<Mana>();
player.Set(new Frozen());
mana.Value--;                              // undefined

// RIGHT: copy what you need, or re-fetch after the structural change
int value = player.Get<Mana>().Value;
player.Set(new Frozen());
player.Get<Mana>().Value = value - 1;
```

Practical habits:

1. Keep the **`Entity`** in your data structures, never the `ref`; re-fetch when you need it.
2. Take the value out first when you must call other APIs in between.
3. Inside a query loop you may read and write the components you iterate; add/remove/destroy made *inside* the loop are deferred until it ends (reads still see the old state).
4. Register components in `OnEnable` and add them at spawn/join time, so a later `Set` does not move the entity.

## Queries

A query is a cached, indexed view over all entities that have a set of components. Create it once and iterate it every tick.

```csharp
private Query<Mana> _mana = null!;

protected override void OnEnable()
{
    Ecs.RegisterComponent<Mana>("mymana:mana");
    _mana = Ecs.Query<Mana>();                // up to three components: Query<T1, T2, T3>
}

private void Regen()
{
    foreach (var chunk in _mana)              // one chunk per archetype: a Span<T> per component
    {
        var mana = chunk.Field<Mana>(0);      // term index = order of the type arguments
        for (int i = 0; i < mana.Length; i++)
            if (mana[i].Value < mana[i].Max) mana[i].Value++;
    }
}

// the convenient form, a little slower:
_mana.Each((Entity entity, ref Mana m) => { if (m.Value > m.Max) m.Value = m.Max; });
```

`chunk.Entities` gives the entity handles in the same order as the columns. Breaking out of the `foreach` early releases the native iterator.

## Systems

A system is a function the server calls once per tick in a chosen phase. See [systems and phases](systems.md).
