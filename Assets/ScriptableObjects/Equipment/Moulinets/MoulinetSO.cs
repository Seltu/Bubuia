using UnityEngine;

[CreateAssetMenu(fileName = "Moulinet", menuName = "ScriptableObjects/Items/Moulinet")]
public class MoulinetSO : EquipableItemSO
{
    public int pullForce;
    public override EquipSlot GetSlot()
    {
        return EquipSlot.Moulinet;
    }
}
