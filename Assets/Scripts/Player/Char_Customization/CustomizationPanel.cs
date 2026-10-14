using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomizationPanel : MonoBehaviour
{
    [SerializeField] private PlayerCustomSO _playerCustomization;
    [SerializeField] private CharCustomization _charCustom;
    [SerializeField] private CustomizationCatalogueSO _customizationList;

    [Header("First Customization Checker")]
    [SerializeField] private bool _isPlayerFirstCustomization = false;
    [SerializeField] private string _gameSceneName;
    [SerializeField] private PlayerCustomSO _defaultPlayerCustom;

    [Header("Player Name")]
    [SerializeField] private TMP_InputField _playerNameTxt;

    [Header("Texts")]
    [SerializeField] private TMP_Text _clothingOptionTxt;
    [SerializeField] private TMP_Text _hairOptionTxt;

    [Header("Unified Sprites for Coloring")]
    [SerializeField] private SpriteRenderer[] _skinParts;
    [SerializeField] private SpriteRenderer[] _hairParts;
    private string _tempSkinColor;
    private string _tempHairColor;

    [Header("Color Buttons")]
    [SerializeField] private Image[] _skinColorButtons;
    [SerializeField] private Image[] _hairColorButtons;

    // The Ravi Char / Player Char in the custom panel shall be a copy just for the panel,
    // so that player char is not changed unless confirmed with the confirm new customization butotn
    [Header("CUSTOM PANEL PLAYER CHARACTER")]
    [Header("Hair Parts")]
    [SerializeField] private SpriteRenderer _franja;
    [SerializeField] private SpriteRenderer _cabelo;
    [SerializeField] private SpriteRenderer _pelo4;
    [SerializeField] private SpriteRenderer _pelo3;
    [SerializeField] private SpriteRenderer _pelo1;
    [SerializeField] private SpriteRenderer _pelo2;

    [Header("Eyes")]
    [SerializeField] private SpriteRenderer[] _eyes;
    private string _tempEyeColor;

    [Header("CLOTHED BODY PARTS - UPPER BODY")]
    [Header("Braco Esquerdo")]
    [SerializeField] private SpriteRenderer _bracoE;
    [SerializeField] private SpriteRenderer _mangaE;
    [SerializeField] private SpriteRenderer _maoE;

    [Header("Braco Direito")]
    [SerializeField] private SpriteRenderer _maoD;
    [SerializeField] private SpriteRenderer _bracoD;
    [SerializeField] private SpriteRenderer _golaD;

    [Header("Blusa")]
    [SerializeField] private SpriteRenderer _blusa;
    [SerializeField] private SpriteRenderer _roupa2;
    [SerializeField] private SpriteRenderer _tronco;

    [Header("CLOTHED BODY PARTS - LOWER BODY")]
    [Header("Base")]
    [SerializeField] private SpriteRenderer _base;

    [Header("Perna Direita")]
    [SerializeField] private SpriteRenderer _shortD;
    [SerializeField] private SpriteRenderer _pernaD;
    [SerializeField] private SpriteRenderer _peD;

    [Header("Perna Esquerda")]
    [SerializeField] private SpriteRenderer _shortE;
    [SerializeField] private SpriteRenderer _peE;
    [SerializeField] private SpriteRenderer _pernaE;

    private string _tempHairCode;
    private string _tempWholeBodyClothes;
    private string _tempUpperBodyClothes;
    private string _tempLowerBodyClothes;

    private int _wholeBodyClothesIndex;
    private int _upperBodyClothesIndex;
    private int _lowerBodyClothesIndex;
    private int _hairsIndex;


    private void OnEnable()
    {
        if(_isPlayerFirstCustomization)
        {
            PlayerCustomPrefs.Save(_defaultPlayerCustom);
            SetCustomizationValues(_defaultPlayerCustom);
        }

        SetTemporaryCustomizationValues();
        SetPanelTextValues();
        SetPreviewColorValues();
        SetDefaultHairOption();
        SetDefaultClothingOption();

        _charCustom.ReloadCharacterCustomization();
    }

    #region Color Buttons
    public void HairColorButtonPressed(Image button)
    {
        Color newCol = button.color;
        foreach (SpriteRenderer part in _hairParts)
        {
            part.color = newCol;
            _tempHairColor = "#" + ColorUtility.ToHtmlStringRGBA(newCol);
        }

        SetColorButtonHighlights(_hairColorButtons, newCol);
    }

    public void SkinColorButtonPressed(Image button)
    {
        Color newCol = button.color;
        foreach (SpriteRenderer part in _skinParts)
        {
            part.color = newCol;
            _tempSkinColor = "#" + ColorUtility.ToHtmlStringRGBA(newCol);
        }

        SetColorButtonHighlights(_skinColorButtons, newCol);
    }

    #endregion

    #region Arrow Buttons

    public void ClothingButton(int dir)
    {
        if (MoveClothingIndex(ref _wholeBodyClothesIndex, dir, _customizationList.clothing.Count))
        {
            ChangeShownClothing();
        }
    }

    public void UpperClothingButton(int dir)
    {
        if (MoveClothingIndex(ref _upperBodyClothesIndex, dir, _customizationList.upperClothes.Count))
        {
            ChangeUpperClothing();
        }
    }

    public void LowerClothingButton(int dir)
    {
        if (MoveClothingIndex(ref _lowerBodyClothesIndex, dir, _customizationList.lowerClothes.Count))
        {
            ChangeLowerClothing();
        }
    }

    public void HairButtons(int dir)
    {
        if (MoveHairIndex(dir))
        {
            ChangeShownHair();
        }
    }

    #endregion

    #region Change Shown Clothing

    private void ChangeShownClothing()
    {
        ChangeWholeClothing();
    }

    private void ChangeWholeClothing()
    {
        PlayerClothesSO_WholeBody clothes = _customizationList.clothing[_wholeBodyClothesIndex];

        SetUpperBodyClothingSprites(
            clothes.bracoE,
            clothes.mangaE,
            clothes.maoE,
            clothes.maoD,
            clothes.bracoD,
            clothes.golaD,
            clothes.blusa,
            clothes.roupa2,
            clothes.tronco);

        SetLowerBodyClothingSprites(
            clothes.shortBase,
            clothes.shortD,
            clothes.pernaD,
            clothes.peD,
            clothes.shortE,
            clothes.peE,
            clothes.pernaE);

        _clothingOptionTxt.text = clothes.clothingName;
        _tempWholeBodyClothes = clothes.clothingName;
        _tempUpperBodyClothes = string.Empty;
        _tempLowerBodyClothes = string.Empty;
    }

    private void ChangeUpperClothing()
    {
        PlayerClothesSO_UpperBody clothes = _customizationList.upperClothes[_upperBodyClothesIndex];

        SetUpperBodyClothingSprites(
            clothes.bracoE,
            clothes.mangaE,
            clothes.maoE,
            clothes.maoD,
            clothes.bracoD,
            clothes.golaD,
            clothes.blusa,
            clothes.roupa2,
            clothes.tronco);

        _tempWholeBodyClothes = string.Empty;
        _tempUpperBodyClothes = clothes.torsoClothingName;
        _clothingOptionTxt.text = clothes.torsoClothingName;
    }

    private void ChangeLowerClothing()
    {
        PlayerClothesSO_LowerBody clothes = _customizationList.lowerClothes[_lowerBodyClothesIndex];

        SetLowerBodyClothingSprites(
            clothes.shortBase,
            clothes.shortD,
            clothes.pernaD,
            clothes.peD,
            clothes.shortE,
            clothes.peE,
            clothes.pernaE);

        _tempWholeBodyClothes = string.Empty;
        _tempLowerBodyClothes = clothes.legsClothingName;
        _clothingOptionTxt.text = clothes.legsClothingName;
    }

    #endregion

    #region Change Shown Hair
    private void ChangeShownHair()
    {
        PlayerHairs_SO hair = _customizationList.hairs[_hairsIndex];

        SetSpriteIfAvailable(_franja, hair.franja);
        SetSpriteIfAvailable(_cabelo, hair.cabelo);
        SetSpriteIfAvailable(_pelo4, hair.pelo4);
        SetSpriteIfAvailable(_pelo3, hair.pelo3);
        SetSpriteIfAvailable(_pelo1, hair.pelo1);
        SetSpriteIfAvailable(_pelo2, hair.pelo2);

        _hairOptionTxt.text = hair.hairName;
        _tempHairCode = hair.hairName;
    }

    #endregion

    private void SaveCustomizationChoices()
    {
        _playerCustomization.playerName = _playerNameTxt.text;

        if (_tempSkinColor != null)
        {
            _playerCustomization.skinColor = HexToColor(_tempSkinColor);
        }

        if (_tempEyeColor != null)
        {
            _playerCustomization.eyesColor = HexToColor(_tempEyeColor);
        }

        if (_tempHairColor != null)
        {
            _playerCustomization.hairColor = HexToColor(_tempHairColor);
        }

        SetStringIfAvailable(ref _playerCustomization.hairCode, _tempHairCode);

        SetStringIfAvailable(ref _playerCustomization.dressCode, _tempWholeBodyClothes);
        SetStringIfAvailable(ref _playerCustomization.upperDressCode, _tempUpperBodyClothes);
        SetStringIfAvailable(ref _playerCustomization.lowerDressCode, _tempLowerBodyClothes);

        PlayerCustomPrefs.Save(_playerCustomization);
    }

    #region Helpers
    private void SetCustomizationValues(PlayerCustomSO sourceCustomization)
    {
        _playerCustomization.playerName = sourceCustomization.playerName;
        _playerCustomization.skinColor = sourceCustomization.skinColor;
        _playerCustomization.eyesColor = sourceCustomization.eyesColor;
        _playerCustomization.hairColor = sourceCustomization.hairColor;
        _playerCustomization.hairCode = sourceCustomization.hairCode;
        _playerCustomization.dressCode = sourceCustomization.dressCode;
        _playerCustomization.upperDressCode = sourceCustomization.upperDressCode;
        _playerCustomization.lowerDressCode = sourceCustomization.lowerDressCode;
    }

    private void SetTemporaryCustomizationValues()
    {
        _tempSkinColor = "#" + ColorUtility.ToHtmlStringRGBA(_playerCustomization.skinColor);
        _tempEyeColor = "#" + ColorUtility.ToHtmlStringRGBA(_playerCustomization.eyesColor);
        _tempHairColor = "#" + ColorUtility.ToHtmlStringRGBA(_playerCustomization.hairColor);
        _tempHairCode = _playerCustomization.hairCode;
        _tempWholeBodyClothes = _playerCustomization.dressCode;
        _tempUpperBodyClothes = _playerCustomization.upperDressCode;
        _tempLowerBodyClothes = _playerCustomization.lowerDressCode;
    }

    private void SetPanelTextValues()
    {
        _playerNameTxt.text = _playerCustomization.playerName;
        _clothingOptionTxt.text = string.Empty;
        _hairOptionTxt.text = string.Empty;
    }

    private void SetPreviewColorValues()
    {
        for (int i = 0; i < _skinParts.Length; i++)
        {
            _skinParts[i].color = _playerCustomization.skinColor;
        }

        for (int i = 0; i < _hairParts.Length; i++)
        {
            _hairParts[i].color = _playerCustomization.hairColor;
        }

        for (int i = 0; i < _eyes.Length; i++)
        {
            _eyes[i].color = _playerCustomization.eyesColor;
        }

        SetColorButtonHighlights(_skinColorButtons, _playerCustomization.skinColor);
        SetColorButtonHighlights(_hairColorButtons, _playerCustomization.hairColor);
    }

    private void SetDefaultHairOption()
    {
        if (_customizationList.hairs.Count <= 0) return;

        _hairsIndex = FindHairIndex(_playerCustomization.hairCode);

        ChangeShownHair();
    }

    private int FindHairIndex(string hairName)
    {
        int SavedHairIndex;
        if (int.TryParse(hairName, out SavedHairIndex))
        {
            if (SavedHairIndex >= 0 && SavedHairIndex < _customizationList.hairs.Count)
            {
                return SavedHairIndex;
            }
        }

        for (int i = 0; i < _customizationList.hairs.Count; i++)
        {
            if (_customizationList.hairs[i].hairName == hairName)
            {
                return i;
            }
        }

        return 0;
    }

    private void SetDefaultClothingOption()
    {
        if (!string.IsNullOrEmpty(_playerCustomization.dressCode) && _customizationList.clothing.Count > 0)
        {
            _wholeBodyClothesIndex = FindWholeBodyClothingIndex(_playerCustomization.dressCode);
            ChangeWholeClothing();
            return;
        }

        bool HasUpperBodyClothes = !string.IsNullOrEmpty(_playerCustomization.upperDressCode);
        bool HasLowerBodyClothes = !string.IsNullOrEmpty(_playerCustomization.lowerDressCode);
        string UpperBodyClothesName = string.Empty;
        string LowerBodyClothesName = string.Empty;

        if (HasUpperBodyClothes && _customizationList.upperClothes.Count > 0)
        {
            _upperBodyClothesIndex = FindUpperBodyClothingIndex(_playerCustomization.upperDressCode);
            ChangeUpperClothing();
            UpperBodyClothesName = _tempUpperBodyClothes;
        }

        if (HasLowerBodyClothes && _customizationList.lowerClothes.Count > 0)
        {
            _lowerBodyClothesIndex = FindLowerBodyClothingIndex(_playerCustomization.lowerDressCode);
            ChangeLowerClothing();
            LowerBodyClothesName = _tempLowerBodyClothes;
        }

        if (UpperBodyClothesName != string.Empty && LowerBodyClothesName != string.Empty)
        {
            _clothingOptionTxt.text = UpperBodyClothesName + " / " + LowerBodyClothesName;
        }
        else if (!HasUpperBodyClothes && !HasLowerBodyClothes && _customizationList.clothing.Count > 0)
        {
            _wholeBodyClothesIndex = 0;
            ChangeWholeClothing();
        }
    }

    private int FindWholeBodyClothingIndex(string clothingName)
    {
        for (int i = 0; i < _customizationList.clothing.Count; i++)
        {
            if (_customizationList.clothing[i].clothingName == clothingName)
            {
                return i;
            }
        }

        return 0;
    }

    private int FindUpperBodyClothingIndex(string clothingName)
    {
        for (int i = 0; i < _customizationList.upperClothes.Count; i++)
        {
            if (_customizationList.upperClothes[i].torsoClothingName == clothingName)
            {
                return i;
            }
        }

        return 0;
    }

    private int FindLowerBodyClothingIndex(string clothingName)
    {
        for (int i = 0; i < _customizationList.lowerClothes.Count; i++)
        {
            if (_customizationList.lowerClothes[i].legsClothingName == clothingName)
            {
                return i;
            }
        }

        return 0;
    }

    private bool MoveClothingIndex(ref int clothingIndex, int direction, int optionCount)
    {
        if (optionCount <= 0) return false;

        clothingIndex += direction;

        if (clothingIndex >= optionCount)
        {
            clothingIndex = 0;
        }
        else if (clothingIndex < 0)
        {
            clothingIndex = optionCount - 1;
        }

        return true;
    }

    private bool MoveHairIndex(int direction)
    {
        int HairCount = _customizationList.hairs.Count;
        if (HairCount <= 0) return false;

        _hairsIndex += direction;

        if (_hairsIndex >= HairCount)
        {
            _hairsIndex = 0;
        }
        else if (_hairsIndex < 0)
        {
            _hairsIndex = HairCount - 1;
        }

        return true;
    }

    private void SetColorButtonHighlights(Image[] colorButtons, Color selectedColor)
    {
        foreach (Image ButtonImage in colorButtons)
        {
            Transform ButtonParent = ButtonImage.transform.parent;
            if (ButtonParent == null) continue;

            Image HighlightImage = ButtonParent.GetComponent<Image>();
            if (HighlightImage == null) continue;

            if (selectedColor == ButtonImage.color)
            {
                HighlightImage.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                HighlightImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    private void SetUpperBodyClothingSprites(
        Sprite leftArm,
        Sprite leftSleeve,
        Sprite leftHand,
        Sprite rightHand,
        Sprite rightArm,
        Sprite rightCollar,
        Sprite shirt,
        Sprite secondClothingLayer,
        Sprite torso)
    {
        SetSpriteIfAvailable(_bracoE, leftArm);
        SetSpriteIfAvailable(_mangaE, leftSleeve);
        SetSpriteIfAvailable(_maoE, leftHand);
        SetSpriteIfAvailable(_maoD, rightHand);
        SetSpriteIfAvailable(_bracoD, rightArm);
        SetSpriteIfAvailable(_golaD, rightCollar);
        SetSpriteIfAvailable(_blusa, shirt);
        SetSpriteIfAvailable(_roupa2, secondClothingLayer);
        SetSpriteIfAvailable(_tronco, torso);
    }

    private void SetLowerBodyClothingSprites(
        Sprite baseShorts,
        Sprite rightShorts,
        Sprite rightLeg,
        Sprite rightFoot,
        Sprite leftShorts,
        Sprite leftFoot,
        Sprite leftLeg)
    {
        SetSpriteIfAvailable(_base, baseShorts);
        SetSpriteIfAvailable(_shortD, rightShorts);
        SetSpriteIfAvailable(_pernaD, rightLeg);
        SetSpriteIfAvailable(_peD, rightFoot);
        SetSpriteIfAvailable(_shortE, leftShorts);
        SetSpriteIfAvailable(_peE, leftFoot);
        SetSpriteIfAvailable(_pernaE, leftLeg);
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var col))
            return col;
        return Color.white;
    }

    private void SetStringIfAvailable(ref string targetString, string temp)
    {
        if (temp != null)
        {
            targetString = temp;
        }
    }
    private void SetSpriteIfAvailable(SpriteRenderer targetRenderer, Sprite sprite)
    {
        if (sprite != null)
        {
            targetRenderer.sprite = sprite;
        }
    }
    #endregion

    public void ConfirmButton()
    {
        SaveCustomizationChoices();

        if (_isPlayerFirstCustomization && _gameSceneName != string.Empty)
        {
            SceneManager.LoadScene(_gameSceneName);
        }
        else
        {
            _charCustom.ReloadCharacterCustomization();
        }

        gameObject.SetActive(false);
    }


}
