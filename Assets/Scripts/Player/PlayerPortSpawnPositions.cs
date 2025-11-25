using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPortSpawnPositions : MonoBehaviour
{
    [SerializeField] private Transform _playerMarketSpawn;
    [SerializeField] private Transform _playerBoatSpawn;
    [SerializeField] private PlayerSceneHistorySO _playerHistory;

    private GameObject _player;
    

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        if (_playerHistory.previousScene == "FishingScene")
        {
            _player.transform.position = _playerBoatSpawn.transform.position;
            Debug.Log("boat");
        }
        else if( _playerHistory.previousScene == "MarketScene")
        {
            _player.transform.position = _playerMarketSpawn.transform.position;
            Debug.Log("market");
        }
    }
}
