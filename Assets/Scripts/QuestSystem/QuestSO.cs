using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestSO", menuName = "Scriptable Objects/QuestSO")]
public class QuestSO : ScriptableObject
{
    public string questId;
    public string questFlag;
    public string questName;
    public string questDesc;
    public List<QuestObjective> objectives;

    public bool IsCompleted => GlobalFlagsManager.GetFlag(questFlag) >= GetTotalProgress();
    public int CurrentProgress => GlobalFlagsManager.GetFlag(questFlag);

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
        if(progress == 1) return "(0/" + objectives.Count + ")";
        progress--;
        int current = 1;
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

    public int GetObjectiveProgress(int objective)
    {
        int progress = GlobalFlagsManager.GetFlag(questFlag);
        for (int i = 0; i < objectives.Count; i++)
        {
            QuestObjective obj = objectives[i];
            progress -= obj.requiredAmount;
            if (i == objective)
            {
                if (progress <= 0)
                {
                    int objectiveProgress = Math.Max(0, (progress + obj.requiredAmount));
                    return objectiveProgress;
                }
                else
                {
                    return obj.requiredAmount;
                }
            }
        }
        return 0;
    }

    public bool PassedObjective(int objective)
    {
        int progress = GlobalFlagsManager.GetFlag(questFlag);
        for (int i = 0; i < objectives.Count; i++)
        {
            QuestObjective obj = objectives[i];
            progress -= obj.requiredAmount;
            if (i == objective)
            {
                if (progress <= 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
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
}

public enum ObjectiveType { CollectItem, ReachLocation, TalkNPC, Custom, CatchTreasure}