using UnityEngine;

public abstract class DescriptionDataSO : ScriptableObject
{
    public string entryName;
    public Sprite icon;
    [TextArea]
    public string description;
}
