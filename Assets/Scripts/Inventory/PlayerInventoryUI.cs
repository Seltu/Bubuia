using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        foreach (Transform child in itemSlotsParent)
        {
            Destroy(child.gameObject);
        }

        List<InventoryItem> orderedItemList = inventorySO.items // Ordena os itens do inventário
            .Where(x => x.amount > 0) // Exclui aqueles com quantidade menor que 1
            .OrderBy(x => GetOrderPriority(x.itemData)) // Agrupa por tipo
            .ThenBy(x => x.itemData.entryName) // Ordena por nome
            .ToList();

        foreach (InventoryItem inventoryItem in orderedItemList)
        {
            var itemSlot = Instantiate(itemSlotPrefab, itemSlotsParent);
            itemSlot.SetSlot(inventoryItem);

            InventoryItem capturedItem = inventoryItem;
            itemSlot.GetButton().onClick.AddListener(() => SelectItem(capturedItem));
        }
    }

    private int GetOrderPriority(DescriptionDataSO item)
    {
        if (item is FishTypeSO) return 0;
        if (item is BaitTypeSO) return 1;

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
    }
}