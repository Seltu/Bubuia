using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private Button questButton;
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private Image questProgressImage;
    [SerializeField] private GameObject questDescriptionPanel;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private Sprite toDoSprite;
    [SerializeField] private Sprite doingSprite;
    [SerializeField] private Sprite doneSprite;
    private QuestSO _questSO;

    private void Start()
    {
        questButton.onClick.AddListener(ButtonClick);
    }

    private void ButtonClick()
    {
        questDescriptionPanel.SetActive(!questDescriptionPanel.activeSelf);
    }

    internal void SetQuest(QuestSO quest)
    {
        _questSO = quest;
        int progress = GlobalFlagsManager.GetFlag(quest.questFlag);
        questNameText.text = quest.questName + " " + quest.GetProgressString(progress);
        if(progress > 1)
        {
            if(progress < quest.GetTotalProgress()-1)
                questProgressImage.sprite = doingSprite;
            else
                questProgressImage.sprite = doneSprite;
        }
        else
            questProgressImage.sprite = toDoSprite;
        string description = quest.questDesc + "\n\n";
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            QuestObjective obj = quest.objectives[i];
            description += obj.description + " " + quest.GetObjectiveProgress(i, progress) + "\n";
        }
        questDescriptionText.text = description;
    }
}
