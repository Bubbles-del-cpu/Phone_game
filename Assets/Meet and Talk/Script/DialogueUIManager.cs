using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Text.RegularExpressions;
using MeetAndTalk.GlobalValue;
using System.Linq;

namespace MeetAndTalk
{
    public class DialogueUIManager : MonoBehaviour
    {
        public static DialogueUIManager Instance;

        [Header("Type Writing")]                    // Premium Feature
        public bool EnableTypeWriting = false;      // Premium Feature
        public float typingSpeed = 50.0f;           // Premium Feature

        [Header("Dialogue UI")]
        public bool showSeparateName = false;
        public bool clearNameColor = false;         // Premium Feature
        public TMP_Text nameLabel;
        public MessagingBubble[] messageBubblePrefabs;
        [Space()]
        public UICanvas dialogueCanvas;
        public GameObject SkipButton;
        [SerializeField] bool _displayHints;
        public bool DisplayHints
        {
            get => _displayHints;
            set
            {
                _displayHints = value;
                SaveAndLoadManager.Instance.CurrentSave.DisplayHints = value;
                SaveAndLoadManager.Instance.AutoSave();
            }
        }

        [Header("Message & Notification Settings")]
        public float MessagePanelAutoScrollSpeed = 4;
        public float NotificationDisplayLength = 2;

        [Header("Dynamic Dialogue UI")]
        public MessagingResponseButton ButtonPrefab;

        [Header("Component References")]

        [SerializeField] Notification notificationPrefab;
        [SerializeField] RectTransform notificationsContainer;
        //public UIPanel ButtonContainer;

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
            if (Instance == null) Instance = this;

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
                if (obj != null) { obj.SetActive(left!=null); }
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

        public List<DialogueCharacterSO> Rollback(List<DialogueCharacterSO> characterPanel)
        {
            var emptyList = new List<DialogueCharacterSO>();
            foreach (var unique in characterPanel.Distinct())
            {
                var targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(unique);

                foreach (Transform child in targetPanel.ResponsesPanel.ResponseButtonsContainer.transform)
                    Destroy(child.gameObject);

                var countToRemove = characterPanel.Count(x => x == unique);
                targetPanel.RemoveElements(countToRemove);
                if (countToRemove >= targetPanel.ChildCount)
                    emptyList.Add(unique);
            }

            //List of dialogue character for whoes conversation panels are empty and therefore shouldn't be displayed on the contacts list yet
            return emptyList;

            // switch (nodeData)
            // {
            //     case DialogueNodeData nd:
            //         {


            //             if (nd.Post != null)
            //             {
            //                 GameManager.Instance.SocialMediaCanvas.Rollback(nd);
            //             }
            //         }
            //         break;
            //     case DialogueChoiceNodeData nd:
            //         {
            //             //DialogueChoiceNodes need to perform rollback twice so that we remove the player choice as well as the text from the character
            //             for (var index = 0; index < 2; index++)
            //             {
            //                 var targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
            //                 targetPanel.RemoveLastElement();
            //             }
            //         }
            //         break;
            // }
        }

