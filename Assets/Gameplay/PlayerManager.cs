using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//TODO: proper namespace?
//namespace Assets.Gameplay
//{

/// <summary>
/// A class to manage player instances
/// </summary>
public class PlayerManager : ManagerBase<PlayerManager>
{
    [SerializeField]
    Transform _spawnZoneTransform = null;
    //TODO: multiple player prefabs?
    [SerializeField]
    Player _playerPrefab = null;

    List<Player> _players = new List<Player>();

    private void Awake()
    {
        //for editor... making sure we start off with a clean list of players
        _players = new List<Player>();
        
        /*
        for (int i = 0; i < 10; i++)
        {
            Spawn("HORN COOM", GetRandomSpawnPosition());

        }*/
    }

    /// <summary>
    /// Gets a random spawn position relative to the _spawnZoneTransform
    /// </summary>
    /// <returns></returns>
    public Vector2 GetRandomSpawnPosition()
    {
        //read spawnzone bounds
        var scaleX = _spawnZoneTransform.transform.localScale.x;
        var scaleY = _spawnZoneTransform.transform.localScale.y;
        //rng       
        var x = UnityEngine.Random.Range(-scaleX / 2, scaleX / 2);
        var y = UnityEngine.Random.Range(1, 1+(scaleY / 2));

        return _spawnZoneTransform.position + new Vector3(x, y);
    }

    /// <summary>
    /// Spawns a player
    /// </summary>
    /// <param name="playerName">name to display on top of the player sprite</param>
    /// <param name="position">position to spawn player at</param>
    /// <returns>instance of the player, can only create 1 player per playerName</returns>
    public Player Spawn(string playerName, Vector2 position)
    {
        //TODO: only allow 1 player instance?
        var existingPlayer = FindPlayerByName(playerName);
        if (existingPlayer != null) return null;

        //create instance of player prefab at position
        var instance = Instantiate(_playerPrefab, position, Quaternion.identity);
        //set instance player name to display
        instance.displayName = playerName;
      
        //register ondestroy hook that will stop tracking the player in the _players list
        instance.onDestroy += (self) => {
            _players.Remove(self);
        };

        //start tracking player instance
        _players.Add(instance);

        return instance;
    }

    /// <summary>
    /// Spawns a player at default spawnzone
    /// </summary>
    /// <param name="playerName">name to display on top of the player sprite</param>
    /// <returns>instance of the player, can only create 1 player per playerName</returns>
    public Player Spawn(string playerName)
    {
        //TODO: make sure that players dont spawn too near each other?
        return Spawn(playerName, GetRandomSpawnPosition());
    }

    [ContextMenu("Spawn player")]
    void SpawnDebug()
    {
        Spawn("Player "+UnityEngine.Random.Range(0, float.MaxValue), GetRandomSpawnPosition());
    }

    [ContextMenu("Spawn 10 players")]
    void Spawn10Debug()
    {
        for (int i = 0; i < 10; i++)
        {
            SpawnDebug();
        }
    }

    //TODO: maybe store players in dictionary for faster search access?
    public Player FindPlayerByName(string name)
    {
        return _players.FirstOrDefault(x => x.displayName.ToLower() == name.ToLower());
    }
}
//}
