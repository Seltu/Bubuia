using UnityEngine;

public class NpcDialogueTrigger : DialogueTrigger
{
    [SerializeField] private CityNpc npc;
    protected override void Awake()
    {
        base.Awake();
        npc.InteractEvent += TriggerDialogue;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        npc.InteractEvent -= TriggerDialogue;
    }
}
