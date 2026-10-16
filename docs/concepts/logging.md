# Logs and debugging

Where a plugin's log lines, exceptions and compiler errors end up, the file formats, and how to read them quickly (by hand or with an AI coding agent).

The server writes a `logs/` folder in its working directory. Next to its own log it keeps **one folder per plugin**, so you can read everything about your plugin without the rest of the server, and a source plugin's compiler output in full. The console shows the same lines as always: the files are in addition to it, not instead of it.

## The layout

```
logs/
  latest.log               the whole server, the console line for line
  latest.jsonl             the same lines as JSON (plugin lines name the plugin)
  2026-10-15-1.log.gz      earlier runs and days, compressed
  plugins/
    <plugin-id>/
      latest.log           everything about this plugin (this run)
      latest.jsonl         the same as JSON lines
      2026-10-15-1.log.gz  earlier runs of this plugin's log
      build.log            the last compilation of a source plugin, every diagnostic
      build.jsonl          the same as JSON lines
```

`<plugin-id>` is the plugin's id (`plugin.json` `id`, or the id derived from the file name: lower case, `a-z 0-9 _ -`). The paths are stable: an agent can be told "read `logs/plugins/homes/latest.log`" and it will find it on every server.

Every start archives the previous `latest.log` to `<date>-<n>.log.gz` (as vanilla does), so `latest.log` is always the current run. A plugin's `latest.log` is archived the same way the first time the plugin logs in a new run. A `/plugins reload` does not start a new file: the reload's `disabled` and `enabled` lines show where it happened.

## What goes into a plugin's latest.log

| Source | What |
|---|---|
| `plugin` | everything your plugin writes through its `Log` (`Log.Info`, `Log.Warn`, `Log.Error(message, exception)`, ...), and its `enabled` / `disabled` lines |
| `plugin` | exceptions your code throws into the server (event handlers, commands and their suggestions, scheduled and async tasks, plugin channel handlers): the runtime catches them and logs them with your logger, plus its warning when a handler runs long on the server thread |
| `runtime` | the plugin runtime's lines about your plugin: why it failed to load, a reload that failed and kept the old version, dependents unloaded or rebuilt with it |
| `engine` | the server's lines about your plugin: an event handler that ran past its time budget, the plugin muted after too many of those, lines dropped because it logged too fast |
| `build` | the result of each build of a source plugin, pointing at `build.log` |

Lines you write with `Console.WriteLine`, or through a `new Logger("...")` you made yourself, are not tied to your plugin: they reach the server log only. Use your plugin's `Log`.

## Line format

One record per line; a multi-line message (a stack trace) repeats the header on every line, so a search for your plugin id or a level finds all of it:

```
[2026-10-16 02:33:10.123] [INFO] [greeter] [plugin] enabled v1.0.0 (id 3)
[2026-10-16 02:33:11.004] [ERROR] [greeter] [plugin] unhandled exception: System.NullReferenceException: Object reference not set to an instance of an object.
[2026-10-16 02:33:11.004] [ERROR] [greeter] [plugin]    at Greeter.Main.OnJoin(PlayerJoinEvent& e) in C:\dev\Greeter\Main.cs:line 31
[2026-10-16 02:33:11.004] [ERROR] [greeter] [plugin]    at (Ramjet plugin runtime)
[2026-10-16 02:33:15.500] [WARN] [greeter] [engine] event player_join (seq 812) not answered within 5000 us: the plugin's handler ran too long (scripting.ipc.event_budget_us)
```

`[time UTC] [LEVEL] [plugin-id] [source] message`. As a regular expression:

```
^\[(\d{4}-\d\d-\d\d \d\d:\d\d:\d\d\.\d{3})\] \[(TRACE|DEBUG|INFO|WARN|ERROR)\] \[([a-z0-9_-]+)\] \[(plugin|runtime|engine|build)\] (.*)$
```

Stack traces keep the file and line of **your** code (when your build has debug symbols; source plugins always do). Frames inside the server's plugin runtime are shown as one `at (Ramjet plugin runtime)` line, and API frames (`Mc.Scripting.*`) show their method without a path.

`latest.jsonl` has one object per line, with line breaks kept inside `message`:

```json
{"time":"2026-10-16T02:33:10.123Z","level":"INFO","plugin":"greeter","source":"plugin","message":"enabled v1.0.0 (id 3)"}
```

## Compiler output of source plugins (build.log)

A source plugin (a `.cs` file or a folder of them, see [project layout](../getting-started/project-layout.md)) is compiled by the server. Its `build.log` is rewritten on every build, so it always describes the build that is running (or failed) now: a summary line, then **every** error and warning in the compiler's usual `file(line,col): error CODE: message` form, which editors and agents parse as is.

```
[2026-10-16 02:33:09.002] [BUILD] [greeter] plugins/Greeter: failed to compile, 1 error, 0 warnings (3 files, 812 ms)
plugins/Greeter/Main.cs(12,5): error CS1002: ; expected
```

A build served from the server's compile cache says so (`... unchanged, using the compiled plugin from the cache (4 ms)`). File paths are relative to the server folder. A plugin whose `plugin.json` or files cannot be read gets a failed build with that one error. `build.jsonl` holds a `{"type":"build",...}` summary line (`result` is `compiled`, `cached` or `failed`; `ms`, `files`, `errors`, `warnings`) followed by one `{"type":"diagnostic","severity","code","file","line","column","message"}` line per diagnostic.

The console and the server's `latest.log` keep a short version (the first errors and warnings and a summary line); the full list is only in `build.log`. A failed build of a plugin that was already running keeps the old version running: `build.log` shows the new errors, `latest.log` says the previous version was kept.

## Limits

- Lines per second: a plugin that logs faster than the server's limit (`scripting.ipc.log_per_second` in `ramjet.toml`, 200 by default) loses the rest of that second; the next line in its file says how many were dropped. Each plugin has its own budget, so a noisy plugin does not hide another plugin's lines.
- Size: a plugin's `latest.log` is archived when it reaches `logs.plugin_max_kb` (4 MiB by default), and only `logs.plugin_max_archives` (5) archives are kept, so a plugin that logs in a loop cannot fill the disk.
- A very long message is cut at 64 KiB.

## Settings (ramjet.toml, [logs])

| Key | Default | |
|---|---|---|
| `enabled` | `true` | write log files at all |
| `directory` | `logs` | the folder, relative to the server's working directory |
| `jsonl` | `true` | the `.jsonl` twin of every `.log` |
| `max_mb` | `256` | the server's `latest.log` is archived past this size (it is also archived at every start and every new day) |
| `max_archives` | `0` | archives of the server log kept, oldest deleted first; 0 keeps all |
| `plugins` | `true` | the per-plugin folders |
| `plugin_max_kb` | `4096` | size of a plugin's `latest.log` before it is archived |
| `plugin_max_archives` | `5` | archives kept per plugin |

## Debugging a plugin with an AI agent

Point the agent at the two files of the plugin, not the server log:

1. `logs/plugins/<id>/build.log`: does it compile? Every diagnostic, with file, line and column.
2. `logs/plugins/<id>/latest.log` (or `latest.jsonl` for a tool): did it enable, what did it log, which exceptions, which budget warnings. Filter by `[ERROR]` or `"level":"ERROR"`.

On a hosted server, download the `logs/plugins/<id>/` folder from the file manager; the paths are the same.
