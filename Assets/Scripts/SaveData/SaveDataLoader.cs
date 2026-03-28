using UnityEngine;

public class SaveDataLoader : Singleton<SaveDataLoader>
{
    [SerializeField] private PlayerInventorySO _playerInventorySO;
    protected override void Awake()
    {
        base.Awake();
        _playerInventorySO.LoadInventory();
        GlobalFlagsManager.LoadFlags();
    }
}
