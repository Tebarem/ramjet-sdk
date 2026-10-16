namespace PermissionAdmin;

/// <summary>Sample: the <c>/perm</c> admin command for the built-in permission backend (groups, per-player entries, reload). It shows how a plugin declares permission nodes,
/// protects commands with them and talks to <see cref="FilePermissionProvider"/>; a plugin that replaces the backend can reuse the same command layout.
/// <code>
/// /perm user &lt;player&gt; set|unset|check &lt;node&gt; [true|false]
/// /perm user &lt;player&gt; group add|remove &lt;group&gt;
/// /perm group &lt;group&gt; create [weight] | delete | set &lt;node&gt; [true|false] | unset &lt;node&gt; | parent &lt;group&gt; | list
/// /perm list | /perm reload
/// </code></summary>
[PluginInfo("PermissionAdmin", Version = "1.0.0", Author = "Ramjet", Description = "/perm: manage groups and player permissions of the built-in backend")]
public sealed class PermissionAdminPlugin : Plugin
{
    private const string Admin = "permissions.admin";

    protected override void OnEnable()
    {
        Permissions.Register(Admin, PermissionDefault.OpLevel(3), "Use /perm to edit permissions");

        var node = Command.Literal("perm").Permission(Admin)
            .Then(Command.Literal("reload").Executes(ctx =>
            {
                PermissionManager.Reload();
                ctx.Reply("Permissions reloaded");
                return 1;
            }))
            .Then(Command.Literal("list").Executes(ctx =>
            {
                foreach (var p in PermissionManager.Registered) ctx.Reply($"{p.Node} (default {p.Default}): {p.Description}");
                return PermissionManager.Registered.Count;
            }))
            .Then(Command.Literal("user").Then(Command.Argument("player", Arg.Player())
                .Then(Command.Literal("set").Then(Command.Argument("node", Arg.GreedyString()).Executes(SetUser)))
                .Then(Command.Literal("unset").Then(Command.Argument("node", Arg.GreedyString()).Executes(ctx =>
                {
                    var player = ctx.GetPlayer("player");
                    Backend().SetPlayerPermission(player.Uuid, ctx.GetString("node"), null, player.Name);
                    ctx.Reply($"Removed {ctx.GetString("node")} from {player.Name}");
                    return 1;
                })))
                .Then(Command.Literal("check").Then(Command.Argument("node", Arg.GreedyString()).Executes(ctx =>
                {
                    var player = ctx.GetPlayer("player");
                    string node = ctx.GetString("node");
                    bool has = player.HasPermission(node);
                    var explicitValue = PermissionManager.GetExplicit(player.Uuid, node);
                    ctx.Reply($"{player.Name} {(has ? "has" : "does not have")} {node} (explicit: {explicitValue})");
                    return has ? 1 : 0;
                })))
                .Then(Command.Literal("group")
                    .Then(Command.Literal("add").Then(Command.Argument("group", Arg.Word()).Suggests(GroupNames).Executes(ctx =>
                    {
                        var player = ctx.GetPlayer("player");
                        Backend().AddToGroup(player.Uuid, Group(ctx), player.Name);
                        ctx.Reply($"{player.Name} is now in {Group(ctx)}");
                        return 1;
                    })))
                    .Then(Command.Literal("remove").Then(Command.Argument("group", Arg.Word()).Suggests(GroupNames).Executes(ctx =>
                    {
                        var player = ctx.GetPlayer("player");
                        bool removed = Backend().RemoveFromGroup(player.Uuid, Group(ctx));
                        ctx.Reply(removed ? $"{player.Name} left {Group(ctx)}" : $"{player.Name} was not in {Group(ctx)}");
                        return removed ? 1 : 0;
                    }))))
                .Then(Command.Literal("info").Executes(ctx =>
                {
                    var player = ctx.GetPlayer("player");
                    ctx.Reply($"{player.Name}: op level {player.PermissionLevel}, groups [{string.Join(", ", PermissionManager.Provider.Groups(player.Uuid))}]");
                    foreach (var (n, v) in Backend().PlayerEntries(player.Uuid)) ctx.Reply($"  {n} = {v}");
                    return 1;
                }))))
            .Then(Command.Literal("group").Then(Command.Argument("group", Arg.Word()).Suggests(GroupNames)
                .Then(Command.Literal("create").Executes(ctx => CreateGroup(ctx, 0))
                    .Then(Command.Argument("weight", Arg.Int(0, 10000)).Executes(ctx => CreateGroup(ctx, ctx.GetInt("weight")))))
                .Then(Command.Literal("delete").Executes(ctx =>
                {
                    bool ok = Backend().DeleteGroup(Group(ctx));
                    ctx.Reply(ok ? $"Deleted {Group(ctx)}" : $"No group {Group(ctx)} (the default group cannot be deleted)");
                    return ok ? 1 : 0;
                }))
                .Then(Command.Literal("set").Then(Command.Argument("node", Arg.GreedyString()).Executes(SetGroup)))
                .Then(Command.Literal("unset").Then(Command.Argument("node", Arg.GreedyString()).Executes(ctx =>
                {
                    Backend().SetGroupPermission(Group(ctx), ctx.GetString("node"), null);
                    ctx.Reply($"Removed {ctx.GetString("node")} from {Group(ctx)}");
                    return 1;
                })))
                .Then(Command.Literal("parent").Then(Command.Argument("parent", Arg.Word()).Suggests(GroupNames).Executes(ctx =>
                {
                    Backend().SetGroupParents(Group(ctx), [ctx.GetString("parent")]);
                    ctx.Reply($"{Group(ctx)} inherits from {ctx.GetString("parent")}");
                    return 1;
                })))
                .Then(Command.Literal("list").Executes(ctx =>
                {
                    var entries = Backend().GroupEntries(Group(ctx));
                    foreach (var (n, v) in entries) ctx.Reply($"  {n} = {v}");
                    return entries.Count;
                }))));

        Commands.Register(node, aliases: ["permission"], description: "Manage permissions");
    }

