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

    public bool ParticipatingInBR = false;

    internal StateMachine<Player> stateMachine;

    [SerializeField]
    GameObject CrewDisplayHolder;

    [SerializeField]
    Transform crewSpriteDisplayHolder;

    [SerializeField]
    GameObject crewSpritePrefab;

    [SerializeField]
    private Projectile projectilePrefab;

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

    internal void PickedUpPowerup(Transform t)
    {
        switch (t.name) {
            case "HealthPowerUp":
                health.Current += 2;
                break;
        }
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

        transform.position = Vector2.MoveTowards(transform.position, pos, Time.deltaTime * 0.5f);

        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * 0.5f);
    }

    internal Projectile Fire(Player other)
    {
        var projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectileInstance.Setup(this, other);

        return projectileInstance;
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

    Vector2 crewPos;

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
                Crew.First(i => i.Name == name).img = sprite;

                GameObject go = Instantiate(crewSpritePrefab, crewSpriteDisplayHolder);
                crewPos.x += 0.2f;
                go.transform.position = crewPos;
                go.GetComponent<SpriteRenderer>().sprite = sprite;

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
