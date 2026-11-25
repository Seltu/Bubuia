using UnityEngine;
using UnityEngine.UI;

public class CharCustomization : MonoBehaviour
{
    [SerializeField] private PlayerCustomSO _playerCustomization;
    [SerializeField] private CustomizationCatalogueSO _clothingList;
    private PlayerClothesSO_WholeBody _playerClothes;
    private PlayerClothesSO_UpperBody _upperBodyClothing;
    private PlayerClothesSO_LowerBody _lowerBodyClothing;
    private PlayerHairs_SO _playerHair;

    [Header("Player Colorable Parts")]
    [SerializeField] private SpriteRenderer[] _playerBodyParts;
    [SerializeField] private SpriteRenderer[] _playerHairParts;
    [SerializeField] private SpriteRenderer[] _playerEyes;

    [Header("Hair Parts")]
    [SerializeField] private SpriteRenderer _franja;
    [SerializeField] private SpriteRenderer _cabelo;
    [SerializeField] private SpriteRenderer _pelo4;
    [SerializeField] private SpriteRenderer _pelo3;
    [SerializeField] private SpriteRenderer _pelo1;
    [SerializeField] private SpriteRenderer _pelo2;

    [Header("CLOTHED BODY PARTS - UPPER BODY")]
    [Header("Bra�o Esquerdo")]
    [SerializeField] private SpriteRenderer _bracoE;
    [SerializeField] private SpriteRenderer _mangaE;
    [SerializeField] private SpriteRenderer _maoE;

    [Header("Bra�o Direito")]
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


    private void Awake()
    {
        CheckCustomizationInitialized();
    }

    private void OnEnable()
    {
        CheckCustomizationInitialized();

        ApplyUpperBodyClothes();
        ApplyLowerBodyClothes();

        if (_playerCustomization != null)
        {
            ReloadCharacterCustomization();
        }
    }

    public void ReloadCharacterCustomization()
    {
        SetCustomizationInfo();

        ApplySkinColor();
        
        ApplyEyesColor();

        ApplyHairColor();
        ApplyHair();

        ApplyUpperBodyClothes();
        ApplyLowerBodyClothes();
    }

    #region Get Customization Info from Memory

    private void CheckCustomizationInitialized()
    {
        if (_playerCustomization == null) return;
        if (_playerCustomization.playerCustomizationLoaded) return;

        PlayerCustomPrefs.LoadInto(_playerCustomization);
        _playerCustomization.playerCustomizationLoaded = true;

        SetCustomizationInfo();
    }

    private void SetCustomizationInfo()
    {
        SetHairInfo();
        SetWholeBodyClothingInfo();
        SetUpperBodyClothingInfo();
        SetLowerBodyClothingInfo();
    }

    private void SetHairInfo()
    {
        _playerHair = null;

        if (_playerCustomization.hairCode == string.Empty) return;

        int HairIndex;
        if (int.TryParse(_playerCustomization.hairCode, out HairIndex))
        {
            if (HairIndex >= 0 && HairIndex < _clothingList.hairs.Count)
            {
                _playerHair = _clothingList.hairs[HairIndex];
                return;
            }
        }

        _playerHair = _clothingList.hairs.Find((x) => x.hairName == _playerCustomization.hairCode);
    }

    private void SetWholeBodyClothingInfo()
    {
        _playerClothes = null;

        if (_playerCustomization.dressCode == string.Empty) return;

        _playerClothes = _clothingList.clothing.Find((x) => x.clothingName == _playerCustomization.dressCode);
    }

    private void SetUpperBodyClothingInfo()
    {
        _upperBodyClothing = null;

        if (_playerCustomization.upperDressCode == string.Empty) return;

        _upperBodyClothing = _clothingList.upperClothes.Find((x) => x.torsoClothingName == _playerCustomization.upperDressCode);
    }

    private void SetLowerBodyClothingInfo()
    {
        _lowerBodyClothing = null;

        if (_playerCustomization.lowerDressCode == string.Empty) return;

        _lowerBodyClothing = _clothingList.lowerClothes.Find((x) => x.legsClothingName == _playerCustomization.lowerDressCode);
    }

    #endregion

    #region Apply Color 
    private void ApplySkinColor()
    {
        foreach(SpriteRenderer part in _playerBodyParts)
        {
            part.color = _playerCustomization.skinColor;
        }
    }

    private void ApplyEyesColor()
    {
        foreach(SpriteRenderer part in _playerEyes)
        {
            part.color = _playerCustomization.eyesColor;
        }
    }

    private void ApplyHairColor()
    {
        foreach(SpriteRenderer part in _playerHairParts)
        {
            part.color = _playerCustomization.hairColor;
        }
    }
    #endregion

    #region Apply Hair

    private void ApplyHair()
    {
        if (_playerCustomization == null) return;
        if (_playerHair == null) return;

        SetSpriteIfAvailable(_franja, _playerHair.franja);
        SetSpriteIfAvailable(_cabelo, _playerHair.cabelo);
        SetSpriteIfAvailable(_pelo4, _playerHair.pelo4);
        SetSpriteIfAvailable(_pelo3, _playerHair.pelo3);
        SetSpriteIfAvailable(_pelo1, _playerHair.pelo1);
        SetSpriteIfAvailable(_pelo2, _playerHair.pelo2);
    }

