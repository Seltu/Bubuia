using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Player Inventory SO", menuName = "ScriptableObjects/PlayerInventorySO")]
public class PlayerInventorySO : ScriptableObject
{
    public int playerMoney;
    public PlayerBait[] playerBaits;
    public PlayerFishes[] playerFishes;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}

[System.Serializable]
public struct PlayerBait
{
    public BaitTypeSO baitType;
    public int baitNum;

    public PlayerBait(BaitTypeSO baitType, int baitNum)
    {
        this.baitType = baitType;
        this.baitNum = baitNum;
    }
}

[System.Serializable]
public class PlayerFishes
{
    public FishTypeSO fishType;
    public int fishNum;

    public PlayerFishes(FishTypeSO fish, int fishNum)
    {
        this.fishType = fish;
        this.fishNum = fishNum;
    }
}