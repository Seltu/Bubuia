using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AlmanacSO", menuName = "ScriptableObjects/AlmanacSO")]

public class AlmanacSO : ScriptableObject
{
    public AlmanacFishes[] almanacFishes;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}


[System.Serializable]
public class AlmanacFishes
{
    public FishTypeSO fishType;
    public bool hasCaught;

    public AlmanacFishes(FishTypeSO fish, bool hasCaught)
    {
        this.fishType = fish;
        this.hasCaught = hasCaught;
    }
}
