using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestProgressionSO", menuName = "Scriptable Objects/Quest Progression SO")]
public class QuestProgressionSO : ScriptableObject
{
    [SerializeField] private List<QuestSO> quests;

    public List<QuestSO> GetUnlockedQuests()
    {
        List<QuestSO> unlockedQuests = new();

        foreach (QuestSO questEntry in quests)
        {
            int questflag = GlobalFlagsManager.GetFlag(questEntry.questFlag);
            if (questflag > 0 && questflag < questEntry.GetTotalProgress())
                unlockedQuests.Add(questEntry);
        }

        return unlockedQuests;
    }
}
