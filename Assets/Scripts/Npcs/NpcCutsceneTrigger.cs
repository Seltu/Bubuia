using System.Collections;
using UnityEngine;
using UnityEngine.Playables;


public class NPCCutsceneTrigger : DialogueTrigger
{
    [SerializeField] private CutsceneConditionSO _cutsceneCondition;
    //[SerializeField] private NPCExit npc;
    [SerializeField] private PlayableDirector _cutsceneTimeline;
    [SerializeField] private Animator _npcAnimator;
    [SerializeField] private bool _disappearOnConditionFail;

    private bool started;
    private bool dialogueFinished;

    private void Start()
    {
        _focusable = true;
        if (_cutsceneCondition != null && _cutsceneCondition.CheckCutsceneCondition() == false)
        {
            if (_disappearOnConditionFail)
                Destroy(gameObject);
        }
    }

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

        started = true;

        _npcAnimator.SetBool("isInteracting", true);

        EventManager.TriggerEvent("CutsceneStarted");
        TriggerDialogue();
    }

    private void OnDialogueFinished()
    {
        if (!started)
            return;

        dialogueFinished = true;

        _npcAnimator.SetBool("isInteracting", false);

        _cutsceneTimeline.Play();

        InputLock.movementLocked = true;
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
