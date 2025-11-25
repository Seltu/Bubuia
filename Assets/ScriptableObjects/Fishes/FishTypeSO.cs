using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fish Type", menuName = "ScriptableObjects/FishType")]
public class FishTypeSO : ScriptableObject
{
    public string fishName;
    public Sprite fishSprite;
    public Sprite darkFishSprite;
    public string descricao;
    public int valor;
    public int fishingGoalScore;
    public List<FishingPatternWave> fishingPattern;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}

[Serializable]
public struct FishingPatternWave
{
    public float speed;
    public  float secondsDelay;
}