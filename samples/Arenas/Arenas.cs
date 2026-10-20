namespace Arenas;

/// <summary>Sample of runtime worlds: one temporary arena world per player, made from a slime template. <c>/arena</c> copies the template into a new temporary
/// void world and teleports the player there; leaving the world (<c>/arena leave</c>, another teleport, quitting) unloads it again, and nothing of it is saved.
/// The template (<c>arena.rjwb</c> in the plugin's data folder) is built once: a void world with a small platform, exported as a blob. Replace the file with your
/// own build (<c>/world export</c> of a slime or temporary world) to change every new arena.</summary>
[PluginInfo("Arenas", Version = "1.0.0", Author = "Ramjet", Description = "Runtime worlds sample: temporary arena worlds copied from a slime template")]
public sealed class ArenasPlugin : Plugin
{
    private const string TemplateFile = "arena.rjwb";
    private static readonly BlockPos Spawn = new(0, 65, 0);

    private byte[]? _template;
    private Task<byte[]>? _building;
    private readonly Dictionary<Uuid, World> _arenas = [];
    private int _next;

    protected override void OnEnable()
    {
        Permissions.Register("arenas.play", PermissionDefault.True, "Use /arena");
        string file = Path.Combine(DataFolder, TemplateFile);
        if (File.Exists(file)) _template = File.ReadAllBytes(file);

        Commands.Register(Command.Literal("arena").Permission("arenas.play")
            .Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                if (_arenas.ContainsKey(player.Uuid)) throw new CommandFailure("You are in an arena already: /arena leave");
                ctx.Reply("Preparing your arena ...");
                _ = Join(player);
                return 1;
            })
            .Then(Command.Literal("leave").Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                if (!_arenas.ContainsKey(player.Uuid)) throw new CommandFailure("You are not in an arena");
                player.Teleport(Server.Overworld.SpawnLocation);  // PlayerChangedWorldEvent closes the arena
                return 1;
            })),
            description: "Play in an arena world of your own");

        // Whoever leaves their arena (any teleport, death respawn, /arena leave) or the server: the arena world goes, with nothing saved
        Events.On((ref PlayerChangedWorldEvent e) =>
        {
            if (_arenas.TryGetValue(e.Player.Uuid, out var arena) && e.FromWorld == arena) Close(e.Player.Uuid);
        });
        Events.On((ref PlayerQuitEvent e) => Close(e.Uuid));
    }

    protected override void OnDisable()
    {
        foreach (var uuid in _arenas.Keys.ToList()) Close(uuid);
    }

    private async Task Join(Player player)
    {
        try
        {
            byte[] template = _template ?? await (_building ??= BuildTemplate());
            // A temporary world: in memory only, gone when it is unloaded. It starts as a copy of the template (generator, spawn and blocks come from it).
            World arena = await Server.CreateWorldAsync(new WorldOptions($"arenas:arena_{++_next}") { Temporary = true, Template = template });
            // (the code after an await of a world task runs on the tick thread: the world can be used at once)
            if (!player.IsOnline)
            {
                await Server.UnloadWorldAsync(arena, save: false);
                return;
            }
            _arenas[player.Uuid] = arena;
            player.Teleport(new Location(arena, Spawn.X + 0.5, Spawn.Y, Spawn.Z + 0.5));
            player.SendMessage($"Welcome to {arena.Key}. /arena leave to go back.");
        }
        catch (Exception e)
        {
            if (!Server.IsTickThread) await Scheduler.ToTick();
            if (_template == null) _building = null;  // try to build the template again next time
            Log.Error("could not make an arena", e);
            if (player.IsOnline) player.SendMessage("Could not make your arena: " + e.Message);
        }
    }

    private void Close(Uuid player)
    {
        if (!_arenas.Remove(player, out var arena) || !arena.IsValid) return;
        // movePlayers: anyone else still inside goes to the overworld spawn first; save: false, a temporary world keeps nothing anyway
        Server.UnloadWorldAsync(arena, save: false, movePlayers: true)
            .ContinueWith(t => Log.Error("could not unload " + arena, t.Exception!), TaskContinuationOptions.OnlyOnFaulted);
    }

    // The first template: an empty void world with a 9 x 9 platform under the spawn, exported as a blob and kept in the data folder.
    private async Task<byte[]> BuildTemplate()
    {
        World world = await Server.CreateWorldAsync(new WorldOptions("arenas:template") { Generator = WorldGenerator.Void, Temporary = true, Spawn = Spawn });
        var tickets = ChunkTickets.AddArea(world, 0, 0, radius: 1);
        try
        {
            while (!tickets.All(t => t.IsLoaded)) await Scheduler.ToTick();  // blocks can only be set in loaded chunks
            var floor = BlockState.Parse("minecraft:smooth_stone");
            var edge = BlockState.Parse("minecraft:glass");
            for (int x = -4; x <= 4; x++)
                for (int z = -4; z <= 4; z++)
                {
                    bool border = Math.Abs(x) == 4 || Math.Abs(z) == 4;
                    world.SetBlock(new BlockPos(Spawn.X + x, Spawn.Y - 1, Spawn.Z + z), floor);
                    if (border) world.SetBlock(new BlockPos(Spawn.X + x, Spawn.Y, Spawn.Z + z), edge);
                }
            byte[] blob = await world.ExportAsync();
            Directory.CreateDirectory(DataFolder);
            await File.WriteAllBytesAsync(Path.Combine(DataFolder, TemplateFile), blob);
            await Scheduler.ToTick();  // back to the tick thread after the file write
            _template = blob;
            Log.Info($"arena template built ({blob.Length} bytes)");
            return blob;
        }
        finally
        {
            if (!Server.IsTickThread) await Scheduler.ToTick();
            foreach (var t in tickets) t.Release();
            if (world.IsValid) await Server.UnloadWorldAsync(world, save: false);
        }
    }
}
