using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Custom Data", menuName = "ScriptableObjects/PlayerCustomData")]
public class PlayerCustomSO: ScriptableObject
{
    public Color hairColor;
    public Color skinColor;
    public int dressCode;
    public int hairCode;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}
