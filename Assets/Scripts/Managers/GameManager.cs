using UnityEngine;
using MeetAndTalk;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    bool _prevFullScreen;

    [HideInInspector] public bool ResettingSave;

    [SerializeField] DialogueCharacterSO[] characters;
    [SerializeField] DialogueManager dialogueManager;
    [SerializeField] DialogueContainerSO dialogue;
    [SerializeField] MessagingCanvas messagingCanvas;
    [SerializeField] GalleryCanvas galleryCanvas;
    [SerializeField] ContactsCanvas contactsCanvas;
    [SerializeField] SocialMediaCanvas socialMediaCanvas;
    [SerializeField] UICanvas settingsCanvas;
    [SerializeField] OverlayCanvas overlayCanvas;

    [Header("Config")]
    public GalleryUnlockConfig GalleryConfig;


    [Header("Audio")]
    [SerializeField] AudioClip notificationFX;
    [SerializeField] AudioClip receiveTextFX;
    [SerializeField] AudioClip sendTextFX;

    public AudioSource audioSource;

    [Header("Video Components")]
    [SerializeField] private VideoThumbnailGenerator _thumbnailGenerator;
    public VideoPlayer MainVideoPlayer;

    [Header("Component References")]
    [SerializeField]
    private RectTransform _mainDialogContentContainer;

    [Header("Universal Prefabs")]
    [SerializeField]
    private DialogWarningMessage _dialogPrefab;

    public int MaximumResponseLength = 46;
    [HideInInspector] public bool NextChapterReady = false;

    #region Props

    public DialogWarningMessage DialogPrefab => _dialogPrefab;
    public RectTransform DialogContainer => _mainDialogContentContainer;

    #endregion

    Dictionary<DialogueCharacterSO, CharacterData> characterData = new Dictionary<DialogueCharacterSO, CharacterData>();
    Dictionary<DialogueCharacterSO, bool> hasNewMessage = new Dictionary<DialogueCharacterSO, bool>();
    public Dictionary<VideoClip, Texture2D> Thumbnails = new Dictionary<VideoClip, Texture2D>();

    public static string ToUTF32FromPair(string input)
    {
        var output = input;

        Regex pattern = new Regex(@"\\u[a-zA-Z0-9]*\\u[a-zA-Z0-9]*");

        while (output.Contains(@"\u"))
        {
            output = pattern.Replace(output,
                m =>
                {
                    var pair = m.Value;
                    var first = pair.Substring(0, 6);
                    var second = pair.Substring(6, 6);
                    var firstInt = Convert.ToInt32(first.Substring(2), 16);
                    var secondInt = Convert.ToInt32(second.Substring(2), 16);
                    var codePoint = (firstInt - 0xD800) * 0x400 + (secondInt - 0xDC00) + 0x10000;
                    return @"\U" + codePoint.ToString("X8");
                },
                1
            );
        }

        return output;
    }

    public static string ToUTF32(string input)
    {
        string output = input;
        output = output.Replace(@"\u", @"\U000");
        return output;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        MainVideoPlayer.sendFrameReadyEvents = true;
        MainVideoPlayer.renderMode = VideoRenderMode.APIOnly;
        MainVideoPlayer.playOnAwake = false;
    }

    private void Start()
    {
        foreach (DialogueCharacterSO _character in characters)
            characterData.Add(_character, new CharacterData());
    }

    private void Update()
    {
        if (Screen.fullScreen != _prevFullScreen)
        {
            if (Screen.fullScreen)
            {
                SetupFullscreen();
            }
            else
            {
                SetupWindowed();
            }

            _prevFullScreen = Screen.fullScreen;
        }
    }

    public void TriggerDialogueChapter(DialogueContainerSO chapter)
    {
        dialogue = chapter;
        StartCoroutine(CoStartDialogue());
    }

    public void DisplayDialog(string message, Action eventToTrigger, string confirmButtonText = "Yes", bool twoButtonSetup = true, string cancelButtonTest = "No")
    {
        if (overlayCanvas)
        {
            overlayCanvas.ShowDialog(message, eventToTrigger, confirmButtonText, twoButtonSetup, cancelButtonTest);
        }
    }

    public void DisplayPopup(GameObject popup)
    {
        overlayCanvas.ShowDialog(popup);
    }

    public void ResetGameState()
    {
        StartCoroutine(CoResetConversations());

        //Reset the navigation stack
        NavigationManager.Instance.ResetStack();
    }

    IEnumerator CoResetConversations()
    {
        NextChapterReady = false;

        //Clear the messages within each conversation panel
        messagingCanvas.ClearConversations();

        DialogueManager.Instance.DisplaySpeedMultipler = DialogueManager.ResponseSpeed.X1;

        socialMediaCanvas.Clear();
        messagingCanvas.Close();

        //Restart the dialogue trees
        yield return new WaitForSeconds(.1f);

        settingsCanvas.Close();

        foreach(var item in FindObjectsByType<Notification>(FindObjectsSortMode.None))
        {
            item.gameObject.SetActive(false);
            Destroy(item);
        }

        //Double call to messaging canvas close in order to shut the contants window AND the message window
        messagingCanvas.Close();
    }

    IEnumerator CoStartDialogue()
    {
        if (SaveAndLoadManager.Instance.CurrentSave.CurrentChapterData.CurrentGUID == string.Empty)
            yield return new WaitForSeconds(3f);
        else
            yield return new WaitForSeconds(.25f);

        dialogueManager.StartDialogue(dialogue, SaveAndLoadManager.Instance.CurrentSave.CurrentChapterData);
        messagingCanvas.gameObject.SetActive(true);
    }

    public void PlayNotificationFX() {
        audioSource.PlayOneShot(notificationFX);
    }

    public void PlayReceiveTextFX()
    {
        audioSource.PlayOneShot(receiveTextFX);
    }

    public void PlaySendTextFX()
    {
        audioSource.PlayOneShot(sendTextFX);
    }

    public Texture2D GetVideoFrame(VideoClip clip)
    {
        //Create the texture
        if (!Thumbnails.ContainsKey(clip))
        {
            // _thumbnailVideoPrefab.clip = clip;
            // _thumbnailVideoPrefab.frame = 5;
            // _thumbnailVideoPrefab.Play();
            // _thumbnailVideoPrefab.Pause();
            // _thumbnailVideoPrefab.Prepare();

            _thumbnailGenerator.GenerateThumbnail(clip, (newTexture) =>
            {
                Thumbnails[clip] = newTexture;
            });

            Thumbnails.Add(clip, new Texture2D(256, 256, TextureFormat.RGB24, false));
        }

        return Thumbnails[clip];
    }

    public void SetVideoFrame(VideoClip clip, Sprite thumbnail)
    {
        if (!Thumbnails.ContainsKey(clip))
            Thumbnails.Add(clip, null);

        Thumbnails[clip] = thumbnail.texture;
    }

    public CharacterData GetCharacterData(DialogueCharacterSO _character)
    {
        return characterData[_character];
    }

    public void SetNewMessage(DialogueCharacterSO _character, bool _value = true)
    {
        if (MessagingCanvas.GetConversationPanel(_character).IsOpen && _value)
            return;

        hasNewMessage[_character] = _value;
        MessagingCanvas.GetConversationButton(_character).HasNewMessage = _value;
        if (_value)
            MessagingCanvas.GetConversationButton(_character).transform.SetAsFirstSibling();
    }

    public void SetupFullscreen()
    {
        // Your fullscreen setup logic here
        //Screen.SetResolution(720, 1280, FullScreenMode.FullScreenWindow);
    }

    public void SetupWindowed()
    {
        // Your windowed setup logic here
        //Screen.SetResolution(720, 1280, FullScreenMode.Windowed);
    }

    public DialogueCharacterSO[] Characters { get { return characters; } }

    public MessagingCanvas MessagingCanvas{ get { return messagingCanvas; } }
    public GalleryCanvas GalleryCanvas { get { return galleryCanvas; } }
    public ContactsCanvas ContactsCanvas { get { return contactsCanvas; } }
    public SocialMediaCanvas SocialMediaCanvas { get { return socialMediaCanvas; } }
    public UICanvas SettingsCanvas { get { return settingsCanvas; } }

    public class CharacterData
    {
        public List<Sprite> GalleryImages = new List<Sprite>();
    }

    public Dictionary<DialogueCharacterSO, bool> HasNewMessage { get { return hasNewMessage; } }
}
