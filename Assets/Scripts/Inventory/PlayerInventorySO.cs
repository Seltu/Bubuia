using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "Player Inventory SO", menuName = "ScriptableObjects/PlayerInventorySO")]
public class PlayerInventorySO : ScriptableObject
{
    public int playerMoney;
    public List<InventoryItem> items;
    [SerializeField] private FloatVariable _currentBaitPowerSO;
    [SerializeField] private FloatVariable _currentCatchRadiusSO;
    private Dictionary<EquipSlot, InventoryItem> _equippedItems = new();

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    public void EquipItem(InventoryItem item)
    {
        if (item.itemData is EquipableItemSO targetItem)
        {

            var targetSlot = targetItem.GetSlot();

            _equippedItems[targetSlot] = item;

            if (targetItem is BaitTypeSO bait)
                _currentBaitPowerSO.Value = bait.baitPower;
            else if (targetItem is FishingRodSO fishingRod)
                _currentCatchRadiusSO.Value = fishingRod.catchRadius;
        }
    }

    public InventoryItem GetEquippedItem(EquipSlot slot)
    {
        return _equippedItems[slot];
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