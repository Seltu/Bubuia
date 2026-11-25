using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputfieldButtonChecker : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _confirmButton;

    private void OnEnable()
    {
        _inputField.onValueChanged.AddListener(UpdateConfirmButtonState);
        UpdateConfirmButtonState(_inputField.text);
    }

    private void OnDisable()
    {
        _inputField.onValueChanged.RemoveListener(UpdateConfirmButtonState);
    }

    private void UpdateConfirmButtonState(string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            _confirmButton.interactable = false;
        }
        else
        {
            _confirmButton.interactable = true;
        }
    }

}