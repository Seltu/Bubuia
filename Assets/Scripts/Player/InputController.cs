using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;


    private void Start()
    {
        EventManager.AddListener("OnBlockPlayerMovement", BlockPlayerMovement);
        EventManager.AddListener("OnAllowPlayerMovement", AllowPlayerMovement);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("OnBlockPlayerMovement", BlockPlayerMovement);
        EventManager.RemoveListener("OnAllowPlayerMovement", AllowPlayerMovement);
    }

    public void CallInteraction()
    {
        EventManager.TriggerEvent("Interact");
    }

    public void CallCloseMenu()
    {
        EventManager.TriggerEvent("CloseMenu");
    }

    private void BlockAllInputs()
    {
        _playerInput.DeactivateInput();
    }

    private void EnableAllInputs()
    {
        _playerInput.ActivateInput();
    }

    private void BlockPlayerMovement()
    {
        _playerInput.actions.FindAction("Movement").Disable();
    }

    private void AllowPlayerMovement()
    {
        _playerInput.actions.FindAction("Movement").Enable();
    }
}
