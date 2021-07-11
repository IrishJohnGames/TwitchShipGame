using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using System.Linq;
using CoreTwitchLibSetup;
using StateMachineLogic;
using System.Collections;

//TODO: proper namespace?
//namespace Assets.Gameplay
//{
public class Player : MonoBehaviour
{
    const int SHOW_CREW_DISPLAY_FOR_TIME = 5;

    int Level = 0;

    [SerializeField]
    float speed = 0.5f;

    public bool ParticipatingInBR = false;

    internal StateMachine<Player> stateMachine;

    [SerializeField]
    GameObject CrewDisplayHolder;

    [SerializeField]
    Transform crewSpriteDisplayHolder;

    [SerializeField]
    GameObject crewSpritePrefab;

    [SerializeField]
    private DisplayMembers displayMembers;

    internal int GetNumberOfCrew() => Crew.Count;

    [SerializeField]
    private Health health;

    [Serializable]
    private class DisplayMembers
    {
        [SerializeField]
        internal SpriteRenderer BaseSprite;

        [SerializeField]
        internal TextMeshPro ShipName, CptName, CrewNames;

        [SerializeField]
        internal SpriteRenderer Flag;

        [SerializeField]
        // Overall crew level.
        internal TextMeshPro Level;
    }

    [Serializable]
    private class Health
    {
        [SerializeField]
        internal SpriteRenderer Healthbar;

        [SerializeField]
        internal int Max;
        int _Current = 0;
        internal int Current
        {
            get { return _Current; }
            set
            {
                _Current = value;
                if (_Current <= 0)
                {
                    _Current = 0;
                }

                OnHealthChanged();
            }
        }

        internal void OnHealthChanged()
        {
            var percentage = (float)Current / (float)Max;
            Healthbar.material.SetFloat("_HealthPercent", percentage);
        }

        public void Respawn()
        {
            Current = Max;
        }
    }

    internal Player FindTarget()
    {
        var Owner = this;
        IEnumerable<Player> playersAroundThisPlayer = PlayerManager.Instance.GetPlayersInBRAroundVector2(Owner.transform.position);

        if (playersAroundThisPlayer.Count() > 1)
            return playersAroundThisPlayer.OrderBy(p => Vector2.Distance(Owner.transform.position, p.transform.position)).
                First(o => o.gameObject != Owner.gameObject);
        else return playersAroundThisPlayer.FirstOrDefault();

    }

    internal void PickedUpPowerup(Transform t)
    {
        switch (t.name) {
            case "HealthPowerUp":
                health.Current += 2;
                break;
        }
    }

    

    internal void DecreaseMovementSpeed(float f)
    {
        speed -= f;
        if (speed <= 0.01f) speed = 0.01f;
    }

    internal void DestroyTarget(Player currentTarget)
    {
        Destroy(currentTarget.gameObject);
    }

    internal void RecalcTotalLevelAndDisplay()
    {
        Level = Crew.Sum(o => o.Level);
        UpdateLevelDisplay();
    }

    internal void DealDamage(Player target, int amount)
    {
        
        target.health.Current -= amount;
        if (target.health.Current <= 0)
        {
            DestroyTarget(target);
        }
    }
   
