using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Player Inventory SO", menuName = "ScriptableObjects/PlayerInventorySO")]
public class PlayerInventorySO : ScriptableObject
{
    public int playerMoney;
    public List<InventoryItem> items;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}

[System.Serializable]
public class InventoryItem
{
    public DescriptionDataSO itemData;
    public int amount;

    public InventoryItem(DescriptionDataSO itemData, int baitNum)
    {
        this.itemData = itemData;
        this.amount = baitNum;
    }
}