using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Button itemButton;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemCountText;
    private FishTypeSO fishInfo;
    private BaitTypeSO baitInfo;

    public void SetSlot(PlayerFishes fish)
    {
        fishInfo = fish.fishType;
        itemIcon.sprite = fishInfo.fishSprite;
        itemCountText.text = "x" + fish.fishNum.ToString();
    }

    public void SetSlot(PlayerBait bait)
    {
        baitInfo = bait.baitType;
        itemIcon.sprite = baitInfo.baitSprite;
        itemCountText.text = "x" + bait.baitNum.ToString();
    }

    public Button GetButton()
    {
        return itemButton;
    }
}
