using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class NPCCutsceneTrigger : MonoBehaviour
{
    [SerializeField] private CutsceneConditionSO _cutsceneCondition;
    [SerializeField] private DialogueSO dialogue;
    //[SerializeField] private NPCExit npc;
    [SerializeField] private PlayableDirector _cutsceneTimeline;
    [SerializeField] private Animator _npcAnimator;

    private bool started;
    private bool dialogueFinished;

    private void OnEnable()
    {
        EventManager.AddListener("EndDialogue", OnDialogueFinished);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("EndDialogue", OnDialogueFinished);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (started)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (_cutsceneCondition != null && _cutsceneCondition.CheckCutsceneCondition() == false)
            return;

        started = true;

        InputLock.movementLocked = true;

        _npcAnimator.SetBool("isInteracting", true);

        EventManager.TriggerEvent("CutsceneStarted");
        EventManager.TriggerEvent("LoadDialogue", dialogue);
    }

    private void OnDialogueFinished()
    {
        if (!started)
            return;

        dialogueFinished = true;

        _npcAnimator.SetBool("isInteracting", false);

        _cutsceneTimeline.Play();
        StartCoroutine(ExitSequence());
    }

    private IEnumerator ExitSequence()
    {
        //SpriteRenderer sprite = npc.GetComponentInChildren<SpriteRenderer>();

        while (_cutsceneTimeline.time < _cutsceneTimeline.duration)
            yield return null;

        InputLock.movementLocked = false;

        EventManager.TriggerEvent("CutsceneEnded");

        Destroy(gameObject);
    }
}
