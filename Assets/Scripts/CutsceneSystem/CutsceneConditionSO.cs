using UnityEngine;

[CreateAssetMenu(fileName = "New Cutscene Trigger Condition", menuName = "CutsceneSystem/Cutscene Condition")]
public class CutsceneConditionSO : ScriptableObject
{
    public ChoiceCondition condition;
    public virtual bool CheckCutsceneCondition()
    {
        return condition.Decide();
    }
}
