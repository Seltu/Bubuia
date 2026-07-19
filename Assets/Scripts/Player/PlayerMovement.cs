using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private const float INPUT_DEAD_ZONE = 0.01f;
    private const float SPRITE_FLIP_DEAD_ZONE = 0.05f;

    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private GameObject _playerSpritesHolder;
    [SerializeField] private Animator _animator;

    [Header("Movement")]
    [SerializeField] private bool _startFlipped = true;
    [SerializeField] private float _playerSpeed = 5f;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -25f;
    [SerializeField] private float _groundStickForce = -2f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private float _groundCheckDistance = 0.25f;

    private Vector2 _moveInput;
    private Vector3 _groundNormal = Vector3.up;
    private float _verticalVelocity;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        EventManager.AddListener<string>("OnOpenStore", BlockPlayerMovement);
        EventManager.AddListener("OnCloseStore", AllowPlayerMovement);
        if (_startFlipped)
            RotateSprites(Vector2.right);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<string>("OnOpenStore", BlockPlayerMovement);
        EventManager.RemoveListener("OnCloseStore", AllowPlayerMovement);
    }

    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        UpdateGroundNormal();

        bool MovementBlocked = InputLock.movementLocked;
        bool HasInput = _moveInput.sqrMagnitude > INPUT_DEAD_ZONE * INPUT_DEAD_ZONE;

        Vector3 InputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);

        if (InputDirection.sqrMagnitude > 1f)
            InputDirection.Normalize();

        Vector3 HorizontalMovement = Vector3.zero;

        if (HasInput && !MovementBlocked)
        {
            HorizontalMovement = Vector3.ProjectOnPlane(InputDirection, _groundNormal).normalized;
            HorizontalMovement *= _playerSpeed;

            RotateSprites(InputDirection);

            _animator.SetBool("isWalking", true);
        }
        else
        {
            _animator.SetBool("isWalking", false);
        }

        ApplyGravity();

        Vector3 FinalMovement = HorizontalMovement;
        FinalMovement.y = _verticalVelocity;

        CollisionFlags CollisionFlags = _characterController.Move(FinalMovement * Time.deltaTime);

        if ((CollisionFlags & CollisionFlags.Below) != 0 && _verticalVelocity < 0f)
            _verticalVelocity = _groundStickForce;
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = _groundStickForce;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
    }

    private void UpdateGroundNormal()
    {
        _groundNormal = Vector3.up;

        Vector3 SphereOrigin = transform.TransformPoint(_characterController.center);
        float SphereRadius = _characterController.radius * 0.95f;
        float CheckDistance = (_characterController.height * 0.5f) + _groundCheckDistance;

        bool HitGround = Physics.SphereCast(
            SphereOrigin,
            SphereRadius,
            Vector3.down,
            out RaycastHit Hit,
            CheckDistance,
            _groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (HitGround)
            _groundNormal = Hit.normal;
    }

    private void RotateSprites(Vector3 inputDirection)
    {
        if (Mathf.Abs(inputDirection.x) < SPRITE_FLIP_DEAD_ZONE)
            return;

        if (inputDirection.x < 0f)
        {
            //_playerSpritesHolder.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
            _playerSpritesHolder.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            //_playerSpritesHolder.transform.rotation = Quaternion.Euler(-25f, 180f, 0f);
            _playerSpritesHolder.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        _playerSpritesHolder.transform.localPosition = Vector3.zero;
    }

    public void MovePlayerInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void BlockPlayerMovement(string storeId)
    {
        _moveInput = Vector2.zero;
        EventManager.TriggerEvent("OnBlockPlayerMovement");
    }

    private void AllowPlayerMovement()
    {
        EventManager.TriggerEvent("OnAllowPlayerMovement");
    }
}