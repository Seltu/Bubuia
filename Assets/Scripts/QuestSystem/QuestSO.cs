using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestSO", menuName = "Scriptable Objects/QuestSO")]
public class QuestSO : ScriptableObject
{
    public string questId;
    public string questFlag;
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

    public int GetTotalProgress()
    {
        int total = 1;
        foreach(QuestObjective obj in objectives)
        {
            total += obj.requiredAmount;
        }
        return total;
    }

    public QuestObjective GetCurrentObjective()
    {
        int progress = GlobalFlagsManager.GetFlag(questFlag);
        foreach (QuestObjective obj in objectives)
        {
            progress -= obj.requiredAmount;
            if (progress <= 0)
            {
                return obj;
            }
        }
        return objectives[0];
    }

    public string GetProgressString(int progress)
    {
        progress--;
        int current = 0;
        foreach (QuestObjective obj in objectives)
        {
            progress -= obj.requiredAmount;
            if (progress < 0)
            {
                return "(" + current + "/" + objectives.Count + ")";
            }
            current++;
        }
        return "(" + current + "/" + objectives.Count + ")";
    }

    public string GetObjectiveProgress(int objective, int progress)
    {
        progress--;
        for (int i = 0; i < objectives.Count; i++)
        {
            QuestObjective obj = objectives[i];
            progress -= obj.requiredAmount;
            if (i == objective)
            {
                if (progress <= 0)
                {
                    int objectiveProgress = Math.Max(0, (progress + obj.requiredAmount));
                    return "(" + objectiveProgress + "/" + obj.requiredAmount + ")";
                }
                else
                {
                    return "(" + obj.requiredAmount  + "/" + obj.requiredAmount + ")";
                }
            }
        }
        return "(1/1)";
    }
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveId; // mathc with item id to be collected, npc to be interacted, etc
    public string description;
    public ObjectiveType type;
    public DescriptionDataSO itemData;
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
