# Travelling between worlds

A /hop command that takes a player to the same spot in another dimension on safe ground: chunk tickets, waiting for a chunk, reading the terrain, building a platform, cross-dimension teleports and the portal events.

Plugin: **WorldHop**. `/hop <dimension>` takes the player to the same x and z in the overworld, the nether or the end (nether coordinates scaled by 8, like a portal), standing on solid ground, or on a small obsidian platform where there is none (the void of the end, an ocean). Travel to the end needs a permission, by command and by portal.

| Command | Does | Permission |
|---|---|---|
| `/hop <dimension>` | teleports to the same spot in `minecraft:overworld`, `minecraft:the_nether` or `minecraft:the_end` | `worldhop.use` (default: operators) |
| (portals) | an end portal only works with the permission | `worldhop.end` (default: operators) |

The [worlds concept page](../concepts/worlds.md) explains each call used here.

## How it works

1. **Resolve the world.** `Arg.Dimension()` lets the client complete dimension keys; `Server.GetWorld(ctx.GetText("dimension"))` turns the key into a `World` handle (null for an unknown key).
2. **Hold the destination chunk.** It is almost never loaded: `ChunkTickets.Add(world, x >> 4, z >> 4)` asks for it, and loading (or generating) it takes some ticks. Reading blocks of a chunk that is not loaded gives air, so the plugin must wait.
3. **Wait without blocking.** A task repeated every tick (`Scheduler.RunEvery(1, ...)`) checks `ticket.IsLoaded`; it gives up after 10 seconds or when the player leaves, and cancels itself either way. Never wait in a loop inside a handler: the chunk only loads between ticks.
4. **Read the terrain.** `HighestBlockY` (cheap, from the height maps) gives the surface. The nether has a bedrock roof that the height maps see, so there the plugin searches down from y 120 with `GetBlock`. Ground is anything that is not air and not a fluid; `IsAir` alone is not enough, because caves and the void have their own air blocks.
5. **Build when there is no ground.** Nine `SetBlock` calls make a platform and clear the two layers above it.
6. **Teleport and let go.** `player.Teleport(new Location(world, ...))` crosses dimensions. It fires `PlayerTeleportEvent` (cause `Plugin`), which another plugin may cancel: the return value says whether the player moved. The ticket is released afterwards in every case; the player's own view keeps the chunks around them loaded.

`PlayerPortalEvent` stops end portals for players without `worldhop.end` before they move, and `PlayerChangedWorldEvent` logs every change of dimension, whatever caused it.

## The plugin

The whole plugin is one file. Create a project from the template ([create a plugin](../getting-started/create-a-plugin.md)), replace its plugin class with this file (`WorldHop.cs`) and build: the server hot reloads it.