    private static FilePermissionProvider Backend()
        => PermissionManager.Provider as FilePermissionProvider
           ?? throw new CommandFailure($"The permission backend is {PermissionManager.Provider.Name}; manage permissions with its own commands");

    private static string Group(CommandContext ctx) => ctx.GetString("group");

    private static IEnumerable<string> GroupNames(CommandContext ctx)
        => PermissionManager.Provider is FilePermissionProvider file ? file.GroupNames : [];

    // "node [true|false]": the node comes from a greedy string because a wildcard ('homes.*', '*') is not valid in an unquoted word
    private static (string Node, bool Value) NodeAndValue(CommandContext ctx)
    {
        string text = ctx.GetString("node").Trim();
        int space = text.LastIndexOf(' ');
        if (space > 0 && bool.TryParse(text[(space + 1)..], out bool value)) return (text[..space].Trim(), value);
        return (text, true);
    }

    private static int SetUser(CommandContext ctx)
    {
        var player = ctx.GetPlayer("player");
        var (node, value) = NodeAndValue(ctx);
        Backend().SetPlayerPermission(player.Uuid, node, value, player.Name);
        ctx.Reply($"{player.Name}: {node} = {value}");
        return 1;
    }

    private static int SetGroup(CommandContext ctx)
    {
        var (node, value) = NodeAndValue(ctx);
        Backend().SetGroupPermission(Group(ctx), node, value);
        ctx.Reply($"{Group(ctx)}: {node} = {value}");
        return 1;
    }

    private static int CreateGroup(CommandContext ctx, int weight)
    {
        Backend().CreateGroup(Group(ctx), weight);
        ctx.Reply($"Group {Group(ctx)} (weight {weight}) ready");
        return 1;
    }
}
