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

An operator can list the loaded plugins with `/plugins` and restart every plugin with `/plugins reload`, for example after changing a file the server does not watch.

## What survives a reload

- **ECS components with an unchanged layout keep their values** on every entity. Change a field type or order and the values are dropped (and the component recreated), but only if no other plugin is using the same component key.
- **Entities** are not owned by plugins, they survive.
- **Plugin-owned objects** (your fields, caches, timers) are gone: the plugin is a new instance. Keep state that must outlive a reload in components or on disk. See [hot reload](../concepts/hot-reload.md) for the full rules.

## A failed build changes nothing

A compile error never produces a dll, so the server never sees it. If the new assembly reaches the server but cannot be loaded (for example an exception in the new `OnEnable`), the exception is logged and the previous version keeps running:

```
[ERROR] failed to load plugin .../Homes.dll ...
[WARN] reload of .../Homes.dll failed; keeping the previous version
```

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
