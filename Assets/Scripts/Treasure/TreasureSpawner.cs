using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform player;

    [Header("Prefabs")]
    [SerializeField] private GameObject treasureSpotPrefab;
    [SerializeField] private GameObject coinPrefab;

    [Header("Treasure Placement")]
    [SerializeField] private float minDistanceFromPlayer = 20f;
    [SerializeField] private float maxDistanceFromPlayer = 60f;
    [SerializeField] private Vector2 maxPosition;
    [SerializeField] private Vector2 minPosition;
    [SerializeField] private int maxPlacementAttempts = 60;
    [SerializeField] private float spawnDelay = 3f;

    [Header("Coin Trail")]
    [SerializeField] private float coinSpacing = 3.0f;
    [SerializeField] private float coinStartPaddingFromPlayer = 5f; // evita coin colada no player
    [SerializeField] private float coinEndPaddingFromTreasure = 2.5f; // evita coin colada no spot

    [Header("Cleanup")]
    [SerializeField] private bool destroyCoinsWhenTreasureIsGone = true;

    private GameObject _currentTreasure;
    private Camera _mainCamera;
    private readonly List<Coin> _spawnedCoins = new();
    private void Awake()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        if (_currentTreasure == null) return;
        if (Vector3.Distance(_currentTreasure.transform.position, player.position ) > maxDistanceFromPlayer)
        {
            Destroy(_currentTreasure);
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Só começa a contar tempo se NÃO existir treasure ativo
            yield return new WaitUntil(() => _currentTreasure == null);

            yield return new WaitForSeconds(spawnDelay);

            // (opcional) limpeza quando o treasure some
            if (destroyCoinsWhenTreasureIsGone)
                CleanupCoins();

            // Se ainda não existe, spawna
            if (_currentTreasure == null)
                SpawnTreasureAndTrail();
        }
    }

    private void SpawnTreasureAndTrail()
    {
        if (_mainCamera == null || treasureSpotPrefab == null || coinPrefab == null)
        {
            Debug.LogWarning("[TreasureDirector] Missing references.");
            return;
        }

        if (!TryGetOffscreenTreasurePosition(out Vector3 treasurePos))
        {
            // Se não achou lugar fora da câmera, não spawna agora (tenta de novo no próximo loop)
            return;
        }

        _currentTreasure = Instantiate(treasureSpotPrefab, treasurePos, Quaternion.identity);

        // Cria trilha de moedas igualmente espaçadas do player até o treasure,
        // mas só instancia moedas fora da câmera no momento do spawn.
        SpawnCoinTrail(player.position, treasurePos);
    }

    private bool TryGetOffscreenTreasurePosition(out Vector3 result)
    {
        Vector3 origin = _mainCamera.transform.position;
        origin.y = 0f;

        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector2 dir2 = Random.insideUnitCircle.normalized;
            float dist = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

            Vector3 candidate = origin + new Vector3(dir2.x, 0f, dir2.y) * dist;

            if (candidate.x > maxPosition.x || candidate.x < minPosition.x || candidate.z > maxPosition.y || candidate.z < minPosition.y)
                continue;

            result = candidate;
            return true;
        }

        result = default;
        return false;
    }

    private void SpawnCoinTrail(Vector3 fromPlayer, Vector3 toTreasure)
    {
        CleanupCoins();

        Vector3 start = fromPlayer;
        Vector3 end = toTreasure;

        // Trabalha no plano XZ para o cálculo, mas depois “gruda no chão” se snapToGround
        Vector3 startFlat = new Vector3(start.x, 0f, start.z);
        Vector3 endFlat = new Vector3(end.x, 0f, end.z);

        Vector3 dir = (endFlat - startFlat).normalized;
        float totalDist = Vector3.Distance(startFlat, endFlat);

        float usableDist = totalDist - coinStartPaddingFromPlayer - coinEndPaddingFromTreasure;
        if (usableDist <= coinSpacing) return;

        int count = Mathf.FloorToInt(usableDist / coinSpacing);
        float first = coinStartPaddingFromPlayer + coinSpacing; // primeira moeda “real”

        for (int i = 0; i < count; i++)
        {
            float d = first + (i * coinSpacing);
            if (d >= totalDist - coinEndPaddingFromTreasure) break;

            Vector3 pos = startFlat + dir * d;
            pos.y = start.y; // base

            Coin coin = (Coin) PoolManager.Instance.ReuseComponent(coinPrefab, pos, Quaternion.identity);
            _spawnedCoins.Add(coin);
        }
    }

    private void CleanupCoins()
    {
        // Se as moedas se auto-destroem, isso só limpa referências antigas e evita lixo.
        for (int i = _spawnedCoins.Count - 1; i >= 0; i--)
        {
            if (_spawnedCoins[i] == null)
            {
                _spawnedCoins.RemoveAt(i);
                continue;
            }

            if (destroyCoinsWhenTreasureIsGone)
                _spawnedCoins[i].Vanish();
        }
        _spawnedCoins.Clear();
    }

    // Opcional: se você quiser matar tudo ao desabilitar cena/objeto
    private void OnDisable()
    {
        if (destroyCoinsWhenTreasureIsGone)
            CleanupCoins();
    }
}
