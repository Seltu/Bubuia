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

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    public void EquipItem(InventoryItem item)
    {
        if (item.itemData is not EquipableItemSO targetItem)
            return;

        var targetSlot = targetItem.GetSlot();

        foreach (var inventoryItem in items)
        {
            if (inventoryItem.itemData is EquipableItemSO equipable &&
                equipable.GetSlot() == targetSlot)
            {
                equipable.equipped = inventoryItem == item;
            }
        }

        if (targetItem is BaitTypeSO bait)
            _currentBaitPowerSO.Value = bait.baitPower;
        else if (targetItem is FishingRodSO fishingRod)
            _currentCatchRadiusSO.Value = fishingRod.catchRadius;
    }

    public InventoryItem GetEquippedItem(EquipSlot slot)
    {
        return items.Find(x => x.itemData is EquipableItemSO sO && sO.GetSlot() == slot && sO.equipped);
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