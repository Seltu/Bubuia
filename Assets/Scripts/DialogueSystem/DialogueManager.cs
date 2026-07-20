using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SearchService;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private Image _dialoguePanel;
    [SerializeField] private Transform _choicePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private DialogueSO _loadedDialogue;
    [SerializeField] private DialogueChoiceUI _choicePrefab;
    [SerializeField] private bool _playDialogueOnAwake;
    [SerializeField] private float _textSpeed = 0.02f;
    [SerializeField] private float _boxResizeSpeed = 10f;
    [SerializeField] private float _minBoxHeight = 10f;
    [SerializeField] private InputActionReference _clickAction;
    [SerializeField] private InputActionReference _rightClickAction;
    [SerializeField] private PlayerInventorySO _playerInventory;
    [SerializeField] private Animator _optionalAnimator;
    private Vector2 _textSize;
    private DialogueSegment _currentSegment;
    private bool _awaitingInput;
    private bool _skipTyping;
    private bool _showChoices;
    private bool _hasAnimator;
    private float _sizeAdjustment;
    private bool isCutsceneDialogue;
    private CityNpc _dialoguingNpc;

    [Header("Conversation Log")]
    [SerializeField] private GameObject _conversationLogPanel;
    [SerializeField] private List<ConversationEntry> _conversationLog;

    [Header("Skip Icons")]
    [SerializeField] private GameObject _skipIcons;

    private void Awake()
    {
        if (_playDialogueOnAwake)
            StartDialogue(_loadedDialogue);
        EventManager.AddListener<DialogueSO>("LoadDialogue", StartDialogue);
        EventManager.AddListener<CityNpc>("SetActiveDialogueNpc", SetActiveDialogueNpc);
        if(_optionalAnimator != null)
            _hasAnimator = true;
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<DialogueSO>("LoadDialogue", StartDialogue);
        EventManager.RemoveListener<CityNpc>("SetActiveDialogueNpc", SetActiveDialogueNpc);
    }

    private void Update()
    {
        if (_clickAction.action.WasPressedThisFrame() && _dialoguePanel.gameObject.activeSelf && !_conversationLogPanel.activeSelf)
        {
            if (_awaitingInput)
            {
                _awaitingInput = false;
                if (_showChoices)
                    StartCoroutine(DisplayChoices());
                else
                    NextSegment(0);
            }
            else
            {
                _skipTyping = true;
            }
        }

        if (_sizeAdjustment < 1)
        {
            _sizeAdjustment = Mathf.Clamp01(_sizeAdjustment + Time.deltaTime * _boxResizeSpeed);
            _dialoguePanel.rectTransform.sizeDelta = Vector2.LerpUnclamped(_dialoguePanel.rectTransform.sizeDelta, _textSize, _sizeAdjustment);
        }

        if (_rightClickAction.action.IsPressed() && _awaitingInput)
        {
            _awaitingInput = false;

            if (!_conversationLogPanel.activeSelf)
                StartCoroutine(ShowConversationLog());
            else
                StartCoroutine(HideConversationLog());
        }
    }

    private void NextSegment(int option)
    {
        foreach (Transform child in _choicePanel)
        {
            Destroy(child.gameObject);
        }
        var nextNode = _currentSegment.GetNextDialogue(option);

        foreach (ActionNode action in _currentSegment.GetActions())
        {
            action.Act();
        }

        if (nextNode is DialogueSegment dialogueSegment)
        {
            StartCoroutine(DisplaySegment(dialogueSegment));
        }
        else if (nextNode is ChoiceNode choiceNode)
        {
            StartCoroutine(DisplaySegment(choiceNode.Decide(_playerInventory)));
        }
        else
        {
            EndDialogue();
        }
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        _dialoguePanel.gameObject.SetActive(true);
        _skipIcons.SetActive(true);
        InputLock.movementLocked = true;
        InputLock.clickLocked = true;
        isCutsceneDialogue = dialogue.isCutsceneDialogue;
        if (isCutsceneDialogue)
        {

        }
        StartCoroutine(DisplaySegment(dialogue.getStartSegment()));
    }

    private IEnumerator DisplayChoices()
    {
        foreach(Transform child in _choicePanel)
        {
            Destroy(child.gameObject);
        }
        _dialogueText.text = "";
        _skipIcons.SetActive(false);
        var choiceText = "";
        List<string> list = _currentSegment.GetChoices();
        for (int i = 0; i < list.Count; i++)
        {
            string choice = list[i];
            choiceText += choice.ToString();
            if(i < list.Count - 1)
            choiceText += "\n\n";
        }
        _textSize.y = _dialogueText.GetPreferredValues(choiceText).y;
        _sizeAdjustment = 0;
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < list.Count; i++)
        {
            string choice = list[i];
            var _choiceText = Instantiate(_choicePrefab, _choicePanel);
            _choiceText.Choice = i;
            _choiceText.Text = choice;
            _choiceText.Button.onClick.AddListener(() =>
            {
                _conversationLog.Add(
                    new ConversationEntry(
                        "Player",
                        _choiceText.Text
                    )
                );

                NextSegment(_choiceText.Choice);
            });
        }
        _showChoices = false;
    }

    public IEnumerator DisplaySegment(DialogueSegment dialogueSegment)
    {
        _currentSegment = dialogueSegment;
        _dialogueText.text = "";
        _textSize.y = Mathf.Max(_dialogueText.GetPreferredValues(dialogueSegment.GetSentence()).y, _minBoxHeight);
        _sizeAdjustment = 0;
        yield return new WaitForSeconds(0.2f);
        SetDialoguingNpcTalkingStatus(true);
        yield return StartCoroutine(TypeSentence(dialogueSegment.GetSentence()));
        SetDialoguingNpcTalkingStatus(false);
        if (dialogueSegment.HasChoices)
            _showChoices = true;
        _awaitingInput = true;

        // Add current segment to conversation log
        _conversationLog.Add(
            new ConversationEntry(
                dialogueSegment.GetActorName(),
                dialogueSegment.GetSentence()
            )
        );
    }

    private IEnumerator TypeSentence(string sentence)
    {
        string hold = "";
        bool holding = false;
        _skipTyping = false;
        if (_hasAnimator)
            _optionalAnimator.SetBool("Typing", true);
        foreach (var letter in sentence.ToCharArray())
        {
            if (letter == '<')
                holding = true;
            else if (letter == '>')
            {
                _dialogueText.text += hold;
                holding = false;
                hold = "";
            }
            if (holding)
                hold += letter;
            else
            {
                _dialogueText.text += letter;
                if(!_skipTyping)
                    yield return new WaitForSeconds(_textSpeed);
            }
        }
        if (_hasAnimator)
            _optionalAnimator.SetBool("Typing", false);

        _skipIcons.SetActive(true);
    }

    private void EndDialogue()
    {
        _dialoguePanel.gameObject.SetActive(false);
        StartCoroutine(InputUnlockDelay());
        SetDialoguingNpcTalkingStatus(false);
        EventManager.TriggerEvent("EndDialogue");
        _dialoguingNpc = null;
        // Clear conversation log
        _conversationLog.Clear();
    }

    private void SetActiveDialogueNpc(CityNpc cityNpc)
    {
        _dialoguingNpc = cityNpc;
    }

    private void SetDialoguingNpcTalkingStatus(bool isTalking)
    {
        if (_dialoguingNpc == null)
            return;

        _dialoguingNpc.SetNpcTalkingManually(isTalking);
    }

    IEnumerator InputUnlockDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        if (!isCutsceneDialogue) InputLock.movementLocked = false;
        InputLock.clickLocked = false;
    }

    IEnumerator ShowConversationLog()
    {
        _conversationLogPanel.SetActive(true);
        _conversationLogPanel.GetComponentInChildren<ConversationLogUIHelper>().UpdateConversationLog(_currentSegment.GetActorName(), _conversationLog);

        yield return new WaitForSeconds(0.5f);

        _awaitingInput = true;
    }

    IEnumerator HideConversationLog()
    {
        _conversationLogPanel.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        _awaitingInput = true;
    }
}

[System.Serializable]
public class ConversationEntry
{
    public string Speaker;
    public string Text;

    public ConversationEntry(string speaker, string text)
    {
        Speaker = speaker;
        Text = text;
    }
}
