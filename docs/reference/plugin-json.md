# plugin.json reference

Every field of the plugin manifest, where it lives, how dependencies and versions are resolved.

## Location

| Layout | Manifest |
|---|---|
| `plugins/<dir>/<dir>.dll` | `plugins/<dir>/plugin.json` |
| `plugins/<Name>.dll` (loose) | `plugins/<Name>.plugin.json` |
| single `.cs` file or folder of sources | `plugins/<Name>.plugin.json` / `plugin.json` in the folder, or the `[assembly: McPlugin(...)]` attribute in the sources |
| no manifest | the `[PluginInfo]` attribute is the fallback; the id is the lower-cased assembly name |

The manifest is read **before any assembly is loaded**: the server builds the dependency graph from manifests alone, then loads in order. Comments and trailing commas are allowed in the file.

## Example

```json
{
  "id": "homes",
  "name": "Homes",
  "version": "1.2.0",
  "api": "1.0",
  "main": "Homes.HomesPlugin",
  "authors": ["Your Name"],
  "description": "Set and teleport to homes.",
  "website": "https://example.org",
  "depends": ["economy >=1.2"],
  "softDepends": ["permissions-ui"],
  "loadBefore": ["chat-format"],
  "permissions": [
    { "node": "homes.use", "default": "true", "description": "Use /home" }
  ]
}
```

## Fields

| Field | Type | Required | Meaning |
|---|---|---|---|
| `id` | string | yes | stable plugin id: lower-case `[a-z0-9_-]{1,32}`. Used for the data folder (`plugins/<id>/data/`), dependencies and logs. A duplicate id skips the second plugin |
| `name` | string | no | display name, used as the log prefix (defaults to the id) |
| `version` | string | recommended | plugin version, compared against dependants' constraints (defaults to `0.0.0`) |
| `api` | string | recommended | the API version the plugin was built for, `"major.minor"`. If missing, the plugin is assumed to match the server |
| `main` | string | no | full name of the `Plugin` subclass; needed only when the assembly has several candidates |
| `authors` | string[] | no | |
| `description` | string | no | one line |
| `website` | string | no | |
| `depends` | string[] | no | **hard** dependencies: the plugin is skipped if one is missing or too old |
| `softDepends` | string[] | no | loaded first when present; the plugin works without them |
| `loadBefore` | string[] | no | an edge from this plugin to the named one: this plugin loads first |
| `permissions` | object[] | no | `node`, `default` and `description` for each permission the plugin checks |

For a permission, `default` is `"true"`, `"false"`, `"op"` (operator level 2 and up), `"op:N"` (level N and up, 0-4) or `"not_op"`; when it is missing it is `"op"`. Declaring permissions in the manifest makes them exist before your code runs. An invalid default stops the manifest from loading. If your code also calls `Permissions.Register` for the same node, the code's registration replaces the manifest's (no error); a node that a different loaded plugin has already declared stops this plugin from loading. See [declared in `plugin.json`, in code, or both](../concepts/permissions.md#declared-in-pluginjson-in-code-or-both).

Unknown fields are ignored (forward compatible). Malformed JSON, a missing or invalid `id`, a duplicate id or an unsatisfiable API version are reported with the file path and the offending key.

### Version constraints

A `depends` / `softDepends` entry may carry a constraint after the id: `"economy >=1.2"`. Supported operators: `>=`, `>`, `=`, `<`, `<=` and `^1.2` (caret: same major, at least that version). A pre-release suffix after `-` is ignored when versions are compared.

### API version

The server declares an API version (`ApiVersion.Current`, `1.0` for the first release): minor = additive, major = breaking. A plugin loads when its `major` equals the server's and its `minor` is at most the server's, and is refused otherwise with a message such as `plugin 'homes' needs API 1.4, this server provides 1.2`. Within a major, no member is removed and no signature changes; new overloads are added instead of optional parameters, and events only gain fields.

## Load order and failures

Plugins are discovered in `plugins/` and ordered by a topological sort of `depends`, `softDepends` (only if present) and `loadBefore`, ties broken by id so the order is deterministic.

| Situation | Result |
|---|---|
| missing hard dependency or version mismatch | the plugin and everything depending on it is skipped: `plugin 'homes' requires 'economy >=1.2' which is not installed` |
| dependency cycle | every plugin on the cycle is skipped and the cycle is printed: `dependency cycle: a -> b -> c -> a` |
| a plugin whose `OnEnable` throws | disabled; its dependents are skipped with that cause |
| unload | reverse load order; hot reload of a plugin also reloads its hard dependents |

A plugin can read these problems at run time with `Plugins.Problems`, and look at the loaded plugins with `Plugins.All`, `Plugins.Get(id)` and `Plugins.IsLoaded(id)`.
