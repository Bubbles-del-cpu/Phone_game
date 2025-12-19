using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;

namespace MeetAndTalk
{
    public class DialogueUIManager : MonoBehaviour
    {
        private static DialogueUIManager _instance;
        public static DialogueUIManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindFirstObjectByType<DialogueUIManager>();

                return _instance;
            }
        }

        [Header("Type Writing")]                    // Premium Feature
        public bool EnableTypeWriting = false;      // Premium Feature
        public float typingSpeed = 50.0f;           // Premium Feature
        public float MessagingBubbleFadeInSpeed = 2f;

        [Header("Dialogue UI")]
        public bool showSeparateName = false;
        public bool clearNameColor = false;         // Premium Feature
        public TMP_Text nameLabel;
        public MessagingBubble[] messageBubblePrefabs;
        [Space()]
        public UICanvas dialogueCanvas;
        [SerializeField] bool _displayHints;
        public bool DisplayHints
        {
            get => _displayHints;
            set
            {
                _displayHints = value;
                SaveAndLoadManager.Instance.CurrentSave.DisplayHints = value;
            }
        }

        [Header("Dynamic Dialogue UI")]
        public MessagingResponseButton ButtonPrefab;

        [Header("Message & Notification Settings")]
        public float MessagePanelAutoScrollSpeed = 4;
        public float NotificationDisplayLength = 2;
        public float MaxMessageSize = 200;


        [Header("Hide IF Condition")]
        public List<GameObject> HideIfLeftAvatarEmpty = new List<GameObject>();         // Premium Feature
        public List<GameObject> HideIfRightAvatarEmpty = new List<GameObject>();        // Premium Feature
        public List<GameObject> HideIfChoiceEmpty = new List<GameObject>();             // Premium Feature

        [HideInInspector] public string prefixText;
        [HideInInspector] public string fullText;
        private string currentText = "";
        private int characterIndex = 0;
        private float lastTypingTime;

        public enum MessageSource
        {
            Character,
            Player
        }

        private void Awake()
        {
            // Premium Feature: Type-Writing
            if (EnableTypeWriting) lastTypingTime = Time.time;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void UpdateAvatars(DialogueCharacterSO left, DialogueCharacterSO right, AvatarType emotion)
        {
            foreach (GameObject obj in HideIfLeftAvatarEmpty)
            {
                if (obj != null) { obj.SetActive(left != null); }
            }
            foreach (GameObject obj in HideIfRightAvatarEmpty)
            {
                if (obj != null) { obj.SetActive(right != null); }
            }
        }

        public void ResetText(string prefix)
        {
            // Premium Feature: Clean Name
            if (clearNameColor) prefix = RemoveRichTextTags(prefix);

            currentText = prefix;
            prefixText = prefix;
            characterIndex = 0;
        }

        public void SetSeparateName(string name)
        {
            // Premium Feature: Clean Name
            if (clearNameColor) name = RemoveRichTextTags(name);

            nameLabel.text = name;
        }

        public List<DialogueCharacterSO> Rollback(Dictionary<DialogueCharacterSO, int> characterPanel)
        {
            var emptyList = new List<DialogueCharacterSO>();
            foreach (var (character, count) in characterPanel)
            {
                var targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(character);

                //Clear the response panel of any options
                foreach (Transform child in targetPanel.ResponsesPanel.ResponseButtonsContainer.transform)
                    Destroy(child.gameObject);

                targetPanel.RemoveElements(count);
                if (targetPanel.ChildCount == 0)
                    emptyList.Add(character);
            }

            //List of dialogue character for whoes conversation panels are empty and therefore shouldn't be displayed on the contacts list yet
            return emptyList;
        }

        public void PopulatePreviousMessage(List<LanguageGeneric<string>> texts, BaseNodeData _nodeData, MessageSource messageSource)
        {
            MessagingConversationPanel targetPanel = null;
            var prefab = messageBubblePrefabs[(int)messageSource];
            switch (_nodeData)
            {
                case DialogueNodeData nd when _nodeData is DialogueNodeData:
                    targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                    break;
                case DialogueChoiceNodeData nd when _nodeData is DialogueChoiceNodeData:
                    targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                    break;
            }
            targetPanel.AddElement(_nodeData, DialogueLocalizationHelper.GetConvertedText(DialogueLocalizationHelper.GetText(texts)), messageSource);
        }

        public void SetFullText(List<LanguageGeneric<string>> texts, BaseNodeData nodeData, MessageSource messageSource, bool notification = true, bool updateSave = true)
        {
            MessagingConversationPanel targetPanel = null;

            //Notifcation if required
            if (messageSource == MessageSource.Character)
            {
                switch (nodeData)
                {
                    case DialogueNodeData nd when nodeData is DialogueNodeData:
                        var notificationText = DialogueLocalizationHelper.GetConvertedText(DialogueLocalizationHelper.GetText(nd.Texts));
                        if (notificationText == string.Empty)
                        {
                            if (nd.Image != null || nd.Video != null)
                            {
                                notificationText = $"has sent a new {(nd.MediaType == MediaType.Sprite ? "picture" : "video")}";
                                if (notification)
                                    NotificationCanvas.Instance.SpawnNotification(Notification.NotificationType.Message, nd.Character, notificationText);
                            }
                        }
                        else
                        {
                            if (notification)
                                NotificationCanvas.Instance.SpawnNotification(Notification.NotificationType.Message, nd.Character, notificationText);
                        }

                        GameManager.Instance.SetNewMessage(nd.Character);
                        break;
                    case DialogueChoiceNodeData nd when nodeData is DialogueChoiceNodeData:
                        if (nd.RequireCharacterInput)
                        {
                            if (notification)
                                NotificationCanvas.Instance.SpawnNotification(Notification.NotificationType.Message,
                                    nd.Character,
                                    DialogueLocalizationHelper.GetConvertedText(DialogueLocalizationHelper.GetText(nd.TextType)));

                            GameManager.Instance.SetNewMessage(nd.Character);
                        }
                        break;
                }
            }

            if (updateSave && messageSource == MessageSource.Character)
            {
                SaveAndLoadManager.Instance.CurrentSave.UpdateText(nodeData, texts);
            }

            //Spawn the messaging bubbles
            switch (messageSource)
            {
                case MessageSource.Player:
                    {
                        switch (nodeData)
                        {
                            case DialogueNodeData nd when nodeData is DialogueNodeData:
                                if (nd.Post != null)
                                    SocialMediaCanvas.PostToSoicalMediaApp(nd.Post, nd, showNotification: true);
                                targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                                break;
                            case DialogueChoiceNodeData nd when nodeData is DialogueChoiceNodeData:
                                targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                                break;
                        }
                        var newText = DialogueLocalizationHelper.GetConvertedText(DialogueLocalizationHelper.GetText(texts));
                        targetPanel.AddElement(nodeData, newText, messageSource);
                    }
                    break;
                case MessageSource.Character:
                    {
                        switch (nodeData)
                        {
                            case DialogueNodeData nd:
                                {
                                    if (nd.Post != null)
                                        SocialMediaCanvas.PostToSoicalMediaApp(nd.Post, nd, showNotification: true);
                                    targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);

                                    //Unlock any media associated with this node
                                    var saveData = SaveAndLoadManager.Instance.CurrentSave;
                                    saveData.UnlockMedia(nd);
                                    GameManager.Instance.GalleryCanvas.UnlockMediaButton(nd, reloadedGallery: true);
                                    if (notification)
                                        GameManager.Instance.MessagingCanvas.SetNewNotification(nd.Character, responseNotification: false, messageSeen: false);
                                }
                                break;
                            case DialogueChoiceNodeData nd:
                                {
                                    targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);

                                    if (notification)
                                        GameManager.Instance.MessagingCanvas.SetNewNotification(nd.Character, responseNotification: true, messageSeen: false);
                                }
                                break;
                        }

                        var newText = DialogueLocalizationHelper.GetConvertedText(DialogueLocalizationHelper.GetText(texts));
                        targetPanel.AddElement(nodeData, newText, messageSource);
                    }
                    break;
            }
        }

        public void SetButtons(DialogueCharacterSO character, BaseNodeData baseNode, List<List<LanguageGeneric<string>>> texts, List<List<LanguageGeneric<string>>> hints, List<UnityAction> unityActions, bool showTimer)
        {
            // Hide If Choice Empty
            GameManager.Instance.MessagingCanvas.GetConversationButton(character).HasResponseReady = texts.Count > 0;
            foreach (GameObject obj in HideIfChoiceEmpty)
            {
                if (obj != null && texts.Count > 0) { obj.SetActive(true); }
                else if (obj != null) { obj.SetActive(false); }
            }

            MessagingConversationPanel _panel = GameManager.Instance.MessagingCanvas.GetConversationPanel(character);
            foreach (Transform child in _panel.ResponsesPanel.ResponseButtonsContainer.transform)
                GameObject.Destroy(child.gameObject);


            var stringLengthLimit = GameManager.Instance.MaximumResponseLength;
            for (int i = 0; i < texts.Count; i++)
            {
                MessagingResponseButton btn = Instantiate(ButtonPrefab, _panel.ResponsesPanel.ResponseButtonsContainer.transform);
                btn.Init(baseNode, texts[i], hints[i], _panel.ResponsesPanel,
                () =>
                {
                    GameManager.Instance.MessagingCanvas.SetNewNotification(character, responseNotification: true, messageSeen: true);
                },
                unityActions[i]);
            }

            _panel.ResponsesPanel.Open();
        }

        string RemoveRichTextTags(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }

    }
}
