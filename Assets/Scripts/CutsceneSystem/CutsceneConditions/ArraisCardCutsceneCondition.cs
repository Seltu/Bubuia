using UnityEngine;

[CreateAssetMenu(fileName = "New Cutscene Trigger Condition", menuName = "CutsceneSystem/Arrais Card Cutscene Condition")]
public class ArraisCardCutsceneCondition : CutsceneConditionSO
{
    public override bool CheckCutsceneCondition()
    {
        if(GlobalFlagsManager.GetFlag("TutorialFishing2") >= 5)
            return true;
        else
            return false;
    }
}
