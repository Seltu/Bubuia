using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDetection : MonoBehaviour
{
    [SerializeField] protected GameObject _balloon;
    protected bool _isInReach = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _balloon.SetActive(true);
        _isInReach = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _balloon.SetActive(false);
        _isInReach = false;
    }
}
