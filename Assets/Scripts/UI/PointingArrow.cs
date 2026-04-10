using UnityEngine;
using UnityEngine.UI;

public class PointingArrow : MonoBehaviour
{
    [SerializeField] private Image _img;
    [SerializeField] private Camera _cam;
    [SerializeField] private RectTransform _arrowRect;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 25f;
    [SerializeField] private LayerMask coinLayer;
    [SerializeField] private LayerMask treasureLayer;
    [SerializeField] private Transform player;

    private Transform closestCoin;
    private Collider[] _results = new Collider[64];
    private LayerMask _currentCompassLayer;

    private void Start()
    {
        _currentCompassLayer = coinLayer;

        if (_cam == null)
            _cam = Camera.main;

        if (_arrowRect == null)
            _arrowRect = GetComponent<RectTransform>();
    }

    private void Reset()
    {
        player = transform;
    }

    private void Update()
    {
        closestCoin = FindClosestTarget();

        if (closestCoin != null)
            LookAtTargetUI();
    }

    private Transform FindClosestTarget()
    {
        Transform bestTarget = null;
        float bestDistanceSqr = float.MaxValue;

        int hits = Physics.OverlapSphereNonAlloc(
            player.position,
            detectionRadius,
            _results,
            _currentCompassLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hits; i++)
        {
            Collider col = _results[i];
            if (col == null) continue;

            float distSqr = (col.transform.position - player.position).sqrMagnitude;
            if (distSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distSqr;
                bestTarget = col.transform;
            }

            _results[i] = null;
        }

        return bestTarget;
    }

    private void LookAtTargetUI()
    {
        if (closestCoin == null || _cam == null || _arrowRect == null) return;

        Vector3 playerScreenPos = _cam.WorldToScreenPoint(player.position);
        Vector3 targetScreenPos = _cam.WorldToScreenPoint(closestCoin.position);

        Vector2 dir = (targetScreenPos - playerScreenPos);

        if (dir.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Arrow sprite points LEFT in its default state
        _arrowRect.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
    }

    public Transform GetClosestCoin()
    {
        return closestCoin;
    }

    public void SetLookAtTreasure()
    {
        _currentCompassLayer = treasureLayer;
    }

    public void SetLookAtCoins()
    {
        _currentCompassLayer = coinLayer;
    }

}