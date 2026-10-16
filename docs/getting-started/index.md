# Write a plugin in an hour

From zero to a hot-reloading C# plugin on a Ramjet server, and what to read next. Written for people who know Paper/Bukkit.

Ramjet plugins are plain C# class libraries (.NET 10) loaded into the running server. They subscribe to events, register commands, components and per-tick systems, and are **hot reloaded every time you rebuild**: no server restart, no lost state in the ECS.

If you have written Paper plugins you already know the shapes: a plugin class with enable/disable hooks, event listeners with priorities and cancellation, commands, permissions, a scheduler. The differences that matter are in how you write them, and they are explained in [Concepts](../concepts/lifecycle.md) and in the [migration table](../guides/migrating-from-paper.md).

## What you need

- The **.NET 10 SDK**.
- The **`Ramjet.Api`** NuGet package: the plugin API as a reference assembly. It is what your plugin compiles against; the server supplies the real API at run time. Until the package is on nuget.org you add the folder that holds `Ramjet.Api.<version>.nupkg` as a local NuGet source (see [create a plugin](create-a-plugin.md)).
- The **`ramjet-plugin` template** (`dotnet new install Ramjet.Templates`).
- A **Ramjet server to test on**: a hosted server (you upload your plugin into its `plugins/` folder through the panel or SFTP) or one that somebody gives you access to. You do not need the server's source code and you cannot build the server yourself; you only need to be able to put a file into its `plugins/` folder and read its log.

Use a test server, not one people play on. A plugin that throws is isolated, but a plugin that blocks the tick thread slows every player down.

## The one-hour path

| Step | Time | Page |
|---|---|---|
| 1. Install the .NET 10 SDK, the template and the API package | 10 min | [Create a plugin](create-a-plugin.md) |
| 2. Create a project from the template | 5 min | [Create a plugin](create-a-plugin.md) |
| 3. Understand the layout and `plugin.json` | 5 min | [Project layout](project-layout.md) |
| 4. Build, put the plugin on your test server, edit, watch it reload | 15 min | [Build and hot reload](build-and-hot-reload.md) |
| 5. Write something real | 25 min | [Your first plugin](../guides/first-plugin.md) |

Then read the five concept pages that carry the most weight: [threading](../concepts/threading.md), [entities and the ECS](../concepts/entities-and-ecs.md), [events](../concepts/events.md), [hot reload](../concepts/hot-reload.md) and [performance](../concepts/performance.md).

## Where things are

- [Concepts](../concepts/lifecycle.md): how plugins work.
- [Guides](../guides/first-plugin.md): full, working code.
- [Samples](../../samples/): complete plugins (BossBarTimer, DiscordWebhook, Greeter, HelloPlugin, Homes, Kits, PermissionAdmin, RubyOre, RubyWand, ShopGui, SidebarStats, SpawnProtect, WorldTools) that build against `Ramjet.Api`.
- Reference: the [event catalogue](../reference/events.md), the [`plugin.json` reference](../reference/plugin-json.md) and the [command argument types](../reference/command-arguments.md). Every public type and member of the API is documented in the `Ramjet.Api` package, so you get the documentation as IntelliSense in your editor.
- The [documentation index](../README.md) lists every page.
