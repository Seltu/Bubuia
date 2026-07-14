using System.Collections.Generic;
using UnityEngine;

public class PlayerPortSpawnPositions : MonoBehaviour
{
    [SerializeField] private Transform defaultPosition;
    [SerializeField] private List<Transform> spawnPositions = new List<Transform>();
    [SerializeField] private List<string> sceneConditions = new List<string>();
    [SerializeField] private PlayerSceneHistorySO _playerHistory;

    private GameObject _player;

    private void OnValidate()
    {
        if (spawnPositions.Count != sceneConditions.Count)
            Debug.LogWarning("Make sure there's exactly 1 scene condition for every spawn position");
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (sceneConditions.Contains(_playerHistory.previousScene))
            _player.transform.position = spawnPositions[sceneConditions.IndexOf(_playerHistory.previousScene)].position;
        else
            _player.transform.position = defaultPosition.position;
    }
}
