using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private Image _healthBarBackground;
    [SerializeField] private float _updateSpeed;
    [SerializeField] private float _fadeSpeed;

    private float targetFill = 1;
    private float alpha;

    private void Awake()
    {
        EventManager.AddListener<int, int>("ShipHealthUpdate", UpdateLife);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<int, int>("ShipHealthUpdate", UpdateLife);
    }


    private void UpdateLife(int currentHealth, int maxHealth)
    {
        targetFill = (float)currentHealth / maxHealth;
    }

    private void Update()
    {
        if(Mathf.Abs(_healthBarImage.fillAmount - targetFill) > 0.001f)
        {
            alpha = Mathf.Lerp(alpha, 1, Time.unscaledDeltaTime * _fadeSpeed);
            _healthBarImage.fillAmount = Mathf.Lerp(_healthBarImage.fillAmount, targetFill, Time.unscaledDeltaTime * _updateSpeed);
        }
        else
            alpha = Mathf.Lerp(alpha, 0, Time.unscaledDeltaTime * _fadeSpeed);
        var hue = _healthBarImage.color;
        var bgHue = _healthBarBackground.color;
        _healthBarImage.color = new Color(hue.r, hue.g, hue.b, alpha);
        _healthBarBackground.color = new Color(bgHue.r, bgHue.g, bgHue.b, alpha);
    }
}
