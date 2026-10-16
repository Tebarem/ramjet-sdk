# Command argument types

The Brigadier argument types available to commands.

The 62 argument types of vanilla's command argument registry (Minecraft 26.3, protocol 777), as the server's command engine implements them. Plugin commands use them through the `Arg` factories: `Command.Argument("name", Arg.Int(1, 10))`. The value is read in the executor with the typed getters of `CommandContext` (`GetInt`, `GetString`, `GetPlayer`, `GetBlockPos`, `GetVec3`, ...); `GetText` returns the text of any argument as typed (block states, items, components, NBT, uuids, resource ids), and `Has("name")` tells whether an optional argument was given.

The flat `Commands.Register("name", handler)` form gives you the raw argument strings instead of typed arguments.

Factories that take parameters: `Arg.Int(min, max)`, `Arg.Long(min, max)`, `Arg.Float(min, max)`, `Arg.Double(min, max)`, `Arg.Time(minTicks)`, `Arg.ScoreHolder(multiple)`, and `Arg.Resource(registry)`, `ResourceKey`, `ResourceOrTag`, `ResourceOrTagKey`, `ResourceSelector` which take the name of a registry such as `"minecraft:enchantment"`.

| `Arg` factory | Vanilla argument type | Description |
|---|---|---|
| `Arg.Bool` | `brigadier:bool` | true or false. |
| `Arg.Float` | `brigadier:float` | Single-precision number, optional min/max. |
| `Arg.Double` | `brigadier:double` | Double-precision number, optional min/max. |
| `Arg.Int` | `brigadier:integer` | Signed 32-bit integer, optional min/max. |
| `Arg.Long` | `brigadier:long` | Signed 64-bit integer, optional min/max. |
| `Arg.Word / String / GreedyString` | `brigadier:string` | A word, a quoted phrase, or the rest of the line (single word / quotable phrase / greedy phrase). |
| `Arg.Player / Players / Entity / Entities` | `minecraft:entity` | An entity selector or player name; single or multiple, players only or any entity. |
| `Arg.GameProfile` | `minecraft:game_profile` | A player profile: a name, a selector or a UUID (may name offline players). |
| `Arg.BlockPos` | `minecraft:block_pos` | Integer block coordinates; supports ~ relative and ^ local coordinates. |
| `Arg.ColumnPos` | `minecraft:column_pos` | X and Z block coordinates. |
| `Arg.Vec3` | `minecraft:vec3` | Three double coordinates (x y z), with ~ and ^. |
| `Arg.Vec2` | `minecraft:vec2` | Two double coordinates (x z). |
| `Arg.BlockState` | `minecraft:block_state` | A block id with optional [properties] and {nbt}. |
| `Arg.BlockPredicate` | `minecraft:block_predicate` | A block state or #tag with optional [properties] and {nbt}. |
| `Arg.Item` | `minecraft:item_stack` | An item id with optional [component=value] list. |
| `Arg.ItemPredicate` | `minecraft:item_predicate` | An item id, #tag or * with component tests. |
| `Arg.Color` | `minecraft:team_color` | - |
| `Arg.HexColor` | `minecraft:hex_color` | An RRGGBB hex colour. |
| `Arg.Component` | `minecraft:component` | A text component as SNBT or JSON. |
| `Arg.Style` | `minecraft:style` | A text component style. |
| `Arg.Message` | `minecraft:message` | The rest of the line, with entity selectors expanded. |
| `Arg.NbtCompound` | `minecraft:nbt_compound_tag` | A compound tag in SNBT. |
| `Arg.NbtTag` | `minecraft:nbt_tag` | Any NBT tag in SNBT. |
| `Arg.NbtPath` | `minecraft:nbt_path` | A path into an NBT structure. |
| `Arg.Objective` | `minecraft:objective` | The name of a scoreboard objective. |
| `Arg.ObjectiveCriteria` | `minecraft:objective_criteria` | A scoreboard criterion (dummy, health, stat types ...). |
| `Arg.Operation` | `minecraft:operation` | A scoreboard operation (=, +=, <, > ...). |
| `Arg.Particle` | `minecraft:particle` | A particle id with its options. |
| `Arg.Angle` | `minecraft:angle` | A yaw angle, with ~ for relative. |
| `Arg.Rotation` | `minecraft:rotation` | Yaw and pitch (two angles). |
| `Arg.ScoreboardSlot` | `minecraft:scoreboard_slot` | A scoreboard display slot. |
| `Arg.ScoreHolder` | `minecraft:score_holder` | An entity, selector, name or * that holds scores. |
| `Arg.Swizzle` | `minecraft:swizzle` | A combination of x, y and z axes. |
| `Arg.Team` | `minecraft:team` | The name of a team. |
| `Arg.ItemSlot` | `minecraft:item_slot` | One inventory slot name (container.0, armor.head ...). |
| `Arg.ItemSlots` | `minecraft:item_slots` | A slot range name such as container.* or hotbar.*. |
| `Arg.ResourceLocation` | `minecraft:resource_location` | A namespaced id such as minecraft:stone. |
| `Arg.Function` | `minecraft:function` | A function id or #tag. |
| `Arg.EntityAnchor` | `minecraft:entity_anchor` | feet or eyes. |
| `Arg.IntRange` | `minecraft:int_range` | An integer range such as 1..5. |
| `Arg.FloatRange` | `minecraft:float_range` | A decimal range such as 0.5..2. |
| `Arg.Dimension` | `minecraft:dimension` | A dimension id. |
| `Arg.GameMode` | `minecraft:gamemode` | survival, creative, adventure or spectator. |
| `Arg.Time` | `minecraft:time` | A duration with an optional unit (t, s, d). |
| `Arg.ResourceOrTag` | `minecraft:resource_or_tag` | A registry entry id or #tag. |
| `Arg.ResourceOrTagKey` | `minecraft:resource_or_tag_key` | - |
| `Arg.Resource` | `minecraft:resource` | A registry entry id. |
| `Arg.ResourceKey` | `minecraft:resource_key` | - |
| `Arg.ResourceSelector` | `minecraft:resource_selector` | - |
| `Arg.TemplateMirror` | `minecraft:template_mirror` | none, left_right or front_back. |
| `Arg.TemplateRotation` | `minecraft:template_rotation` | none, clockwise_90, 180 or counterclockwise_90. |
| `Arg.Heightmap` | `minecraft:heightmap` | A heightmap type. |
| `Arg.LootTable` | `minecraft:loot_table` | A loot table id or inline definition. |
| `Arg.LootPredicate` | `minecraft:loot_predicate` | - |
| `Arg.LootModifier` | `minecraft:loot_modifier` | - |
| `Arg.ContextFloatProvider` | `minecraft:context_float_provider` | - |
| `Arg.ContextIntProvider` | `minecraft:context_int_provider` | - |
| `Arg.SlotSource` | `minecraft:slot_source` | - |
| `Arg.Dialog` | `minecraft:dialog` | - |
| `Arg.Feature` | `minecraft:feature` | - |
| `Arg.SwingAnimation` | `minecraft:swing_animation` | - |
| `Arg.Uuid` | `minecraft:uuid` | A UUID. |
