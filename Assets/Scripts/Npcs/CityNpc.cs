using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class CityNpc : NpcDetection
{
    [SerializeField] private InputActionReference _npcInteractionInput;
    [SerializeField] private Animator _npcAnimator;
    internal event Action InteractEvent;
    private bool _isInteracting;

    private void Awake()
    {
        _npcInteractionInput.action.performed += OnInteract;
        EventManager.AddListener("StartDialogue", SetAnimationBool);
    }

    private void OnDestroy()
    {
        _npcInteractionInput.action.performed -= OnInteract;
        EventManager.RemoveListener("EndDialogue", SetAnimationBool);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!_isInReach || _isInteracting) return;
        if(!_npcInteractionAllowed.value) return;

        InteractEvent.Invoke();
        _isInteracting = true;
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", false);
        _npcAnimator.SetBool("isInteracting", _isInteracting);
    }

    public void OnDialogueEnd()
    {
        _isInteracting = false;
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", true);
        _npcAnimator.SetBool("isInteracting", _isInteracting);
    }

    public void SetAnimationBool()
    {
        _npcAnimator.SetBool("isInteracting", _isInteracting);
    }
}
