using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.EventSystems;

public class FishingRodController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hookObject;
    [SerializeField] private Transform defaultHookPos;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private InputActionReference pointerPositionAction;
    [SerializeField] private InputActionReference moveAction;

    [Header("Settings")]
    [SerializeField] private float arcHeight = 1.5f;
    [SerializeField] private float castDuration = 0.75f;
    [SerializeField] private float maxCastDistance = 8f;
    [SerializeField] private float recallSpeed = 6f;
    [SerializeField] private float instantRecallSpeed = 25f;

    private bool _canCast = true;
    private bool _isRecalling = false;
    private bool _hookInWater = false;
    private bool _hookingFish = false;
    private bool _stopped = false;
    private bool _recallHeld = false;
    private float _hookingDistance;
    private float _currentMaxDistance;
    private Vector3 _castTarget;
    private Camera mainCamera;
    private Fish hookedFish;
    private TreasureSpot _hookedTreasure;
    private bool _hookingTreasure;


    #region Unity Methods
    private void Start()
    {
        mainCamera = Camera.main;
        hookObject.gameObject.SetActive(false);

        EventManager.AddListener("TurnOffControls", PauseControl);
        EventManager.AddListener<Fish>("FishBiteHook", HookFish);
        EventManager.AddListener<TreasureSpot>("HookedTreasure", HookTreasure);
        EventManager.AddListener<bool>("EndFishingMinigame", EndHooking);
        EventManager.AddListener<int, int>("DistanceUpdate", HookingDistanceUpdate);
        EventManager.AddListener("TreasureFail", TreasureFail);

        EventManager.TriggerEvent("CallTutorial", "Tutorial_FishRodHold");
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("TurnOffControls", PauseControl);
        EventManager.RemoveListener<Fish>("FishBiteHook", HookFish);
        EventManager.RemoveListener<TreasureSpot>("HookedTreasure", HookTreasure);
        EventManager.RemoveListener<bool>("EndFishingMinigame", EndHooking);
        EventManager.RemoveListener<int, int>("DistanceUpdate", HookingDistanceUpdate);
        EventManager.RemoveListener("TreasureFail", TreasureFail);
    }

    private void Update()
    {
        if (_hookingFish)
        {
            HookingFishMovement();
            return;
        }

        if (_hookInWater && _recallHeld && !_stopped && !_isRecalling)
            GradualRecallStep();

        float distXZ = Vector3.Distance(
            new Vector3(hookObject.position.x, 0, hookObject.position.z),
            new Vector3(transform.position.x, 0, transform.position.z)
        );

        if (distXZ < 5f)
            StartCoroutine(InstantRecall());

        ClampHookToMaxDistance();
    }
    #endregion

    #region Inputs

    public void OnFishingAction(InputAction.CallbackContext ctx)
    {
        if (_stopped || InputLock.clickLocked)
            return;

        if (ctx.started)
        {
            return;
        }

        if (ctx.performed)
        {
            if (ctx.interaction is TapInteraction)
            {
                // ----> TAP confirmado <----
                OnTap();
                return;
            }
            else if (ctx.interaction is HoldInteraction)
            {
                if (moveAction.action.ReadValue<Vector2>().magnitude > 0.001f)
                    return;
                // ----> HOLD confirmado <----
                _recallHeld = true;
                return;
            }

            return;
        }

        if (ctx.canceled)
        {
            // HOLD terminou
            _recallHeld = false;
        }
    }

    private void OnTap()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 pos = pointerPositionAction.action.ReadValue<Vector2>();

        if (_isRecalling)
            return;

        if (_hookInWater && _hookingTreasure)
        {
            // Check pra impedir o jogador de pegar o tesouro se exceder o limite de distância
            Vector3 rodXZ = new(defaultHookPos.position.x, 0, defaultHookPos.position.z);
            Vector3 hookXZ = new(hookObject.position.x, 0, hookObject.position.z);

            float dist = Vector3.Distance(rodXZ, hookXZ);

            if (dist <= _currentMaxDistance + 5f) // +5 de tolerância
            CatchTreasure();
            return;
        }

        if (_hookInWater && !_hookingFish)
        {
            StartCoroutine(InstantRecall());
            return;
        }

        if (!_hookInWater && _canCast)
            TryCastToPosition(pos);
    }

    // CAST — XZ only
    private void TryCastToPosition(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, waterLayer))
        {
            if (!hit.transform.CompareTag("Water")) return;
            _castTarget = hit.point;

            Vector3 startXZ = new(defaultHookPos.position.x, 0, defaultHookPos.position.z);
            Vector3 targetXZ = new(_castTarget.x, 0, _castTarget.z);

            float dist = Vector3.Distance(startXZ, targetXZ);

            if (dist > maxCastDistance)
            {
                Vector3 dir = (targetXZ - startXZ).normalized;
                Vector3 clampedXZ = startXZ + dir * maxCastDistance;
                _castTarget = new Vector3(clampedXZ.x, hit.point.y, clampedXZ.z);
            }

            CastLine();
        }
    }

    private void PauseControl() => _stopped = true;

    #endregion

    #region Behavior
    private void CastLine()
    {
        _canCast = false;
        _hookInWater = false;
        _isRecalling = false;

        hookObject.SetParent(null);

        hookObject.gameObject.SetActive(true);
        hookObject.position = defaultHookPos.position;

        _currentMaxDistance = maxCastDistance;

        StartCoroutine(ParabolicThrow());
    }

    private IEnumerator ParabolicThrow()
    {
        Vector3 start = defaultHookPos.position;
        Vector3 mid = (start + _castTarget) * 0.5f;
        mid.y += arcHeight;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / castDuration;
            t = Mathf.Clamp01(t);

            Vector3 a = Vector3.Lerp(start, mid, t);
            Vector3 b = Vector3.Lerp(mid, _castTarget, t);

            hookObject.position = Vector3.Lerp(a, b, t);

            yield return null;
        }

        hookObject.position = _castTarget;
        _hookInWater = true;
    }

    // Clamp distance to line length
    private void ClampHookToMaxDistance()
    {
        if (!_hookInWater || _hookingFish || _hookingTreasure || _isRecalling)
            return;

        Vector3 rodXZ = new(defaultHookPos.position.x, 0, defaultHookPos.position.z);
        Vector3 hookXZ = new(hookObject.position.x, 0, hookObject.position.z);

        float dist = Vector3.Distance(rodXZ, hookXZ);

        if (dist > _currentMaxDistance)
        {
            Vector3 dir = (hookXZ - rodXZ).normalized;
            Vector3 clamped = rodXZ + dir * _currentMaxDistance;

            hookObject.position = new Vector3(
                clamped.x,
                hookObject.position.y,
                clamped.z
            );
        }
    }

    // Gradual recall tightens the line
    private void GradualRecallStep()
    {
        if (moveAction.action.ReadValue<Vector2>().magnitude > 0.001f)
        {
            _recallHeld = false;
            return;
        }

        Vector3 rodXZ = new(defaultHookPos.position.x, 0, defaultHookPos.position.z);
        Vector3 hookXZ = new(hookObject.position.x, 0, hookObject.position.z);

        float dist = Vector3.Distance(rodXZ, hookXZ);

        if (_currentMaxDistance > dist)
            _currentMaxDistance = dist;

        _currentMaxDistance = Mathf.Max(
            0f,
            _currentMaxDistance - recallSpeed * Time.deltaTime
        );
    }

    // INSTANT RECALL
    private IEnumerator InstantRecall()
    {
        _isRecalling = true;

        while (_hookInWater)
        {
            hookObject.position = Vector3.MoveTowards(
                hookObject.position,
                defaultHookPos.position,
                instantRecallSpeed * Time.deltaTime
            );

            if (Vector3.Distance(hookObject.position, defaultHookPos.position) < 0.05f)
            {
                FinishRecall();
                break;
            }

            yield return null;
        }

        _isRecalling = false;
    }

    #endregion

    #region Minigame
    private void HookingFishMovement()
    {
        Vector3 dir = (hookObject.position - defaultHookPos.position);
        Vector2 dirXZ = new(dir.x, dir.z);

        float currentDist = dirXZ.magnitude;

        float bias = (_hookingDistance - currentDist) * 30f;
        float jitter = (Random.value - 0.5f) * 20f;

        float forward = (bias + jitter) * Time.deltaTime;

        Vector2 perp = new Vector2(-dirXZ.y, dirXZ.x).normalized;
        Vector2 lateral = perp * ((Random.value - 0.5f) * 10f);

        Vector2 finalXZ = dirXZ.normalized * forward + lateral;
        Vector2 projected = Vector2.Dot(finalXZ, dirXZ.normalized) * dirXZ.normalized;

        hookObject.position += new Vector3(projected.x, 0, projected.y) * recallSpeed * Time.deltaTime;
    }

    private void FinishRecall()
    {
        _hookInWater = false;
        _canCast = true;
        _recallHeld = false;

        if (hookedFish != null)
        {
            Destroy(hookedFish.gameObject);
            hookedFish = null;
        }

        hookObject.SetParent(defaultHookPos);
        hookObject.localPosition = Vector3.zero;

        hookObject.gameObject.SetActive(false);

        EventManager.TriggerEvent("RecallLine");
    }

    private void HookFish(Fish fish)
    {
        if (_hookingFish || _isRecalling)
        {
            fish.EscapeHook();
            return;
        }

        _hookingFish = true;
        _hookingDistance = 10f;
        hookedFish = fish;

        fish.transform.SetParent(hookObject);
        fish.transform.localPosition = Vector3.zero;
        hookObject.tag = "Untagged";

        EventManager.TriggerEvent("StartFishingMinigame", fish);
    }

    private void HookingDistanceUpdate(int currentScore, int maxScore)
    {
        _hookingDistance = 10f - 10f * ((float)currentScore / maxScore);
    }

    private void EndHooking(bool caught)
    {
        _hookingFish = false;
        hookObject.tag = "Hook";

        if (!caught && hookedFish != null)
        {
            hookedFish.transform.SetParent(null);
            hookedFish.EscapeHook();
            hookedFish = null;
        }

        StartCoroutine(InstantRecall());
    }

    private int GetCurrentFingerId()
    {
        foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            if (touch.began)
                return touch.finger.index;
        }
        return -1;
    }

    #endregion

    #region Treasure
    private void HookTreasure(TreasureSpot treasureSpot)
    {
        if (_hookingFish || _hookingTreasure || _isRecalling)
        {
            return;
        }
        EventManager.TriggerEvent("TurnOffMovement");
        _hookingTreasure = true;
        _hookedTreasure = treasureSpot;
        _castTarget = treasureSpot.transform.position + Vector3.down/5f;
        hookObject.tag = "Untagged";
    }

    private void CatchTreasure()
    {
        _hookedTreasure.OnPull();
        if (_hookedTreasure.IsFullyPulled())
        {
            EventManager.TriggerEvent("TurnOnMovement");
            _hookingTreasure = false;
            hookObject.tag = "Hook";
            _hookedTreasure.OnCaught();
            _hookedTreasure = null;
            StartCoroutine(InstantRecall());
        }
    }

    private void TreasureFail()
    {
        _hookingTreasure = false;
        _hookedTreasure = null;
        hookObject.tag = "Hook";
        StartCoroutine(InstantRecall());
    }


    #endregion

    internal bool GetCanCast() => _canCast;
    internal bool IsHookInWater() => _hookInWater;
    internal Vector3 GetCastTarget() => _castTarget;
}
