namespace SpawnProtect;

/// <summary>Sample: protects the area around the world spawn from breaking, placing and fire, and makes it pvp-free.
/// Shows cancellation, enum fields and per-event filtering.</summary>
[PluginInfo("SpawnProtect", Version = "1.0.0", Author = "Ramjet", Description = "Blocks building near the world spawn")]
public sealed class SpawnProtectPlugin : Plugin
{
    private const int Radius = 16;

    private static bool Protected(BlockPos pos) => Math.Abs(pos.X) <= Radius && Math.Abs(pos.Z) <= Radius;

    protected override void OnEnable()
    {
        Events.On((ref BlockBreakEvent e) =>
        {
            if (!Protected(e.Pos)) return;
            e.Cancelled = true;
            e.Player.SendActionBar(Component.Text("The spawn area is protected").Color(TextColor.Red));
        });

        Events.On((ref BlockPlaceEvent e) =>
        {
            if (!Protected(e.Pos)) return;
            e.Cancelled = true;
            e.Player.SendActionBar(Component.Text("You cannot build here").Color(TextColor.Red));
        });

        // fire must not spread (or be lit) inside the protected area either
        Events.On((ref BlockIgniteEvent e) => { if (Protected(e.Pos)) e.Cancelled = true; });
        Events.On((ref BlockBurnEvent e) => { if (Protected(e.Pos)) e.Cancelled = true; });

        // players cannot be hurt by other players inside the area (pvp-free spawn)
        Events.On((ref EntityDamageEvent e) =>
        {
            var victim = new Player(e.Target);
            if (e.Cause == DamageCause.EntityAttack && !e.Attacker.IsNull && victim.IsOnline)
            {
                var pos = victim.Position;
                if (Math.Abs(pos.X) <= Radius && Math.Abs(pos.Z) <= Radius) e.Cancelled = true;
            }
        });
    }
}
