using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using System.Linq;
using CoreTwitchLibSetup;
using StateMachineLogic;

//TODO: proper namespace?
//namespace Assets.Gameplay
//{
public class Player : MonoBehaviour
{
    internal StateMachine<Player> stateMachine;

    [SerializeField]
    private DisplayMembers displayMembers;

    [Serializable]
    private class DisplayMembers {
        [SerializeField]
        internal TextMeshPro ShipName, CptName, CrewNames;

        [SerializeField]
        internal SpriteRenderer Flag;
    }

    [SerializeField]
    private List<CrewMate> Crew = new List<CrewMate>();
    private string ShipName;

    internal string GetShipName() => ShipName;

    [Serializable]
    class CrewMate
    {
        internal int index = -1;
        internal string Name = "";
        internal string Role = "";
    }

    private void Start()
    {
        stateMachine = new StateMachine<Player>(this);
        stateMachine.ChangeState(new PlayerST_Spawned());
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
        ShipName = shipName;
        AddCrewmate(captain);

        StartCoroutine(TwitchLibCtrl.Instance.GetUserProfileIcon(captain, (sprite) => {
            displayMembers.Flag.sprite = sprite;
        }));
    }

    public void AddCrewmate(string name)
    {
        Crew.Add(new CrewMate()
        {
            index = Crew.Count,
            Name = name,
            Role = NameManager.Instance.GetCrewRoleName(Crew.Count)
        });

        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        displayMembers.CrewNames.text = "";

        displayMembers.ShipName.text = ShipName;
        displayMembers.CptName.text = Crew.First(o=>o.index == 0).Name;

        foreach (CrewMate c in Crew?.OrderBy(o => o.index))
            displayMembers.CrewNames.text += $"{c.Role} {c.Name}\n";
    }

    private void OnDestroy() => onDestroy?.Invoke(this);

    internal string GetCrewmate(string name) => Crew.FirstOrDefault(o => o.Name == name)?.Name;

    internal decimal GetcrewCount() => Crew.Count();
}
