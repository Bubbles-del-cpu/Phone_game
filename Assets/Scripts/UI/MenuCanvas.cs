using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class MenuCanvas : UICanvas
{
    //[Space(10f)]
    // [Header("Menu")]
    // [SerializeField] private Button chapterReplayButton;
    // [SerializeField] private Button galleryButton;
    // [SerializeField] private Button settingsButton;
    // [SerializeField] private Button socialMediaButton;
    // [SerializeField] private Button messagingButton;
    // [SerializeField] private Button standaloneChapterButton;


    protected override void Awake()
    {
        base.Awake();

        // messagingButton.onClick.AddListener(() => GameManager.Instance.MessagingCanvas.Open());
        // galleryButton.onClick.AddListener(() => GameManager.Instance.GalleryCanvas.Open(MediaType.Sprite));
        // socialMediaButton.onClick.AddListener(() => GameManager.Instance.SocialMediaCanvas.Open());

        // //Open the settings canvas when settings button is clicked
        // settingsButton.onClick.AddListener(() => GameManager.Instance.SettingsCanvas.Open());
    }

    // public void SpawnNotification(
    //     Notification.NotificationType _type,
    //     DialogueCharacterSO _character,
    //     string _label)
    // {
    //     switch(_type)
    //     {
    //         default:
    //             GameManager.Instance.PlayReceiveTextFX();
    //             if (GameManager.Instance.MessagingCanvas.GetConversationPanel(_character).IsOpen)
    //                 return;
    //             break;
    //         case Notification.NotificationType.SocialMedia:
    //             GameManager.Instance.PlayNotificationFX();
    //             if (GameManager.Instance.SocialMediaCanvas.IsOpen)
    //                 return;
    //             break;
    //     }
    //     Notification _notification = Instantiate(notificationPrefab, notificationsContainer);
    //     _notification.Type = _type;
    //     _notification.Character = _character;
    //     _notification.Label = _label;
    // }
}
