using UnityEngine;

public class FishingRing : MonoBehaviour
{
    [SerializeField] private Animator _ringAnimator;
    [SerializeField] private SpriteRenderer _visualRenderer;
    [SerializeField] private Sprite _regularSprite;
    [SerializeField] private Sprite _doubleRingSprite;
    private int _health = 1;
    private Transform _fishTransform;
    private bool _shrinking = true;
    private bool _failed;
    private bool _frozen;
    private float _speed;
    private void Update()
    {
        if (_failed)
        {
            _visualRenderer.color = Color.red;

            Vector3 currentPos = _visualRenderer.transform.localPosition;
            Vector2 randomJitter = Random.insideUnitCircle * 0.3f;

            // Direction toward zero
            Vector3 toZero = -currentPos.normalized;

            // Magnitude of attraction (stronger when farther from zero)
            float attractionStrength = currentPos.magnitude;

            // Combine random with bias towards center
            Vector3 biasedOffset = (Vector3)randomJitter + toZero * attractionStrength * 0.05f;

            _visualRenderer.transform.localPosition = currentPos + biasedOffset;
        }

        if (_fishTransform != null)
            transform.position = _fishTransform.position+Vector3.back*0.1f;
        if (!_shrinking) return;
        transform.localScale -= Vector3.one * Time.deltaTime * _speed;

        if (transform.localScale.x < 0.4f)
        {
            if (!_frozen)
                FailRing();
            else
                FadeRing();
            EventManager.TriggerEvent("RingMiss");
        }
    }

    internal void ExplodeRing()
    {
        SetHealth(_health-1);
        if( _health <= 0 )
        {
            _shrinking = false;
            _visualRenderer.color = Color.green;
            _ringAnimator.SetTrigger("Explode");
            Destroy(gameObject, 2f);
        }
    }

    internal bool hasExploded()
    {
        return !_shrinking;
    }
        
    internal void SetRing(Transform fishTransform, float speed)
    {
        _fishTransform = fishTransform;
        _speed = speed;
        _health = 1;
    }

    internal void SetFrozen(bool frozen)
    {
        _frozen = frozen;
    }
    internal void SetHealth(int health)
    {
        _health = health;
        switch (_health) {
            case 1:
                _visualRenderer.sprite = _regularSprite;
                break;
            case 2:
                _visualRenderer.sprite = _doubleRingSprite;
                break;
        }
    }

    internal void FailRing()
    {
        _shrinking = false;
        _failed = true;
        _ringAnimator.SetTrigger("Explode");
        Destroy(gameObject, 2f);
    }

    internal void FadeRing()
    {
        _shrinking = false;
        _ringAnimator.SetTrigger("Fade");
        Destroy(gameObject, 2f);
    }

}
