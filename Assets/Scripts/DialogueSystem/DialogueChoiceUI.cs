using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChoiceUI : MonoBehaviour
{
    [SerializeField] private int choice;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;

    public int Choice { get => choice; set => choice = value; }
    public string Text { get => text.text; set => text.text = value; }
    public Button Button { get => button; set => button = value; }
}
