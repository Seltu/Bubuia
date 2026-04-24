using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private Button questButton;
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private GameObject questDescriptionPanel;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
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
        string description = quest.questDesc + "\n";
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            QuestObjective obj = quest.objectives[i];
            description += obj.description + " " + quest.GetObjectiveProgress(i, progress) + "\n";
        }
        questDescriptionText.text = description;
    }
}
