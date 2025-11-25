using System.Collections;
using System.Collections.Generic;
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
        _fishIcon.sprite = _fishType.fishSprite;
        _fishTxt.text = _fishType.fishName;
    }

    public void RevealFishInShop(int fishNum)
    {
        _fishIcon.sprite = _fishType.fishSprite;
        _fishTxt.text = _fishType.fishName + "\n" + fishNum;
    }

    public void UpdateFishAmountInButton()
    {
        if(_fishTxt.text == "?") return;

        foreach (var fish in _playerInventory.playerFishes)
        {
            if (fish.fishType == _fishType)
            {
                _fishTxt.text = _fishType.fishName + "\n" + fish.fishNum;
                return;
            }
        }

        _fishTxt.text = _fishType.fishName + "\n0";
    }
}
