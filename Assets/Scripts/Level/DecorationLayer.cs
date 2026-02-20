using System.Collections.Generic;
using UnityEngine;
public enum DecorationLayerType
{
    Ground,
    Underwater,
    WaterSurface
}


[System.Serializable]
public class DecorationLayer
{
    public DecorationLayerType type;
    public float density;
    public List<GameObject> prefabs;
    public float minDistance;
}
