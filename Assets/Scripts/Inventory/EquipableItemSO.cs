using UnityEngine;

public enum EquipSlot { None, Bait, FishingRod, Moulinet, FishingLine, Hook }

public abstract class EquipableItemSO : DescriptionDataSO
{
    public virtual EquipSlot GetSlot()
    {
        return EquipSlot.None;
    }
}