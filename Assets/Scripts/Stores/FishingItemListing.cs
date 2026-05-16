using System.Collections;
using UnityEngine;
using static UnityEditor.Progress;

public class FishingItemListing : MonoBehaviour
{
    [SerializeField] private GameObject _itemObjectPrefab;
    [SerializeField] private Transform _scrollViewContent;
    [SerializeField] private FishingSupliesStore _fishingStore;
    [SerializeField] private AudioCaller _audioCaller;
    [SerializeField] private PlayerInventorySO _playerInventorySO;
    [SerializeField] private EquipableItemSO[] _itens;

    private bool _created;

    private void OnEnable()
    {
        EventManager.AddListener("ResetItemStore", ResetItens);
        CreateItens();
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("ResetItemStore", ResetItens);
    }

    private void CreateItens()
    {
        if (_created) return;

        foreach (EquipableItemSO item in _itens)
        {
            GameObject itemButtonObject = Instantiate(_itemObjectPrefab, _scrollViewContent);

            if (itemButtonObject.TryGetComponent<ItemButton>(out ItemButton itemButton))
            {
                itemButton.SetButtonData(item, _fishingStore, _audioCaller);

                bool isOwned = item.GetSlot() != EquipSlot.Bait && _playerInventorySO.HasItem(item);
                itemButton.SetOwnedVisuals(isOwned);
            }
        }

        _created = true;
    }

    private void ResetItens()
    {
        foreach (Transform child in _scrollViewContent)
        {
            if (child.TryGetComponent<ItemButton>(out ItemButton itemButton))
            {
                EquipableItemSO item = itemButton.GetButtonItem();
                bool isOwned = item.GetSlot() != EquipSlot.Bait && _playerInventorySO.HasItem(item);
                itemButton.SetOwnedVisuals(isOwned);
            }
        }
    }
}