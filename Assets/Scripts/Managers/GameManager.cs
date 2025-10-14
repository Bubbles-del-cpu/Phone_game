using UnityEngine;
using MeetAndTalk;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] private Image _backgroundImageComponent;
    public Sprite DefaultBackgroundSprite;


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
    public Dictionary<VideoClip, (Texture2D, Sprite, bool)> Thumbnails = new Dictionary<VideoClip, (Texture2D, Sprite, bool)>();

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

    public void SetBackgroundImage(DialogueNodeData nodeData, bool socialMediaPost, bool save = true)
    {
        if (nodeData != null)
        {
            _backgroundImageComponent.sprite = socialMediaPost ? nodeData.Post.Image : nodeData.Image;
            var targetFileName = _backgroundImageComponent.sprite.name;
            var items = SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia.Where(x => x.NodeGUID == nodeData.NodeGuid);
            foreach (var item in items)
            {
                if (item.FileName == targetFileName)
                {
                    SaveAndLoadManager.Instance.CurrentSave.UnlockMedia(nodeData, false);
                    SaveAndLoadManager.Instance.CurrentSave.CustomBackgroundImage = item;

                    if (save)
                        SaveAndLoadManager.Instance.AutoSave();
                    break;
                }
            }
        }
    }

    public void SetBackgroundImage(Sprite image)
    {
        _backgroundImageComponent.sprite = image;
    }

    public void TriggerDialogueChapter(DialogueContainerSO chapter)
    {
        dialogue = chapter;
        StartCoroutine(CoStartDialogue());
    }

    public void DisplayDialog(string messageKey, Action eventToTrigger, string confirmButtonKey = "dialog_button_yes", object[] args = null, bool twoButtonSetup = true, string cancelButtonKey = "dialog_button_no")
    {
        if (overlayCanvas)
        {
            overlayCanvas.ShowDialog(messageKey, eventToTrigger, confirmButtonKey, twoButtonSetup, cancelButtonKey, args);
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

    public void GenerateThumbnails()
    {
        _thumbnailGenerator.RegenerateAll();
    }
    public (Texture2D, Sprite, bool) GetVideoFrame(VideoClip clip)
    {
        //Create the texture
        if (!Thumbnails.ContainsKey(clip))
        {
            _thumbnailGenerator.GenerateThumbnail(clip, (newTexture) =>
            {
                var sprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
                Thumbnails[clip] = (newTexture, sprite, Thumbnails[clip].Item3);
            });

            Thumbnails.Add(clip, (null, null, false));
        }

        return Thumbnails[clip];
    }

    public void SetVideoFrame(VideoClip clip, Sprite thumbnail)
    {
        if (clip == null)
            return;

        if (!Thumbnails.ContainsKey(clip))
            Thumbnails.Add(clip, (null, null, false));

        if (thumbnail != null)
            Thumbnails[clip] = (thumbnail.texture, thumbnail, thumbnail != null);
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
