using UnityEngine;

[CreateAssetMenu(fileName = "New Cutscene Trigger Condition", menuName = "CutsceneSystem/Cutscene Condition")]
public class CutsceneConditionSO : ScriptableObject
{
    public ChoiceCondition condition;
    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
    public virtual bool CheckCutsceneCondition()
    {
        return condition.Decide();
    }
}
