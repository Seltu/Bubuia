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

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float turnSpeed = 200f;
    [SerializeField] private float drag = 2f;

    [Header("Boost Settings")]
    [SerializeField] private float boostMoveSpeed = 9f;     // velocidade máxima com boost
    [SerializeField] private float boostMax = 1f;           // capacidade (1 = 100%)
    [SerializeField] private float boostDrainPerSecond = 0.35f;
    [SerializeField] private float boostRegainPerSecond = 0.25f;
    [SerializeField] private float boostRegainDelay = 0.75f; // tempo sem consumir para começar a regenerar

    [Header("Effects")]
    [SerializeField] private float splashStrength = 0.2f;
    [SerializeField] private float splashInterval = 0.1f;

    private float _splashTimer;
    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private bool _stopped;

    // Boost runtime
    private float _boost;
    private bool _boostHeld;
    private bool _boostLockedUntilFull;  // trava quando zera
    private float _timeSinceBoostUse;    // contador pra delay de regen

    private void Awake()
    {
        if (shipRigidbody == null) shipRigidbody = GetComponent<Rigidbody>();
        _boost = boostMax;
    }

    private void Start()
    {
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

        // --- Convert input (X,Y) from InputSystem to (X,0,Z)
        Vector3 desiredInput = new Vector3(moveInput.x, 0, moveInput.z).normalized;
        bool isMoving = desiredInput.sqrMagnitude > 0.001f;

        // --- Boost state update (drain / regen / lock)
        bool boostActive = UpdateBoost(isMoving);

        // --- Smooth rotation only if moving
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredInput, Vector3.up);
            shipSprite.rotation = Quaternion.RotateTowards(
                shipSprite.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }

        // --- Choose max speed depending on boost
        float maxSpeed = boostActive ? boostMoveSpeed : moveSpeed;

        // --- Apply acceleration toward target velocity
        Vector3 targetVelocity = desiredInput * maxSpeed;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        // --- Apply velocity to Rigidbody
        shipRigidbody.linearVelocity = currentVelocity;

        // --- Auto-drag when not pressing movement
        if (!isMoving)
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                drag * Time.deltaTime
            );
        }

        // --- Splash effect
        if (currentVelocity.sqrMagnitude > 0.25f)
        {
            _splashTimer -= Time.deltaTime;
            if (_splashTimer < 0)
            {
                _splashTimer = splashInterval;
                EventManager.TriggerEvent("Splash", transform.position, splashStrength);
            }
        }
    }

    private bool UpdateBoost(bool isMoving)
    {
        float dt = Time.deltaTime;

        // Só considera boost se: segurando, movendo, não travado e tem carga
        bool canBoost =
            _boostHeld &&
            isMoving &&
            !_boostLockedUntilFull &&
            _boost > 0.001f;

        if (canBoost)
        {
            // Consumo
            _boost = Mathf.Max(0f, _boost - boostDrainPerSecond * dt);
            _timeSinceBoostUse = 0f;

            if (_boost <= 0.001f)
            {
                _boost = 0f;
                _boostLockedUntilFull = true;
                return false;
            }

            return true; // boost ativo
        }


        // Regenera depois de um tempo sem consumir
        _timeSinceBoostUse += dt;
        if (_timeSinceBoostUse >= boostRegainDelay)
        {
            _boost = Mathf.MoveTowards(_boost, boostMax, boostRegainPerSecond * dt);

            // Se estava travado por ter zerado, só destrava quando encher 100%
            if (_boostLockedUntilFull && _boost >= boostMax - 0.0001f)
            {
                _boost = boostMax;
                _boostLockedUntilFull = false;
            }
        }

        return false;
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
