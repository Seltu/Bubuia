using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestProgressionSO", menuName = "Scriptable Objects/Quest Progression SO")]
public class QuestProgressionSO : ScriptableObject
{
    [SerializeField] private List<ProgressionQuestEntry> quests;

    public List<QuestSO> GetUnlockedQuests()
    {
        List<QuestSO> unlockedQuests = new List<QuestSO>();

        foreach (ProgressionQuestEntry questEntry in quests)
        {
            bool unlocked = true;
            foreach (string prerequisite in questEntry.prerequisiteFlags)
            {
                if (!GlobalFlagsManager.GetFlag(prerequisite))
                    unlocked = false;
            }
            if(unlocked)
                unlockedQuests.Add(questEntry.quest);
        }

        return unlockedQuests;
    }

    public bool IsQuestUnlocked(QuestSO quest)
    {
        bool unlocked = true;
        ProgressionQuestEntry questEntry = quests.Find(x => x.quest == quest);
        foreach (string prerequisite in questEntry.prerequisiteFlags)
        {
            if (!GlobalFlagsManager.GetFlag(prerequisite))
                unlocked = false;
        }
        return unlocked;
    }
}
