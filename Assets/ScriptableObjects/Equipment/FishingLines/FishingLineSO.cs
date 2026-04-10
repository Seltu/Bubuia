using UnityEngine;

[CreateAssetMenu(fileName = "Fishing Line", menuName = "ScriptableObjects/Items/Fishing Line")]
public class FishingLineSO : EquipableItemSO
{
    public int durability;
    public override EquipSlot GetSlot()
    {
        return EquipSlot.FishingLine;
    }
}
