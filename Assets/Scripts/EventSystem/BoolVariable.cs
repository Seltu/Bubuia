using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBoolVariable", menuName = "ScriptableObjects/BoolVariable", order = 1)]
public class BoolVariable : ScriptableObject
{
    public bool value = true;
}
