using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bait Type", menuName = "ScriptableObjects/Items/Bait Type")]
public class BaitTypeSO : EquipableItemSO
{
    public float baitPower;
    public override EquipSlot GetSlot()
    {
        return EquipSlot.Bait;
    }
}
