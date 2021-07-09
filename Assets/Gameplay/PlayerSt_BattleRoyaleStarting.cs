using UnityEngine;
using StateMachineLogic;

internal class PlayerSt_BattleRoyaleStarting : State<Player>
{
    Vector2 StartPosition;

    public override void EnterState()
    {
        StartPosition = PlayerManager.Instance.GetRandomBattleZoneStartPosition();
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        Owner.transform.position = Vector2.MoveTowards(Owner.transform.position, StartPosition, Time.deltaTime * 0.5f);
    }
}