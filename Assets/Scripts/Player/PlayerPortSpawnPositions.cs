using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPortSpawnPositions : MonoBehaviour
{
    [SerializeField] private Transform defaultPosition;
    [SerializeField] private List<Transform> spawnPositions = new List<Transform>();
    [SerializeField] private List<ChoiceCondition> spawnConditions = new List<ChoiceCondition>();
    [SerializeField] private PlayerMovement _player;

    private void OnValidate()
    {
        if (spawnPositions.Count != spawnConditions.Count)
            Debug.LogWarning("Make sure there's exactly 1 scene condition for every spawn position");
    }

    private void Start()
    {
        for (int i = 0; i < spawnConditions.Count; i++)
        {
            ChoiceCondition condition = spawnConditions[i];
            if (condition.Decide())
            {
                _player.Teleport(spawnPositions[i].transform.position);
                return;
            }
        }
        _player.transform.position = defaultPosition.position;
    }
}
