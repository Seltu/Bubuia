using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _treasureBoxInstance;
    private TreasureSpot _treasureSpot;

    public void SetSpot(TreasureSpot spot)
    {
        _treasureSpot = spot;
    }

    public void OpenChest()
    {
        for (var i = 0; i < 10; i++)
        {
            Coin coin = (Coin)PoolManager.Instance.ReuseComponent(_coinPrefab, _treasureBoxInstance.transform.position + new Vector3(Random.Range(-2f, 2f), 2.5f, Random.Range(-2f, 2f)), Quaternion.identity);
            coin.SetTreasure(_treasureSpot);
        }
        EventManager.TriggerEvent("TreasureCaught", _treasureSpot);

        Destroy(gameObject, 2f);
    }
}
