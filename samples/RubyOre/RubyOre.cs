namespace RubyOre;

/// <summary>Sample: a custom block. <c>rubyore:ruby_ore</c> lives in a free state of the note block (the world stores plain vanilla block states, a server without the plugin shows a note
/// block), mines in a second, drops one to three <c>rubyore:ruby</c> items and answers a right click. <c>/rubyore give</c> hands out the ore as an item that places the block,
/// <c>/rubyore pack</c> sends the resource pack that gives the block its texture (see the RubyWand sample for the pack host).</summary>
[PluginInfo("RubyOre", Version = "1.0.0", Author = "Ramjet", Description = "A custom block and the item it drops")]
public sealed class RubyOrePlugin : Plugin
{
    private CustomItem _ruby = null!;
    private CustomItem _oreItem = null!;
    private CustomBlock _ore = null!;

    protected override void OnEnable()
    {
        Permissions.Register("rubyore.give", PermissionDefault.OpLevel(2), "Give out ruby ore and send the pack");

        _ruby = new CustomItem("rubyore:ruby", "minecraft:paper")
            .Model("rubyore:ruby")
            .Name(Component.Text("Ruby").Color(TextColor.Red))
            .WithRarity(ItemRarity.Rare);
        Content.Items.Register(_ruby);

        _ore = new CustomBlock("rubyore:ruby_ore")
            .Hardness(0.8f)
            .Model("rubyore:block/ruby_ore")
            .Drops(_ => [_ruby.CreateStack(Random.Shared.Next(1, 4))])
            .OnInteract(ctx =>
            {
                ctx.Player.SendActionBar(Component.Text("A vein of rubies").Color(TextColor.Red));
                ctx.Cancelled = false;  // let the held block be placed against it like against any block
            });
        Content.Blocks.Register(_ore);

        _oreItem = new CustomItem("rubyore:ruby_ore_item", "minecraft:amethyst_shard")
            .Model("rubyore:ruby_ore")
            .Name(Component.Text("Ruby Ore").Color(TextColor.Red))
            .Places(_ore);
        Content.Items.Register(_oreItem);

        Commands.Register(Command.Literal("rubyore")
            .Permission("rubyore.give")
            .Then(Command.Literal("give").Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                player.Inventory.Give(_oreItem.CreateStack(16));
                ctx.Reply("Here is your ruby ore");
                return 1;
            }))
            .Then(Command.Literal("pack").Executes(ctx =>
            {
                var player = ctx.RequirePlayer();
                try
                {
                    ResourcePackBuilder.Create("rubyore")
                        .Description("Ruby ore")
                        .AddItem(_ruby, PackImage.Solid(0xFFE0115F))
                        .AddItem(_oreItem, PackImage.Checker(0xFF5A5A5A, 0xFFE0115F, cell: 4))
                        .AddCustomBlock(_ore, PackImage.Checker(0xFF5A5A5A, 0xFFE0115F, cell: 4))
                        .Build()
                        .SendTo(player, required: false, Component.Text("Ruby ore textures"));
                    ctx.Reply("Sent the resource pack");
                    return 1;
                }
                catch (InvalidOperationException e)
                {
                    throw new CommandFailure(e.Message);
                }
            })), description: "Ruby ore tools");
    }
}
