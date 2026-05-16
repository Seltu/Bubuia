using UnityEngine;

public class NPC_Controller : MonoBehaviour
{
    [SerializeField] private BoolVariable _npcInteractionAllowed;
    [SerializeField] private bool _canInteractFromStart = true;

    private void Start()
    {
        EventManager.AddListener<bool>("OnChangeNpcInteractionStatus", SetNpcIntection);

        SetNpcIntection(_canInteractFromStart);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<bool>("OnChangeNpcInteractionStatus", SetNpcIntection);
    }

    private void SetNpcIntection(bool status)
    {
        _npcInteractionAllowed.value = status;
    }
}
