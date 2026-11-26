using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CityNpc : NpcDetection
{
    [Header("Interaction")]
    [SerializeField] private InputActionReference _npcInteractionInput;
    [SerializeField] private Animator _npcAnimator;

    [Header("Commute")]
    [Tooltip("Pontos em ordem, do trabalho até a casa. O último é a porta.")]
    [SerializeField] private Vector3[] _homePath = Array.Empty<Vector3>();
    [SerializeField, Min(0.01f)] private float _walkSpeed = 2f;
    [SerializeField, Min(0.01f)] private float _arrivalDistance = 0.08f;
    [SerializeField, Min(0f)] private float _gravity = 20f;

    internal event Action InteractEvent;

    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int WalkingBack = Animator.StringToHash("WalkingBack");
    private static readonly int Interacting = Animator.StringToHash("isInteracting");

    private enum Location
    {
        AtWork,
        GoingHome,
        AtHome,
        GoingToWork
    }

    private CharacterController _controller;
    private Vector3 _workPosition;
    private Location _location = Location.AtWork;
    private bool _workHours = true;
    private bool _isInteracting;
    private int _nextPoint;
    private float _verticalSpeed;

    private bool HasRoute => _homePath != null && _homePath.Length > 0;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _workPosition = transform.position;

        _npcInteractionInput.action.performed += OnInteract;
        EventManager.AddListener("StartDialogue", SetAnimationBool);
        EventManager.AddListener<int, int>("TimePassed", ReactToTime);
    }

    private void OnDestroy()
    {
        _npcInteractionInput.action.performed -= OnInteract;
        EventManager.RemoveListener("StartDialogue", SetAnimationBool);
        EventManager.RemoveListener<int, int>("TimePassed", ReactToTime);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || !_workHours || _location != Location.AtWork)
            return;

        if (!_isInReach || _isInteracting || !_npcInteractionAllowed.value)
            return;

        var interact = InteractEvent;
        if (interact == null)
            return;

        _isInteracting = true;
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", false);
        SetAnimationBool();
        interact.Invoke();
    }

    public void OnDialogueEnd()
    {
        _isInteracting = false;
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", true);
        SetAnimationBool();

        // Se deu 19h no meio da conversa, sai agora.
        ApplySchedule();
    }

    public void SetAnimationBool()
    {
        _npcAnimator.SetBool(Interacting, _isInteracting);
    }

    public void SetNpcTalkingManually(bool isTalking)
    {
        _npcAnimator.SetBool(Interacting, isTalking);
    }

    private void ReactToTime(int hours, int minutes)
    {
        if (hours == 19)
            _workHours = false;
        else if (hours == 6)
            _workHours = true;
        else
            return;

        ApplySchedule();
    }

    private void ApplySchedule()
    {
        if (_workHours)
        {
            if (_location == Location.AtHome)
            {
                // -1 é o ponto implícito do trabalho.
                // Já estamos no último ponto da rota, então ele é pulado na volta.
                _location = HasRoute ? Location.GoingToWork : Location.AtWork;
                _nextPoint = HasRoute ? _homePath.Length - 2 : -1;
                _verticalSpeed = 0f;
                gameObject.SetActive(true);
            }
            else if (_location == Location.GoingHome)
            {
                _location = Location.GoingToWork;
                _npcInteractionAllowed.value = false;
                _nextPoint--;
            }
        }
        else if (_location == Location.AtWork && !_isInteracting)
        {
            if (!HasRoute)
            {
                ArriveHome();
                return;
            }

            _location = Location.GoingHome;
            _npcInteractionAllowed.value = false;
            _nextPoint = 0;
            _verticalSpeed = 0f;
        }
        else if (_location == Location.GoingToWork)
        {
            _location = Location.GoingHome;
            _nextPoint++;
        }
    }

    private void Update()
    {
        bool goingHome = _location == Location.GoingHome;
        if (!goingHome && _location != Location.GoingToWork)
            return;

        while (_nextPoint >= -1 && _nextPoint < _homePath.Length)
        {
            Vector3 target = _nextPoint == -1
                ? _workPosition
                : _workPosition + _homePath[_nextPoint];

            Vector3 offset = target - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= _arrivalDistance * _arrivalDistance)
            {
                _nextPoint += goingHome ? 1 : -1;
                continue;
            }

            MoveTowardsPoint(offset);
            return;
        }

        if (goingHome)
        {
            ArriveHome();
        }
        else
        {
            _location = Location.AtWork;
            _npcInteractionAllowed.value = true;
            _npcAnimator.SetBool(Walking, false);
        }
    }

    private void MoveTowardsPoint(Vector3 offset)
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        _verticalSpeed = _controller.isGrounded && _verticalSpeed <= 0f
            ? -2f
            : _verticalSpeed - _gravity * dt;

        Vector3 horizontalStep = Vector3.ClampMagnitude(offset, _walkSpeed * dt);
        Vector3 before = transform.position;

        _controller.Move(horizontalStep + Vector3.up * (_verticalSpeed * dt));

        if(horizontalStep.z > 0)
            _npcAnimator.SetBool(WalkingBack, true);
        else
            _npcAnimator.SetBool(WalkingBack, false);
        if (horizontalStep.x > 0)
            _npcAnimator.transform.localScale = new Vector3(-1, 1, 1);
        else
            _npcAnimator.transform.localScale = new Vector3(1, 1, 1);

        // Se bater numa parede, não fica andando no lugar.
        Vector3 moved = transform.position - before;
        moved.y = 0f;
        _npcAnimator.SetBool(Walking, moved.sqrMagnitude > 0.000001f);
    }

    private void ArriveHome()
    {
        _location = Location.AtHome;
        _npcAnimator.SetBool(Walking, false);
        gameObject.SetActive(false);
    }
}