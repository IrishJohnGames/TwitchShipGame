using StateMachineLogic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
internal class PlayerST_Fighting : State<Player>
{
    Vector2 WanderToPosition;

    Player currentTarget;

    public override void EnterState()
    {
        WanderToPosition = PlayerManager.Instance.GetRandomPositionInBattleZone();
    }

    public override void ExitState()
    {
    }

    float timeToCheckForEnemies = 0;
    const float TIME_INCREMENT = 1;

    public override void UpdateState()
    {
        if(currentTarget != null)
        {
            if (Random.Range(0, 5) == 3)
            {
                Owner.DestroyTarget(currentTarget);
                currentTarget = null;
                return;
            }
            Debug.DrawLine(Owner.transform.position, currentTarget.transform.position, Color.red);
            return;
        }

        Owner.MoveTo(WanderToPosition);

        if (Time.time > timeToCheckForEnemies)
        {
            timeToCheckForEnemies = Time.time + TIME_INCREMENT;

            IEnumerable<Player> playersAroundThisPlayer = PlayerManager.Instance.GetPlayersAroundVector2(Owner.transform.position);

            if (playersAroundThisPlayer.Count() > 1)
                currentTarget = playersAroundThisPlayer.OrderBy(p=> Vector2.Distance(Owner.transform.position, p.transform.position)).
                    First(o => o.gameObject != Owner.gameObject);
        }
    }
}