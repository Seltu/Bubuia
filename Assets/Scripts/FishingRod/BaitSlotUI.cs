using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaitSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Animator _slotAnimator;

    internal void SetSelected(bool value)
    {
        _slotAnimator.SetBool("Selected", value);
    }

    internal void SetCount(int count)
    {
        _countText.text = count.ToString();
    }
}
