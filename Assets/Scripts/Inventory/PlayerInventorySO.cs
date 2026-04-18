using System;
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
    private Dictionary<EquipSlot, InventoryItem> _equippedItems = new();

    private const string MONEY_KEY = "INV_MONEY";
    private const string ITEM_KEY_PREFIX = "INV_ITEM_";
    private const string EQUIPMENT_KEY_PREFIX = "INV_EQUIPPED_";

    private string ItemKey(InventoryItem item) => ITEM_KEY_PREFIX + item.itemData.entryName;
    private string EquipmentKey(EquipSlot slot) => EQUIPMENT_KEY_PREFIX + slot.ToString();

    public BaitTypeSO CurrentBait => (BaitTypeSO)GetEquippedItem(EquipSlot.Bait).itemData;
    public FishingRodSO CurrentFishingRod => (FishingRodSO)GetEquippedItem(EquipSlot.FishingRod).itemData;
    public MoulinetSO CurrentMoulinet => (MoulinetSO)GetEquippedItem(EquipSlot.Moulinet).itemData;
    public FishingLineSO CurrentFishingLine => (FishingLineSO)GetEquippedItem(EquipSlot.FishingLine).itemData;
    public HookSO CurrentHook => (HookSO)GetEquippedItem(EquipSlot.Hook).itemData;


    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    private void OnValidate()
    {
        foreach(var item in items)
        {
            PlayerPrefs.SetInt(ItemKey(item), item.amount);
        }
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        playerMoney = PlayerPrefs.GetInt(MONEY_KEY, playerMoney);

        for (int i = 0; i < items.Count; i++)
        {
            var entry = items[i];
            entry.amount = PlayerPrefs.GetInt(ItemKey(entry), entry.amount);
            items[i] = entry;
        }

        foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
        {
            if (slot == EquipSlot.None)
                continue;

            var itemEntry = PlayerPrefs.GetString(EquipmentKey(slot), "");
            if (items.Find(o => o.itemData.entryName == itemEntry && o.amount > 0) != null)
            {
                EquipItem(items.Find(o => o.itemData.entryName == itemEntry));
            }
            else
            {
                try
                {

                    EquipItem(items.Find(o => o.itemData is EquipableItemSO equip && equip.GetSlot() == slot));
                }
                catch (NullReferenceException e)
                {
                    Debug.LogError("Não encontrado item padrão para a categoria de item: " + slot.ToString() +
                        "! Adicionar pelo menos um equipamento dessa categoria ao inventário do jogador\n" + e);
                }
            }
        }
    }
    
    public void AddMoney(int amount)
    {
        playerMoney += amount;
        PlayerPrefs.SetInt(MONEY_KEY, playerMoney);
        PlayerPrefs.Save();
    }

    public void AddItem(DescriptionDataSO item, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemData == item)
            {
                var entry = items[i];
                entry.amount += amount;
                items[i] = entry;

                SaveItem(entry);
                return;
            }
        }
    }

    public int GetAmount(DescriptionDataSO item)
    {
        return items.Find(o=>o.itemData==item).amount;
    }

    private void SaveItem(InventoryItem item)
    {
        PlayerPrefs.SetInt(ItemKey(item), item.amount);
        if (item.itemData is EquipableItemSO equipment)
            if (GetEquippedItem(equipment.GetSlot()) == item)
            {
                PlayerPrefs.SetString(EquipmentKey(equipment.GetSlot()), item.itemData.entryName);
            }
        PlayerPrefs.Save();
    }

    public void EquipItem(InventoryItem item)
    {
        if (item.itemData is EquipableItemSO targetItem)
        {
            var targetSlot = targetItem.GetSlot();

            _equippedItems[targetSlot] = item;

            if (targetItem is BaitTypeSO bait)
                _currentBaitPowerSO.Value = bait.baitPower;

            SaveItem(item);
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