    internal void MoveTo(Vector3 pos)
    {
        Debug.DrawLine(pos, transform.position, UnityEngine.Random.ColorHSV());

        Vector3 targetDir = pos - transform.position;

        transform.position = Vector2.MoveTowards(transform.position, pos, Time.deltaTime * speed);

        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * 0.5f);
    }

    class Cooldown
    {
        private float amount;
        private float lastUseTime = 0;
        public Cooldown(float cooldown)
        {
            this.amount = cooldown;
        }

        internal void Use()
        {
            lastUseTime = Time.time;
        }

        internal bool Ready()
        {
            return lastUseTime + amount <= Time.time;
        
        }

        internal void Reset()
        {
            lastUseTime = 0;
        }
    }

    Dictionary<string, Cooldown> _cooldowns = new Dictionary<string, Cooldown>();

    internal void ResetCooldown(string cooldown)
    {
        if(_cooldowns.TryGetValue(cooldown, out var c))
        {
            c.Reset();
        }
    }

    internal Projectile Fire(string projectileKey, ProjectileTarget target)
    {
        var prefab = ProjectileManager.Instance.GetProjectilePrefab(projectileKey);
        if (prefab != null)
        {
            if (!_cooldowns.TryGetValue(projectileKey, out var cooldown))
            {
                cooldown = new Cooldown(prefab.cooldown);
                _cooldowns.Add(projectileKey, cooldown);
            }

            if (cooldown.Ready())
            {
                cooldown.Use();
                var projectileInstance = Instantiate(prefab, transform.position, Quaternion.identity);
                projectileInstance.Setup(this, target);
                Debug.Log("projectile fired");

                return projectileInstance;
            }
        }
        else
        {
            Debug.Log("no projectile found with key:"+projectileKey);
        }
        return null;
    }

    [SerializeField]
    private List<CrewMate> Crew = new List<CrewMate>();

    private string ShipName;

    internal string GetShipName() => ShipName;

    [Serializable]
    internal class CrewMate
    {
        internal int index = -1;
        internal string Name = "";
        internal string Role = "";
        internal Sprite img;
        internal int Level;

        internal GameObject iconRenderer;
    
    }

    private void Start()
    {
        stateMachine = new StateMachine<Player>(this);
        stateMachine.ChangeState(new PlayerST_Spawned());
        health.Respawn();

        crewPos = crewSpriteDisplayHolder.position;

        onDestroy += (self) =>
        {
            //make sure the application is not quitting before releasing particle systems
            if (!PlayerManager.isApplicationQuitting)
            {
                //release ongoing particlesystems from this gameobject
                GetComponentsInChildren<ParticleSystem>().ToList().ForEach(x => x.transform.parent = null);
            }
        };
    }

    private void Update()
    {
        stateMachine.Update();
    }

    /// <summary>
    /// will be called at Player.OnDestroy unity event
    /// </summary>
    public event Action<Player> onDestroy;

    public void InitialisePlayer(string shipName, string captain)
    {
        // Ship name is not being set always.
        ShipName = shipName;

        AddCrewmate(captain);

        displayMembers.BaseSprite.color = UnityEngine.Random.ColorHSV();

        try
        {
            StartCoroutine(TwitchLibCtrl.Instance.GetUserProfileIcon(captain, (sprite) =>
            {
                displayMembers.Flag.sprite = sprite;
            }));
        }
        catch (Exception ex)
        {
            Debug.Log("failed to fetch user profile icon, this is most likely because your secrets are not initialized");
            Debug.LogError(ex);
        }

        if (PlayerManager.Instance.battleRoyaleState == PlayerManager.BattleRoyaleState.SubmissionsClosed)
            stateMachine.ChangeState(new PlayerSt_BattleRoyaleStarting());
    }

    [ContextMenu("Shoot machine gun")]
    public void ShootMachineGun()
    {
        var target = FindTarget();
        if (target != null)
        {
            if(!_cooldowns.TryGetValue("machinegun", out var cooldown))
            {
                _cooldowns.Add("machinegun", cooldown = new Cooldown(10));
            }    

            if(cooldown.Ready())
            {
                cooldown.Use();
                StartCoroutine(MachineGunFire(target));

            }
        }
    }

    [ContextMenu("Shoot Aoe fire")]
    public void ShootAoeFire()
    {
        var target = FindTarget();
        if (target != null)
        {
            if (!_cooldowns.TryGetValue("aoefire", out var cooldown))
            {
                _cooldowns.Add("aoefire", cooldown = new Cooldown(10));
            }

            if (cooldown.Ready())
            {
                cooldown.Use();
                ResetCooldown("standard");
                var center = transform.position;
                for (int i = 0; i < 10; i++)
                {
                    var radius = 2.5f;

                    var ang = UnityEngine.Random.value * 360; 
                    var pos = new Vector3();
                    pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
                    pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad); 
                    //pos.y = center.y; 
                    Fire("standard", new ProjectileTarget(pos).WithPlayerProvider((target)=> {
                        var players = PlayerManager.Instance.GetPlayersInBRAroundVector2ForRange(pos, 2);
                        return players.FirstOrDefault();
                    }));
                    ResetCooldown("standard");
                }
            }
        }
    }


    IEnumerator MachineGunFire(Player target)
    {
        ResetCooldown("standard");
        for (int i = 0; i < 10; i++)
        {
            if (target != null)
            {
                Fire("standard", new ProjectileTarget(target));
                ResetCooldown("standard");
                yield return new WaitForSeconds(.25f);
            }
            else
            {
                yield break;
            }
        }
    }


    [ContextMenu("Shoot chainshot")]
    public void ShootChainshot()
    {
        var target = FindTarget();
        if (target != null)
        {
            Fire("chain", new ProjectileTarget(target));
        }
    }

    [ContextMenu("Shoot grapeshot")]
    public void ShootGrapeshot()
    {
        var target = FindTarget();
        if (target != null)
        {
            Fire("grape", new ProjectileTarget(target));
        }
    }



    Vector2 crewPos;

    internal void KillCrewMate()
    {
        if (Crew.Count > 0)
        {
            var idx = UnityEngine.Random.Range(0, Crew.Count);
            var crewMate = Crew[idx];

            if(crewMate!=null)
            {
                if(crewMate.iconRenderer!=null)
                {
                    Destroy(crewMate.iconRenderer);
                }

                Crew.RemoveAt(idx);
                health.Max -= 1;
                health.OnHealthChanged();
                RefreshDisplay();
            }
        }
    }
    
    public void AddCrewmate(string name)
    {
        Crew.Add(new CrewMate()
        {
            index = Crew.Count,
            Name = name,
            Role = NameManager.Instance.GetCrewRoleName(Crew.Count),
        });

        StartCoroutine(PlayerManager.Instance.GetPlayerLevel(this, Crew.Last()));

        try
        {
            StartCoroutine(TwitchLibCtrl.Instance.GetUserProfileIcon(name, (sprite) =>
            {
                var crewMate = Crew.First(i => i.Name == name);
                crewMate.img = sprite;

                GameObject go = Instantiate(crewSpritePrefab, crewSpriteDisplayHolder);
                crewPos.x += 0.2f;
                go.transform.position = crewPos;
                go.GetComponent<SpriteRenderer>().sprite = sprite;
                crewMate.iconRenderer = go;
                RefreshDisplay();
            }));
        }
        catch (Exception ex)
        {
            Debug.Log("failed to fetch user profile icon, this is most likely because your secrets are not initialized");
            Debug.LogError(ex);
        }

        health.Max += 1;
        health.OnHealthChanged();
        RefreshDisplay();
    }

    internal IEnumerable<CrewMate> GetCrew() => Crew;

    void RefreshDisplay()
    {
        displayMembers.CrewNames.text = "";

        displayMembers.ShipName.text = ShipName;
        displayMembers.CptName.text = Crew.First(o => o.index == 0).Name;

        foreach (CrewMate c in Crew?.OrderBy(o => o.index))
            displayMembers.CrewNames.text += $"{c.Role} {c.Name}\n";

        StartCoroutine(ShowDisplay());
    }

    void UpdateLevelDisplay()
    {
        displayMembers.Level.text = "Lv."+ Level.ToString();
    }

    IEnumerator ShowDisplay()
    {
        CrewDisplayHolder?.SetActive(true);
        yield return new WaitForSecondsRealtime(SHOW_CREW_DISPLAY_FOR_TIME);
        CrewDisplayHolder?.SetActive(false);
    }

    private void OnDestroy() => onDestroy?.Invoke(this);

    internal string GetCrewmate(string name) => Crew.FirstOrDefault(o => o.Name == name)?.Name;

    internal decimal GetCrewCount() => Crew.Count();
}
