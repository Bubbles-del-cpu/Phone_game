using MeetAndTalk;
using UnityEngine;
using System.Collections.Generic;

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
    
    // 1. ADDED: The missing button reference for Standalone Chapters
    [SerializeField] private GameAppButton _standaloneChapterButton; 
    
    // (Optional: Kept this if you still use it)
    [SerializeField] private GameAppButton _replayAppButton; 

    private List<GameAppButton> _allButtons;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // 2. UPDATED: Add the new button to the list for easier clearing
        _allButtons = new List<GameAppButton> 
        { 
            _messagingAppButton, 
            _socialMediaAppButton, 
            _galleryAppButton, 
            _settingsAppButton, 
            _replayAppButton,
            _standaloneChapterButton // Added here
        };

        // Setup button listeners
        if(_messagingAppButton) _messagingAppButton.OnClick.AddListener(OpenMessagingApp);
        if(_socialMediaAppButton) _socialMediaAppButton.OnClick.AddListener(OpenSocialMediaApp);
        if(_galleryAppButton) _galleryAppButton.OnClick.AddListener(OpenGalleryApp);
        if(_settingsAppButton) _settingsAppButton.OnClick.AddListener(OpenSettingsApp);
        
        // 3. UPDATED: Ensure Replay points to the correct method
        if(_replayAppButton) _replayAppButton.OnClick.AddListener(OpenReplayApp);

        // 4. UPDATED: Add listener for the new button
        if (_standaloneChapterButton) 
        {
            _standaloneChapterButton.OnClick.AddListener(OpenStandaloneChapter);
        }
    }

    private void OnDestroy()
    {
        if (_messagingAppButton) _messagingAppButton.OnClick.RemoveListener(OpenMessagingApp);
        if (_socialMediaAppButton) _socialMediaAppButton.OnClick.RemoveListener(OpenSocialMediaApp);
        if (_galleryAppButton) _galleryAppButton.OnClick.RemoveListener(OpenGalleryApp);
        if (_settingsAppButton) _settingsAppButton.OnClick.RemoveListener(OpenSettingsApp);
        if (_replayAppButton) _replayAppButton.OnClick.RemoveListener(OpenReplayApp);
        
        // 5. UPDATED: Clean up listener
        if (_standaloneChapterButton) _standaloneChapterButton.OnClick.RemoveListener(OpenStandaloneChapter);
    }

    public void ClearButtons()
    {
        foreach (var btn in _allButtons)
        {
            if (btn != null) btn.ClearAllNotifications();
        }
    }

    #region Navigation Methods

    private void OpenMessagingApp() => GameManager.Instance.MessagingCanvas.Open();
    private void OpenSocialMediaApp() => GameManager.Instance.SocialMediaCanvas.Open();
    private void OpenGalleryApp() => GameManager.Instance.GalleryCanvas.Open();
    private void OpenSettingsApp() => SettingsCanvas.Instance.Open();
    
    // 6. FIX: Point to the specific method that loads Story Chapters
    // Previously this was just .Open(), which confused the manager.
    private void OpenReplayApp() 
    {
        DialogueChapterManager.Instance.OpenChapterSelect();
    }

    // 7. FIX: Point to the specific method that loads Standalone Chapters
    private void OpenStandaloneChapter()
    {
        DialogueChapterManager.Instance.OpenStandaloneChapterSelect();
    }

    #endregion

    #region Notification Logic

    public void SetMessagingAppNotification(DialogueCharacterSO character, bool messageSeen, bool isResponseNotification = false)
    {
        if (_messagingAppButton == null) return;

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

    public void SetSocialMediaAppNotification(DialogueCharacterSO character, bool postSeen)
    {
        if (_socialMediaAppButton == null) return;

        if (postSeen)
            _socialMediaAppButton.RemoveNotifications(character);
        else
            _socialMediaAppButton.AddNotification(character);
    }

    #endregion
}