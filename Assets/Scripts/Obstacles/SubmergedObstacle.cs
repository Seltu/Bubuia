using DG.Tweening;
using UnityEngine;

public class SubmergedObstacle : MonoBehaviour
{
    private const float MINIMUM_BUBBLE_RISE_SPEED = 0.01f;

    private enum ObstacleState
    {
        Submerged,
        Warning,
        Rising,
        Surfaced
    }

    [SerializeField] private bool _isPrePositioned; // For scenes where this object already is positioned on scene and not instantiated by terrain.
    [SerializeField] private Transform _movingRoot;
    [SerializeField] private ParticleSystem _bubbleParticles;
    [SerializeField] private float _warningDuration = 1.5f;
    [SerializeField] private float _riseDuration = 2f;
    [SerializeField] private float _riseBounceOvershoot = 1.15f;
    [SerializeField] private float _surfaceOffset = 0f;
    [SerializeField] private Vector2 _randomYRotationRange = new Vector2(0f, 360f);
    [SerializeField] private Vector2 _randomZRotationRange = new Vector2(-12f, 12f);
    [SerializeField] private float _bubbleLifetimeSurfacePadding = 0.2f;
    [Tooltip("Only assign physical blocking colliders. Do not include the player detection trigger.")]
    [SerializeField] private Collider[] _blockingColliders = new Collider[0];
    [SerializeField] private float _pushAwayDistance = 5f;
    [SerializeField] private float _pushAwayVelocity = 4f;

    private ObstacleState _currentState;
    private Vector3 _submergedPosition;
    private Vector3 _surfacePosition;
    private float _stateTimer;
    private int _playerTriggerContacts;
    private Transform _playerTransform;
    private Rigidbody _playerRigidbody;
    private Tween _riseTween;

    private void OnEnable()
    {
        if (_isPrePositioned) Initialize(-0.1f);
    }

    private void OnDisable()
    {
        KillRiseTween();
    }

    public void Initialize(float WaterHeight)
    {
        KillRiseTween();

        _submergedPosition = _movingRoot.position;
        _surfacePosition = new Vector3(
            _submergedPosition.x,
            WaterHeight + _surfaceOffset,
            _submergedPosition.z
        );

        _movingRoot.position = _submergedPosition;
        if (!_isPrePositioned) ApplyRandomVisualRotation();

        ConfigureBubbleLifetime();
        SetBlockingCollidersEnabled(false);

        _stateTimer = 0f;
        _playerTriggerContacts = 0;
        _playerTransform = null;
        _playerRigidbody = null;
        _currentState = ObstacleState.Submerged;
    }

    private void Update()
    {
        if (_currentState == ObstacleState.Warning)
        {
            UpdateWarning();
        }
    }

    private void OnTriggerEnter(Collider Other)
    {
        if (!Other.CompareTag("Player"))
        {
            return;
        }

        CachePlayer(Other);
        _playerTriggerContacts++;

        if (_currentState != ObstacleState.Submerged)
        {
            return;
        }

        StartWarning();
    }

    private void OnTriggerExit(Collider Other)
    {
        if (!Other.CompareTag("Player"))
        {
            return;
        }

        _playerTriggerContacts--;

        if (_playerTriggerContacts < 0)
        {
            _playerTriggerContacts = 0;
        }
    }

    private void StartWarning()
    {
        _currentState = ObstacleState.Warning;
        _stateTimer = 0f;
        SetBlockingCollidersEnabled(false);
        _bubbleParticles.Play();
    }

    private void UpdateWarning()
    {
        _stateTimer += Time.deltaTime;

        if (_stateTimer >= _warningDuration)
        {
            StartRising();
        }
    }

    private void StartRising()
    {
        _currentState = ObstacleState.Rising;
        _stateTimer = 0f;
        KillRiseTween();

        _riseTween = _movingRoot
            .DOMove(_surfacePosition, _riseDuration)
            .SetEase(Ease.OutElastic, _riseBounceOvershoot)
            .OnComplete(CompleteRise);
    }

