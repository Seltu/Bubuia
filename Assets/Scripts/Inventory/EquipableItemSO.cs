using UnityEngine;

public enum EquipSlot { None, Bait, FishingRod }

public abstract class EquipableItemSO : DescriptionDataSO
{
    public virtual EquipSlot GetSlot()
    {
        return EquipSlot.None;
    }
}