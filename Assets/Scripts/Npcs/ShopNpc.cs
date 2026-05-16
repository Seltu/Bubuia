using System;
using System.Collections;
using UnityEngine;

public class ShopNpc : MonoBehaviour
{
    [SerializeField] protected string _npcId; // Mesmo ID do NPC
    [SerializeField] private BoolVariable _npcInteractionAllowed;
    protected bool _storeIsActive = false;

    private void Start()     {
        EventManager.AddListener("CloseMenu", OnCloseMenu);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("CloseMenu", OnCloseMenu);
    }

    private void OnCloseMenu()
    {
        if (_storeIsActive)
        {
            EventManager.TriggerEvent("OnCloseStore");
            _storeIsActive = false;
            EventManager.TriggerEvent("OnChangeNpcInteractionStatus", true);
        }
    }

    public virtual void OpenStore()
    {
        EventManager.TriggerEvent("OnChangeNpcInteractionStatus", false);
        EventManager.TriggerEvent("OnOpenStore", _npcId);
        EventManager.TriggerEvent("OnBlockPlayerMovement");
        _storeIsActive = true;
        //gameObject.GetComponent<AudioCaller>().CallSFX("NpcBaloon");
    }
}
