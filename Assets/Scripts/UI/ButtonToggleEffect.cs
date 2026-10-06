using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonToggleEffect : MonoBehaviour
{
    [Header("References")]
    public Image buttonImage;
    public TextMeshProUGUI buttonText;
    private Toggle _buttonToggle;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite toggleSprite;

    [Header("Colors")]
    public Color normalTextColor = Color.white;
    public Color toggleTextColor = Color.yellow;

    [Header("Animation Settings")]
    public float scaleUpSize = 1.1f;
    public Vector2 positionChange = Vector2.zero;
    public float duration = 0.2f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onToggle;
    [SerializeField] private UnityEvent _onUntoggle;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Awake()
    {
        _buttonToggle = GetComponent<Toggle>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        originalScale = transform.localScale;

        if (positionChange.magnitude > 0)
            originalPosition = transform.position;

        _buttonToggle.onValueChanged.AddListener(OnToggleValueChanged);

        OnToggleValueChanged(_buttonToggle.isOn);
    }

    private void OnEnable()
    {
        if (_buttonToggle == null)
            return;

        OnToggleValueChanged(_buttonToggle.isOn);
    }

    private void OnDestroy()
    {
        _buttonToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    public void ToggleEffect()
    {
        if (buttonImage != null)
            buttonImage.sprite = toggleSprite;
        if (buttonText != null)
            buttonText.DOColor(toggleTextColor, duration).SetUpdate(true); ;
        transform.DOScale(originalScale * scaleUpSize, duration).SetEase(Ease.OutBack).SetUpdate(true);
        if (positionChange.magnitude > 0)
            transform.DOMove(originalPosition + (Vector3)positionChange, duration).SetEase(Ease.OutBack).SetUpdate(true);
        _onToggle.Invoke();
    }

    public void UntoggleEffect()
    {
        if (_buttonToggle != null && _buttonToggle.isOn)
            return;

        if (buttonImage != null)
            buttonImage.sprite = normalSprite;
        if (buttonText != null)
            buttonText.DOColor(normalTextColor, duration).SetUpdate(true); ;
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack).SetUpdate(true);
        if (positionChange.magnitude > 0)
            transform.DOMove(originalPosition, duration).SetEase(Ease.OutBack).SetUpdate(true);
        _onUntoggle.Invoke();
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (_buttonToggle.isOn)
            ToggleEffect();
        else
            UntoggleEffect();
    }
}
