# Config and data

Typed JSON config with defaults, the plugin data folder, atomic writes and reload.

## The data folder

Every plugin gets its own data folder, `plugins/<id>/data/` (the `id` from `plugin.json`, or the lower-cased plugin name), exposed as `DataFolder`. The server creates it before the plugin worker starts. Put every file the plugin writes there; `Config.Load` writes the defaults there on first run. A plugin folder `plugins/<id>/` (a `<id>.dll`, or `.cs` sources) holds it as its `data/` subfolder: the sources in `data/` are never compiled, and hot reload ignores everything in it, so writing files there never reloads your plugin.

The plugin worker is sandboxed: the data folder is the **only** place a plugin can write (the server's shared source-plugin cache `plugins/.cache` is the worker's own). Writing anywhere else, with `Config` or with plain `System.IO`, fails with `UnauthorizedAccessException`.

## Typed config

Describe your configuration as a plain class with defaults, and register it with System.Text.Json **source generation** (no reflection at run time, AOT-friendly):

```csharp
using System.Text.Json.Serialization;

public sealed class HomesConfig
{
    public int MaxHomes { get; set; } = 3;
    public int CooldownSeconds { get; set; } = 5;
    public string Greeting { get; set; } = "Welcome home!";
}

[JsonSerializable(typeof(HomesConfig))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class MyJson : JsonSerializerContext { }
```

```csharp
protected override void OnEnable()
{
    var cfg = Config.Load(MyJson.Default.HomesConfig);   // reads plugins/<id>/data/config.json
    int max = cfg.Value.MaxHomes;                         // an immutable snapshot
    cfg.Changed += value => Log.Info($"config reloaded, limit {value.MaxHomes}");
}
```

`Config.Load(typeInfo, fileName = "config.json")` returns a `Config<T>`:

| Member | Behaviour |
|---|---|
| `Config.Load(typeInfo)` | reads `config.json`; if it does not exist the **defaults (`new T()` including property initialisers) are written first**. Pass a second argument for another file name |
| `cfg.Value` | immutable snapshot, replaced atomically on reload; thread-safe to read |
| `cfg.Reload()` | rereads the file; returns false when the file could not be parsed |
| `cfg.Save()` | writes the current value (pretty-printed) |
| `cfg.Update(newValue)` | replaces the value and saves it |
| `cfg.Changed` | `event Action<T>`, raised on the tick thread after a reload or `Update` |
| `cfg.Watch = true` | edits of the file on disk are picked up (debounced) and reported through `Changed` |
| `cfg.Path` | the path of the file |

Comments and trailing commas are allowed on read, so admins can annotate the file. **A parse error keeps the previous snapshot** (or the defaults on first load) and logs the file and line; the file is never overwritten after an error.

Because `Value` is an immutable snapshot, change a copy and hand it to `Update`, rather than mutating `Value` in place.

## Your own files

For data that is not configuration (homes per player, a ban list), the plugin's `Config` offers `ReadText(fileName)` (null when the file does not exist) and `WriteText(fileName, text)` for files in the data folder. The write is **atomic** (temp file + rename), so a crash never leaves half a file:

```csharp
string? json = Config.ReadText("homes.json");
Config.WriteText("homes.json", JsonSerializer.Serialize(_data, MyJson.Default.HomesData));
```

Do the disk work off the tick thread for anything big (see [threading](threading.md)). Plain `System.IO` works too, inside `DataFolder` only: the sandbox refuses writes anywhere else.

Remember that ECS component values survive a hot reload while your fields do not, so state that must survive a reload belongs in components or on disk.
