using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerupManager : ManagerBase<PowerupManager>
{
    [SerializeField]
    GameObject[] powerupPrefabs;

    const int SPAWN_TIME_INCREMENT = 5;
    float timeToSpawn = 0;

    const float PROXIMITY_TIME_INCREMENT = 0.5f;
    float timeToCheckProximity = 0;

    List<Transform> spawned = new List<Transform>();

    internal void ClearAllSpawnedPrefabs()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            Transform t = spawned[i];
            Destroy(t.gameObject);
        }
        spawned = new List<Transform>();
    }

    private void Update()
    {
        if(PlayerManager.Instance.battleRoyaleState == PlayerManager.BattleRoyaleState.InProgress) { 
            if(Time.time > timeToSpawn && spawned.Count <= 5)
            {
                timeToSpawn = Time.time + SPAWN_TIME_INCREMENT;
                spawned.Add(Instantiate(powerupPrefabs[Random.Range(0, powerupPrefabs.Length - 1)], this.transform).transform);
                spawned.Last().position = PlayerManager.Instance.GetRandomPositionInBattleZone();
            }

            if(Time.time > timeToCheckProximity)
            {
                timeToCheckProximity = Time.time + PROXIMITY_TIME_INCREMENT;
                
                for(int i = 0; i < spawned.Count; i++)
                {
                    Transform t = spawned[i];
                    if (t == null) continue;
                    Player playerNearby = PlayerManager.Instance.GetPlayersInBRAroundVector2ForPowerUp(t.position)?.OrderBy(o => Vector2.SqrMagnitude(o.transform.position - t.position))?.FirstOrDefault();

                    if (playerNearby != null)
                    {
                        playerNearby.PickedUpPowerup(t);
                        Destroy(t.gameObject);
                    }
                }

                spawned.RemoveAll(o => o == null);
            }
        }
    }
}
