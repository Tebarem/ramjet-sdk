# Ramjet.Api

The compile-time API for C# plugins of the **Ramjet** Minecraft server (`net10.0`).

```xml
<PackageReference Include="Ramjet.Api" Version="1.0.0-preview.7" />
```

The package holds a reference assembly: every public type and member of `Mc.Scripting.Api`, with its documentation, and no code. Your
plugin compiles against it; the server supplies the real API when it loads the plugin, so nothing from this package is copied next to
your dll.

What the package also sets up:

- `EnableDynamicLoading` (each plugin loads into its own unloadable context) and implicit usings;
- global usings for the API namespaces (`Mc.Scripting`, `.Commands`, `.Events`, `.Text`, `.Worlds`, ...). `RamjetGlobalUsings=false`
  turns them off;
- with `RAMJET_PLUGINS` (or `-p:RamjetPluginsDir=...`) set to a server's plugins folder, every build copies the plugin there, and the
  server hot-reloads it.

Start a project with the template: `dotnet new install Ramjet.Templates`, then `dotnet new ramjet-plugin -n MyPlugin`.
