using System;

public enum FlagOperator { Equals, Higher, Lower }

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
