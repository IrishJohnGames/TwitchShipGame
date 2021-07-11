using System;
using System.Collections;
using System.Linq;
using UnityEngine;
/// <summary>
/// projectile implementation, to add setup events add component that inherits from ProjectileSetupHandler
/// to add onhit events add component that inherits from ProjectileOnHitHandler
/// </summary>
public class Projectile : MonoBehaviour
{
    //projectile spawnposition, used for lerping the position when the projectile is fired
    protected Vector3 _spawnPosition = Vector3.zero;

    protected Player _owner = null;

    protected ProjectileTarget _target = null;

    //track progress of the projectile
    protected float _elapsed = 0;

    //animation curve to modify how the y of the projectile should move
    [SerializeField]
    protected AnimationCurve _trajectory = default(AnimationCurve);

    [SerializeField]
    protected ParticleSystem _hitFx = null;
    //how long the projectile will travel

    [SerializeField]
    protected float _travelTime = 1.5f;

    [SerializeField]
    protected int _damage = 1;
    [SerializeField]
    protected float _cooldown = 1;

    public float cooldown => _cooldown;

    [SerializeField]
    bool _applyRandomness = true;

    //store random number
    protected int _rng = 0;

    protected bool _isSetup = false;

    internal void Setup(Player player, ProjectileTarget target)
    {
        _isSetup = true;
        _spawnPosition = transform.position;
        _owner = player;
        _target = target;

        if (_applyRandomness)
        {
            //add some randomness to the trajectory direction
            _rng = UnityEngine.Random.value > .5f ? 1 : 0;
        }
        //find setup handlers in this gameobject
        GetComponentsInChildren<ProjectileSetupHandler>().ToList().ForEach(x => x.Setup(this, player, target));
    }



    private void Update()
    {
        if (_owner && _target != null)
        {
            //add progress
            _elapsed += Time.deltaTime;

            //calculate how far projectile should be
            var progress = _elapsed / _travelTime;

            //store target position in temp value
            var targetPos = _target.position;
            //evaluate trajectory based on progress
            var eval = _trajectory.Evaluate(progress);

            //apply randomness
            if (_rng == 0) targetPos.y += eval;
            else targetPos.y -= eval;
            //lerp to targetpos
            transform.position = Vector3.Lerp(_spawnPosition, targetPos, progress);

            //if hit, then destroy the projectile
            if (progress >= 1)
            {

                HitTarget();
                return;

            }

        }

        if (_isSetup && (!_owner || _target == null))
        {
            Destroy(gameObject);
        }

    }

    protected virtual Player GetPlayerFromTarget() => _target.GetPlayer();
    //public event Action onHitTarget;

    protected void OnHitTarget(Player targetPlayer)
    {
        _owner.DealDamage(targetPlayer, _damage);
        //find on hit handlers in this gameobect
        GetComponentsInChildren<ProjectileOnHitHandler>().ToList().ForEach(x => x.Setup(this, _owner, _target));
    }

    private void HitTarget()
    {
        PlayerManager.Instance.StartCoroutine(SpawnFx());
        //onHitTarget?.Invoke();
        var targetPlayer = GetPlayerFromTarget();
        if (targetPlayer != null)
        {
            OnHitTarget(targetPlayer);

        }

        Destroy(gameObject);

    }

    IEnumerator SpawnFx()
    {
        if (_hitFx == null) yield break;

        Transform parent = null;
        if (_target != null)
        {
            parent = _target.transform;
        }

        var fx = Instantiate(_hitFx, transform.position, Quaternion.identity, parent);

        yield return new WaitForSeconds(fx.main.duration);
        if (fx != null)
        {
            Destroy(fx.gameObject);
        }
    }
}
