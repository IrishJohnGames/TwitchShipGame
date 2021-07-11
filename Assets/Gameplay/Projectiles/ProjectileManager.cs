using System;
using System.Linq;
using UnityEngine;
public class ProjectileManager : ManagerBase<ProjectileManager>
{
    [SerializeField]
    ProjectileContainer[] _projectiles = null;

    public Projectile GetProjectilePrefab(string key)
    {
        var info = _projectiles.FirstOrDefault(x => x.key == key);
        if (info != null)
            return info.projectile;
        return null;
    }

    [Serializable]
    public class ProjectileContainer
    {
        public Projectile projectile;
        public string key;
    }
}
//}
