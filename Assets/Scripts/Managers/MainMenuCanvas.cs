using MeetAndTalk;
using UnityEngine;

public class MainMenuCanvas : MonoBehaviour
{
    private static MainMenuCanvas _instance;
    public static MainMenuCanvas Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<MainMenuCanvas>();

            return _instance;
        }
    }

    [Header("App Buttons")]
    [SerializeField] private GameAppButton _messagingAppButton;
    [SerializeField] private GameAppButton _socialMediaAppButton;
    [SerializeField] private GameAppButton _galleryAppButton;
    [SerializeField] private GameAppButton _settingsAppButton;
    [SerializeField] private GameAppButton _replayAppButton;

    private void Awake()
    {
        // Setup button listeners
        _messagingAppButton.OnClick.AddListener(OpenMessagingApp);
        _socialMediaAppButton.OnClick.AddListener(OpenSocialMediaApp);
        _galleryAppButton.OnClick.AddListener(OpenGalleryApp);
        _replayAppButton.OnClick.AddListener(OpenReplayApp);

        _settingsAppButton.OnClick.AddListener(() =>
        {
            SettingsCanvas.Instance.Open();
        });
    }

    public void ClearButtons()
    {
        _messagingAppButton.ClearAllNotifications();
        _socialMediaAppButton.ClearAllNotifications();
        _galleryAppButton.ClearAllNotifications();
        _replayAppButton.ClearAllNotifications();
        _settingsAppButton.ClearAllNotifications();
    }

    /// <summary>
    /// Opens the messaging app for the current character
    /// </summary>
    private void OpenMessagingApp()
    {
        GameManager.Instance.MessagingCanvas.Open();
    }

    /// <summary>
    /// Opens the social media app for the current character
    /// </summary>
    private void OpenSocialMediaApp()
    {
        GameManager.Instance.SocialMediaCanvas.Open();
    }

    /// <summary>
    /// Opens the gallery app for the current character
    /// </summary>
    private void OpenGalleryApp()
    {
        GameManager.Instance.GalleryCanvas.Open();
    }

    private void OpenReplayApp()
    {
        DialogueChapterManager.Instance.Open();
    }

    /// <summary>
    /// Sets the messaging app notification state
    /// </summary>
    /// <param name="character">The character for whom the notification state is being set</param>
    /// <param name="messageSeen">Indicates whether the message has been seen</param>
    /// <param name="isResponseNotification">Indicates whether the notification is a response notification</param>
    public void SetMessagingAppNotification(DialogueCharacterSO character, bool messageSeen, bool isResponseNotification = false)
    {
        if (messageSeen)
        {
            if (isResponseNotification)
                _messagingAppButton.RemoveResponseNotification(character);
            else
                _messagingAppButton.RemoveNotifications(character);
        }
        else
        {
            if (isResponseNotification)
                _messagingAppButton.AddResponseNotification(character);
            else
                _messagingAppButton.AddNotification(character);
        }
    }

    /// <summary>
    /// Sets the social media app notification state
    /// </summary>
    /// <param name="character">The character for whom the notification state is being set</param>
    /// <param name="postSeen">Indicates whether the post has been seen</param>
    public void SetSocialMediaAppNotification(DialogueCharacterSO character, bool postSeen)
    {
        if (postSeen)
        {
            _socialMediaAppButton.RemoveNotifications(character);
        }
        else
        {
            _socialMediaAppButton.AddNotification(character);
        }
    }
}