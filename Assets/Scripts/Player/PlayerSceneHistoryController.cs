using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneHistoryController : MonoBehaviour
{
    [SerializeField] private PlayerSceneHistorySO _history;

    private void Awake()
    {
        _history.previousScene = _history.currentScene;
        _history.currentScene = SceneManager.GetActiveScene().name;
    }
}
