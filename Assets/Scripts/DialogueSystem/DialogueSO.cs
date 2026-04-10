using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
[CreateAssetMenu(fileName = "New Dialogue", menuName = "DialogueSystem/Dialogue")]
public class DialogueSO : NodeGraph
{
    private void OnValidate()
    {
        int startNodes = 0;
        foreach (Node node in nodes)
        {
            if (node is DialogueSegment dialogueSegment)
                if (!dialogueSegment.HasOrigin())
                    startNodes++;
        }
        if (startNodes > 1)
            Debug.LogWarning(name + " dialogue has too many Start Segments, please leave only one.");
        else if (startNodes == 0)
            Debug.LogWarning(name + " dialogue has no Start Segments, please add one segment with no origin segment connected to it.");
    }

    public DialogueSegment getStartSegment()
    {
        foreach (Node node in nodes)
        {
            if (node is DialogueSegment dialogueSegment)
                if (!dialogueSegment.HasOrigin())
                    return dialogueSegment;
        }
        Debug.LogError(name + " dialogue has no Start Segments, please add one segment with no origin segment connected to it.");
        return null;
    }
}