using System;
using TMPro;
using UnityEngine;
//TODO: proper namespace?
//namespace Assets.Gameplay
//{
public class Player : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer _spriteRenderer = null;

    [SerializeField]
    TextMeshPro _displayNameRenderer = null;
    
    /// <summary>
    /// will be called at Player.OnDestroy unity event
    /// </summary>
    public event Action<Player> onDestroy;
    
    /// <summary>
    /// sets name rendered on top of player sprite
    /// </summary>
    public string displayName
    {
        get => _displayNameRenderer.text;
        set => _displayNameRenderer.text = value;
    }

    /// <summary>
    /// store random value for wave animation purposes
    /// </summary>
    float _wavingRng = 0;

    private void Awake()
    {
        _wavingRng = UnityEngine.Random.Range(0.00001f, 0.00005f);
    }

    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        //simple wave animation
        transform.position += new Vector3(Mathf.Sin(Time.time) * (0.00001f + _wavingRng), Mathf.Sin(Time.time) * (_wavingRng + 0.0001f), 0);
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke(this);
    }
}
//}
