using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Scene History", menuName = "ScriptableObjects/PlayerSceneHistory")]
public class PlayerSceneHistorySO : ScriptableObject
{
    public string currentScene;
    public string previousScene;
}
