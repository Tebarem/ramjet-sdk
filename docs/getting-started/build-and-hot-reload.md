# Build, run and hot reload

The edit-build-reload loop of a Ramjet plugin, and how to test it on your server.

## The loop

1. Have a test server running with your plugin folder in reach (a hosted server, or one you were given access to), and keep its log open.

2. In your plugin project, build:

   ```bash
   dotnet build -c Release
   ```

3. Get the plugin onto the server:

   - **With `RAMJET_PLUGINS` set** (see [create a plugin](create-a-plugin.md)) the build copies the plugin for you into `<plugins>/Homes/`.
   - **Otherwise** upload the files yourself, through the hosting panel's file manager or SFTP, into the server's `plugins/` folder: `bin/Release/net10.0/Homes.dll` (and its `.pdb`, which gives you line numbers in stack traces) either directly as `plugins/Homes.dll`, or, if the plugin has dependencies, together with `plugin.json` and the dependency dlls as `plugins/Homes/Homes.dll`. Upload the main dll **last**: the server reacts to the dll.

4. The server notices the new file, waits 250 ms for the write to finish, and loads the plugin. Watch the log for the line that says it is up:

   ```
   [INFO] [Homes] enabled v1.0.0 (id 1)
   ```

   The plugin also creates its data folder, `plugins/homes/`, when it first asks for it (configuration, saved data).

5. Change the code, build again. The server **reloads the plugin**:

   ```
   [INFO] hot reloading Homes.dll
   [INFO] [Homes] enabled v1.0.1 (id 1)
   [INFO] [Homes] disabled
   ```

   The new version is enabled **before** the old one is released, so there is no window in which your commands or event handlers are missing. Edit, build, test again: a few seconds per cycle.

`/plugins` (alias `/pl`) shows the state of the plugin worker, and `/plugins reload` restarts every plugin. Both need operator level 4 (the console always has it). When you need `/plugins reload`, and when you do not, is in [plugins added while the server runs](#plugins-added-while-the-server-runs).

## What survives a reload

- **ECS components with an unchanged layout keep their values** on every entity. Change a field type or order and the values are dropped (and the component recreated), but only if no other plugin is using the same component key.
- **Entities** are not owned by plugins, they survive.
- **Plugin-owned objects** (your fields, caches, timers) are gone: the plugin is a new instance. Keep state that must outlive a reload in components or on disk. See [hot reload](../concepts/hot-reload.md) for the full rules.

## A failed build changes nothing

A compile error never produces a dll, so the server never sees it. If the new assembly reaches the server but cannot be loaded (for example an exception in the new `OnEnable`), the exception is logged and the previous version keeps running. The warning names the reason after the colon:

```
[ERROR] [scripting] failed to load plugin .../Homes.dll: System.InvalidOperationException: no such home table ...
[WARN] [scripting] reload of .../Homes.dll failed: it threw InvalidOperationException: no such home table; keeping the previous version
```

The reason is one of: `it threw <ExceptionType>: <message>` (from `OnEnable`); `it does not compile (the errors are logged above)` (a [source plugin](project-layout.md#source-plugins)); `its data folder <id>/data is not writable` (see below); `no IPlugin implementation with a parameterless constructor`; `plugins.allow_source is false`; or the first problem found in the manifest or the dependencies (for example `plugin 'homes' requires 'economy >=1.2' which is not installed`). The same problems are in `Plugins.Problems`.

## Plugins added while the server runs

Copy a new plugin (a `Name.dll`, a `Name/Name.dll` folder, a `.cs` file or a folder of `.cs` files) into `plugins/` while the server runs and it is picked up **without a restart**, within a few seconds. A new plugin needs its data folder to be writable, and the sandboxed plugin worker gets write access only to the data folders that existed when it started. So the server logs `new plugin '<id>': the worker restarts so that its data folder can be written` and restarts the worker: every plugin is disabled (`OnDisable` runs, so they can save) and then all of them are loaded again, yours and the ones that were already running. Plugin fields are reset, as on any reload.

You need `/plugins reload` yourself when:

- hot reload is switched off on the server (new plugins then wait for a restart);
- you changed something the server does not watch for changes (for example only a data file);
- a new plugin keeps logging `its data folder ... is not writable` after the restart: the id the plugin declares is not the one the server derives from its file or folder name. Put `"id": "<id>"` in its `plugin.json`, or name the file or folder after the id, then run `/plugins reload`.

Replacing a plugin that is already running never restarts the worker: it is a normal hot reload of that plugin.

## Logging

```csharp
Log.Debug("...");
Log.Info("...");
Log.Warn("...");
Log.Error("...", exception);
```

Plugin log lines carry the plugin name: `[Homes] ...`. Debug lines are only shown if the server is set up to show debug output.

## Getting a player in

Test with a real Minecraft client (Java Edition, the version the server runs). If the server is a private test server, ask for a flat world with `online-mode` off or an allow-listed account so you can join quickly; the join runs your `PlayerJoinEvent` handler, and typing in chat runs your `PlayerChatEvent` handler. Give yourself operator rights on the test server so you can use `/plugins`.

## Stopping

On shutdown every plugin gets `OnDisable` in turn and its registrations are released.
