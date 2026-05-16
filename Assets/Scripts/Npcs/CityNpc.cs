using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CityNpc : NpcDetection
{
    [SerializeField] private InputActionReference _npcInteractionInput;
    internal event Action InteractEvent;
    private bool _isInteracting;

    private void Awake()
    {
        _npcInteractionInput.action.performed += OnInteract;
    }

    private void OnDestroy()
    {
        _npcInteractionInput.action.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!_isInReach || _isInteracting) return;
        if(!_npcInteractionAllowed.value) return;

        InteractEvent.Invoke();
        _isInteracting = true;
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", false);
    }

    public void OnDialogueEnd()
    {
        _isInteracting = false;
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", true);
    }
}
