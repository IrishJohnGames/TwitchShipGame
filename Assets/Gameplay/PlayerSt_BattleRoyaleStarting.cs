using UnityEngine;
using StateMachineLogic;

internal class PlayerSt_BattleRoyaleStarting : State<Player>
{
    Vector2 StartPosition;

    static Vector2 desiredScale = new Vector2(0.2f, 0.2f);

    public override void EnterState()
    {
        StartPosition = PlayerManager.Instance.GetRandomPositionInBattleZone();
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        Owner.MoveTo(StartPosition);

        // Bring down the size a bit.
        Owner.transform.localScale = Vector2.Lerp(Owner.transform.localScale, desiredScale, Time.deltaTime * 0.5f);
    }

}