using System.Collections;
using System.Collections.Generic;
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

    private void OnEnable()
    {
        foreach(Transform child in itemSlotsParent)
        {
            Destroy(child.gameObject);
        }
        foreach(PlayerFishes fish in inventorySO.playerFishes)
        {
            if (fish.fishNum > 0)
            {
                var itemSlot = Instantiate(itemSlotPrefab, itemSlotsParent);
                itemSlot.SetSlot(fish);
                itemSlot.GetButton().onClick.AddListener(() => { DisplayItem(fish); });
            }
        }
        foreach (PlayerBait bait in inventorySO.playerBaits)
        {
            if (bait.baitNum > 0)
            {
                var itemSlot = Instantiate(itemSlotPrefab, itemSlotsParent);
                itemSlot.SetSlot(bait);
                itemSlot.GetButton().onClick.AddListener(() => { DisplayItem(bait); });
            }
        } 
    }

    private void DisplayItem(PlayerFishes fish)
    {
        itemDisplayImage.sprite = fish.fishType.fishSprite;
        itemDisplayName.text = fish.fishType.fishName;
        itemDisplayDescription.text = fish.fishType.descricao;
    }

    private void DisplayItem(PlayerBait bait)
    {
        itemDisplayImage.sprite = bait.baitType.baitSprite;
        itemDisplayName.text = bait.baitType.baitName;
        itemDisplayDescription.text = "Poder de Isca: " + bait.baitType.baitPower + "\n" + bait.baitType.descricao;
    }
}
