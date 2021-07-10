using UnityEngine;
using StateMachineLogic;

internal class PlayerST_Chilling : State<Player>
{
    Vector2 desiredScale = new Vector2(0.5f, 0.5f);
    public override void EnterState()
    {
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        if (Owner.transform.localScale.x != desiredScale.x)
            Owner.transform.localScale = Vector2.Lerp(Owner.transform.localScale, desiredScale, Time.deltaTime * 0.5f);
    }
}