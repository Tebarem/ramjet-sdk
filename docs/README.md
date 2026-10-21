# Ramjet plugin documentation

Guides and references for writing C# plugins for a Ramjet Minecraft server. Plugins are .NET 10 class libraries that compile against the `Ramjet.Api` NuGet package and are hot reloaded by the server on every rebuild. Complete example plugins are in [`samples/`](../samples/).

## Getting started

- [Write a plugin in an hour](getting-started/index.md): the overview, what you need and the one-hour path.
- [Create a plugin from the template](getting-started/create-a-plugin.md): install the `ramjet-plugin` template and the `Ramjet.Api` package source, create a project, build it.
- [Project layout and plugin.json](getting-started/project-layout.md): what the template creates, how the server finds plugins, source plugins and the manifest.
- [Build, run and hot reload](getting-started/build-and-hot-reload.md): the edit-build-reload loop on your test server.

## Concepts

- [Plugin lifecycle](concepts/lifecycle.md): how a plugin is loaded, enabled, disabled and unloaded, and what the plugin context owns.
- [Events](concepts/events.md): subscribing, priorities, cancellation, mutable fields, borrowed strings and custom events.
- [Commands](concepts/commands.md): Brigadier command trees, typed arguments, suggestions, and the flat form.
- [Permissions](concepts/permissions.md): nodes, defaults, wildcards, groups, replacing the backend, vanilla ops.
- [Administration](concepts/administration.md): the operator list, the whitelist and the ban lists, the login order, and what a change does to online players.
- [Config and data](concepts/config-and-data.md): typed JSON config with defaults, the data folder, atomic writes.
- [Entities, handles and the ECS](concepts/entities-and-ecs.md): why an `Entity` is a handle, components, queries and the reference rule.
- [Systems and tick phases](concepts/systems.md): per-tick systems, ordering against the server's own systems.
- [The scheduler](concepts/scheduler.md): tick-based delayed and repeating tasks, async helpers.
- [The threading model](concepts/threading.md): the tick thread owns the world; what is thread-safe; async work.
- [Hot reload](concepts/hot-reload.md): what survives a reload and the rules that keep it leak-free.
- [Logs and debugging](concepts/logging.md): the per-plugin log folder, exceptions, the compiler output of source plugins, and reading them with an agent.
- [Performance rules](concepts/performance.md): allocation-free hot paths and what each call costs.
- [Worlds, blocks and chunks](concepts/worlds.md): the three dimensions, reading and changing blocks, chunk tickets, entities and effects, time and weather, teleporting between worlds, what is saved.
- [Loot tables and tags](concepts/loot-and-tags.md): rolling the datapack's loot tables with a context, seeded rolls, setting a container's loot, and the block, item, entity type and biome tags.
- [Living entities](concepts/living-entities.md): health and damage, potion effects, attributes and modifiers, equipment, the mob flags and attack targets, through `LivingEntity`.

## Guides

- [Your first plugin](guides/first-plugin.md): a component, events, a system, a scheduled task and a command, hot reloaded.
- [Commands with arguments and suggestions](guides/commands-with-arguments.md): a typed `/calc` command, subcommands and aliases.
- [A homes plugin with config and permissions](guides/homes-plugin.md): `/home`, `/sethome`, a config file, a data file and cooldowns.
- [Custom GUIs (menus)](guides/custom-guis.md): chest menus with clickable icons, and a warp selector.
- [Boss bars, titles and action bars](guides/boss-bars-and-titles.md): a countdown boss bar and a welcome title.
- [A scoreboard sidebar](guides/scoreboard-sidebar.md): shared and per-player sidebars with live lines.
- [Inter-plugin services](guides/inter-plugin-services.md): one plugin offering an API to another, with dependencies and hot reload.
- [Travelling between worlds](guides/worlds-and-teleporting.md): a `/hop` command to another dimension on safe ground: chunk tickets, terrain checks, a platform, portal events.
- [Migrating from Paper / Bukkit](guides/migrating-from-paper.md): a concept-by-concept mapping with side-by-side code.

## Reference

- [Event catalogue](reference/events.md): every event with its fields, mutability and cancellability.
- [Events compared with Paper](reference/events-compared-with-paper.md): which Paper events Ramjet covers, what is missing and what is planned.
- [plugin.json reference](reference/plugin-json.md): every manifest field, dependencies, versions and load order.
- [Command argument types](reference/command-arguments.md): the `Arg` factories and the vanilla argument types behind them.

## Samples

Ready-to-build plugins: [BossBarTimer](../samples/BossBarTimer/), [DiscordWebhook](../samples/DiscordWebhook/), [Greeter](../samples/Greeter/), [HelloPlugin](../samples/HelloPlugin/), [Homes](../samples/Homes/), [Kits](../samples/Kits/), [PermissionAdmin](../samples/PermissionAdmin/), [RubyOre](../samples/RubyOre/), [RubyWand](../samples/RubyWand/), [ShopGui](../samples/ShopGui/), [SidebarStats](../samples/SidebarStats/), [SpawnProtect](../samples/SpawnProtect/) and [WorldTools](../samples/WorldTools/).
