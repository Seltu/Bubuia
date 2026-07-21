using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public Vector2 positionChange = Vector2.zero;
    public float duration = 0.2f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onHover;
    [SerializeField] private UnityEvent _onUnhover;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Awake()
    {
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        originalScale = transform.localScale;
        if(positionChange.magnitude > 0)
            originalPosition = transform.position;
    }

    private void OnEnable()
    {
        if (buttonImage != null)
            buttonImage.sprite = normalSprite;
        if (buttonText != null)
            buttonText.color = normalTextColor;
        transform.localScale = originalScale;
        if (positionChange.magnitude > 0)
            transform.position = originalPosition;
        _onUnhover.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.sprite = hoverSprite;
        if (buttonText != null)
            buttonText.DOColor(hoverTextColor, duration).SetUpdate(true); ;
        transform.DOScale(originalScale * scaleUpSize, duration).SetEase(Ease.OutBack).SetUpdate(true);
        if (positionChange.magnitude > 0)
            transform.DOMove(originalPosition + (Vector3)positionChange, duration).SetEase(Ease.OutBack).SetUpdate(true);
        _onHover.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(buttonImage != null)
            buttonImage.sprite = normalSprite;
        if (buttonText != null)
            buttonText.DOColor(normalTextColor, duration).SetUpdate(true); ;
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack).SetUpdate(true);
        if (positionChange.magnitude > 0)
            transform.DOMove(originalPosition, duration).SetEase(Ease.OutBack).SetUpdate(true);
        _onUnhover.Invoke();
    }
}
