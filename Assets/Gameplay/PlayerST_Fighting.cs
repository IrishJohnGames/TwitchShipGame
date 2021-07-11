using StateMachineLogic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
internal class PlayerST_Fighting : State<Player>
{
    internal Vector2 moveToPos;

    Player currentTarget;

    public override void EnterState()
    {
        moveToPos = PlayerManager.Instance.GetRandomPositionInBattleZone();
    }

    public override void ExitState()
    {
    }

    float timeToCheckForEnemies = 0;
    const float TIME_INCREMENT = 1;

    public override void UpdateState()
    {
        if (Vector2.Distance(Owner.transform.position, moveToPos) < 1)
        {
            moveToPos = PlayerManager.Instance.GetRandomPositionInBattleZone();
        }

        Owner.MoveTo(moveToPos);

        if (currentTarget != null)
        {
            if (Random.Range(0, 5) == 3)
            {
                Owner.Fire("standard", new ProjectileTarget(currentTarget));
                currentTarget = null;
                return;
            }
            Debug.DrawLine(Owner.transform.position, currentTarget.transform.position, Color.red);
            return;
        }

        if (Time.time > timeToCheckForEnemies)
        {
            timeToCheckForEnemies = Time.time + TIME_INCREMENT;

            currentTarget = Owner.FindTarget();
        }
    }
}