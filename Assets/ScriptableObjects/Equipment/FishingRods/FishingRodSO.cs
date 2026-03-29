using UnityEngine;

[CreateAssetMenu(fileName = "Fishing Rod", menuName = "ScriptableObjects/Items/Fishing Rod")]
public class FishingRodSO : EquipableItemSO
{
    public Sprite rodSprite;
    public Sprite bobberSprite;
    public Sprite indicatorRingSprite;
    public float catchRadius;

    public override EquipSlot GetSlot()
    {
        return EquipSlot.FishingRod;
    }
}
