using System;
using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private PlayerInventorySO _playerInventory;
    [SerializeField] private Animator _coinAnimator;
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _collectRadius = 2f;
    private bool _collected;
    private TreasureSpot _treasureSpot;

    private void OnEnable()
    {
        EventManager.AddListener<TreasureSpot>("TreasureCaught", CheckTreasureCatch);
        _coinAnimator.Play("Appear");
        _collected = false;
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<TreasureSpot>("TreasureCaught", CheckTreasureCatch);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")||other.CompareTag("Hook"))
        {
            var approachPos = Vector3.Lerp(transform.position, other.transform.position, Time.deltaTime * _speed);
            transform.position = new Vector3(approachPos.x, transform.position.y, approachPos.z);
            if (!_collected && Vector3.Distance(transform.position, other.transform.position) < _collectRadius)
            {
                Collect();
            }
        }
    }

    private void CheckTreasureCatch(TreasureSpot treasure)
    {
        if (treasure == _treasureSpot)
            Collect();
    }

    private void Collect()
    {
        _collected = true;
        _coinAnimator.Play("Collect");
        _playerInventory.AddMoney(1);
        StartCoroutine(DisappearAfterSeconds(1f));
        EventManager.TriggerEvent("OnUpdateMoneyUI");
    }

    private IEnumerator DisappearAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }

    public void SetTreasure(TreasureSpot treasure)
    {
        _treasureSpot = treasure;
    }

    public void Vanish()
    {
        _coinAnimator.Play("Vanish");
        StartCoroutine(DisappearAfterSeconds(1f));
    }
}
 