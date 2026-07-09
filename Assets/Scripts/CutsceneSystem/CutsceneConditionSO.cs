using UnityEngine;

public abstract class CutsceneConditionSO : ScriptableObject
{
    public virtual bool CheckCutsceneCondition()
    {
        return true;
    }
}
