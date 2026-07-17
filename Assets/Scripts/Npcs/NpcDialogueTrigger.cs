using UnityEngine;

public class NpcDialogueTrigger : DialogueTrigger
{
    [SerializeField] private CityNpc npc;
    protected override void Awake()
    {
        base.Awake();
        npc.InteractEvent += TriggerNpcDialogue;
        _focusable = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        npc.InteractEvent -= TriggerNpcDialogue;
    }

    private void TriggerNpcDialogue()
    {
        EventManager.TriggerEvent("SetActiveDialogueNpc", npc);
        TriggerDialogue();
    }
}
