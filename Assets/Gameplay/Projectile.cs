using System;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //projectile spawnposition, used for lerping the position when the projectile is fired
    Vector3 _spawnPosition = Vector3.zero;

    Player _owner = null;

    Player _target = null;
    //track progress of the projectile
    float _elapsed = 0;

    //animation curve to modify how the y of the projectile should move
    [SerializeField]
    AnimationCurve _trajectory = default(AnimationCurve);
    [SerializeField]
    ParticleSystem _hitFx = null;
    //how long the projectile will travel
    [SerializeField]
    float _travelTime = 1.5f;
    [SerializeField]
    int _damage = 1;
    //store random number
    int _rng = 0;
    bool _isSetup = false;
    internal void Setup(Player player, Player other)
    {
        _isSetup = true;
        _spawnPosition = transform.position;
        _owner = player;
        _target = other;
        //add some randomness to the trajectory direction
        _rng = UnityEngine.Random.value > .5f ? 1 : 0;
    }

    private void Update()
    {
        if (_owner && _target)
        {
            //add progress
            _elapsed += Time.deltaTime;

            //calculate how far projectile should be
            var progress = _elapsed / _travelTime;

            //store target position in temp value
            var targetPos = _target.transform.position;
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
        
        if(_isSetup && (!_owner || !_target))
        {
            Destroy(gameObject);
        }

    }

    private void HitTarget()
    {
        PlayerManager.Instance.StartCoroutine(SpawnFx());
        _owner.DealDamage(_target, _damage);
        Destroy(gameObject);

    }

    IEnumerator SpawnFx()
    {
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