        public void SetFullText(string text, BaseNodeData _nodeData, MessageSource messageSource, bool notification = true, bool updateSave = true)
        {
            string newText = GameManager.ToUTF32(text);

            Regex regex = new Regex(@"\{(.*?)\}");
            MatchEvaluator matchEvaluator = new MatchEvaluator(match =>
            {
                string OldText = match.Groups[1].Value;
                return ChangeReplaceableText(OldText);
            });

            newText = regex.Replace(newText, matchEvaluator);

            if (updateSave && messageSource == 0)
            {
                SaveAndLoadManager.Instance.CurrentSave.UpdateText(_nodeData, newText);
                SaveAndLoadManager.Instance.AutoSave();
            }

            MessagingConversationPanel targetPanel = null;
            var prefab = messageBubblePrefabs[(int)messageSource];

            //Notifcation if required
            if (messageSource == MessageSource.Character)
            {
                switch (_nodeData)
                {
                    case DialogueNodeData nd when _nodeData is DialogueNodeData:
                        if (notification)
                            SpawnNotification(Notification.NotificationType.Message, nd.Character, newText);

                        GameManager.Instance.SetNewMessage(nd.Character);
                        break;
                    case DialogueChoiceNodeData nd when _nodeData is DialogueChoiceNodeData:
                        if (nd.RequireCharacterInput)
                        {
                            if (notification)
                                SpawnNotification(Notification.NotificationType.Message, nd.Character, newText);

                            GameManager.Instance.SetNewMessage(nd.Character);
                        }
                        break;
                }
            }

            //Spawn the messaging bubbles
            switch (messageSource)
            {
                case MessageSource.Player:
                    switch (_nodeData)
                    {
                        case DialogueNodeData nd when _nodeData is DialogueNodeData:
                            targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                            break;
                        case DialogueChoiceNodeData nd when _nodeData is DialogueChoiceNodeData:
                            targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                            break;
                    }
                    targetPanel.AddElement(_nodeData, prefab, newText, messageSource, notification);
                    break;
                case MessageSource.Character:
                    switch (_nodeData)
                    {
                        case DialogueChoiceNodeData nd when _nodeData is DialogueChoiceNodeData:
                            {
                                targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                                targetPanel.AddElement(_nodeData, prefab, newText, messageSource, notification);
                            }
                            break;
                        case DialogueNodeData nd when _nodeData is DialogueNodeData:
                            {
                                targetPanel = GameManager.Instance.MessagingCanvas.GetConversationPanel(nd.Character);
                                targetPanel.AddElement(_nodeData, prefab, newText, messageSource, notification);

                                GameManager.Instance.GalleryCanvas.UnlockMediaButton(nd);
                                SaveAndLoadManager.Instance.CurrentSave.UnlockMedia(nd);
                            }
                            break;
                    }
                    break;
            }
        }



        public void SpawnNotification(Notification.NotificationType type, DialogueCharacterSO character, string label)
        {
            switch(type)
            {
                default:
                    GameManager.Instance.PlayReceiveTextFX();
                    if (GameManager.Instance.MessagingCanvas.GetConversationPanel(character).IsOpen)
                        return;
                    break;
                case Notification.NotificationType.SocialMedia:
                    GameManager.Instance.PlayNotificationFX();
                    if (GameManager.Instance.SocialMediaCanvas.IsOpen)
                        return;
                    break;
            }
            Notification _notification = Instantiate(notificationPrefab, notificationsContainer);
            _notification.Type = type;
            _notification.Character = character;
            _notification.Label = label;
        }

        public void SetButtons(DialogueCharacterSO character, BaseNodeData baseNode, List<string> texts, List<string> hints, List<UnityAction> unityActions, bool showTimer)
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
                btn.Init(baseNode, texts[i], hints[i], _panel.ResponsesPanel, () =>
                {
                    GameManager.Instance.MessagingCanvas.GetConversationButton(character).HasResponseReady = false;

                }, unityActions[i]);
            }

            _panel.ResponsesPanel.Open();
        }

        string ChangeReplaceableText(string text)
        {
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            string TextToReplace = "[Error Value]";
            /* Global Value */
            for (int i = 0; i < manager.IntValues.Count; i++) { if (text == manager.IntValues[i].ValueName) TextToReplace = manager.IntValues[i].Value.ToString(); }
            for (int i = 0; i < manager.FloatValues.Count; i++) { if (text == manager.FloatValues[i].ValueName) TextToReplace = manager.FloatValues[i].Value.ToString(); }
            for (int i = 0; i < manager.BoolValues.Count; i++) { if (text == manager.BoolValues[i].ValueName) TextToReplace = manager.BoolValues[i].Value.ToString(); }
            for (int i = 0; i < manager.StringValues.Count; i++) { if (text == manager.StringValues[i].ValueName) TextToReplace = manager.StringValues[i].Value; }

            //
            if(text.Contains(","))
            {
                string[] tmp = text.Split(',');
                for (int i = 0; i < manager.IntValues.Count; i++) { if (tmp[0] == manager.IntValues[i].ValueName) TextToReplace = Mathf.Abs(manager.IntValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
                for (int i = 0; i < manager.FloatValues.Count; i++) { if (tmp[0] == manager.FloatValues[i].ValueName) TextToReplace = Mathf.Abs(manager.FloatValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
            }

            return TextToReplace;
        }

        string RemoveRichTextTags(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }

    }
}
