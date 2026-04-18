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
    [SerializeField] private float _bubbleResizeSpeed = 10f;
    [SerializeField] private InputActionReference _clickAction;
    [SerializeField] private PlayerInventorySO _playerInventory;
    private Vector2 _textSize;
    private DialogueSegment _currentSegment;
    private bool _awaitingInput;
    private bool _showChoices;
    private float _sizeAdjustment;

    private void Awake()
    {
        if (_playDialogueOnAwake)
            StartDialogue(_loadedDialogue);
        EventManager.AddListener<DialogueSO>("LoadDialogue", StartDialogue);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<DialogueSO>("LoadDialogue", StartDialogue);
    }

    private void Update()
    {
        if (_clickAction.action.IsPressed() && _awaitingInput)
        {
            _awaitingInput = false;
            if (_showChoices)
                StartCoroutine(DisplayChoices());
            else
                NextSegment(0);
        }
        if(_sizeAdjustment < 1)
        {
            _dialoguePanel.rectTransform.sizeDelta = Vector2.LerpUnclamped(_dialoguePanel.rectTransform.sizeDelta, _textSize, _sizeAdjustment);
            _sizeAdjustment = Mathf.Clamp01(_sizeAdjustment + Time.deltaTime * _bubbleResizeSpeed);
        }
    }

    private void NextSegment(int option)
    {
        foreach (Transform child in _choicePanel)
        {
            Destroy(child.gameObject);
        }
        var nextNode = _currentSegment.GetNextDialogue(option);

        if(nextNode is DialogueSegment dialogueSegment)
        {
            foreach (ActionNode action in dialogueSegment.GetActions())
            {
                action.Act();
            }
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
        InputLock.movementLocked = true;
        StartCoroutine(DisplaySegment(dialogue.getStartSegment()));
    }

    private IEnumerator DisplayChoices()
    {
        foreach(Transform child in _choicePanel)
        {
            Destroy(child.gameObject);
        }
        _dialogueText.text = "";
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
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < list.Count; i++)
        {
            string choice = list[i];
            var _choiceText = Instantiate(_choicePrefab, _choicePanel);
            _choiceText.Choice = i;
            _choiceText.Text = choice;
            _choiceText.Button.onClick.AddListener(() => { NextSegment(_choiceText.Choice); });
        }
        _showChoices = false;
    }

    public IEnumerator DisplaySegment(DialogueSegment dialogueSegment)
    {
        _currentSegment = dialogueSegment;
        _dialogueText.text = "";
        _textSize.y = _dialogueText.GetPreferredValues(dialogueSegment.GetSentence()).y;
        _sizeAdjustment = 0;
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeSentence(dialogueSegment.GetSentence()));
        if (dialogueSegment.HasChoices)
            _showChoices = true;
        _awaitingInput = true;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        string hold = "";
        bool holding = false;
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
                yield return new WaitForSeconds(_textSpeed);
            }
        }
    }

    private void EndDialogue()
    {
        _dialoguePanel.gameObject.SetActive(false);
        InputLock.movementLocked = false;
        EventManager.TriggerEvent("EndDialogue");
    }
}
