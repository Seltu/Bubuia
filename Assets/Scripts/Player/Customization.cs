using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Customization : MonoBehaviour
{
    [Header("Scriptables")]
    [SerializeField] protected SpriteList _clothesList;
    [SerializeField] protected SpriteList _hairList;
    [SerializeField] protected PlayerCustomSO _playerCustomSO;
    [SerializeField] protected GameObject _playerPrefab;

    [Header("Texts")]
    [SerializeField] protected TMP_Text _typeHair;
    [SerializeField] protected TMP_Text _typeClothes;

    [Header("Images")]
    [SerializeField] protected Image _charBody;
    [SerializeField] protected Image _charHair;
    [SerializeField] protected Image _charClothes;

    [Header("Buttons")]
    [SerializeField] protected Button[] _skins;
    [SerializeField] protected Button[] _hairs;

    protected int _hairIndex;
    protected int _clothesIndex;

    protected virtual void Start()
    {
        _skins[Random.Range(0, _skins.Length)].onClick.Invoke();
        _hairs[Random.Range(0, _skins.Length)].onClick.Invoke();

        _hairIndex = Random.Range(0, _hairList.sprites.Length);
        _typeHair.text = "Tipo " + (_hairIndex + 1);
        _charHair.sprite = _hairList.sprites[_hairIndex];

        _clothesIndex = Random.Range(0, _clothesList.sprites.Length);
        _typeClothes.text = "Tipo " + (_clothesIndex + 1);
        _charClothes.sprite = _clothesList.sprites[_clothesIndex];
    }

    public void HairArrowButton(int leftRight)
    {
        _hairIndex += leftRight;

        if(_hairIndex < 0)
            _hairIndex = _hairList.sprites.Length - 1;
        else if(_hairIndex >= _hairList.sprites.Length)
            _hairIndex = 0;

        _typeHair.text = "Tipo " + (_hairIndex + 1);
        _charHair.sprite = _hairList.sprites[_hairIndex];
    }

    public void ClothesArrowButton(int leftRight)
    {
        _clothesIndex += leftRight;

        if (_clothesIndex < 0)
            _clothesIndex = _clothesList.sprites.Length - 1;
        else if (_clothesIndex >= _clothesList.sprites.Length)
            _clothesIndex = 0;

        _typeClothes.text = "Tipo " + (_clothesIndex + 1);
        _charClothes.sprite = _clothesList.sprites[_clothesIndex];
    }

    public void ChangeSkinColor(Image _spr)
    {
        _charBody.color = _spr.color;
    }

    public void ChangeHairColor(Image _spr)
    {
        _charHair.color = _spr.color;
    }

    public void HiglightHairButton(GameObject press)
    {
        for (int i = 0; i < _hairs.Length; i++)
        {
            if (_hairs[i].gameObject == press)
            {
                _hairs[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                _hairs[i].GetComponent<Image>().color = new Color(1, 1, 1, 0);
            }
        }
    }

    public void HiglightSkinButton(GameObject press)
    {
        for (int i = 0; i < _skins.Length; i++)
        {
            if (_skins[i].gameObject == press)
            {
                _skins[i].GetComponent<Image>().color = new Color(1, 1, 1, 1); 
            }
            else
            {
                _skins[i].GetComponent<Image>().color = new Color(1, 1, 1, 0); 
            }
        }
    }

    public virtual void ConfirmButton()
    {
        _playerCustomSO.skinColor = _charBody.color;
        _playerCustomSO.hairColor = _charHair.color;
        _playerCustomSO.dressCode = _clothesIndex;
        _playerCustomSO.hairCode = _hairIndex;

        PlayerCustomPrefs.Save(_playerCustomSO);

        gameObject.SetActive(false);
    }
}
