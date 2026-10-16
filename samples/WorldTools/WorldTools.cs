namespace WorldTools;

/// <summary>Sample of the world API: <c>/wt</c> subcommands that change blocks with update flags, edit a sign through its NBT, spawn mobs with NBT, make explosions, change the
/// time and weather, keep chunks loaded with tickets and print an entity's data. Each subcommand is a few lines: this is the plugin to copy snippets from.</summary>
[PluginInfo("WorldTools", Version = "1.0.0", Author = "Ramjet", Description = "World API sample: blocks, signs, mobs, explosions, time, weather, chunk tickets")]
public sealed class WorldToolsPlugin : Plugin
{
    private readonly List<ChunkTicket> _tickets = [];

    protected override void OnEnable()
    {
        Permissions.Register("worldtools.use", PermissionDefault.OpLevel(2), "Use /wt");

        Commands.Register(Command.Literal("wt").Permission("worldtools.use")
            // /wt sphere <block> <radius>: a hollow sphere around the player, written without neighbour updates (fast) and with clients told
            .Then(Command.Literal("sphere").Then(Command.Argument("block", Arg.BlockState()).Then(Command.Argument("radius", Arg.Int(1, 24)).Executes(Sphere))))
            // /wt sign <text>: puts a sign in front of the player and writes its text through the block entity data
            .Then(Command.Literal("sign").Then(Command.Argument("text", Arg.GreedyString()).Executes(Sign)))
            // /wt mob <type> <count>: spawns mobs around the player with NBT (a custom name, no drops)
            .Then(Command.Literal("mob").Then(Command.Argument("type", Arg.Word()).Then(Command.Argument("count", Arg.Int(1, 20)).Executes(Mob))))
            // /wt boom <power> [terrain]: an explosion at the player's feet that may leave the terrain alone
            .Then(Command.Literal("boom").Then(Command.Argument("power", Arg.Float(0.5f, 10f)).Then(Command.Argument("terrain", Arg.Bool()).Executes(Boom))))
            // /wt time <ticks> and /wt storm
            .Then(Command.Literal("time").Then(Command.Argument("ticks", Arg.Int(0)).Executes(ctx =>
            {
                var world = ctx.RequirePlayer().World;
                if (!world.HasDayTime) throw new CommandFailure($"{world.Key} has no day/night cycle");
                world.DayTime = ctx.GetInt("ticks");
                ctx.Reply($"It is {world.DayTime % 24000} ticks into the day");
                return 1;
            })))
            .Then(Command.Literal("storm").Executes(ctx =>
            {
                var world = ctx.RequirePlayer().World;
                bool ok = world.SetWeather(WeatherType.Thunder, durationTicks: 6000);
                ctx.Reply(ok ? "Thunderstorm for five minutes" : $"{world.Key} has no weather");
                return ok ? 1 : 0;
            }))
            // /wt keep <radius>: keeps the chunks around the player loaded until /wt release (or until the plugin unloads)
            .Then(Command.Literal("keep").Then(Command.Argument("radius", Arg.Int(0, 4)).Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                var pos = player.Location.BlockPos;
                _tickets.AddRange(ChunkTickets.AddArea(player.World, pos.X >> 4, pos.Z >> 4, ctx.GetInt("radius")));
                ctx.Reply($"Holding {_tickets.Count} chunks");
                return _tickets.Count;
            })))
            .Then(Command.Literal("release").Executes(ctx =>
            {
                int n = _tickets.Count;
                foreach (var t in _tickets) t.Release();
                _tickets.Clear();
                ctx.Reply($"Released {n} chunks");
                return n;
            }))
            // /wt nearby: the entities within 8 blocks with their type and health (read from their NBT)
            .Then(Command.Literal("nearby").Executes(Nearby)),
            description: "World API examples");
    }

    private static int Sphere(CommandContext ctx)
    {
        var player = ctx.RequirePlayer();
        var world = player.World;
        var state = ctx.GetBlockState("block");
        int r = ctx.GetInt("radius");
        var center = player.Location.BlockPos;
        int changed = 0;
        for (int x = -r; x <= r; x++)
            for (int y = -r; y <= r; y++)
                for (int z = -r; z <= r; z++)
                {
                    double d = Math.Sqrt(x * x + y * y + z * z);
                    if (d > r || d < r - 1) continue;  // the shell only
                    var pos = new BlockPos(center.X + x, center.Y + y, center.Z + z);
                    if (!world.TryGetBlock(pos, out var old) || !old.IsAir) continue;  // unloaded chunk or not free
                    // Clients only: no neighbour updates, so redstone and plants around do not react to every block of the shell
                    if (world.SetBlock(pos, state, BlockUpdateFlags.Clients)) changed++;
                }
        ctx.Reply($"{changed} blocks placed");
        return changed;
    }

    private static int Sign(CommandContext ctx)
    {
        var player = ctx.RequirePlayer();
        var world = player.World;
        var pos = player.Location.BlockPos;
        if (!world.SetBlock(pos, "minecraft:oak_sign[rotation=0]")) throw new CommandFailure("Cannot place a sign here");
        // The sign's text lives in its block entity: four JSON components for the front side
        var messages = new NbtList();
        string text = ctx.GetString("text");
        for (int line = 0; line < 4; line++)
            messages.Add(new NbtString(line == 0 ? $"\"{text.Replace("\"", "\\\"")}\"" : "\"\""));
        var data = new NbtCompound().Put("front_text", new NbtCompound().Put("messages", messages));
        if (!world.SetBlockEntityData(pos, data)) throw new CommandFailure("The sign has no block entity");
        ctx.Reply("Sign placed: " + world.GetBlockEntityData(pos)!.GetCompound("front_text")!.GetList("messages")![0]);
        return 1;
    }

    private static int Mob(CommandContext ctx)
    {
        var player = ctx.RequirePlayer();
        var world = player.World;
        int spawned = 0;
        for (int i = 0; i < ctx.GetInt("count"); i++)
        {
            var offset = new Vec3d(Random.Shared.NextDouble() * 6 - 3, 0, Random.Shared.NextDouble() * 6 - 3);
            // NBT is applied as given: here a custom name and a health value; leaving the compound out rolls the vanilla random attributes instead
            var nbt = new NbtCompound().PutString("CustomName", "Sample " + (i + 1)).PutFloat("Health", 10f);
            var mob = world.SpawnEntity(ctx.GetString("type"), player.Position + offset, nbt);
            if (!mob.IsNull) spawned++;
        }
        ctx.Reply($"{spawned} spawned");
        return spawned;
    }

    private static int Boom(CommandContext ctx)
    {
        var player = ctx.RequirePlayer();
        bool ok = player.World.Explode(player.Position, ctx.GetFloat("power"), setFire: false, breakBlocks: ctx.GetBool("terrain"), source: player.Entity);
        return ok ? 1 : 0;
    }

    private static int Nearby(CommandContext ctx)
    {
        var player = ctx.RequirePlayer();
        int count = 0;
        foreach (var e in player.World.Entities)
        {
            if (e == player.Entity || !e.TryGetLocation(out var where)) continue;
            if ((where.Position - player.Position).Length > 8) continue;
            double health = e.GetNbt()?.GetDouble("Health", -1) ?? -1;
            ctx.Reply($"{e.TypeKey} at {where.BlockPos.X},{where.BlockPos.Y},{where.BlockPos.Z}" + (health >= 0 ? $" health {health}" : ""));
            count++;
        }
        return count;
    }
}
