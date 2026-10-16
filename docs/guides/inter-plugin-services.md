# Inter-plugin services

Let one plugin offer an API to another with the service manager: a Bank provider and a Shop consumer, with dependencies and hot reload.

Paper's equivalent is the `ServicesManager` (Vault-style). The idea: a plugin **registers an interface implementation** as a service; other plugins **look it up by interface** without knowing which plugin provides it.

The API ships a few service contracts of its own, so independent plugins can cooperate without agreeing on a private interface: `IEconomy` (balances, deposits, transfers, a `Currency`), `IBanList`, `IPlaceholders` (text placeholders such as `%server_online%`) and `IPlayerMeta` (prefixes, suffixes and typed values for players). Use them when they fit; the example below defines its own interface to show the mechanics, and calls it `IBank` so it does not clash with `IEconomy`.

## 1. The interface

Put the contract in a type both plugins can see. The simplest layout is that the provider plugin's assembly contains it and the consumer references that assembly. The consumer declares the provider in `depends` (or `softDepends`) so the server resolves the provider's assembly into the consumer's load context, and the interface type is **the same type** on both sides.

```csharp
// Bank/IBank.cs
namespace Bank;

public interface IBank
{
    double GetBalance(Uuid player);
    bool Withdraw(Uuid player, double amount);      // false when the balance is too low
    void Deposit(Uuid player, double amount);
}
```

For a larger ecosystem, publish the interface in its own small assembly (`Bank.Api.dll`) that provider and consumers both reference with `<Private>false</Private>` and that the server can resolve, so consumers do not depend on the provider's internals.

## 2. The provider

```csharp
// Bank/PluginMain.cs
namespace Bank;

public sealed class PluginMain : Plugin
{
    protected override void OnEnable()
    {
        Services.Register<IBank>(new SimpleBank(), ServicePriority.Normal);
    }
}

internal sealed class SimpleBank : IBank
{
    private readonly Dictionary<Uuid, double> _balances = new();

    public double GetBalance(Uuid player) => _balances.GetValueOrDefault(player);
    public void Deposit(Uuid player, double amount) => _balances[player] = GetBalance(player) + amount;
    public bool Withdraw(Uuid player, double amount)
    {
        if (GetBalance(player) < amount) return false;
        _balances[player] -= amount;
        return true;
    }
}
```

Registrations are **owned by the registering plugin** and removed when it unloads. If several providers register the same interface, `ServicePriority` (`Lowest`, `Low`, `Normal`, `High`, `Highest`) decides which one `Get` returns (highest wins), so an admin can swap implementations.

## 3. The consumer

```json
{
  "id": "shop",
  "name": "Shop",
  "version": "1.0.0",
  "api": "1.0",
  "main": "Shop.PluginMain",
  "depends": ["bank >=1.0"]
}
```

`depends` makes the server load Bank first (a topological sort of the manifests), skip Shop with a precise message when Bank is missing or too old, and let Shop's load context resolve Bank's assembly.

```csharp
// Shop/PluginMain.cs
using Bank;

namespace Shop;

public sealed class PluginMain : Plugin
{
    private IBank? _bank;

    protected override void OnEnable()
    {
        _bank = Services.TryGet<IBank>();                     // highest priority provider, null if none

        // re-resolve whenever the set of providers changes (also fires when a provider unloads)
        Services.Changed<IBank>(() => _bank = Services.TryGet<IBank>());

        Commands.Register(Command.Literal("buy")
            .Then(Command.Argument("item", Arg.Word())
                .Executes(ctx => Buy(ctx.RequirePlayer(), ctx.GetString("item")))));
    }

    private int Buy(Player player, string item)
    {
        if (_bank is null) throw new CommandFailure("The shop is closed (no bank plugin).");
        if (!_bank.Withdraw(player.Uuid, 10)) throw new CommandFailure("Not enough money (10 needed).");
        player.SendMessage($"Bought {item} for 10.");
        return 1;
    }
}
```

`Services.Get<T>()` is the same lookup but throws `ServiceUnavailableException` when nothing is registered; `Services.GetAll<T>()` lists every registration with its provider and priority; `Services.Unregister<T>()` removes your own registration early.

## Hot reload and unloading

- A **service object that outlives its provider is dangerous**: after the provider unloaded, the cached object still belongs to the unloaded assembly. The `Changed` notification is your signal to drop the reference and fetch a new one, as above. Never cache the service in a static field.
- **Reloading the provider reloads its hard dependents** (`depends`), in reverse order and back, because their types come from it. A `softDepends` consumer is not reloaded; it only receives the `Changed` notification.
- Interfaces are best kept **small and stable**: adding a member to `IBank` breaks every consumer compiled against the old shape. Add a new interface (`IBankV2`) instead.

## When not to use services

For fire-and-forget notifications between plugins ("a player bought something"), define a [custom event](../concepts/events.md#custom-events) and let others subscribe. Services are for request/response APIs where the caller needs an answer.
