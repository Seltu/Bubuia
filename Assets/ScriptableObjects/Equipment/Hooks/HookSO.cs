using UnityEngine;

[CreateAssetMenu(fileName = "Hook", menuName = "ScriptableObjects/Items/Hook")]
public class HookSO : EquipableItemSO
{
    public FishSize sizeCategory;
    public override EquipSlot GetSlot()
    {
        return EquipSlot.Hook;
    }
}
