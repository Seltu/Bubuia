using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingSupliesStore : BaseStore
{
    [Header("Fishing Store")]
    [SerializeField] private Image _iconImg;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _desc;
    [SerializeField] private TMP_Text _value;
    [SerializeField] private GameObject _comprarButton;

    private EquipableItemSO _currentItem;

    protected override void Start()
    {
        base.Start();
        EventManager.AddListener("OnCloseStore", HideItemDesc);

        HideItemDesc();
        _playerMoney.text = "R$ " + _playerInventory.playerMoney.ToString();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.RemoveListener("OnCloseStore", HideItemDesc);
    }


    private void HideItemDesc()
    {
        _iconImg.gameObject.SetActive(false);
        _name.gameObject.SetActive(false);
        _desc.gameObject.SetActive(false);
        _value.gameObject.SetActive(false);
        _comprarButton.gameObject.SetActive(false);
        _currentItem = null;
    }

    private void ShowItemDesc()
    {
        _iconImg.gameObject.SetActive(true);
        _name.gameObject.SetActive(true);
        _desc.gameObject.SetActive(true);
        _value.gameObject.SetActive(true);
    }

    public void SetCurrentItem(EquipableItemSO item)
    {
        _iconImg.sprite = item.icon;
        _name.text = item.entryName;
        _desc.text = item.description;
        _value.text = "R$ " + item.valor;
        _currentItem = item;

        HideBuyButton();
        ShowItemDesc();
    }

    public void BuyButton()
    {
        if(_playerInventory.playerMoney >= _currentItem.valor)
        {
            _playerInventory.AddMoney(_currentItem.valor * -1);
            _playerMoney.text = "R$ " + _playerInventory.playerMoney.ToString();
            _playerInventory.AddItem(_currentItem, 1);
            EventManager.TriggerEvent("ResetItemStore");
            HideBuyButton();
        }
    }

    private void HideBuyButton()
    {
        if (_currentItem == null)
        {
            _comprarButton.SetActive(false);
            return;
        }

        bool isBait = _currentItem is BaitTypeSO;
        bool alreadyBought = _playerInventory.HasItem(_currentItem);

        if (!isBait && alreadyBought)
        {
            _comprarButton.SetActive(false);
            return;
        }

        _comprarButton.SetActive(_playerInventory.playerMoney >= _currentItem.valor);
    }
}
