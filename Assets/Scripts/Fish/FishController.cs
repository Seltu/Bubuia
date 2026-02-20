using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class Fish : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fishSpriteRenderer;
    [SerializeField] private FishTypeSO fishTypeSO;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float neighborRadius = 2.5f;
    [SerializeField] private float separationDistance = 1.7f;
    [SerializeField] private float offscreenTime = 10f;

    [Header("Behavior Weights")]
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 0.5f;
    [SerializeField] private float separationWeight = 3f;

    [Header("World Bounds")]
    [SerializeField] private float minX = -500f;
    [SerializeField] private float maxX = 500f;
    [SerializeField] private float minZ = -500f;
    [SerializeField] private float maxZ = 500f;
    [SerializeField] private float boundsForce = 2.5f;

    [Header("Noise")]
    [SerializeField] private float jitterStrength = 0.3f;

    [Header("Scared Behavior")]
    [SerializeField] private float scareThreshold = 0.5f;
    [SerializeField] private float scareDecayRate = 0.1f;
    [SerializeField] private float scareDuration = 0.5f;
    [SerializeField] private float scareSpeedMultiplier = 20f;

    [Header("Baited Behavior")]
    [SerializeField] private float baitedThreshold = 0.5f;
    [SerializeField] private float baitedDecayRate = 0.1f;

    [Header("ScriptableObjects")]
    [SerializeField] private FloatVariable currentBaitPowerSO;

    private Transform _detectedHook;
    private float _scareLevel = 0f;
    private float _scareTimer = 0f;
    private float _baitedLevel = 0f;
    private Vector3 _scareDirection;
    private bool _isScared = false;
    private bool _isBaited = false;
    private bool _isHooked = false;
    private float _offscreenTimer = 0f;

    [HideInInspector] public FishManager manager;
    private Rigidbody rb;

    private void Start()
    {
        EventManager.AddListener<Vector2, float>("Splash", HearSplash);
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY;

        _offscreenTimer = offscreenTime;
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<Vector2, float>("Splash", HearSplash);
    }

    private void HearSplash(Vector2 splashPosition, float splashSize)
    {
        Vector3 splashPos3D = new Vector3(splashPosition.x, 0f, splashPosition.y);
        float distance = Vector3.Distance(transform.position, splashPos3D);

        if (distance > 10f) return;

        float scareAmount = splashSize / (distance / 3f + 1f);
        _scareLevel += scareAmount;

        if (!_isScared)
            _scareDirection = Random.value > 0.5f ? transform.forward : -transform.forward;

        if (_scareLevel >= scareThreshold)
        {
            _isScared = true;
            _scareTimer = scareDuration;
        }
    }

    private void FixedUpdate()
    {
        if (manager == null) return;

        if (_isHooked)
        {
            float currentY = transform.rotation.eulerAngles.y;
            if (currentY > 180f) currentY -= 360f;

            float randomJitter = (Random.value - 0.5f) * 1000f;
            float biasToZero = -currentY * 2f;

            float rotationChange = (randomJitter + biasToZero) * Time.fixedDeltaTime;
            transform.rotation = Quaternion.Euler(0f, currentY + rotationChange, 0f);

            return;
        }

        if (IsVisibleToCamera())
        {
            _offscreenTimer = offscreenTime;
        }
        else
        {
            _offscreenTimer -= Time.deltaTime;
            if(_offscreenTimer < 0)
            {
                Destroy(gameObject);
            }
        }

        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;
        int count = 0;

        List<Fish> toRemove = new List<Fish>();

        foreach (Fish other in manager.boids)
        {
            if (other == this) continue;

            if (other == null)
            {
                toRemove.Add(other);
                continue;
            }

            Vector3 posA = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 posB = new Vector3(other.transform.position.x, 0, other.transform.position.z);

            float dist = Vector3.Distance(posA, posB);
            if (dist < neighborRadius)
            {
                Vector3 otherForward = new Vector3(other.transform.forward.x, 0, other.transform.forward.z);
                alignment += otherForward;

                cohesion += posB;

                if (dist < separationDistance)
                    separation += (posA - posB).normalized / dist;

                count++;
            }
        }

        manager.boids.RemoveAll(o => toRemove.Contains(o));

        if (count > 0)
        {
            alignment = alignment.normalized;
            cohesion = ((cohesion / count) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            separation = separation.normalized;
        }

        if (_isScared == false)
        {
            _scareLevel = Mathf.Max(0f, _scareLevel - scareDecayRate * Time.fixedDeltaTime);
        }
        else
        {
            _scareTimer -= Time.fixedDeltaTime;
            if (_scareTimer <= 0f)
            {
                _isScared = false;
                _scareLevel = 0f;
            }
        }

        if (_baitedLevel > 0)
        {
            _baitedLevel = Mathf.Max(0f, _baitedLevel - baitedDecayRate * Time.fixedDeltaTime);
        }

        Vector3 moveDir =
        alignment * alignmentWeight +
        cohesion * cohesionWeight +
        separation * separationWeight;

        // Bounds steering
        Vector3 boundsAvoidance = CalculateBoundsAvoidance();
        moveDir += boundsAvoidance;

        if (_isScared)
        {
            moveDir = _scareDirection * scareSpeedMultiplier;
        }
        else if (_isBaited)
        {
            if (!_detectedHook.gameObject.activeSelf)
            {
                _isBaited = false;
            }
            else
            {
                Vector3 hookPosXZ = new Vector3(_detectedHook.position.x, 0, _detectedHook.position.z);
                Vector3 posXZ = new Vector3(transform.position.x, 0, transform.position.z);
                moveDir = (hookPosXZ - posXZ).normalized * scareSpeedMultiplier;
            }
        }

        Vector3 jitter = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * jitterStrength;
        moveDir += jitter;

        Vector3 currentDir = new Vector3(transform.forward.x, 0, transform.forward.z);
        Vector3 smoothedDir = _isScared ?
            Vector3.Lerp(currentDir, moveDir, scareSpeedMultiplier) :
            Vector3.Lerp(currentDir, moveDir, 0.1f);

        if (smoothedDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(smoothedDir, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * 100f * Time.fixedDeltaTime));
        }

        if (_isScared)
            rb.linearVelocity = transform.forward * speed * scareSpeedMultiplier;
        else if (_isBaited)
        {
            rb.linearVelocity = transform.forward * speed;
            if (Vector3.Distance(transform.position,
                new Vector3(_detectedHook.position.x, 0, _detectedHook.position.z)) < 1f)
            {
                BiteHook();
            }
        }
        else
            rb.linearVelocity = transform.forward * speed * (1f - _baitedLevel / baitedThreshold);

        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private void BiteHook()
    {
        rb.isKinematic = true;
        _isHooked = true;
        EventManager.TriggerEvent("FishBiteHook", this);
    }

    internal void EscapeHook()
    {
        if (!_isScared)
            _scareDirection = Random.value > 0.5f ? Vector3.left : Vector3.right;

        _isScared = true;
        _scareTimer = scareDuration;

        _isBaited = false;
        _baitedLevel = 0;

        rb.isKinematic = false;
        _isHooked = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Hook"))
        {
            if (_baitedLevel < baitedThreshold)
            {
                _baitedLevel = Mathf.Min(_baitedLevel + currentBaitPowerSO.Value * 2 * Time.deltaTime, currentBaitPowerSO.Value * 4);
                if (_baitedLevel >= baitedThreshold)
                {
                    _isBaited = true;
                    Vector3 toBait = other.transform.position - transform.position;
                    toBait.y = 0;
                    transform.forward = toBait.normalized;
                    if (_detectedHook == null)
                        _detectedHook = other.transform;
                }
            }
        }
    }

    private Vector3 CalculateBoundsAvoidance()
    {
        Vector3 steer = Vector3.zero;
        Vector3 pos = transform.position;

        // X axis
        if (pos.x < minX)
        {
            float t = Mathf.InverseLerp(minX, minX, pos.x);
            steer += Vector3.right * (1f - t);
        }
        else if (pos.x > maxX)
        {
            float t = Mathf.InverseLerp(maxX, maxX, pos.x);
            steer += Vector3.left * (1f - t);
        }

        // Z axis
        if (pos.z < minZ)
        {
            float t = Mathf.InverseLerp(minZ, minZ, pos.z);
            steer += Vector3.forward * (1f - t);
        }
        else if (pos.z > maxZ)
        {
            float t = Mathf.InverseLerp(maxZ, maxZ, pos.z);
            steer += Vector3.back * (1f - t);
        }

        return steer.normalized * boundsForce;
    }


    private bool IsVisibleToCamera()
    {
        if (Camera.main == null) return true;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return viewportPos.x >= 0 && viewportPos.x <= 1 &&
               viewportPos.y >= 0 && viewportPos.y <= 1;
    }

    internal FishTypeSO GetFishTypeSO()
    {
        return fishTypeSO;
    }

    internal float GetSpeed()
    {
        return speed;
    }
}
