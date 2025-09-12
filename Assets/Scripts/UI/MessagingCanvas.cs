using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;

public class MessagingCanvas : UICanvas
{
    [Space(10f)]
    [Header("Messaging")]
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
        conversationsPanel.Close();
    }

    // public override void Close()
    // {
    //     bool _wasConversationOpen = false;
    //     foreach(UIPanel _panel in conversations.Values)
    //     {
    //         if (_panel.IsOpen)
    //         {
    //             _wasConversationOpen = true;
    //             _panel.Close();
    //         }
    //     }

    //     if (!_wasConversationOpen)
    //         base.Close();
    // }

    public void Close(bool fullClose, DialogueCharacterSO character)
    {
        var openCount = 0;
        foreach (UIPanel _panel in conversations.Values)
        {
            if (_panel.IsOpen)
                openCount++;
        }

        if (character != null && conversations.ContainsKey(character))
            conversations[character].Close();

        if (openCount <= 1)
        {
            conversationsPanel.Close();
            if (fullClose)
                base.Close();
        }
    }

    public void ClearConversations()
    {
        foreach (var item in conversations)
        {
            item.Value.Close();
            item.Value.Clear();
            Destroy(item.Value.gameObject);
        }

        foreach (var item in buttons)
        {
            Destroy(item.Value.gameObject);
        }

        seenCharacters.Clear();
        buttons.Clear();
        conversations.Clear();
    }

    public override void Open()
    {
        conversationsPanel.Close();
        base.Open();
    }

    public void Open(DialogueCharacterSO _character, bool fromNotification = false)
    {
        if (_character != null)
        {
            var command = new ConversationOpenCommand(this, openState: true, _character, fromNotification);
            NavigationManager.Instance.InvokeCommand(command, allowUndo: true);
        }
    }

    public void SetupPanel(DialogueCharacterSO _character)
    {
        foreach ((var key, var panel) in conversations)
        {
            if (key == _character)
                continue;

            panel.Close();
        }

        conversations[_character].Open();
        conversationsPanel.Open();
    }

    private void CheckCharacter(DialogueCharacterSO character)
    {
        if (!seenCharacters.Contains(character))
        {
            _noContactsMessage?.SetActive(false);

            seenCharacters.Add(character);

            MessagingConversationButton _button = Instantiate(conversationButtonPrefab, conversationButtonsContainer);
            _button.name = $"{character.name} (MessagingConversationButton)";
            _button.Character = character;
            buttons.Add(character, _button);

            MessagingConversationPanel _panel = Instantiate(conversationPanelPrefab, conversationContainer);
            _panel.name = $"{character.name} (MessagingConversationPanel)";
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
