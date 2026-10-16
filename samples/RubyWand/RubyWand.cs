namespace RubyWand;

/// <summary>Sample: a custom item with a right-click ability and a resource pack for its look. The wand is a stick with data components (<c>custom_data</c> holding its id, an
/// <c>item_model</c>, a name, lore, rarity and the enchantment glint), so no client mod is needed. Right-clicking it heals the player and sprays particles (with a two second
/// cooldown the content layer enforces); <c>/rubywand give</c> hands one out and <c>/rubywand pack</c> sends the generated resource pack that gives it a ruby texture.</summary>
/// <remarks>The pack is served by the server's built-in pack host: set <c>content.pack_host_port</c> in Ramjet.toml (and <c>pack_host_bind</c> / <c>pack_host_url</c> for players on other
/// machines), or host the zip yourself and use <c>pack.ToResourcePack(url)</c>.</remarks>
[PluginInfo("RubyWand", Version = "1.0.0", Author = "Ramjet", Description = "A custom item with a right-click ability")]
public sealed class RubyWandPlugin : Plugin
{
    private CustomItem _wand = null!;

    protected override void OnEnable()
    {
        Permissions.Register("rubywand.use", PermissionDefault.True, "Use the ruby wand");
        Permissions.Register("rubywand.give", PermissionDefault.OpLevel(2), "Give out ruby wands and send the pack");

        _wand = new CustomItem("rubywand:wand", "minecraft:stick")
            .Model("rubywand:wand")
            .Name(Component.Text("Ruby Wand").Color(TextColor.Red))
            .Lore(Component.Text("Right click to feel better").Color(TextColor.Gray).Italic(false))
            .MaxStack(1)
            .WithRarity(ItemRarity.Epic)
            .Glint()
            .Cooldown(40)
            .OnUse(Zap);
        Content.Items.Register(_wand);

        Commands.Register(Command.Literal("rubywand")
            .Permission("rubywand.give")
            .Then(Command.Literal("give").Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                player.Inventory.Give(_wand.CreateStack());
                ctx.Reply("Here is your ruby wand");
                return 1;
            }))
            .Then(Command.Literal("pack").Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                try
                {
                    BuildPack().SendTo(player, required: false, Component.Text("Ruby wand textures"));
                    ctx.Reply("Sent the resource pack");
                    return 1;
                }
                catch (InvalidOperationException e)
                {
                    throw new CommandFailure(e.Message);
                }
            })), description: "Ruby wand tools");
    }

    private void Zap(ItemUseContext ctx)
    {
        var player = ctx.Player;
        if (!player.HasPermission("rubywand.use"))
        {
            ctx.Cancelled = true;
            return;
        }
        player.Health = Math.Min(player.Health + 4, player.MaxHealth);
        var at = player.Position + new Vec3d(0, 1, 0);
        player.World.SpawnParticle("heart", at, count: 6, offset: new Vec3d(0.4, 0.4, 0.4), speed: 0.05f);
        player.PlaySound("minecraft:entity.player.levelup", SoundCategory.Players, 0.6f, 1.6f);
        player.SendActionBar(Component.Text("The wand hums").Color(TextColor.Red));
    }

    private ResourcePackFile BuildPack() => ResourcePackBuilder.Create("rubywand")
        .Description("Ruby wand")
        .AddItem(_wand, PackImage.Checker(0xFFE0115F, 0xFF9B0F44, size: 16, cell: 2))
        .Build();
}