    private void CompleteRise()
    {
        _bubbleParticles.Stop();
        _riseTween = null;
        _currentState = ObstacleState.Surfaced;

        PushPlayerAway();
        SetBlockingCollidersEnabled(true);
    }

    private void KillRiseTween()
    {
        if (_riseTween == null)
        {
            return;
        }

        _riseTween.Kill();
        _riseTween = null;
    }

    private void CachePlayer(Collider PlayerCollider)
    {
        Rigidbody AttachedRigidbody = PlayerCollider.attachedRigidbody;

        if (AttachedRigidbody != null)
        {
            _playerRigidbody = AttachedRigidbody;
            _playerTransform = AttachedRigidbody.transform;
            return;
        }

        _playerRigidbody = null;
        _playerTransform = PlayerCollider.transform;
    }

    private void PushPlayerAway()
    {
        if (_playerTransform == null)
        {
            return;
        }

        Vector3 ObstacleHorizontalPosition = _movingRoot.position;
        ObstacleHorizontalPosition.y = 0f;

        Vector3 PlayerHorizontalPosition = _playerTransform.position;
        PlayerHorizontalPosition.y = 0f;

        Vector3 PushDirection = PlayerHorizontalPosition - ObstacleHorizontalPosition;
        float CurrentDistanceSqr = PushDirection.sqrMagnitude;
        float PushAwayDistanceSqr = _pushAwayDistance * _pushAwayDistance;

        if (CurrentDistanceSqr > PushAwayDistanceSqr)
        {
            return;
        }

        if (CurrentDistanceSqr <= 0.001f)
        {
            PushDirection = _movingRoot.forward;
            PushDirection.y = 0f;
        }

        if (PushDirection.sqrMagnitude <= 0.001f)
        {
            PushDirection = Vector3.forward;
        }

        PushDirection.Normalize();

        Vector3 PushPosition = ObstacleHorizontalPosition + PushDirection * _pushAwayDistance;
        PushPosition.y = _playerTransform.position.y;

        if (_playerRigidbody != null)
        {
            _playerRigidbody.position = PushPosition;
            _playerRigidbody.linearVelocity = PushDirection * _pushAwayVelocity;
        }
        else
        {
            _playerTransform.position = PushPosition;
        }
    }

    private void SetBlockingCollidersEnabled(bool Enabled)
    {
        for (int i = 0; i < _blockingColliders.Length; i++)
        {
            _blockingColliders[i].enabled = Enabled;
        }
    }

    private void ApplyRandomVisualRotation()
    {
        Vector3 CurrentRotation = _movingRoot.localEulerAngles;
        float RandomYRotation = Random.Range(_randomYRotationRange.x, _randomYRotationRange.y);
        float RandomZRotation = Random.Range(_randomZRotationRange.x, _randomZRotationRange.y);

        _movingRoot.localRotation = Quaternion.Euler(CurrentRotation.x, RandomYRotation, RandomZRotation);
    }

    private void ConfigureBubbleLifetime()
    {
        ParticleSystem.MainModule MainModule = _bubbleParticles.main;
        ParticleSystem.MinMaxCurve StartSpeed = MainModule.startSpeed;
        float BubbleRiseSpeed = StartSpeed.constantMax;

        if (BubbleRiseSpeed < MINIMUM_BUBBLE_RISE_SPEED)
        {
            BubbleRiseSpeed = MINIMUM_BUBBLE_RISE_SPEED;
        }

        float BubbleTravelHeight = _surfacePosition.y - _bubbleParticles.transform.position.y;

        if (BubbleTravelHeight < 0f)
        {
            BubbleTravelHeight = 0f;
        }

        float BubbleLifetime = BubbleTravelHeight / BubbleRiseSpeed + _bubbleLifetimeSurfacePadding;
        MainModule.startLifetime = BubbleLifetime;
    }
}
