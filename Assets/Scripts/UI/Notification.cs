using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class Notification : MonoBehaviour
{
    [SerializeField] ProfileIcon icon;
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text label;
    [SerializeField] Button closeButton;
    [SerializeField] Image[] typeIcons;
    [SerializeField] float displayTime = 5f;

    NotificationType type;
    Button button;
    DialogueCharacterSO character;
    private CanvasGroup _cg;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClicked());
        closeButton.onClick.AddListener(() => Destroy(gameObject));

        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 1;
        Destroy(gameObject, DialogueUIManager.Instance.NotificationDisplayLength);
    }

    void OnClicked()
    {
        switch(type)
        {
            default:
                //GameManager.Instance.SocialMediaCanvas.Close();
                GameManager.Instance.MessagingCanvas.Open(character, fromNotification: true);
                break;
            case NotificationType.SocialMedia:
                //GameManager.Instance.MessagingCanvas.Close();
                GameManager.Instance.SocialMediaCanvas.Open();
                break;
        }

        Destroy(gameObject);
    }

    public NotificationType Type
    {
        get { return type; }
        set
        {
            type = value;
            typeIcons[(int)value].enabled = true;

            switch(type)
            {
                default:  // message
                    label.text = "sent you a new message";
                    break;
                case NotificationType.SocialMedia:
                    label.text = "shared a new post";
                    break;
            }
        }
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

    public string Label { set { label.text = value; } }

    public enum NotificationType
    {
        Message,
        SocialMedia
    }
}
