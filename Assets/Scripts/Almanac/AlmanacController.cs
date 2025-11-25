using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlmanacController : SellingFishPanel
{
    [Header("Almanac")]
    [SerializeField] private Sprite _defaultFishSprite;
    [SerializeField] private AlmanacSO _fishAlmanac;
    [SerializeField] private TMP_Text _fishDesc;

    protected override void Start()
    {
        EventManager.AddListener<string>("OnOpenStore", ShowShopPanel);
        EventManager.AddListener("OnCloseStore", CloseShopPanel);
        EventManager.AddListener("OnCloseStore", HideItemDesc);

        HideItemDesc();
    }

    protected override void OnDestroy()
    {
        EventManager.RemoveListener<string>("OpenStore", ShowShopPanel);
        EventManager.RemoveListener("OnCloseStore", CloseShopPanel);
        EventManager.RemoveListener("OnCloseStore", HideItemDesc);
    }
    protected virtual void Update()
    {
        CheckButtonInteraction();
    }

    protected override void CheckButtonInteraction()
    {
        for(int i = 0; i < _fishesButtons.Length; i++)
        {
            Button button = _fishesButtons[i].GetComponent<Button>();
            if (button.interactable == true) break;

            for(int j = 0; j < _fishAlmanac.almanacFishes.Length; j++)
            {
                if (_fishesButtons[i].GetFishType() == _fishAlmanac.almanacFishes[j].fishType)
                {
                    if(_fishAlmanac.almanacFishes[j].hasCaught)
                    {
                        button.interactable = true;
                        _fishesButtons[i].RevealTextAndIcon();
                    }
                    else
                    {
                        button.interactable = false;
                    }
                }
            }
        }
    }

    public void OnDisable()
    {
        HideItemDesc();
    }

    public void AlmanacFishButtonClick(FishTypeSO fish)
    {
        _fishIconImg.sprite = fish.fishSprite;

        _fishName.text = fish.fishName;
        _fishDesc.text = fish.descricao;

        ShowItemDesc();
    }

    protected override void HideItemDesc()
    {
        _fishIconImg.sprite = _defaultFishSprite;

        _fishName.text = "???";
        _fishDesc.text = "???????";
    }

    protected override void ShowItemDesc()
    {
        _fishIconImg.gameObject.SetActive(true);
        _fishName.gameObject.SetActive(true);
        _fishDesc.gameObject.SetActive(true);
    }
}
