using System;
using UnityEngine;

public class FishingRodVisuals : MonoBehaviour
{
    [SerializeField] private SpriteRenderer rodSprite;
    [SerializeField] private SpriteRenderer bobberSprite;
    [SerializeField] private SpriteRenderer indicatorRingSprite;
    [SerializeField] private PlayerInventorySO playerInventorySO;

    private void Awake()
    {
        UpdateVisuals();
        EventManager.AddListener<InventoryItem>("UseItem", OnUseItem);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<InventoryItem>("UseItem", OnUseItem);
    }

    private void OnUseItem(InventoryItem item)
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (playerInventorySO.GetEquippedItem(EquipSlot.FishingRod).itemData is FishingRodSO rodData)
        {
            rodSprite.sprite = rodData.rodSprite;
            bobberSprite.sprite = rodData.bobberSprite;
            indicatorRingSprite.sprite = rodData.indicatorRingSprite;
        }
    }
}
