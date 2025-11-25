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
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        buttonImage.sprite = normalSprite;
        buttonText.DOColor(normalTextColor, duration);
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.sprite = hoverSprite;
        buttonText.DOColor(hoverTextColor, duration);
        transform.DOScale(originalScale * scaleUpSize, duration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.sprite = normalSprite;
        buttonText.DOColor(normalTextColor, duration);
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }
}
