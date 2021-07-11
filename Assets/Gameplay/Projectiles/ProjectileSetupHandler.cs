using System;
using UnityEngine;

public abstract class ProjectileSetupHandler : MonoBehaviour
{
    public abstract void Setup(Projectile projectile, Player player, ProjectileTarget target);

    
}
