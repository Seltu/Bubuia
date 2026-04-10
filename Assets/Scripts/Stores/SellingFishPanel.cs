using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellingFishPanel : BaseStore
{
    [Header("Fish Desc Panel")]
    [SerializeField] protected Image _fishIconImg;
    [SerializeField] protected TMP_Text _fishName;
    [SerializeField] protected FishButton[] _fishesButtons;

    [Header("Selling Fish Store")]
    [SerializeField] private TMP_Text _quant;
    [SerializeField] private TMP_Text _value;
    [SerializeField] private GameObject _sellButton;

    protected FishTypeSO _currentFish;
    private int _quantIndex = 0;

    protected override void Start()
    {
        base.Start();
        EventManager.AddListener("OnCloseStore", HideItemDesc);

        HideItemDesc();
        _playerMoney.text = "R$ " + _playerInventory.playerMoney.ToString();
        CheckButtonInteraction();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.RemoveListener("OnCloseStore", HideItemDesc);
    }

    protected virtual void CheckButtonInteraction()
    {
        var playerFishes = _playerInventory.items.Where(x => x.itemData is FishTypeSO).ToList();
        for (int i = 0; i < _fishesButtons.Length; i++)
        {
            Button button = _fishesButtons[i].GetComponent<Button>();
            for (int j = 0; j < playerFishes.Count; j++)
            {
                if (_fishesButtons[i].GetFishType() == playerFishes[j].itemData)
                {
                    if (playerFishes[j].amount > 0)
                    {
                        button.interactable = true;
                        _fishesButtons[i].RevealFishInShop(playerFishes[j].amount);
                    }
                    else
                    {
                        button.interactable = false;
                    }
                }
            }
        }
    }

    #region Item Desc
    protected virtual void HideItemDesc()
    {
        _fishIconImg.gameObject.SetActive(false);
        _fishName.gameObject.SetActive(false);
        _quant.gameObject.SetActive(false);
        _value.gameObject.SetActive(false);
        _sellButton.gameObject.SetActive(false);    
        _currentFish = null;
    }

    protected virtual void ShowItemDesc()
    {
        _fishIconImg.gameObject.SetActive(true);
        _fishName.gameObject.SetActive(true);
        _quant.gameObject.SetActive(true);
        _value.gameObject.SetActive(true);
    }

    public void SetCurrentFish(FishTypeSO fish)
    {
        _fishIconImg.sprite = fish.icon;
        _fishName.text = fish.entryName;
        _quantIndex = 0;
        _quant.text = _quantIndex.ToString();
        _value.text = "R$ " + fish.valor;
        _currentFish = fish;

        HideSellButton();
        ShowItemDesc();
    }
    #endregion

    #region Shoping Buttons
    public void SellButton()
    {
        _playerInventory.AddMoney(_currentFish.valor * int.Parse(_quant.text));
       _playerInventory.AddItem(_currentFish, - int.Parse(_quant.text));
        _playerMoney.text = "R$ " + _playerInventory.playerMoney.ToString();
        SetCurrentFish(_currentFish);
        HideSellButton();
        CheckButtonInteraction();

        foreach (var button in _fishesButtons)
        {
            button.UpdateFishAmountInButton();
        }
    }

    private void HideSellButton()
    {
        _sellButton.gameObject.SetActive(false);
    }

    public void AddUnitButton(int leftRight)
    {
        
        if (_currentFish == null) return;

        int playerFishCount = 0;
        foreach (var fish in _playerInventory.items.Where(x => x.itemData is FishTypeSO).ToList())
        {
            Debug.Log("fish " + fish.itemData.entryName);
            if ((FishTypeSO)fish.itemData == _currentFish)
            {
                playerFishCount = fish.amount;
                break;
            }
        }

        if (playerFishCount == 0) return;

        _quantIndex += leftRight;

        if (_quantIndex < 0)
            _quantIndex = playerFishCount;
        else if (_quantIndex > playerFishCount)
            _quantIndex = 0;

        _quant.text = _quantIndex.ToString();
        _sellButton.SetActive(_quantIndex > 0);
    }
    #endregion
}
