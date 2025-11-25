using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetCurrentClothes : Customization
{
    protected override void Start()
    {
        PlayerCustomPrefs.LoadInto(_playerCustomSO);
        SetCurrentPlayerCustom();
    }

    private void OnEnable()
    {
        PlayerCustomPrefs.LoadInto(_playerCustomSO);
        SetCurrentPlayerCustom();
    }

    private void SetCurrentPlayerCustom()
    {
        _charBody.color = _playerCustomSO.skinColor;
        _charHair.color = _playerCustomSO.hairColor;

        _hairIndex = _playerCustomSO.hairCode;
        _charHair.sprite = _hairList.sprites[_hairIndex];
        _typeHair.text = "Tipo " + (_hairIndex + 1);

        _clothesIndex = _playerCustomSO.dressCode;
        _charClothes.sprite = _clothesList.sprites[_clothesIndex];
        _typeClothes.text = "Tipo " + (_clothesIndex + 1);

        foreach (Button skinBtn in _skins)
        {
            Image img = skinBtn.GetComponentInChildren<Image>();
            if (ColorsAreSimilar(img.color, _playerCustomSO.skinColor))
            {
                HiglightSkinButton(skinBtn.gameObject);
                break;
            }
        }

        foreach (Button hairBtn in _hairs)
        {
            Image img = hairBtn.GetComponentInChildren<Image>();
            if (ColorsAreSimilar(img.color, _playerCustomSO.hairColor))
            {
                HiglightHairButton(hairBtn.gameObject);
                break;
            }
        }
    }

    private bool ColorsAreSimilar(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    public override void ConfirmButton()
    {
        Debug.Log("confirm");
        _playerCustomSO.skinColor = _charBody.color;
        _playerCustomSO.hairColor = _charHair.color;
        _playerCustomSO.dressCode = _clothesIndex;
        _playerCustomSO.hairCode = _hairIndex;

        PlayerCustomPrefs.Save(_playerCustomSO);

        _playerPrefab.GetComponent<PlayerVisuals>().SetConfig();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerVisuals>().SetConfig();

        EventManager.TriggerEvent("OnCloseStore");
    }
}
