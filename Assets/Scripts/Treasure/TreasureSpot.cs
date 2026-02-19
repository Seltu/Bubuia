using System.Collections;
using UnityEngine;

public class TreasureSpot : MonoBehaviour
{
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private Animator _treasureSpotAnimator;
    [SerializeField] private Transform _baseVisual;
    [SerializeField] private Transform _fillVisual;
    [SerializeField] private int _pullToCollect;
    [SerializeField] private int _pullAtStart;
    [SerializeField] private float _pullDecay;
    private float _pullProgress;
    private bool _startedPulling;

    private void Update()
    {
        if (_pullProgress > 0f)
        {
            //Quanto menor estiver o progresso mais rápido vai parecer que você tá ganhando/perdendo progresso
            float t = Mathf.Clamp01(_pullProgress / _pullToCollect);
            float k = 0.99f;
            float eased = Mathf.Pow(t, k);
            _fillVisual.localScale = _baseVisual.localScale * eased;
            _pullProgress -= Time.deltaTime * _pullDecay;
            if (_pullProgress < 0f) OnFail();
        }
    }

    internal void OnPull()
    {
        if (!_startedPulling)
        {
            _pullProgress = _pullAtStart-1;
            _startedPulling = true;
        }
        _pullProgress++;
    }
    internal void OnCaught()
    {
        _treasureSpotAnimator.Play("TreasureCaught");
        for (var i = 0; i < 10; i++)
        {
            PoolManager.Instance.ReuseComponent(_coinPrefab, transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)), Quaternion.identity);
        }
        Destroy(gameObject, 1f);
    }
    internal void OnFail()
    {
        _treasureSpotAnimator.Play("TreasureCaught");
        EventManager.TriggerEvent("TreasureFail");
        Destroy(gameObject, 1f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsFullyPulled()) return;
        if (other.CompareTag("Hook"))
        {
            StartCoroutine(WaitToStartPulling());
            EventManager.TriggerEvent("HookedTreasure", this);
        }
    }

    private IEnumerator WaitToStartPulling()
    {
        yield return new WaitForSeconds(0.1f);
        if (_startedPulling||IsFullyPulled()) yield break;
        _startedPulling = true;
        _pullProgress = _pullAtStart;
    }

    internal bool IsFullyPulled()
    {
        return _pullProgress >= _pullToCollect;
    }
}
