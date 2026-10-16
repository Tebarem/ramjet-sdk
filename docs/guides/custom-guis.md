# Custom GUIs (menus)

Chest-style menus with clickable icons: the model, the locking rules and a complete warp selector.

## The model

A menu is a server-authoritative container GUI owned by the plugin: a chest (1 to 6 rows of 9), a hopper (5 slots) or a dispenser (3 x 3). The client draws it like the vanilla container, but the contents are made of icons you choose, and **a click never moves an item unless you say so**.

```csharp
// plugin.Menus is the factory; a menu belongs to the plugin and is closed for everybody when the plugin unloads
Menu menu = Menus.Chest(3, Component.Text("Warps").Color(TextColor.DarkGreen));

menu.Slot(11, new ItemStack("minecraft:grass_block").WithName(Component.Legacy("&aSpawn")),
          click => click.Player.Teleport(Server.Overworld.SpawnLocation));
menu.FillEmpty(new ItemStack("minecraft:gray_stained_glass_pane").WithName(Component.Text(" ")));   // a background
menu.OnClose((player, reason) => Log.Debug($"{player.Name} closed the menu ({reason})"));
menu.Open(player);
```

The pieces:

- **`Menus`** (`Menus.Chest(rows, title)`, `Hopper(title)`, `Dispenser(title)`, `Create(type, title, rows)`) creates menus owned by your plugin.
- **`ItemStack`** is an immutable value: an item key, a count and data components. Every `With...` method returns a new stack: `new ItemStack("minecraft:diamond_sword")`, `WithName`, `WithLore`, `WithEnchantment`, `WithDamage`, `WithCount`, `WithCustomData`, `WithComponent(key, nbtTag)`, `WithItemModel`, `WithCustomModelData`, `WithUnbreakable`.
- **`menu.Slot(index, icon, onClick)`** sets the icon and the click handler of a slot. `menu.FillEmpty(icon)` fills every empty slot, `menu.OnClick(handler)` sees every click first, `menu.OnClose(handler)` runs when a viewer closes it (with the `MenuCloseReason` in the two-argument form).
- **`MenuClick`** (the handler's argument) has `Player`, `Slot`, `Type` (the kind of click), `Button`, `IsTopInventory`, `Clicked` and `Cursor` (the stacks before the click), and `Allow()`, `Cancel()`, `SetCursor(stack)`, `SetClicked(stack)` and `Close()`.
- **`menu.Open(player)`**, `menu.Close(player)`, `menu.Close()` (all viewers), `menu.Viewers`, `menu.Refresh()` and `menu.Dispose()`. `menu.Title` can be changed while it is open; the contents (`Get`, `Set`, `AddItem`, `Clear`) can be changed while players look at it.

Rules:

- Menu slots are server-authoritative. A click that could move items out of the menu or into it (a click on a menu slot, shift click, double click, drag, number key, off-hand key, drop key) is **denied by default**; `click.Allow()` lets one through, `click.Cancel()` denies one that would pass. A handler that throws denies the click.
- Clicks on the player's own inventory slots go through unless you set `menu.LockPlayerInventory = true`.
- A `Menu` is **closed for all viewers** when its plugin unloads (hot reload included), and its handlers are dropped.
- Items are immutable values: `WithName(...)` returns a new stack.
- Handlers run on the tick thread like all callbacks. Other plugins can observe menu clicks through `MenuClickEvent` and `MenuCloseEvent` (and the general `InventoryClickEvent`).

## A warp selector

```csharp
namespace Warps;

[PluginInfo("Warps", Version = "1.0.0")]
public sealed class PluginMain : Plugin
{
    private Menu _menu = null!;

    protected override void OnEnable()
    {
        _menu = Menus.Chest(3, Component.Text("Warps").Color(TextColor.DarkGreen));
        _menu.Slot(11, new ItemStack("minecraft:grass_block").WithName(Component.Legacy("&aSpawn")),
                   click => Go(click, "minecraft:overworld", 0, 80, 0));
        _menu.Slot(13, new ItemStack("minecraft:netherrack").WithName(Component.Legacy("&cNether hub")),
                   click => Go(click, "minecraft:the_nether", 0, 70, 0));
        _menu.Slot(15, new ItemStack("minecraft:end_stone").WithName(Component.Legacy("&5The End")),
                   click => Go(click, "minecraft:the_end", 100, 50, 0));
        _menu.OnClose((player, reason) => Log.Debug($"{player.Name} closed the warp menu ({reason})"));

        Commands.Register(Command.Literal("warps").Executes(ctx => { _menu.Open(ctx.RequirePlayer()); return 1; }),
                          description: "Open the warp menu");
    }

    private static void Go(MenuClick click, string worldKey, double x, double y, double z)
    {
        if (Server.GetWorld(worldKey) is not { } world) return;
        click.Close();                                           // close the menu, then move the player
        click.Player.Teleport(new Location(world, x, y, z, 0, 0));
    }
}
```

## Using the player's inventory

`player.Inventory` is a `PlayerInventory` (41 slots plus the cursor): `Give(stack)` hands over a stack and drops what does not fit at the player's feet, `AddItem`, `RemoveItem`, `Count(stack)`, `Contains(stack)`, `MainHand`, `OffHand`, `Helmet` ... A shop is a menu whose handlers charge with `RemoveItem` and pay out with `Give`; the [ShopGui sample](../../samples/ShopGui/) does exactly that, with its prices in a live-reloading config file.
