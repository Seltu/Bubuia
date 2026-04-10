using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProgressionQuestEntry
{
    public QuestSO quest;
    public List<string> prerequisiteFlags;
}