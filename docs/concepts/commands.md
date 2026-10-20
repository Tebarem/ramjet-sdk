# Commands

How plugin commands become real nodes of the server's Brigadier command tree, with typed arguments, suggestions and permissions, plus the simple flat form.

## How the server handles commands

The server has a **Brigadier-compatible command engine** (the same model as vanilla's): the full vanilla command tree, all of vanilla's argument types, suggestions, and the command tree the client uses for tab-completion and syntax colouring.

Plugin commands are **nodes of that same tree**. They appear in the client's command tree with typed arguments, suggestions and permission-based visibility, tab-complete, and run through the same execution path as vanilla commands.

```csharp
Commands.Register(Command.Literal("home")
    .Permission("homes.use")                                  // hides and blocks the node for players without it
    .Executes(ctx => Teleport(ctx.RequirePlayer(), "home"))
    .Then(Command.Argument("name", Arg.Word())
        .Suggests(ctx => Homes.Names(ctx.Player))             // custom suggestion provider
        .Executes(ctx => Teleport(ctx.RequirePlayer(), ctx.GetString("name"))))
    .Then(Command.Literal("set")
        .Then(Command.Argument("name", Arg.Word()).Executes(ctx => SetHome(ctx)))),
    aliases: ["h"], description: "Teleport to a home");
```

`Commands.Register(CommandNode root, string[]? aliases = null, string description = "")` returns a `RegisteredCommand`; dispose it to unregister, or let the plugin unload do it. The root must be a literal. Registering a name that a vanilla command or another plugin already holds throws `InvalidOperationException`.

Give names without the leading slash: the client adds it. `Command.Literal("set")` is typed `/set`; a literal `"/set"` is a root of its own that players type (and see) as `//set`, the WorldEdit style.

- **`CommandContext`**: `Source` (`CommandSource`: `Player`, `Entity`, `Position`, `World`, `PermissionLevel`, `IsConsole`), `Sender`, `Player` (null for the console), `RequirePlayer()` (throws a friendly "Only players can use this command" for the console), `Input` (the whole line without the slash), typed getters per argument (`GetInt`, `GetLong`, `GetFloat`, `GetDouble`, `GetBool`, `GetString`, `GetPlayer`, `GetPlayers`, `GetEntities`, `GetBlockPos`, `GetVec3`, `GetBlockState`, `GetText` for the textual form of items, components, NBT, uuids and resource ids), `Has("name")` for optional arguments, and feedback with `Reply(string)` or `Reply(Component)`.
- **The executor's return value** is the command result, like vanilla's: the number `/execute store result` captures, 1 for success. `Executes(Action<CommandContext>)` is the shorthand that returns 1.
- **Argument types** mirror vanilla's: `Arg.Int(min, max)`, `Long`, `Float`, `Double`, `Bool`, `Word`, `String`, `GreedyString`, `Player`, `Players`, `Entity`, `Entities`, `GameProfile`, `BlockPos`, `ColumnPos`, `Vec3`, `Vec2`, `Rotation`, `BlockState`, `BlockPredicate`, `Item`, `ItemPredicate`, `Component`, `NbtCompound`, `NbtPath`, `Uuid`, `Time`, `Dimension`, `GameMode`, `Resource(registry)`, `ScoreHolder`, `Objective`, `Team`, `Message` and more. They are listed on [command argument types](../reference/command-arguments.md).
- **Suggestions**: `Suggests(ctx => IEnumerable<string>)`, or the `SuggestionProvider` form with a `SuggestionsBuilder` and tooltips; `SuggestsAsync` lets a provider complete asynchronously (the client waits for it). Synchronous providers run on the tick thread.
- **Requirements**: `.Permission("node")` (checked per player; node visibility follows it, the console always passes), `.Requires(src => bool)`, `.OpLevel(n)`. A node with a dynamic requirement is encoded per player rather than shared.
- **Failure**: throw `new CommandFailure("message")` for the idiomatic "the command failed" (red message, result 0, no log). Any other exception becomes "An unexpected error occurred" for the sender plus a logged stack trace. `ctx.Source.Success(message, broadcastToOps)` and `ctx.Source.Failure(message)` send vanilla-style feedback without ending the command.
- **Aliases** register redirecting literals, so the client tree shows both.
- **Ownership and reload**: nodes belong to the plugin; unloading removes them and every player's command tree is re-sent once at the end of the tick (N changes in one tick cost one packet per player).
- **Work after the executor returned**: a `CommandContext` is only valid while the executor or provider runs. For deferred work call `ctx.Detach()`, which copies who ran the command, the input, the permission level and the position into a `DetachedCommand` you can use later on the tick thread (`Reply` still works).

The [commands guide](../guides/commands-with-arguments.md) builds a complete example, and the [homes guide](../guides/homes-plugin.md) a larger one.

## The flat form

The simplest form is a command word followed by anything, with the raw arguments split on spaces:

```csharp
Commands.Register("hello", ctx =>
{
    string who = ctx.Args.Length > 0 ? ctx.Args[0] : "world";
    ctx.Reply($"Hello, {who}!");
    ctx.Result = ctx.Args.Length;          // the command's numeric result (vanilla semantics)
}, "Says hello");
```

This registers a `hello` literal plus one greedy `args` argument, so the client shows no typed arguments and offers no completion. `CommandContext.Name`, `RawArgs` and `Args` give you the word and what followed. Prefer the Brigadier form for anything with arguments, suggestions or permissions.

## Intercepting commands: `PlayerCommandEvent`

The server fires `PlayerCommandEvent` for every command a player types (the line without the leading slash). Use it to watch, rewrite or veto commands, including vanilla ones:

```csharp
Events.On((ref PlayerCommandEvent e) =>
{
    // e.Command is the line without the leading slash, e.g. "op Steve"
    if (!e.Command.Bytes.StartsWith("op "u8)) return; // cheap prefix test: no allocation for other commands
    e.Cancelled = true;                               // the command does not run
    e.Player.SendMessage("That command is disabled here.");
});
```

Notes:

- Cancelling suppresses the command entirely. Do not cancel commands you do not handle.
- The handler runs before the command executes, so it is the place for logging, rate limits and aliases; for a command of your own, register a node instead.
