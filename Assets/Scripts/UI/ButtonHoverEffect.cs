using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image buttonImage;
    public TextMeshProUGUI buttonText;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite hoverSprite;

    [Header("Colors")]
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = Color.yellow;

    [Header("Animation Settings")]
    public float scaleUpSize = 1.1f;
    public float duration = 0.2f;

    private Vector3 originalScale;

    private void Awake()
    {
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (buttonImage != null)
            buttonImage.sprite = normalSprite;
        buttonText.DOColor(normalTextColor, duration).SetUpdate(true);
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack).SetUpdate(true); ;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.sprite = hoverSprite;
        buttonText.DOColor(hoverTextColor, duration).SetUpdate(true); ;
        transform.DOScale(originalScale * scaleUpSize, duration).SetEase(Ease.OutBack).SetUpdate(true); ;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(buttonImage != null)
            buttonImage.sprite = normalSprite;
        buttonText.DOColor(normalTextColor, duration).SetUpdate(true); ;
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack).SetUpdate(true); ;
    }
}
