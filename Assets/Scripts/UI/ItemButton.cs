using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _ownedTxt;
    [SerializeField] private Image _ownedIcon;
    private EquipableItemSO _item;

    public void SetButtonData(EquipableItemSO item, FishingSupliesStore store, AudioCaller audioCaller)
    {
        _item = item;
        _button.onClick.AddListener(() => store.SetCurrentItem(item));
        _button.onClick.AddListener(() => audioCaller.CallSFX("Click"));

        _itemName.text = item.entryName;
        _itemIcon.sprite = item.icon;
    }

    public void SetOwnedVisuals(bool owned)
    {
        if (!owned)
        {
            _ownedIcon.gameObject.SetActive(false);
            _ownedTxt.gameObject.SetActive(false);
        }
        else
        {
            _ownedIcon.gameObject.SetActive(true);
            _ownedTxt.gameObject.SetActive(true);
        }
    }

    public EquipableItemSO GetButtonItem()
    {
        return _item;
    }
}
