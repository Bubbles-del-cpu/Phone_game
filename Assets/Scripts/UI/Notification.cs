using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections;
using Unity.Burst.Intrinsics;

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

    private float _alphaTimer = 0;
    private float _timer = 0;
    private bool _revealComplete;
    private bool _destroyTriggered;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClicked());
        closeButton.onClick.AddListener(() => Destroy(gameObject));
        _timer = 0;

        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 1;
        Destroy(gameObject, DialogueUIManager.Instance.NotificationDisplayLength);
    }

    // private void Update()
    // {
    //     if (_revealComplete)
    //     {
    //         _timer += Time.deltaTime;
    //         if (_timer >= DialogueUIManager.Instance.NotificationDisplayLength)
    //         {
    //             _alphaTimer += Time.deltaTime;
    //             _cg.alpha = Mathf.Lerp(0, 1, Mathf.Clamp(1 - (_alphaTimer / DialogueUIManager.Instance.NotificationRevealTime), 0, 1));
    //             if (_cg.alpha <= 0 && !_destroyTriggered)
    //                 StartCoroutine(CoDestroy());
    //         }
    //     }
    //     else
    //     {
    //         _alphaTimer += Time.deltaTime;
    //         _cg.alpha = Mathf.Lerp(0, 1, Mathf.Clamp(_alphaTimer / DialogueUIManager.Instance.NotificationRevealTime, 0, 1));
    //         if (_cg.alpha >= 1)
    //         {
    //             _revealComplete = true;
    //             _alphaTimer = 0;
    //         }
    //     }
    // }

    // private IEnumerator CoDestroy()
    // {
    //     _destroyTriggered = true;
    //     var rectTransform = GetComponent<RectTransform>();
    //     while (rectTransform.sizeDelta.y >= 0)
    //     {
    //         yield return new WaitForEndOfFrame();
    //         rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - (DialogueUIManager.Instance.NotificationRemovalSpeed * Time.deltaTime));
    //     }

    //     Destroy(gameObject);
    // }

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
