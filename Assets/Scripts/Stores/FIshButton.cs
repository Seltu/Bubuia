using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishButton : MonoBehaviour
{
    [SerializeField] protected PlayerInventorySO _playerInventory;
    [SerializeField] protected FishTypeSO _fishType;

    [SerializeField] protected Image _fishIcon;
    [SerializeField] protected TMP_Text _fishTxt;
    [SerializeField] protected Button _button;

    protected bool _playerHasFish = false;

    public FishTypeSO GetFishType()
    {
        return _fishType;
    }

    public void RevealTextAndIcon()
    {
        _fishIcon.sprite = _fishType.icon;
        _fishTxt.text = _fishType.entryName;
    }

    public void RevealFishInShop(int fishNum)
    {
        _fishIcon.sprite = _fishType.icon;
        _fishTxt.text = _fishType.entryName + "\n" + fishNum;
    }

    public void UpdateFishAmountInButton()
    {
        if(_fishTxt.text == "?") return;

        foreach (var fish in _playerInventory.items.Where(x => x.itemData is FishTypeSO).ToList())
        {
            if ((FishTypeSO)fish.itemData == _fishType)
            {
                _fishTxt.text = _fishType.entryName + "\n" + fish.amount;
                return;
            }
        }

        _fishTxt.text = _fishType.entryName + "\n0";
    }
}
