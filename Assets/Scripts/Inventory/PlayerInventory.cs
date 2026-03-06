using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private PlayerInventorySO _inventory;

    private const string MONEY_KEY = "INV_MONEY";
    private const string ITEM_KEY_PREFIX = "INV_ITEM_";

    private string ItemKey(InventoryItem item) => ITEM_KEY_PREFIX + item.itemData.entryName;

    private void Awake()
    {
        LoadInventory();
    }

    private void Start()
    {
        EventManager.AddListener<int>("OnAddToPlayerMoney", AddToPlayerMoney);
        EventManager.AddListener<DescriptionDataSO, int>("OnAddItem", AddItem);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<int>("OnAddToPlayerMoney", AddToPlayerMoney);
        EventManager.RemoveListener<DescriptionDataSO, int>("OnAddItem", AddItem);
    }

    private void AddToPlayerMoney(int amount)
    {
        _inventory.playerMoney += amount;
        PlayerPrefs.SetInt(MONEY_KEY, _inventory.playerMoney);
        PlayerPrefs.Save();
    }

    public void AddItem(DescriptionDataSO item, int amount)
    {
        for (int i = 0; i < _inventory.items.Count; i++)
        {
            if (_inventory.items[i].itemData == item)
            {
                var entry = _inventory.items[i];
                entry.amount += amount;
                _inventory.items[i] = entry;

                SaveItem(entry);
                return;
            }
        }

        InventoryItem newItem = new InventoryItem(item, amount);

        _inventory.items.Add(newItem);

        SaveItem(newItem);
    }

    public int GetAmount(InventoryItem item)
    {
        for (int i = 0; i < _inventory.items.Count; i++)
        {
            if (_inventory.items[i] == item)
                return _inventory.items[i].amount;
        }

        return 0;
    }

    private void SaveItem(InventoryItem item)
    {
        PlayerPrefs.SetInt(ItemKey(item), item.amount);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        _inventory.playerMoney = PlayerPrefs.GetInt(MONEY_KEY, _inventory.playerMoney);

        for (int i = 0; i < _inventory.items.Count; i++)
        {
            var entry = _inventory.items[i];
            entry.amount = PlayerPrefs.GetInt(ItemKey(entry), entry.amount);
            _inventory.items[i] = entry;
        }
    }
}