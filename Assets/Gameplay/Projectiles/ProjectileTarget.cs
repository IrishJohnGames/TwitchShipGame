using System;
using UnityEngine;
/// <summary>
/// wrapper to store either transform or vector3
/// </summary>
public class ProjectileTarget
{

    public Transform transform => _transform;

    public Player GetPlayer()
    {
        if (playerProvider != null) return playerProvider(this);
        if (_transform != null) return _transform.GetComponent<Player>();
        return null;
    }

    /// <summary>
    /// returns transform if stored, if transform is null, will return stored vector3
    /// </summary>
    public Vector3 position
    {
        get
        {
            if (_transform == null) return _position;
            return _transform.position;
        }
    }

    Vector3 _position;
    Transform _transform;
    public event Func<ProjectileTarget, Player> playerProvider;
    public ProjectileTarget(Vector3 position)
    {
        _position = position;
    }

    public ProjectileTarget(Transform t)
    {
        _transform = t;
    }

    public ProjectileTarget(Player p)
    {
        _transform = p.transform;
    }

    /// <summary>
    /// overwrites standard player provider, used for dealing damage
    /// </summary>
    public ProjectileTarget WithPlayerProvider(Func<ProjectileTarget, Player> playerProv)
    {
        this.playerProvider = playerProv;
        return this;
    }
}
