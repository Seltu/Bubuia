using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _treasureBoxInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenChest()
    {
        for (var i = 0; i < 10; i++)
        {
            PoolManager.Instance.ReuseComponent(_coinPrefab, _treasureBoxInstance.transform.position + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)), Quaternion.identity);
        }

        Destroy(gameObject, 2f);
    }
}
