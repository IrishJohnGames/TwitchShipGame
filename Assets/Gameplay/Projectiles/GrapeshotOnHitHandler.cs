public class GrapeshotOnHitHandler : ProjectileOnHitHandler
{
    public override void Setup(Projectile projectile, Player player, ProjectileTarget target)
    {
        var targetPlayer = target.GetPlayer();
        if (targetPlayer != null)
        {
            targetPlayer.KillCrewMate();
        }
    }
}
