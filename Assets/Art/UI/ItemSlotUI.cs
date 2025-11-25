using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private FishTypeSO fishInfo;
    [SerializeField] private BaitTypeSO baitInfo;
    [SerializeField] private PlayerInventorySO inventorySO;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCountText;
    private void OnEnable()
    {
        if(fishInfo != null)
        {
            if(inventorySO.playerFishes.First(o => o.fishType == fishInfo).fishNum <= 0)
            {
                Destroy(gameObject);
            }
            itemIcon.sprite = fishInfo.fishSprite;
            itemNameText.text = fishInfo.fishName;
            itemCountText.text = "x" + inventorySO.playerFishes.First(o => o.fishType == fishInfo).fishNum.ToString();
        }
        else if (baitInfo != null)
        {
            if (inventorySO.playerBaits.First(o => o.baitType == baitInfo).baitNum <= 0)
            {
                Destroy(gameObject);
            }
            itemIcon.sprite = baitInfo.baitSprite;
            itemNameText.text = baitInfo.baitName;
            itemCountText.text = "x" + inventorySO.playerBaits.First(o => o.baitType == baitInfo).baitNum.ToString();
        }
    }
}
