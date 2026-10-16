# Commands with arguments and suggestions

A /calc command written as a typed Brigadier tree with suggestions and a permission, then subcommands, optional arguments and aliases.

We build `/calc <a> <add|sub|mul|div> <b>`. It is a real node of the server's Brigadier command tree: the client knows its shape, validates the numbers, completes the operator, and hides the command from players who lack the permission.

## The command

```csharp
namespace Calc;

[PluginInfo("Calc", Version = "1.0.0")]
public sealed class PluginMain : Plugin
{
    private static readonly string[] Operators = ["add", "sub", "mul", "div"];

    protected override void OnEnable()
    {
        Permissions.Register("calc.use", PermissionDefault.True, "Use /calc");

        Commands.Register(
            Command.Literal("calc")
                .Permission("calc.use")                                       // hides the node and blocks it
                .Then(Command.Argument("a", Arg.Double())
                    .Then(Command.Argument("op", Arg.Word())
                        .Suggests(ctx => Operators)                           // tab completion
                        .Then(Command.Argument("b", Arg.Double())
                            .Executes(Calculate)))),
            description: "Calculate with two numbers");
    }

    private static int Calculate(CommandContext ctx)
    {
        double a = ctx.GetDouble("a"), b = ctx.GetDouble("b");
        string op = ctx.GetString("op");
        double result = op switch
        {
            "add" => a + b,
            "sub" => a - b,
            "mul" => a * b,
            "div" when b != 0 => a / b,
            "div" => throw new CommandFailure("Division by zero"),           // red message, result 0, no log
            _ => throw new CommandFailure($"Unknown operator '{op}' (use add, sub, mul or div)"),
        };
        ctx.Reply($"{a} {op} {b} = {result}");
        return (int)result;                                                   // the command result (usable by /execute store)
    }
}
```

What you get without writing any parsing code:

- The tree is sent to the client: `/calc ` shows `<a>` in the usage hint, number arguments are validated by the client and server (`Arg.Double()` rejects `abc` before your code runs), `op` completes to `add sub mul div`.
- `.Permission("calc.use")` makes the command invisible and non-executable for players without the node ([permissions](../concepts/permissions.md)). The console always passes.
- Errors are `CommandFailure`s: the player sees red text, the result is 0 and nothing is logged. Any other exception is "An unexpected error occurred" plus a logged stack trace.
- The return value is the command's result, exactly like vanilla's (the number `/execute store result` captures).
- Unloading the plugin removes the nodes and re-sends every player's command tree once at the end of the tick.

`Arg.Word()` accepts letters, digits and `_ - . +`. If you want the operator to be `+ - * /`, make each one a literal child (`Command.Literal("+")`, ...), or take a quoted `Arg.String()`.

## Subcommands, optional arguments and aliases

```csharp
Commands.Register(
    Command.Literal("home")
        .Executes(ctx => Teleport(ctx.RequirePlayer(), "home"))                // /home
        .Then(Command.Argument("name", Arg.Word())                              // /home <name>
            .Suggests(OwnHomeNames)
            .Executes(ctx => Teleport(ctx.RequirePlayer(), ctx.GetString("name"))))
        .Then(Command.Literal("set")                                            // /home set <name>
            .Then(Command.Argument("name", Arg.Word()).Executes(SetHome))),
    aliases: ["h"], description: "Teleport to a home");

// suggestions come from the tick thread; ctx.Player is null for the console
private IEnumerable<string> OwnHomeNames(CommandContext ctx)
    => ctx.Player is { } p ? HomesOf(p).Keys : [];
```

An argument node with an executor is an optional last argument; a literal child is a subcommand. See the [homes guide](homes-plugin.md) for the complete plugin and the [command argument types](../reference/command-arguments.md) for everything `Arg` can produce.

## Suggestions with tooltips

When a plain list is not enough, use the `SuggestionProvider` form, which gets a `SuggestionsBuilder`:

```csharp
Command.Argument("kit", Arg.Word()).Suggests((ctx, builder) =>
{
    foreach (var kit in _kits.Keys)
        builder.Suggest(kit, "costs " + _kits[kit].Price);   // the second argument is the tooltip
})
```

`builder.Remaining` is the text typed so far; completions that do not match it are dropped, like in vanilla. For suggestions that need a slow lookup use `SuggestsAsync`, which returns a `Task` the client waits for.

## When the flat form is enough

For a quick command with raw arguments there is `Commands.Register("name", ctx => ...)`, with `ctx.Args` split on spaces (see [commands](../concepts/commands.md#the-flat-form)). It shows no typed arguments and no completion, so prefer the tree for anything players will type often.
