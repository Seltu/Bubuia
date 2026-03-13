using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bait Type", menuName = "ScriptableObjects/Items/Bait Type")]
public class BaitTypeSO : EquipableItemSO
{
    public int valor;
    public float baitPower;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    public override EquipSlot GetSlot()
    {
        return EquipSlot.Bait;
    }
}
