using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [SerializeField] private QuestProgressionSO questProgressionSO;
    [SerializeField] private Transform questsParent;
    [SerializeField] private QuestUI questPrefab;
    private List<QuestStruct> _trackedQuests = new();

    private void Start()
    {
        foreach (Transform child in questsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (QuestSO quest in questProgressionSO.GetUnlockedQuests())
        {
            QuestStruct trackedQuest = new();
            var questUI = Instantiate(questPrefab, questsParent);
            questUI.SetQuest(quest);
            trackedQuest.data = quest;
            trackedQuest.ui = questUI;
            _trackedQuests.Add(trackedQuest);
        }

        EventManager.AddListener<DescriptionDataSO, int>("OnAddItem", CheckQuestItem);
        EventManager.AddListener<TreasureSpot>("TreasureCaught", CheckCaughtTreasure);
        EventManager.AddListener("FishCaught", CheckCaughtFish);
        EventManager.AddListener("EndDialogue", UpdateQuests);
    }

    private void CheckCaughtFish()
    {
        foreach (QuestStruct quest in _trackedQuests)
        {
            QuestObjective currentObjective = quest.data.GetCurrentObjective();
            if (currentObjective.type == ObjectiveType.CatchFish)
            {
                GlobalFlagsManager.SetFlag(quest.data.questFlag, GlobalFlagsManager.GetFlag(quest.data.questFlag) + 1);
            }
        }
        UpdateQuests();
    }

    private void CheckQuestItem(DescriptionDataSO item, int amount)
    {
        foreach (QuestStruct quest in _trackedQuests)
        {
            QuestObjective currentObjective = quest.data.GetCurrentObjective();
            if (currentObjective.type == ObjectiveType.CollectItem)
            {
                if(currentObjective.itemData == item)
                {
                    GlobalFlagsManager.SetFlag(quest.data.questFlag, GlobalFlagsManager.GetFlag(quest.data.questFlag) + 1);
                    //Later do logic for collecting multiple of the same item if necessary
                }
            }
        }
        UpdateQuests();
    }

    private void CheckCaughtTreasure(TreasureSpot treasure)
    {
        foreach (QuestStruct quest in _trackedQuests)
        {
            QuestObjective currentObjective = quest.data.GetCurrentObjective();
            if (currentObjective.type == ObjectiveType.CatchTreasure)
            {
                    GlobalFlagsManager.SetFlag(quest.data.questFlag, GlobalFlagsManager.GetFlag(quest.data.questFlag) + 1);
            }
        }
        UpdateQuests();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<DescriptionDataSO, int>("OnAddItem", CheckQuestItem);
        EventManager.RemoveListener<TreasureSpot>("TreasureCaught", CheckCaughtTreasure);
        EventManager.RemoveListener("FishCaught", CheckCaughtFish);
        EventManager.RemoveListener("EndDialogue", UpdateQuests);
    }

    private void UpdateQuests()
    {
        int position = 0;
        foreach (QuestSO quest in questProgressionSO.GetUnlockedQuests())
        {
            if (!_trackedQuests.Exists(x=>x.data==quest))
            {
                QuestStruct trackedQuest = new();
                var questUI = Instantiate(questPrefab, questsParent);
                questUI.transform.SetSiblingIndex(position);
                questUI.SetQuest(quest);
                trackedQuest.data = quest;
                trackedQuest.ui = questUI;
                _trackedQuests.Add(trackedQuest);
            }
            position++;
        }
        foreach (QuestStruct quest in _trackedQuests)
        {
            quest.ui.SetQuest(quest.data);
            if (GlobalFlagsManager.GetFlag(quest.data.questFlag) >= quest.data.GetTotalProgress())
            {
                Destroy(quest.ui.gameObject); //Later change this to play animation upon completion
            }
        }
        EventManager.TriggerEvent("QuestsUpdated");
    }

    private struct QuestStruct
    {
        public QuestSO data;
        public QuestUI ui;
    }
}
