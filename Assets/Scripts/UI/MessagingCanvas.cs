using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;

public class MessagingCanvas : UICanvas
{
    [Space(10f)]
    [Header("Messaging")]
    [SerializeField] ProfileIcon icon;
    [SerializeField] Image profileImage;
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] MessagingConversationButton conversationButtonPrefab;
    [SerializeField] RectTransform conversationButtonsContainer;
    [SerializeField] RectTransform conversationContainer;
    [SerializeField] UIPanel conversationsPanel;
    [SerializeField] MessagingConversationPanel conversationPanelPrefab;
    [SerializeField] Sprite timelapsePanelBackground;
    [Range(0f, 100f)]
    [SerializeField] float bubbleMarginRight = 32f;
    [SerializeField]
    private GameObject _noContactsMessage;

    Dictionary<DialogueCharacterSO, MessagingConversationButton> buttons = new Dictionary<DialogueCharacterSO, MessagingConversationButton>();
    Dictionary<DialogueCharacterSO, MessagingConversationPanel> conversations = new Dictionary<DialogueCharacterSO, MessagingConversationPanel>();

    List<DialogueCharacterSO> seenCharacters = new List<DialogueCharacterSO>();

    public void ConversationClosed()
    {
        profileImage.enabled = false;
        nameLabel.enabled = false;
    }

    public override void Close()
    {
        bool _wasConversationOpen = false;
        foreach(UIPanel _panel in conversations.Values)
        {
            if (_panel.IsOpen)
            {
                _wasConversationOpen = true;
                _panel.Close();
            }
        }
        if(!_wasConversationOpen)
            base.Close();

    }

    public void ClearConversations()
    {
        foreach (var item in conversations)
        {
            item.Value.Close();
            item.Value.Clear();
        }

        foreach (var item in buttons)
        {
            DestroyImmediate(item.Value.gameObject);
        }

        seenCharacters.Clear();
        buttons.Clear();
        conversations.Clear();
    }

    public void Open(DialogueCharacterSO _character)
    {
        icon.Character = _character;
        nameLabel.text = _character.name;
        GameManager.Instance.MessagingCanvas.Close();
        conversations[_character].Open();
        GameManager.Instance.MessagingCanvas.Open();
        profileImage.enabled = true;
        nameLabel.enabled = true;
    }

    private void CheckCharacter(DialogueCharacterSO character)
    {
        if (!seenCharacters.Contains(character))
        {
            _noContactsMessage?.SetActive(false);

            seenCharacters.Add(character);

            MessagingConversationButton _button = Instantiate(conversationButtonPrefab, conversationButtonsContainer);
            _button.Character = character;
            buttons.Add(character, _button);

            MessagingConversationPanel _panel = Instantiate(conversationPanelPrefab, conversationContainer);
            _panel.transform.SetSiblingIndex(_panel.transform.GetSiblingIndex() - 1);
            _panel.Character = character;
            conversations.Add(character, _panel);
        }
    }

    public MessagingConversationPanel GetConversationPanel(DialogueCharacterSO _character)
    {
        CheckCharacter(_character);
        return conversations[_character];
    }

    public MessagingConversationButton GetConversationButton(DialogueCharacterSO _character)
    {
        CheckCharacter(_character);
        return buttons[_character];
    }

    public void RemoveConversationPanel(DialogueCharacterSO character)
    {
        if (seenCharacters.Contains(character))
        {
            if (buttons.ContainsKey(character))
            {
                buttons[character].gameObject.SetActive(false);
                Destroy(buttons[character].gameObject);
            }

            if (conversations.ContainsKey(character))
            {
                conversations[character].gameObject.SetActive(false);
                Destroy(conversations[character].gameObject);
            }

            buttons.Remove(character);
            conversations.Remove(character);
            seenCharacters.Remove(character);
        }

        if (seenCharacters.Count == 0)
            _noContactsMessage?.SetActive(true);
    }

    public Sprite TimelapsePanelBackground { get { return timelapsePanelBackground; } }
    public float BubbleMarginRight { get { return bubbleMarginRight; } }
}
