using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private PlayerInventorySO _playerInventory;
    [SerializeField] private Animator _coinAnimator;
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _collectRadius = 2f;
    private bool _collected;

    private void OnEnable()
    {
        _collected = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")||other.CompareTag("Hook"))
        {
            var approachPos = Vector3.Lerp(transform.position, other.transform.position, Time.deltaTime * _speed);
            transform.position = new Vector3(approachPos.x, transform.position.y, approachPos.z);
            if (!_collected && Vector3.Distance(transform.position, other.transform.position) < _collectRadius)
            {
                _collected = true;
                _coinAnimator.Play("CoinCollect");
                _playerInventory.playerMoney++;
                StartCoroutine(DisappearAfterSeconds(1f));
            }
        }
    }

    private IEnumerator DisappearAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
 