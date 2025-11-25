using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TutorialStatusSO", menuName = "ScriptableObjects/TutorialStatusSO")]
public class TutorialStatus : ScriptableObject
{
    public bool _tutorialIsConcluded;
}
