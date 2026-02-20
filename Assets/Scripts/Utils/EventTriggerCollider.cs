using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class EventTriggerCollider : MonoBehaviour
{
    [SerializeField] private string enterEvent;
    [SerializeField] private string exitEvent;
    [SerializeField] private string stayEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.TriggerEvent(enterEvent);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.TriggerEvent(exitEvent);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.TriggerEvent(stayEvent);
        }
    }
}
