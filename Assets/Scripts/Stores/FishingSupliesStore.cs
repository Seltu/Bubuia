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

    private BaitTypeSO _currentBait;

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
        _currentBait = null;
    }

    private void ShowItemDesc()
    {
        _iconImg.gameObject.SetActive(true);
        _name.gameObject.SetActive(true);
        _desc.gameObject.SetActive(true);
        _value.gameObject.SetActive(true);
    }

    public void SetCurrentBait(BaitTypeSO bait)
    {
        _iconImg.sprite = bait.icon;
        _name.text = bait.entryName;
        _desc.text = bait.description;
        _value.text = "R$ " + bait.valor;
        _currentBait = bait;

        HideBuyButton();
        ShowItemDesc();
    }

    public void BuyButton()
    {
        if(_playerInventory.playerMoney >= _currentBait.valor)
        {
            EventManager.TriggerEvent("OnAddToPlayerMoney", _currentBait.valor * -1);
            _playerMoney.text = "R$ " + _playerInventory.playerMoney.ToString();
            HideBuyButton();
        }
    }

    private void HideBuyButton()
    {
        if(_playerInventory.playerMoney >= _currentBait.valor)
            _comprarButton.gameObject.SetActive(true);
        else
            _comprarButton.gameObject.SetActive(false);
    }
}
