using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventorySO inventorySO;
    [SerializeField] private ItemSlotUI itemSlotPrefab;
    [SerializeField] private Transform itemSlotsParent;
    [SerializeField] private Image itemDisplayImage;
    [SerializeField] private TextMeshProUGUI itemDisplayName;
    [SerializeField] private TextMeshProUGUI itemDisplayDescription;
    [SerializeField] private Button itemUseButton;
    [SerializeField] private TextMeshProUGUI itemUseButtonText;

    private InventoryItem _selectedItem;

    private void Start()
    {
        itemUseButton.onClick.AddListener(UseButtonClick);
    }

    private void OnEnable()
    {
        UpdateInventory();
    }

    private void UpdateInventory()
    {
        foreach (Transform child in itemSlotsParent)
        {
            Destroy(child.gameObject);
        }

        List<InventoryItem> orderedItemList = inventorySO.items // Ordena os itens do inventário
            .Where(x => x.amount > 0) // Exclui aqueles com quantidade menor que 1
            .OrderBy(x => (x.itemData is EquipableItemSO equip && inventorySO.GetEquippedItem(equip.GetSlot()) == x) ? 0 : 1) // Exibe os itens equipados primeiro
            .ThenBy(x => GetOrderPriority(x.itemData)) // Agrupa por tipo
            .ThenBy(x => x.itemData.entryName) // Ordena por nome
            .ToList();

        foreach (InventoryItem inventoryItem in orderedItemList)
        {
            var itemSlot = Instantiate(itemSlotPrefab, itemSlotsParent);
            if (inventoryItem.itemData is EquipableItemSO equipment)
                itemSlot.SetSlot(inventoryItem, inventorySO.GetEquippedItem(equipment.GetSlot()) == inventoryItem);
            else
                itemSlot.SetSlot(inventoryItem, false);

            InventoryItem capturedItem = inventoryItem;
            itemSlot.GetButton().onClick.AddListener(() => SelectItem(capturedItem));
        }
    }

    private int GetOrderPriority(DescriptionDataSO item)
    {
        if (item is BaitTypeSO) return 0;
        if (item is FishingRodSO) return 1;
        if (item is MoulinetSO) return 2;
        if (item is FishingLineSO) return 3;
        if (item is HookSO) return 4;
        if (item is FishTypeSO) return 5;

        return 999; // outros tipos vão para o final
    }

    private void SelectItem(InventoryItem inventoryItem)
    {
        _selectedItem = inventoryItem;

        itemDisplayImage.sprite = inventoryItem.itemData.icon;
        itemDisplayName.text = inventoryItem.itemData.entryName;

        if (inventoryItem.itemData is BaitTypeSO bait)
        {
            itemDisplayDescription.text = "Poder de Isca: " + bait.baitPower + "\n" + bait.description;
        }
        else
            itemDisplayDescription.text = inventoryItem.itemData.description;

        if (inventoryItem.itemData is EquipableItemSO)
        {
            itemUseButton.gameObject.SetActive(true);
            itemUseButtonText.text = "Equip";
        }
        else
            itemUseButton.gameObject.SetActive(false);
    }

    private void UseButtonClick()
    {
        if (_selectedItem.itemData is EquipableItemSO)
        {
            inventorySO.EquipItem(_selectedItem);
        }
        EventManager.TriggerEvent("UseItem", _selectedItem);
        UpdateInventory();
    }
}