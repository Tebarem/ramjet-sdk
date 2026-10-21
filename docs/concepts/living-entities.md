# Living entities

Health, potion effects, attributes, equipment and the mob flags of the entities that have them: players, mobs and the other living things of the server. Use `LivingEntity` instead of raw NBT: it reads and writes the same state the commands write, and the clients see the change.

## Getting a living entity

| Code | Gives |
|---|---|
| `entity.AsLiving()` | a `LivingEntity?`: null when the entity is not living (an item, a painting, a bare ECS entity, a stale handle) |
| `player.AsLiving()` | the player's `LivingEntity`; every player is living |
| `living.Entity` | the underlying `Entity` (the handle you can keep, pass to events or to components) |
| `living.IsLiving` | whether the handle still names a living entity |

A `LivingEntity` is a handle like `Entity`: cheap to copy, safe to keep. A handle to an entity that has died and been removed reads defaults (`0`, `false`, `null`) and changes nothing. All members run on the tick thread.

## Health, healing and damage

| Member | Meaning |
|---|---|
| `Health` | the health in half hearts (get and set). Setting clamps it to `0 .. MaxHealth`, as vanilla does; `0` kills the entity through the normal death handling |
| `MaxHealth` | the `minecraft:max_health` attribute (20 by default). Read only: change it with an attribute |
| `Absorption` | the golden hearts (get and set), clamped to the `minecraft:max_absorption` attribute |
| `IsDead` | whether the entity is dead or dying |
| `Heal(amount)` | adds health, clamped to the maximum. Nothing happens to a dead entity. Returns whether the entity is alive |
| `Damage(amount, damageType, attacker)` | hurts the entity through the normal damage pipeline. Armour, enchantments, invulnerability and the damage cooldown apply, so the hit can be refused |

`Damage` takes a damage type key such as `"minecraft:generic"`, `"minecraft:magic"` or `"minecraft:player_attack"`. The optional `attacker` is the entity that caused the damage. An unknown key or a negative amount throws, so a typo does not pass silently.

```csharp
var living = player.AsLiving();
living.Health = living.MaxHealth;                       // full health
living.Damage(4, "minecraft:magic", attacker: zombie);  // refused when the player is invulnerable
```

## Potion effects

`AddEffect(PotionEffect)`, `RemoveEffect(key)`, `HasEffect(key)`, `GetEffect(key)` and `GetEffects()` work as they do for players (see [entities and ECS](entities-and-ecs.md)). `GetEffects()` lists every active effect with its duration, amplifier and display flags.

A `PotionEffect` with `DurationTicks = PotionEffect.Infinite` never runs out. The amplifier is the level minus one (0 is level I). An amplifier outside `0 .. 255` throws.

## Attributes and modifiers

```csharp
var speed = living.GetAttribute("minecraft:movement_speed");   // AttributeValue(Base, Value), or null
living.SetAttributeBase("minecraft:max_health", 40);            // like /attribute ... base set
living.AddAttributeModifier("minecraft:max_health", "myplugin:boost", 10, AttributeOperation.AddValue);
living.RemoveAttributeModifier("minecraft:max_health", "myplugin:boost");
```

- `GetAttribute` returns `null` when the entity does not have that attribute. A mob has the attributes of its type; a player has the player set.
- A modifier is identified by its id. Give your modifiers ids in your own namespace (`myplugin:boost`), so you can remove exactly yours. Adding a second modifier with the same id on the same attribute is refused.
- A plugin's modifier id must have a namespace other than `minecraft` (the engine's own modifiers use it, and removing one is refused). An entity holds at most 64 plugin modifiers, across all its attributes; the server also keeps a bounded number of distinct ids. Removing a modifier never uses up room.
- `AttributeOperation` is vanilla's: `AddValue` adds to the base, `AddMultipliedBase` adds a fraction of the base, `AddMultipliedTotal` multiplies the total.
- Setting a base value refreshes the entity, so a lower maximum health clamps the current health.

## Equipment

```csharp
var mob = zombie.AsLiving()!.Value;
mob.SetEquipment(EquipmentSlot.Head, new ItemStack("minecraft:iron_helmet"));
ItemStack hat = mob.GetEquipment(EquipmentSlot.Head);
mob.SetDropChance(EquipmentSlot.Head, 1.0);   // always drops the helmet on death
```

