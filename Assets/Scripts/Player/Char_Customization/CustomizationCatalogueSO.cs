using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomizationListSO", menuName = "Scriptable Objects/CustomizationListSO")]
public class CustomizationCatalogueSO : ScriptableObject
{
    /// <summary>
    /// List of ALL clothes and hairs in the game
    /// </summary>

    [Header("Default Player")]
    public PlayerCustomSO defaultPlayer;

    [Header("Hairs")]
    public List<PlayerHairs_SO> hairs;

    [Header("Clothing")]
    public List<PlayerClothesSO_WholeBody> clothing; 
    public List<PlayerClothesSO_UpperBody> upperClothes;
    public List<PlayerClothesSO_LowerBody> lowerClothes;
}
