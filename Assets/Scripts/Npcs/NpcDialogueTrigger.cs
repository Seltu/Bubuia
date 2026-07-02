using UnityEngine;

public class NpcDialogueTrigger : DialogueTrigger
{
    [SerializeField] private CityNpc npc;
    protected override void Awake()
    {
        base.Awake();
        npc.InteractEvent += TriggerDialogue;
        _focusable = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        npc.InteractEvent -= TriggerDialogue;
    }
}
