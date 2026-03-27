using UnityEngine;

public class SaveDataLoader : Singleton<SaveDataLoader>
{
    [SerializeField] private PlayerInventorySO _playerInventorySO;
    private void Start()
    {
        _playerInventorySO.LoadInventory();
        GlobalFlagsManager.LoadFlags();
    }
}