```csharp
namespace WorldHop;

[PluginInfo("WorldHop", Version = "1.0.0", Description = "Safe teleports between the overworld, the nether and the end")]
public sealed class WorldHopPlugin : Plugin
{
    private const int MaxWaitTicks = 200;  // 10 seconds for the destination chunk to load or generate

    // Parsed once: BlockState.Parse looks the text up in the block registry
    private static readonly BlockState s_caveAir = BlockState.Parse("minecraft:cave_air");
    private static readonly BlockState s_voidAir = BlockState.Parse("minecraft:void_air");
    private static readonly BlockState s_platform = BlockState.Parse("minecraft:obsidian");

    protected override void OnEnable()
    {
        Permissions.Register("worldhop.use", PermissionDefault.Op, "Use /hop");
        Permissions.Register("worldhop.end", PermissionDefault.Op, "Travel to the end, by /hop or by portal");

        // /hop <dimension>: the same spot (nether coordinates scaled by 8) in another dimension, on safe ground
        Commands.Register(Command.Literal("hop").Permission("worldhop.use")
            .Then(Command.Argument("dimension", Arg.Dimension()).Executes(Hop)),
            description: "Teleport to another dimension");

        // Portals are the game's own travel: PlayerPortalEvent can stop them before the player moves
        Events.On((ref PlayerPortalEvent e) =>
        {
            if (e.ToWorld == World.End && !e.Player.HasPermission("worldhop.end"))
            {
                e.Cancelled = true;
                e.Player.SendActionBar("The end is closed");
            }
        });

        // Fires after every change of dimension, whatever moved the player: a portal, /hop, another plugin
        Events.On((ref PlayerChangedWorldEvent e) =>
            Log.Info($"{e.Player.Name}: {e.FromWorld.Key} -> {e.ToWorld.Key}"));
    }

    private int Hop(CommandContext ctx)
    {
        var player = ctx.RequirePlayer();
        var target = Server.GetWorld(ctx.GetText("dimension")) ?? throw new CommandFailure("No such dimension");
        if (target == player.World) throw new CommandFailure("You are already there");
        if (target == World.End && !player.HasPermission("worldhop.end")) throw new CommandFailure("The end is closed");

        var from = player.Location;
        double scale = (from.World == World.Nether ? 8.0 : 1.0) / (target == World.Nether ? 8.0 : 1.0);
        int x = (int)Math.Floor(from.X * scale), z = (int)Math.Floor(from.Z * scale);

        // The destination chunk is probably not loaded: hold it with a ticket and wait until it is
        var ticket = ChunkTickets.Add(target, x >> 4, z >> 4);
        int waited = 0;
        ScheduledTask? wait = null;
        wait = Scheduler.RunEvery(1, () =>
        {
            if (!player.IsOnline || (!ticket.IsLoaded && ++waited < MaxWaitTicks)) return;
            wait!.Cancel();
            if (ticket.IsLoaded && player.IsOnline)
            {
                int y = FindStandY(target, x, z) ?? BuildPlatform(target, x, z);
                if (!player.Teleport(new Location(target, x + 0.5, y, z + 0.5, from.Yaw, from.Pitch)))
                    player.SendMessage("The teleport was cancelled");
            }
            else if (player.IsOnline)
            {
                player.SendMessage("The destination did not load in time");
            }
            ticket.Release();  // the player's own view keeps the chunks around them loaded from here on
        });
        ctx.Reply($"Looking for a safe spot in {target.Key} ...");
        return 1;
    }

    // The feet y of the first spot with solid ground and two free blocks above it, or null
    private static int? FindStandY(World world, int x, int z)
    {
        // The nether has a bedrock roof: the height maps see the roof, so search down from below it
        bool nether = world == World.Nether;
        int top = nether ? 120 : world.HighestBlockY(x, z);
        int bottom = nether ? world.MinY + 1 : top;  // elsewhere only the surface: a column below it would be a cave
        for (int y = top; y >= bottom; y--)
            if (IsGround(world.GetBlock(new BlockPos(x, y, z)))
                && IsFree(world.GetBlock(new BlockPos(x, y + 1, z)))
                && IsFree(world.GetBlock(new BlockPos(x, y + 2, z))))
                return y + 1;
        return null;
    }

    // No ground (the void of the end, an ocean, a lava sea): a 3 x 3 platform with room above it
    private static int BuildPlatform(World world, int x, int z)
    {
        int feet = Math.Clamp(Math.Max(world.HighestBlockY(x, z) + 1, 64), world.MinY + 1, world.MaxY - 2);
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                world.SetBlock(new BlockPos(x + dx, feet - 1, z + dz), s_platform);
                world.SetBlock(new BlockPos(x + dx, feet, z + dz), BlockState.Air);
                world.SetBlock(new BlockPos(x + dx, feet + 1, z + dz), BlockState.Air);
            }
        return feet;
    }

    // IsAir is minecraft:air only: caves and the void have their own air blocks
    private static bool IsFree(BlockState s) => s.IsAir || s == s_caveAir || s == s_voidAir;

    private static bool IsGround(BlockState s)
    {
        if (IsFree(s)) return false;
        string block = s.BlockKey;
        return block != "minecraft:water" && block != "minecraft:lava";
    }
}
```

## Things to notice

- **Everything runs on the tick thread**, the repeating task included, so the plugin can touch the world and the player directly. There is nothing to lock.
- **Tickets are released automatically when the plugin unloads**, so a hot reload in the middle of a hop leaks nothing. The scheduled task is cancelled by the unload too.
- **The player handle is checked again** (`IsOnline`) inside the task: up to 10 seconds pass between the command and the teleport.
- **The block states are parsed once**, in static fields: `BlockState.Parse` looks the text up in the block registry.
- **The changes are saved with the world.** The platform needs no saving code: like any block change it is written by the next save.

## Taking it further

- Remember where each player left each dimension and send them back there: save the world key and coordinates in your [data folder](../concepts/config-and-data.md).
- Add a cooldown or a cost, as in the [homes guide](homes-plugin.md).
- Use `ChunkTickets.AddArea` with a radius of 1 to hold the chunks around the destination as well, if your plugin builds more than one chunk's worth of blocks.
