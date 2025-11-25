using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIButtons : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private PlayerInput _playerInputSystem;
    [SerializeField] private float moveXValue;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("down");
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("up");
        EventManager.TriggerEvent("OnMovementButtonPress", 0);
    }
}
