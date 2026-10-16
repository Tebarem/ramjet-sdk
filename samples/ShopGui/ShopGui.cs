using System.Text.Json.Serialization;

namespace ShopGui;

/// <summary>One thing the shop sells.</summary>
public sealed class ShopItem
{
    /// <summary>The item key, e.g. <c>minecraft:bread</c>.</summary>
    public string Item { get; set; } = "minecraft:bread";
    /// <summary>How many the player gets for one purchase.</summary>
    public int Count { get; set; } = 1;
    /// <summary>The price in units of the currency item.</summary>
    public int Price { get; set; } = 1;
}

/// <summary>The settings of the ShopGui plugin.</summary>
public sealed class ShopConfig
{
    /// <summary>The item players pay with.</summary>
    public string Currency { get; set; } = "minecraft:gold_ingot";
    /// <summary>The title of the shop window.</summary>
    public string Title { get; set; } = "Shop";
    /// <summary>What the shop sells, in slot order (at most 21 entries: three rows of seven).</summary>
    public List<ShopItem> Items { get; set; } =
    [
        new() { Item = "minecraft:bread", Count = 8, Price = 1 },
        new() { Item = "minecraft:arrow", Count = 16, Price = 2 },
        new() { Item = "minecraft:iron_pickaxe", Count = 1, Price = 6 },
        new() { Item = "minecraft:golden_apple", Count = 1, Price = 12 },
        new() { Item = "minecraft:diamond", Count = 1, Price = 20 },
    ];
}

[JsonSerializable(typeof(ShopConfig))]
internal sealed partial class ShopJson : JsonSerializerContext;

/// <summary>Sample: <c>/shop</c> opens a custom chest GUI. Every product is an icon with a price in its lore; clicking it pays with a currency item from the player's inventory
/// (<see cref="EngineInventory.RemoveItem"/>) and hands over the product (<see cref="PlayerInventory.Give"/>). The menu is locked by the engine: nothing in it can be taken out, shift clicked,
/// dragged or swapped away, whatever a handler does or forgets to do, so a shop has no duplication holes. It also shows <c>OnClose</c> and the config file with its live reload.</summary>
[PluginInfo("ShopGui", Version = "1.0.0", Author = "Ramjet", Description = "A shop in a custom GUI")]
public sealed class ShopGuiPlugin : Plugin
{
    private Config<ShopConfig> _config = null!;
    private Menu? _menu;

    protected override void OnEnable()
    {
        _config = Config.Load(ShopJson.Default.ShopConfig);
        _config.Watch = true;
        _config.Changed += _ => Rebuild();
        Permissions.Register("shop.use", PermissionDefault.True, "Open the shop");
        Rebuild();

        Commands.Register(Command.Literal("shop").Permission("shop.use").Executes(ctx =>
        {
            var player = ctx.RequirePlayer();
            _menu!.Open(player);
            return 1;
        }), description: "Open the shop");
    }

    // The menu is rebuilt when the config changes: the old one is closed for its viewers and the new one is opened for them again.
    private void Rebuild()
    {
        var viewers = _menu?.Viewers.ToList() ?? [];
        _menu?.Dispose();
        var config = _config.Value;
        var menu = Menus.Chest(3, Component.Text(config.Title).Color(TextColor.DarkGreen));
        menu.FillEmpty(new ItemStack("minecraft:gray_stained_glass_pane").WithName(Component.Text(" ")));
        for (int i = 0; i < Math.Min(config.Items.Count, 21); i++)
        {
            var product = config.Items[i];
            int slot = (i / 7) * 9 + 1 + (i % 7);  // three rows, columns 1-7
            var stack = new ItemStack(product.Item, product.Count);
            var icon = stack.WithLore(
                Component.Text($"Price: {product.Price} x {Pretty(config.Currency)}").Color(TextColor.Gold).Italic(false),
                Component.Text("Click to buy").Color(TextColor.Gray).Italic(false));
            menu.Slot(slot, icon, click => Buy(click, product, stack));
        }
        menu.OnClose((player, reason) => Log.Info($"{player.Name} left the shop ({reason})"));
        _menu = menu;
        foreach (var viewer in viewers)
            if (viewer.IsOnline) menu.Open(viewer);
    }

    private void Buy(MenuClick click, ShopItem product, ItemStack stack)
    {
        var player = click.Player;
        var inventory = player.Inventory;
        var price = new ItemStack(_config.Value.Currency, Math.Max(product.Price, 1));
        if (inventory.Count(price) < price.Count)
        {
            player.SendActionBar(Component.Text($"You need {price.Count} x {Pretty(price.Type)}").Color(TextColor.Red));
            player.PlaySound("minecraft:entity.villager.no", SoundCategory.Players);
            return;
        }
        inventory.RemoveItem(price);
        int dropped = inventory.Give(stack);
        player.SendActionBar(Component.Text($"Bought {stack.Count} x {Pretty(stack.Type)}" + (dropped > 0 ? " (your inventory was full: it is at your feet)" : "")).Color(TextColor.Green));
        player.PlaySound("minecraft:entity.villager.yes", SoundCategory.Players);
    }

    private static string Pretty(string key) => key[(key.IndexOf(':') + 1)..].Replace('_', ' ');
}
