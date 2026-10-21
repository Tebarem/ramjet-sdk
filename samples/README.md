# Sample plugins

Each folder is a complete plugin. Build one with `dotnet build samples/<Name> -c Release`, or all of them with
`dotnet build samples/Samples.slnx -c Release`. They reference `Ramjet.Api` 1.0.0-preview.5 (set in `Directory.Build.props`).

| Sample | What it shows |
|---|---|
| [`Arenas`](Arenas/) | Runtime worlds sample: temporary arena worlds copied from a slime template |
| [`BossBarTimer`](BossBarTimer/) | Countdown timers on a boss bar |
| [`DiscordWebhook`](DiscordWebhook/) | Join and quit messages to a Discord webhook |
| [`Greeter`](Greeter/) | Join/quit messages and a server list MOTD |
| [`HelloPlugin`](HelloPlugin/) |  |
| [`Homes`](Homes/) | Set homes and teleport back to them |
| [`Kits`](Kits/) | Claimable kits with cooldowns |
| [`PermissionAdmin`](PermissionAdmin/) | /perm: manage groups and player permissions of the built-in backend |
| [`RubyOre`](RubyOre/) | A custom block and the item it drops |
| [`RubyWand`](RubyWand/) | A custom item with a right-click ability |
| [`ShopGui`](ShopGui/) | A shop in a custom GUI |
| [`SidebarStats`](SidebarStats/) | Live personal sidebar scoreboard |
| [`SpawnProtect`](SpawnProtect/) | Blocks building near the world spawn |
| [`WorldTools`](WorldTools/) | World API sample: blocks, signs, mobs, explosions, time, weather, chunk tickets |
