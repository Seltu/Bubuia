using UnityEngine;

public enum EquipSlot { None, Bait, FishingRod }

public abstract class EquipableItemSO : DescriptionDataSO
{
    public bool equipped;

    public virtual EquipSlot GetSlot()
    {
        return EquipSlot.None;
    }
}
