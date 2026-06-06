using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ConversationLogUIHelper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _characterNameText;
    [SerializeField] private TextMeshProUGUI _conversationLogText;

    [Header("Scroll View")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private RectTransform _content;

    private const string _playerLogStyle = "PlayerLog";

    private void OnEnable()
    {
        _characterNameText.text = "";
        _conversationLogText.text = "";
    }

    public void UpdateConversationLog(string characterName, List<ConversationEntry> conversationLog)
    {
        _characterNameText.text = characterName;

        System.Text.StringBuilder sb = new();

        for (int i = 0; i < conversationLog.Count; i++)
        {
            string text = conversationLog[i].Text;

            if (conversationLog[i].Speaker == "Player")
                text = $"<style={_playerLogStyle}>{text}</style>";

            if (i > 0)
                sb.Append("\n\n");

            sb.Append(text);
        }

        _conversationLogText.text = sb.ToString();

        RefreshScrollView();
    }

    private void RefreshScrollView()
    {
        // Force TMP and Layout Groups to update
        Canvas.ForceUpdateCanvases();

        Debug.Log($"Viewport Height: {_viewport.rect.height}");
        Debug.Log($"Content Height: {_content.rect.height}");
        Debug.Log($"Text Preferred Height: {_conversationLogText.preferredHeight}");

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

        float contentHeight = _content.rect.height;
        float viewportHeight = _viewport.rect.height;

        bool needsScrolling = contentHeight > viewportHeight;

        // Only allow scrolling when necessary
        _scrollRect.vertical = needsScrolling;

        if (needsScrolling)
        {
            // Focus latest message (bottom)
            _scrollRect.verticalNormalizedPosition = 0f;
        }
        else
        {
            // Keep at top if everything fits
            _scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