    #endregion

    #region Apply Upper Body Clothes

    private void ApplyUpperBodyClothes()
    {
        ApplyLeftArmClothes();
        ApplyRightArmClothes();
        ApplyTorsoClothes();
    }

    private void ApplyLeftArmClothes()
    {
        if(_playerCustomization == null) return;
        if (_playerClothes == null && _upperBodyClothing == null) return;

        if(_playerClothes != null)
        { 
            SetSpriteIfAvailable(_bracoE, _playerClothes.bracoE);
            SetSpriteIfAvailable(_mangaE, _playerClothes.mangaE);
            SetSpriteIfAvailable(_maoE, _playerClothes.maoE);
        }
        else if(_upperBodyClothing != null)
        {
            SetSpriteIfAvailable(_bracoE, _upperBodyClothing.bracoE);
            SetSpriteIfAvailable(_mangaE, _upperBodyClothing.mangaE);
            SetSpriteIfAvailable(_maoE, _upperBodyClothing.maoE);
        }
    }

    private void ApplyRightArmClothes()
    {
        if(_playerCustomization == null) return;
        if (_playerClothes == null && _upperBodyClothing == null) return;

        if(_playerClothes != null)
        {
            SetSpriteIfAvailable(_maoD, _playerClothes.maoD);
            SetSpriteIfAvailable(_bracoD, _playerClothes.bracoD);
            SetSpriteIfAvailable(_golaD, _playerClothes.golaD);
        }
        else if(_upperBodyClothing != null)
        {
            SetSpriteIfAvailable(_maoD, _upperBodyClothing.maoD);
            SetSpriteIfAvailable(_bracoD, _upperBodyClothing.bracoD);
            SetSpriteIfAvailable(_golaD, _upperBodyClothing.golaD);
        }
    }

    private void ApplyTorsoClothes()
    {
        if (_playerCustomization == null) return;
        if (_playerClothes == null && _upperBodyClothing == null) return;

        if(_playerClothes != null)
        {
            SetSpriteIfAvailable(_blusa, _playerClothes.blusa);
            SetSpriteIfAvailable(_roupa2, _playerClothes.roupa2);
            SetSpriteIfAvailable(_tronco, _playerClothes.tronco);
        }
        else if(_upperBodyClothing != null)
        {
            SetSpriteIfAvailable(_blusa, _upperBodyClothing.blusa);
            SetSpriteIfAvailable(_roupa2, _upperBodyClothing.roupa2);
            SetSpriteIfAvailable(_tronco, _upperBodyClothing.tronco);
        }
    }

    #endregion

    #region Apply Lower Body Clothes

    private void ApplyLowerBodyClothes()
    {
        ApplyBaseClothes();
        ApplyRightLegClothes();
        ApplyLeftLegClothes();
    }

    private void ApplyBaseClothes()
    {
        if (_playerCustomization == null) return;
        if (_playerClothes == null && _lowerBodyClothing == null) return;

        if(_playerClothes != null)
        {
            SetSpriteIfAvailable(_base, _playerClothes.shortBase);
        }
        else if(_lowerBodyClothing != null)
        {
            SetSpriteIfAvailable(_base, _lowerBodyClothing.shortBase);
        }
    }

    private void ApplyRightLegClothes()
    {
        if (_playerCustomization == null) return;
        if (_playerClothes == null && _lowerBodyClothing == null) return;

        if(_playerClothes != null)
        {
            SetSpriteIfAvailable(_shortD, _playerClothes.shortD);
            SetSpriteIfAvailable(_pernaD, _playerClothes.pernaD);
            SetSpriteIfAvailable(_peD, _playerClothes.peD);
        }
        else if(_lowerBodyClothing != null)
        {
            SetSpriteIfAvailable(_shortD, _lowerBodyClothing.shortD);
            SetSpriteIfAvailable(_pernaD, _lowerBodyClothing.pernaD);
            SetSpriteIfAvailable(_peD, _lowerBodyClothing.peD);
        }
    }

    private void ApplyLeftLegClothes()
    {
        if (_playerCustomization == null) return;
        if (_playerClothes == null && _lowerBodyClothing == null) return;

        if(_playerClothes != null)
        {
            SetSpriteIfAvailable(_shortE, _playerClothes.shortE);
            SetSpriteIfAvailable(_peE, _playerClothes.peE);
            SetSpriteIfAvailable(_pernaE, _playerClothes.pernaE);
        }
        else if(_lowerBodyClothing != null)
        {
            SetSpriteIfAvailable(_shortE, _lowerBodyClothing.shortE);
            SetSpriteIfAvailable(_peE, _lowerBodyClothing.peE);
            SetSpriteIfAvailable(_pernaE, _lowerBodyClothing.pernaE);
        }
    }

    #endregion

    private void SetSpriteIfAvailable(SpriteRenderer targetRenderer, Sprite sprite)
    {
        if (sprite != null)
        {
            targetRenderer.sprite = sprite;
        }
    }
}
