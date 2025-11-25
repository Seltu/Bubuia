using System;
using System.Collections;
using UnityEngine;

public class ShopNpc : NpcDetection
{
    [SerializeField] protected string _npcId; // Mesmo ID do NPC
    [SerializeField] private BoolVariable _onMenu;
    protected bool _storeIsActive = false;

    private void Start()
    {
        EventManager.AddListener("Interact", OnInteract);
        EventManager.AddListener("CloseMenu", OnCloseMenu);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("Interact", OnInteract);
        EventManager.RemoveListener("CloseMenu", OnCloseMenu);
    }

    private void OnCloseMenu()
    {
        if (_storeIsActive)
        {
            EventManager.TriggerEvent("OnCloseStore");
            _storeIsActive = false;
            StartCoroutine(MenuFlagDelay());
        }
    }

    protected virtual void OnInteract()
    {
        if (_isInReach)
        {
            EventManager.TriggerEvent("OnOpenStore", _npcId);
            _storeIsActive = true;
            _onMenu.value = true;
            gameObject.GetComponent<AudioCaller>().CallSFX("NpcBaloon");
        }
    }

    private IEnumerator MenuFlagDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        _onMenu.value = false;
    }
    
}
