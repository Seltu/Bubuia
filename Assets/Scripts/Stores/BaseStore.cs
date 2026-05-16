using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseStore : MonoBehaviour
{
    [Header("Bool Variables")]
    [SerializeField] private BoolVariable _onMenu;
    [SerializeField] private BoolVariable _canPause;
    [SerializeField] private float _boolDelay = 0.3f;

    [Header("Player Inventory")]
    [SerializeField] protected PlayerInventorySO _playerInventory;

    [Header("Base store")]
    [SerializeField] protected GameObject _storePanel;
    [SerializeField] protected TMP_Text _playerMoney;
    [SerializeField] private string storeId; // Mesmo ID do NPC
    protected bool _shopIsShowing = false;


    protected virtual void Start()
    {
        EventManager.AddListener<string>("OnOpenStore", ShowShopPanel);
        EventManager.AddListener("OnCloseStore", CloseShopPanel);
    }

    protected virtual void OnDestroy()
    {
        EventManager.RemoveListener<string>("OnOpenStore", ShowShopPanel);
        EventManager.RemoveListener("OnCloseStore", CloseShopPanel);
    }


    protected void ShowShopPanel(string npcId)
    {
        if (storeId != npcId) return;
        EventManager.TriggerEvent("OnBlockPlayerMovement");
        _storePanel.SetActive(true);
        _shopIsShowing = true;
        _onMenu.value = true;
        _canPause.value = false;
    }

    protected void CloseShopPanel()
    {
        _storePanel.SetActive(false);
        _shopIsShowing = false;
        Invoke("DelayedBools", _boolDelay);
        EventManager.TriggerEvent("OnAllowPlayerMovement");
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", true);
    }

    protected void DelayedBools()
    {
        _onMenu.value = false;
        _canPause.value = true;
    }

    public void CloseStoreButton()
    {
        CloseShopPanel();
    }
}