- `EquipmentSlot` has `MainHand`, `OffHand`, `Head`, `Chest`, `Legs`, `Feet` and `Body`.
- A mob's armour slot takes only an item for that slot; the hands take anything. An empty `ItemStack` clears the slot.
- Players: the same call changes the player's inventory (the hotbar slot for `MainHand`). Writing a player's slot plays the equip sound, as the vanilla equip does.
- Drop chances are for mobs: the default is 0.085, a value above 1 always keeps the item. A player's items are always the player's to keep, so the call returns `false` for players.

## Mob flags and state

| Member | Applies to | Meaning |
|---|---|---|
| `Silent` | every living entity | no ambient, hurt or death sounds |
| `Invulnerable` | every living entity | takes no damage (see [invulnerability](#invulnerability)) |
| `Invisible` | every living entity | the clients do not draw it. The invisibility effect sets the same flag while it lasts; clearing your own flag does not end the effect |
| `Glowing` | every living entity the clients track | the glowing outline, for every player online (it follows players who join or leave). The server keeps a limited number of plugin glows, so setting it can do nothing when there is no room; the getter tells you whether it applies to every player |
| `NoAi` | mobs | the mob does not move, look or attack on its own |
| `Persistent` | mobs | the mob is not despawned when no player is near |
| `CustomNameVisible` | mobs | the name tag shows at all times |
| `CustomName` | mobs | the name tag itself: a `Component`, `null` clears it |
| `Target` | mobs | the entity the mob attacks; `null` clears it |

The mob-only members are ignored for other entities (setting them does nothing; reading them gives `false` or `null`). A new `Target` is applied on the mob's next AI tick, through the same path its goals use, so a mob that refuses the target keeps its old one. The mob's goals can change the target again afterwards.

```csharp
var guard = zombieEntity.AsLiving()!.Value;   // a mob, from Entity.AsLiving()
guard.NoAi = true;                            // a statue
guard.CustomName = Component.Text("Guard");
guard.CustomNameVisible = true;
guard.Glowing = true;
```

## Riding

```csharp
var rider = zombie.AsLiving()!.Value;
var horse = horseEntity.AsLiving()!.Value;
bool mounted = horse.Mount(zombieEntity);   // the zombie rides the horse
Entity? vehicle = rider.Vehicle;            // the horse
IReadOnlyList<Entity> riders = horse.Passengers;
rider.Dismount();                           // lands on top of the horse
horse.EjectPassengers();                    // everyone off
```

`Mount` makes a living entity ride a vehicle the way `/ride <target> mount <vehicle>` does: it is refused when the entity already rides something, when the vehicle is a player, when the vehicle is the entity or one of the entity's own vehicles (a cycle), and when the two are in different dimensions. A mount does not ask the vehicle whether it is willing to carry the entity. `Dismount` and `EjectPassengers` are the `/ride ... dismount` and vanilla's `ejectPassengers`.

## Invulnerability

`Invulnerable` refuses damage like vanilla's `Entity.invulnerable` (the damage check of the server reads it for every living entity). Two details matter:

- A damage type tagged as bypassing invulnerability (such as falling into the void) still lands, and so does a hit from a player in creative mode. That is vanilla's rule for the flag.
- A player's creative invulnerability comes from the player's abilities, not from this flag. It refuses damage on its own, and clearing the flag does not switch it off.
- A mob keeps the flag in its saved data (the NBT `Invulnerable` tag, as vanilla writes it). A player's flag is runtime state: it is not written to the player's save, and it does not survive a dimension change or a respawn. Set it again after those.

## What is not here yet

- The saddle slot is not in `EquipmentSlot` yet.

## Thread and error rules

Every member runs on the tick thread (wrong-thread calls throw). Numbers must be finite: `NaN` and infinity throw `ArgumentOutOfRangeException`. Keys may leave out the `minecraft:` namespace. An unknown effect, attribute or modifier operation makes the call return `false` (or `null` for `GetAttribute`); an unknown damage type throws `ArgumentException`, because a hit with the wrong type would be a silent bug.
