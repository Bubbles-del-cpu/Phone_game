using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class MessagingConversationButton : MonoBehaviour
{
    [SerializeField] ProfileIcon icon;
    [SerializeField] TMP_Text nameLabel;

    [SerializeField] Image _unreadIndicator;
    [SerializeField] Image _responseIndicator;

    Button button;
    DialogueCharacterSO character;

    private bool _openSinceLastMessage = false;
    private bool _hasResponseWaiting = false;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            HasNewMessage = false;
            GameManager.Instance.MessagingCanvas.Open(character);
        });
    }

    public DialogueCharacterSO Character
    {
        set
        {
            character = value;
            icon.Character = value;
            nameLabel.text = value.name;
        }
    }

    public bool HasNewMessage
    {
        set
        {
            _openSinceLastMessage = !value;
            if (!value)
            {
                _responseIndicator.enabled = _hasResponseWaiting;
            }
            else
            {
                _responseIndicator.enabled = false;
            }
            
            _unreadIndicator.enabled = value;
        }
    }

    public bool HasResponseReady 
    { 
        set
        {
            _hasResponseWaiting = value;
            if (_hasResponseWaiting && !_unreadIndicator.enabled)
            {
                _responseIndicator.enabled = true;
                _unreadIndicator.enabled = false;
            }
            else
            {
                _responseIndicator.enabled = value;
            }
        }
    }
}
