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
            decision = condition.Decide();
            
            if(condition.itemCondition != null && playerInventory.GetAmount(condition.itemCondition) < condition.itemQuantity)
                decision = false;
        }
        return decision ? (DialogueSegment)GetOutputPort("_yes").Connection.node.GetValue(GetOutputPort("_yes").Connection):
                          (DialogueSegment)GetOutputPort("_no").Connection.node.GetValue(GetOutputPort("_no").Connection);
    }
}

public enum FlagOperator { Equals, Higher, Lower}

[Serializable]
public class ChoiceCondition
{
    public string flagCondition;
    public FlagOperator flagOperator;
    public int flagValue;
    public DescriptionDataSO itemCondition;
    public int itemQuantity;

    public bool Decide()
    {
        bool decision = true;
        switch (flagOperator)
        {
            case FlagOperator.Equals:
                if (flagCondition != "" && GlobalFlagsManager.GetFlag(flagCondition) != flagValue)
                    decision = false;
                break;
            case FlagOperator.Higher:
                if (flagCondition != "" && GlobalFlagsManager.GetFlag(flagCondition) < flagValue)
                    decision = false;
                break;
            case FlagOperator.Lower:
                if (flagCondition != "" && GlobalFlagsManager.GetFlag(flagCondition) > flagValue)
                    decision = false;
                break;
        }
        return decision;
    }
}
