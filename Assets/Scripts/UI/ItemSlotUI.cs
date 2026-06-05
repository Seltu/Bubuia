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
    [SerializeField] private Image itemBorder;
    [SerializeField] private Image itemCountBorder;
    [SerializeField] private TextMeshProUGUI itemCountText;
    private DescriptionDataSO itemData;

    public void SetSlot(InventoryItem item, bool equipped)
    {
        itemData = item.itemData;
        itemIcon.sprite = item.itemData.icon;
        if(equipped)
            itemBorder.color = Color.yellow;
        else
            itemBorder.color = Color.white;
        if (item.amount <= 1)
            itemCountBorder.gameObject.SetActive(false);
        else
            itemCountBorder.gameObject.SetActive(true);
        itemCountText.text = item.amount.ToString();
    }

    public Button GetButton()
    {
        return itemButton;
    }
}
