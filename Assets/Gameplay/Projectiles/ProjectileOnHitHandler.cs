using UnityEngine;

/// <summary>
/// this component will be used to implement on hit events for projectiles
/// </summary>
public abstract class ProjectileOnHitHandler : MonoBehaviour
{
    /// <summary>
    /// called on projectile on hit event
    /// </summary>
    /// <param name="projectile">source projectile </param>
    /// <param name="player">projectile owner player</param>
    /// <param name="target">projectile target context</param>
    public abstract void Setup(Projectile projectile, Player player, ProjectileTarget target);
}
