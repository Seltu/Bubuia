using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[NodeWidth(365)] [NodeTint(0.5f, 0f, 0f)]
public class ChoiceNode : Node
{
    [Input(backingValue = ShowBackingValue.Never)]
    [SerializeField] private ChoiceNode _origin;

    [Output(connectionType = ConnectionType.Override)]
    [SerializeField] private DialogueSegment _yes;

    [SerializeField] private List<ChoiceCondition> _conditions;

    [Output(connectionType = ConnectionType.Override)]
    [SerializeField] private DialogueSegment _no;

    public override object GetValue(NodePort port)
    {
        return this;
    }

    public DialogueSegment Decide(PlayerInventorySO playerInventory)
    {
        bool decision = true;
        foreach (ChoiceCondition condition in _conditions)
        {
            if(condition.flagCondition != "" && GlobalFlagsManager.GetFlag(condition.flagCondition) != condition.flagValue)
                decision = false;
            if(condition.itemCondition != null && playerInventory.GetAmount(condition.itemCondition) < condition.itemQuantity)
                decision = false;
        }
        return decision ? (DialogueSegment)GetOutputPort("_yes").Connection.node.GetValue(GetOutputPort("_yes").Connection):
                          (DialogueSegment)GetOutputPort("_no").Connection.node.GetValue(GetOutputPort("_no").Connection);
    }
}

[Serializable]
public struct ChoiceCondition
{
    public string flagCondition;
    public bool flagValue;
    public DescriptionDataSO itemCondition;
    public int itemQuantity;
}
