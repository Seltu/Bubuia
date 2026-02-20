using UnityEngine;
using UnityEngine.UI;

public class BoostBarUI : MonoBehaviour
{
    [SerializeField] private PlayerShipController _shipController;
    [SerializeField] private Image _boostBarImage;
    [SerializeField] private float _fadeSpeed;

    private Color hue;
    private float alpha;

    private void Update()
    {
        _boostBarImage.fillAmount = _shipController.GetBoost01();
        hue = Color.Lerp(Color.red, Color.green, _boostBarImage.fillAmount);
        if (_boostBarImage.fillAmount > 0.9f)
            alpha = Mathf.Lerp(alpha, 0, Time.deltaTime * _fadeSpeed);
        else alpha = Mathf.Lerp(alpha, 1, Time.deltaTime * _fadeSpeed);
        _boostBarImage.color = new Color(hue.r, hue.g, hue.b, alpha);
    }
}
