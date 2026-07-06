using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShipController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shipSprite;
    [SerializeField] private Transform playerSprite;
    [SerializeField] private Rigidbody shipRigidbody;

    [Header("ShipHealth")]
    [SerializeField] private int maxHealth;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float turnSpeed = 200f;
    [SerializeField] private float drag = 2f;

    [Header("Boost Settings")]
    [SerializeField] private float boostAcceleration = 9f;     // velocidade máxima com boost
    [SerializeField] private float boostMax = 1f;           // capacidade (1 = 100%)
    [SerializeField] private float boostDrainPerSecond = 0.35f;
    [SerializeField] private float boostRegainPerSecond = 0.25f;
    [SerializeField] private float boostRegainDelay = 0.75f; // tempo sem consumir para começar a regenerar

    [Header("Effects")]
    [SerializeField] private float splashStrength = 0.2f;
    [SerializeField] private float splashInterval = 0.1f;

    [Header("Obstacle Collision")]
    [SerializeField] private float obstacleHitSpeedThreshold = 4f;
    [SerializeField] private float knockbackSpeed = 7f;
    [SerializeField] private float knockbackDuration = 0.35f;

    private int _currentHealth;
    private float _splashTimer;
    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private bool _stopped;

    // Boost runtime
    private float _boost;
    private bool _boostHeld;
    private bool _boostLockedUntilFull;  // trava quando zera
    private float _timeSinceBoostUse;    // contador pra delay de regen

    // Collision
    private bool _isKnockbacking;
    private float _knockbackTimer;
    private Vector3 _knockbackVelocity;

    private void Awake()
    {
        if (shipRigidbody == null) shipRigidbody = GetComponent<Rigidbody>();
        _boost = boostMax;
    }

    private void Start()
    {
        _currentHealth = maxHealth;
        EventManager.AddListener("TurnOffControls", PauseMovement);
        EventManager.AddListener("TurnOffMovement", PauseMovement);
        EventManager.AddListener("TurnOnMovement", UnpauseMovement);
        ResolveSpawnOverlap();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("TurnOffControls", PauseMovement);
        EventManager.RemoveListener("TurnOffMovement", PauseMovement);
        EventManager.RemoveListener("TurnOnMovement", UnpauseMovement);
    }

    private void PauseMovement() => _stopped = true;
    private void UnpauseMovement() => _stopped = false;

    private void ResolveSpawnOverlap() // Detecta terreno ao redor do jogador e o reposiciona na água
    {
        Vector3 origin = transform.position;
        float y = origin.y;

        bool IsFree(Vector3 p)
        {
            var hits = Physics.OverlapSphere(p, 5, 10);
            return hits == null || hits.Length == 0;
        }

        // já está livre
        if (IsFree(origin))
            return;
        const int RING_SAMPLES = 24; // pontos por anel

        // busca em anéis (do mais próximo pro mais longe)
        for (float r = 2; r <= 100; r += 2)
        {
            for (int i = 0; i < RING_SAMPLES; i++)
            {
                float t = (i / (float)RING_SAMPLES) * Mathf.PI * 2f;
                Vector3 p = origin + new Vector3(Mathf.Cos(t), 0f, Mathf.Sin(t)) * r;
                p.y = y;
                if (IsFree(p))
                {
                    transform.position = p;
                    if (shipRigidbody != null) shipRigidbody.position = p;
                    return;
                }
            }
        }
    }

    private void Update()
    {
        MoveShip3D();
    }

    private void MoveShip3D()
    {
        if (Time.timeScale == 0) return;

        if (_isKnockbacking)
        {
            UpdateKnockback();
            return;
        }

        if (_stopped || InputLock.movementLocked)
            moveInput = Vector3.zero;

        float steerInput = Mathf.Clamp(moveInput.x, -1f, 1f);     // esquerda / direita
        float throttleInput = Mathf.Clamp(moveInput.z, -1f, 1f); // frente / ré

        // Boost só funciona quando está indo para frente
        bool boostActive = UpdateBoost();

        if (throttleInput <= 1f && boostActive)
            throttleInput = 1f;

        bool wantsToMove = Mathf.Abs(throttleInput) > 0.001f;

        // --- Rotação tipo carro/barco
        if (Mathf.Abs(steerInput) > 0.001f)
        {
            float steeringDirection = 1f;

            // Faz a direção inverter quando está dando ré, igual carro
            if (throttleInput < -0.001f)
                steeringDirection = -1f;

            float turnAmount = steerInput * steeringDirection * turnSpeed * Time.deltaTime;

            shipSprite.Rotate(0f, turnAmount, 0f, Space.World);
        }

        // --- Direção atual do barco
        Vector3 forward = shipSprite.forward;
        forward.y = 0f;
        forward.Normalize();

        // --- Velocidade máxima
        float maxCurrentSpeed = throttleInput >= 0f
            ? maxSpeed
            : maxSpeed * 0.5f;

        Vector3 targetVelocity = forward * throttleInput * maxCurrentSpeed;
        if (throttleInput < 0f)
            targetVelocity = Vector3.zero;

        // --- Aceleração ou desaceleração
        if (wantsToMove)
        {
            if(!boostActive)
                currentVelocity = Vector3.MoveTowards(
                    currentVelocity,
                    targetVelocity,
                    acceleration * Time.deltaTime
                );
            else
                currentVelocity = Vector3.MoveTowards(
                    currentVelocity,
                    targetVelocity,
                    boostAcceleration * Time.deltaTime
                );
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                drag * Time.deltaTime
            );
        }

        // --- Aplica velocidade
        if (_stopped || InputLock.movementLocked)
            currentVelocity = Vector3.zero;
        shipRigidbody.linearVelocity = currentVelocity;

        // --- Splash effect
        if (currentVelocity.sqrMagnitude > 0.25f)
        {
            _splashTimer -= Time.deltaTime;
            if (_splashTimer < 0)
            {
                _splashTimer = splashInterval;
                var calculatedSplashStrength = boostActive ? splashStrength * 2f : splashStrength;
                EventManager.TriggerEvent("Splash", transform.position, calculatedSplashStrength);
            }
        }

        // --- Player object flip
        float angle = shipSprite.eulerAngles.y;

        if (angle <= 0f || angle > 180f)
            playerSprite.transform.localRotation = Quaternion.Euler(0, 0, 0);
        else
            playerSprite.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    private void StartObstacleKnockback(Vector3 direction)
    {
        _isKnockbacking = true;
        _knockbackTimer = knockbackDuration;

        moveInput = Vector3.zero;
        _boostHeld = false;

        direction.y = 0f;
        direction.Normalize();

        _knockbackVelocity = direction * knockbackSpeed;
        currentVelocity = _knockbackVelocity;

        shipRigidbody.linearVelocity = _knockbackVelocity;
    }

    private void UpdateKnockback()
    {
        _knockbackTimer -= Time.deltaTime;

        shipRigidbody.linearVelocity = _knockbackVelocity;

        _knockbackVelocity = _knockbackVelocity.normalized * knockbackSpeed * (_knockbackTimer/knockbackDuration);

        if (_knockbackTimer <= 0f)
        {
            _isKnockbacking = false;

            currentVelocity = Vector3.zero;
            shipRigidbody.linearVelocity = currentVelocity;
        }
    }

    private bool UpdateBoost()
    {
        float dt = Time.deltaTime;

        bool canBoost =
            _boostHeld &&
            !_boostLockedUntilFull &&
            _boost > 0.001f;

        if (canBoost)
        {
            _boost = Mathf.Max(0f, _boost - boostDrainPerSecond * dt);
            _timeSinceBoostUse = 0f;

            if (_boost <= 0.001f)
            {
                _boost = 0f;
                _boostLockedUntilFull = true;
                return false;
            }

            return true;
        }

        _timeSinceBoostUse += dt;

        if (_timeSinceBoostUse >= boostRegainDelay)
        {
            _boost = Mathf.MoveTowards(_boost, boostMax, boostRegainPerSecond * dt);

            if (_boostLockedUntilFull && _boost >= boostMax - 0.0001f)
            {
                _boost = boostMax;
                _boostLockedUntilFull = false;
            }
        }

        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isKnockbacking) return;

        if (!collision.gameObject.CompareTag("Obstacle")) return;

        float speed = currentVelocity.magnitude;

        if (speed <= obstacleHitSpeedThreshold) return;

        _currentHealth--;
        EventManager.TriggerEvent("ShipHealthUpdate", _currentHealth, maxHealth);
        if (_currentHealth <= 0)
        {
            EventManager.TriggerEvent("ShipBreak");
            return;
        }

        Vector3 knockbackDirection = -currentVelocity.normalized;
        knockbackDirection.y = 0f;

        if (knockbackDirection.sqrMagnitude < 0.001f)
        {
            knockbackDirection = transform.position - collision.transform.position;
            knockbackDirection.y = 0f;
            knockbackDirection.Normalize();
        }

        StartObstacleKnockback(knockbackDirection);
    }

    // INPUT
    public void MoveShipInput(InputAction.CallbackContext context)
    {
        if (InputLock.movementLocked)
        {
            moveInput = Vector3.zero;
            return;
        }

        if (!_stopped)
        {
            Vector2 input = context.ReadValue<Vector2>();
            moveInput = new Vector3(input.x, 0, input.y); // Y vira Z
        }
        else
        {
            moveInput = Vector3.zero;
        }
    }

    public void BoostInput(InputAction.CallbackContext context)
    {
        if (InputLock.movementLocked || _stopped)
        {
            _boostHeld = false;
            return;
        }

        // "Segurar para boost"
        if (context.performed)
            _boostHeld = true;
        else if (context.canceled)
            _boostHeld = false;
    }

    public float GetBoost01() => boostMax <= 0f ? 0f : Mathf.Clamp01(_boost / boostMax);
    public bool IsBoostLocked() => _boostLockedUntilFull;
}
