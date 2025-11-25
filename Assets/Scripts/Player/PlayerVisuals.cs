using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private PlayerCustomSO playerCustomSO;
    [SerializeField] private SpriteRenderer skinSpriteRenderer;
    [SerializeField] private SpriteRenderer hairSpriteRenderer;
    [SerializeField] private SpriteSheetSwapper hairSheetSwapper;
    [SerializeField] private SpriteSheetSwapper clothesSheetSwapper;
    [SerializeField] private List<string> defaultHairSheets;
    [SerializeField] private List<string> defaultClothesSheets;

    private void Start()
    {
        PlayerCustomPrefs.LoadInto(playerCustomSO);
        SetConfig();
    }

    public void SetConfig()
    {
        skinSpriteRenderer.color = playerCustomSO.skinColor;
        hairSpriteRenderer.color = playerCustomSO.hairColor;
        foreach (var hairSheet in defaultHairSheets)
        {
            hairSheetSwapper.BuildSpriteMapping(hairSheet, hairSheet.Remove(hairSheet.Length - 5) + playerCustomSO.hairCode.ToString() + ".png", playerCustomSO.hairCode);
        }

        foreach (var clothesSheet in defaultClothesSheets)
        {
            clothesSheetSwapper.BuildSpriteMapping(clothesSheet, clothesSheet.Remove(clothesSheet.Length - 5) + playerCustomSO.dressCode.ToString() + ".png", playerCustomSO.dressCode);
        }

        hairSheetSwapper.SetCurrentAlt(playerCustomSO.hairCode);
        clothesSheetSwapper.SetCurrentAlt(playerCustomSO.dressCode);
    }
}
