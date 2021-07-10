using StateMachineLogic;
using UnityEngine;

internal class PlayerST_BackToChillingZone : State<Player>
{
    Vector2 chillPos;

    public override void EnterState()
    {
        Owner.ParticipatingInBR = false;
        chillPos = PlayerManager.Instance.GetRandomSpawnPosition();
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        if (Vector2.Distance(Owner.transform.position, chillPos) > 1)
            Owner.MoveTo(chillPos);
        else
            Owner.stateMachine.ChangeState(new PlayerST_Chilling());
    }
}