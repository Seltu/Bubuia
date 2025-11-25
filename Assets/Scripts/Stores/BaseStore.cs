using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseStore : MonoBehaviour
{
    [Header("Player Inventory")]
    [SerializeField] protected PlayerInventorySO _playerInventory;

    [Header("Base store")]
    [SerializeField] protected GameObject _storePanel;
    [SerializeField] protected TMP_Text _playerMoney;
    [SerializeField] private string storeId; // Mesmo ID do NPC
    private bool _shopIsShowing = false;


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
        _storePanel.SetActive(true);
        _shopIsShowing = true;
    }

    protected void CloseShopPanel()
    {
        _storePanel.SetActive(false);
        _shopIsShowing = false;
    }
}
