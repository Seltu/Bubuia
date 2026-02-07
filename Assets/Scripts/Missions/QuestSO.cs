using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestSO", menuName = "Scriptable Objects/QuestSO")]
public class QuestSO : ScriptableObject
{
    public string questId;
    public string questName;
    public string questDesc;
    public List<QuestObjective> objectives;

    private void OnValidate()
    {
        if(string.IsNullOrEmpty(questId))
        {
            questId = questName + Guid.NewGuid().ToString();
        }
    }
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveId; // mathc with item id to be collected, npc to be interacted, etc
    public string description;
    public ObjectiveType type;
    public int requiredAmount;
    public int currentAmount;

    public bool isCompleted => currentAmount >= requiredAmount;
}

public enum ObjectiveType { CollectItem, ReachLocation, TalkNPC, Custom }

[System.Serializable]
public class QuestProgress
{
    public QuestSO quest;
    public List<QuestObjective> objectives;

    public QuestProgress(QuestSO quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();

        // deep copy to not modify original list
        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveId = obj.objectiveId,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.isCompleted);
    public string QuestID => quest.questId;

}
