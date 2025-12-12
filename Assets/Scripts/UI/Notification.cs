using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class Notification : MonoBehaviour
{
    public enum NotificationType
    {
        Message,
        SocialMedia
    }

    [SerializeField] ProfileIcon icon;
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text _label;
    [SerializeField] Button closeButton;
    [SerializeField] Image[] typeIcons;
    [SerializeField] float displayTime = 5f;

    private NotificationType _type;
    private Button _button;
    private DialogueCharacterSO _character;
    private CanvasGroup _cg;
    public string Label => _label.text;

    private float _destroyTimer = 0;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnClicked());
        closeButton.onClick.AddListener(() => Destroy(gameObject));

        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 1;
    }

    public void Setup(NotificationType type, DialogueCharacterSO character, string message)
    {
        switch (type)
        {
            case NotificationType.SocialMedia:
                _label.text = "has made a new post";
                break;
            case NotificationType.Message:
                _label.text = message;
                break;
        }

        Type = type;
        Character = character;
        _destroyTimer = 0;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        _destroyTimer += Time.deltaTime;
        if (_destroyTimer >= DialogueUIManager.Instance.NotificationDisplayLength)
        {
            DialogueUIManagerObjectPool.Instance.ReturnNotification(this);
        }
    }

    void OnClicked()
    {
        switch(_type)
        {
            default:
                //GameManager.Instance.SocialMediaCanvas.Close();
                GameManager.Instance.MessagingCanvas.Open(_character, fromNotification: true);
                break;
            case NotificationType.SocialMedia:
                //GameManager.Instance.MessagingCanvas.Close();
                GameManager.Instance.SocialMediaCanvas.Open();
                break;
        }

        DialogueUIManagerObjectPool.Instance.ReturnNotification(this);
    }

    public NotificationType Type
    {
        get { return _type; }
        set
        {
            _type = value;
            for(var index = 0; index < typeIcons.Length; index++)
            {
                typeIcons[index].enabled = index == (int)value;
            }
        }
    }

    public DialogueCharacterSO Character
    {
        get => _character;
        set
        {
            _character = value;
            icon.Character = value;
            nameLabel.text = value.name;
        }
    }
}
