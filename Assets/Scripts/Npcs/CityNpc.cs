using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CityNpc : NpcDetection
{
    internal event Action InteractEvent;
    [SerializeField] private InputActionReference _npcInteractionInput;
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
        InteractEvent.Invoke();
        _isInteracting = true;
    }

    public void OnDialogueEnd()
    {
        _isInteracting = false;
    }

}
