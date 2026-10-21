# Ramjet SDK

Write C# plugins for **Ramjet**, a high-performance, vanilla-compatible Minecraft Java Edition server. Plugins are .NET 10 class
libraries that the server loads, isolates and **hot-reloads** while it runs.

This repository holds everything plugin developers need:

| | |
|---|---|
| [`docs/`](docs/README.md) | getting started, concepts (events, commands, permissions, config, scheduler, ECS), guides and reference |
| [`templates/`](templates/) | the `ramjet-plugin` project template (`dotnet new ramjet-plugin`), with an `AGENTS.md` and a Claude Code skill for AI coding agents |
| [`samples/`](samples/) | complete example plugins: commands, GUIs, boss bars, sidebars, homes, kits, custom items and blocks, webhooks |
| [`src/Ramjet.Api/`](src/Ramjet.Api/) | the `Ramjet.Api` package: the API's reference assembly and docs, and a text listing of its public surface (API changes show up as diffs of [`Mc.Scripting.Api.surface.txt`](src/Ramjet.Api/Mc.Scripting.Api.surface.txt)) |
| [`tools/ApiCheck/`](tools/ApiCheck/) | the check every pack runs: the reference assembly may only hold public API, with no code |

The plugin API itself ships as the **[`Ramjet.Api`](https://www.nuget.org/packages/Ramjet.Api)** NuGet package: a reference assembly
with every public type and its documentation (IntelliSense), and no server code. Your plugin compiles against it, and the server supplies
the real API when it loads the plugin. The project template is [`Ramjet.Templates`](https://www.nuget.org/packages/Ramjet.Templates).
Both come from nuget.org: no extra package source is needed.

## Quick start

```bash
# 1. the project template
dotnet new install Ramjet.Templates

# 2. a plugin
dotnet new ramjet-plugin -n MyPlugin --Author "you" --PluginDescription "what it does"
cd MyPlugin
dotnet build -c Release
```

Put `bin/Release/net10.0/MyPlugin.dll` (with `plugin.json`) into your server's `plugins/MyPlugin/` folder. The server loads it within
seconds and logs `[MyPlugin] enabled v1.0.0`. Every rebuild that replaces the dll is hot-reloaded. Set `RAMJET_PLUGINS` to the server's
plugins folder and each build copies the plugin there for you:

```bash
export RAMJET_PLUGINS=/path/to/server/plugins
dotnet build -c Release
```

Server operators can list and restart plugins with `/plugins` and `/plugins reload`.

> **Preview:** both packages are `1.0.0-preview.4`. The API can still change before 1.0; breaking changes are listed in the
> [changelog](CHANGELOG.md).

## Samples

Each folder under [`samples/`](samples/) is a complete plugin that builds with `dotnet build`. Start with
[`HelloPlugin`](samples/HelloPlugin/) and [`Greeter`](samples/Greeter/).

## Releases

Pushing a version tag (`v1.0.0-preview.4`) runs [the publish workflow](.github/workflows/publish-nuget.yml): CI packs `Ramjet.Api` (after
the API check) and `Ramjet.Templates`, builds every sample and a fresh template project against them, and publishes both to nuget.org
with a GitHub release. The reference assembly is updated together with the server, so changes to `src/Ramjet.Api/ref/` come from
there; pull requests for docs, samples and the template are welcome.

## Questions, bugs, ideas

Open an [issue](../../issues/new/choose): bugs in the API or its documentation, missing events or API, and feature requests are all
welcome. The server engine itself is not open source; this repository covers the plugin side.

## License

The SDK (documentation, template, samples and the `Ramjet.Api` reference package) is [MIT](LICENSE) licensed.
