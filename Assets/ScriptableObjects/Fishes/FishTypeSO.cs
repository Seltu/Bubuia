using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FishSize { Small, Medium, Large }

[CreateAssetMenu(fileName = "Fish Type", menuName = "ScriptableObjects/FishType")]
public class FishTypeSO : DescriptionDataSO
{
    public Sprite darkFishSprite;
    public int valor;
    public int fishingGoalScore;
    public FishSize sizeCategory;
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