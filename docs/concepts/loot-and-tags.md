# Loot tables and tags

Roll the datapack's loot tables the way the game rolls chests, mob drops and fishing, set the loot of a container, and ask the datapack's tags whether a block, item, entity type or biome is in one.

## Loot tables

`LootTables` gives access to every loot table of the datapack: `minecraft:chests/simple_dungeon`, `minecraft:entities/zombie`, `minecraft:blocks/stone`, `minecraft:gameplay/fishing` and the rest. Ids are `namespace:path`; the namespace may be left out for `minecraft`.

```csharp
using Mc.Scripting.Loot;

if (LootTables.Exists("minecraft:entities/zombie"))
{
    var context = new LootContext(zombie.Location.World)
    {
        Origin = zombie.Location,               // where it died
        This = zombie,                          // the mob that died
        Killer = killer,                        // the attacker, who gets the kill
        Player = killerPlayer,                  // the player who last hurt it (killed_by_player conditions)
        DamageType = "minecraft:player_attack", // what the kill was
    };
    foreach (ItemStack drop in LootTables.Generate("minecraft:entities/zombie", context))
        zombie.Location.World.DropItem(zombie.Location.Position, drop);
}
```

`LootTables.Generate(key, context)` returns the stacks the table drops, split to their maximum size, empty stacks removed. `LootTables.Keys()` lists every table id, and `Exists(key)` tells whether one is there.

### The context

A `LootContext` is created with the world the loot is rolled in (its dimension decides the location conditions). Set the parameters the table reads:

| Property | Vanilla parameter | Read by |
|---|---|---|
| `Origin` (a `Location`) | origin | almost every table; the position of the block or entity |
| `This` (an `Entity`) | this entity | mob and gift tables, the opener of a chest |
| `Killer` (an `Entity`) | attacking entity | the attacker, also taken as the direct attacker |
| `Player` (a `Player`) | last damage player | `killed_by_player` and similar conditions |
| `DamageType` (a damage type id) | damage source | mob tables (`minecraft:player_attack`, `minecraft:mob_attack`, `minecraft:magic`) |
| `Tool` (an `ItemStack`) | tool | block drops, fishing, archaeology |
| `BlockState` | block state | block drops |
| `ExplosionRadius` | explosion radius | explosion decay |
| `Luck` | luck | luck-based weights (default `0`; the server does not read a player's Luck for you) |
| `Seed` | seed | a repeatable roll; null (the default) uses the table's random sequence, which the server advances |

`LootContextType` says which parameters the roll is given. The default, `Auto`, takes the type the table itself declares (its `"type"`). Named types are checked against vanilla's sets: `Chest`, `Block`, `Entity`, `Fishing`, `Gift`, `Barter`, `Vault`, `Archaeology`, `Equipment`, `Selector`, `Command` and the two advancement types.

The check is vanilla's: every parameter the type or the table requires must be set, and nothing the type does not allow may be set. A missing one makes `Generate` throw an `ArgumentException` that names it, for example `needs the parameters origin, damage_source`. An entity handle that has died before the roll is an error too. A table without a type is not checked for requirements of its own.

### Seeds and repeatable rolls

With `Seed` set, the same table and context give the same stacks every time, which is what a plugin needs for a reproducible reward. Without it, the roll uses the table's own random sequence, the same one `/loot` uses, so it differs from roll to roll and the server keeps the sequence across restarts.

### Containers

`LootTables.FillContainer(world, pos, key, seed)` puts a loot table on a chest, trapped chest, barrel or shulker box, like the `LootTable` tag of a container in vanilla. The contents are rolled the first time someone opens or changes the container, and replace what it held then. It returns false for any other block. Vanilla also lets hoppers, droppers, dispensers and crafters take a table; the server does not roll those yet, so they return false.

```csharp
bool filled = LootTables.FillContainer(Server.Overworld, chestPos, "minecraft:chests/simple_dungeon", seed: 1234);
```

### Limits

A roll returns at most 512 stacks; a table that drops more throws an `InvalidOperationException`. Loot and tags are read on the tick thread, like the rest of the game API.

## Tags

`Tags` answers whether a block, item, entity type or biome is in a datapack tag. The tag name may be given with or without the leading `#`, and the keys are ids (`minecraft:oak_log`), with the namespace optional for `minecraft`.

```csharp
bool isLog = Tags.Blocks.Contains("#minecraft:logs", "minecraft:oak_log");   // true
IReadOnlyList<string> swords = Tags.Items.Values("minecraft:swords");       // the item ids the tag lists
bool isUndead = Tags.EntityTypes.Contains("minecraft:undead", "minecraft:zombie");
bool isForest = Tags.Biomes.Contains("minecraft:is_forest", "minecraft:birch_forest");
```

Two helpers ask the same question of a value you already have:

```csharp
if (world.GetBlock(pos).IsIn("#minecraft:logs")) { /* ... */ }   // the block of the state, by its id
if (stack.IsIn("#minecraft:swords")) { /* ... */ }       // the item of the stack; components do not matter
```

A tag that lists other tags is expanded the way the datapack expands it, so the answer already includes them. An unknown tag or key is simply not a member, and `Values` of an unknown tag is empty; nothing throws for a name the datapack does not know.

## What is not available yet

* The interacting and target entities, the enchantment level, the block entity and the container of a vanilla context are not in `LootContext`. A table that needs one of them fails the check, and `Generate` throws an `ArgumentException` that names it.
* Weather and time conditions read the defaults: clear weather and game time `0`, whatever the world is doing. The dimension is the world's.
* `FillContainer` only sets the table; it does not roll the contents for you.
