using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using System.Linq;

namespace MeetAndTalk
{
    public class DialogueManager : DialogueGetData
    {
        public const float BASE_NODE_DISPLAY_TIME = 2;
        public const float REPOPULATE_YIELD_COUNT = 10;

        [HideInInspector] public static DialogueManager Instance;
        public LocalizationManager localizationManager;

        [HideInInspector] public DialogueUIManager dialogueUIManager;
        [FormerlySerializedAs("audioSource")] public AudioSource AudioSource;
        public DialogueUIManager MainUI;

        public enum ResponseSpeed
        {
            X1 = 1,
            X2 = 2,
            X4 = 4
        }
        public ResponseSpeed DisplaySpeedMultipler;
        public float PostChoiceDelay;

        public UnityEvent StartDialogueEvent;
        public UnityEvent EndDialogueEvent;

        private BaseNodeData currentDialogueNodeData;
        private BaseNodeData lastDialogueNodeData;
        private TimerChoiceNodeData _nodeTimerInvoke;
        private DialogueNodeData _nodeDialogueInvoke;
        private DialogueChoiceNodeData _nodeChoiceInvoke;

        private List<Coroutine> activeCoroutines = new List<Coroutine>();
        private Stack<BaseNodeData> _visitedNodes = new Stack<BaseNodeData>();

        private bool _populatingHistory = false;
        private bool _populatHistoryFailed = false;
        private bool _dialogueStarted = false;
        private bool _failLoadDialogShown = false;
        private ChapterSaveData _chapterData;
        [SerializeField] private RectTransform _loadingIndicator;
        private void Awake()
        {
            Instance = this;

            // Setup UI
            DialogueUIManager[] all = FindObjectsByType<DialogueUIManager>(FindObjectsSortMode.None);
            foreach (DialogueUIManager ui in all) { if (ui.dialogueCanvas != null) ui.dialogueCanvas.Close(); }

            DialogueUIManager.Instance = MainUI;
            dialogueUIManager = DialogueUIManager.Instance;

            AudioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_dialogueStarted || dialogueContainer == null || _chapterData == null)
            {
                if (_dialogueStarted)
                    _loadingIndicator.gameObject.SetActive(false);

                return;
            }

            if (_populatingHistory)
            {
                _loadingIndicator.gameObject.SetActive(true);
            }
            else
            {
                _loadingIndicator.gameObject.SetActive(false);
                if (_populatHistoryFailed)
                {
                    if (!_failLoadDialogShown)
                    {
                        _failLoadDialogShown = true;
                        //The save file could not be loaded for this chapter, this implies that it is out of date with the latest verison.
                        //Reset the game state and start the Player from the top of the chapter
                        GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.INVALID_SAVE_DATA, () =>
                        {
                            //Reset the chapter data in the save file
                            SaveAndLoadManager.Instance.ClearChapterData(_chapterData.FileIndex);

                            OverlayCanvas.Instance.FadeToBlack(() =>
                            {
                                SaveAndLoadManager.Instance.LoadSave();
                                GameManager.Instance.ResetGameState();
                            });

                        }, GameConstants.UIElementKeys.CONTINUE, args: null, twoButtonSetup: false);
                    }
                }
                else if (!_dialogueStarted)
                {
                    Debug.Log("Loading Dialogue Completed");
                    _dialogueStarted = true;
                    CheckNodeType(GetNodeByGuid(_chapterData.CurrentGUID));
                    StartDialogueEvent.Invoke();
                }
            }
        }

        /// <summary>
        /// Pozwala na zmiane aktualnego UI Dialogu
        /// </summary>
        /// <param name="UI"></param>
        public void ChangeUI(DialogueUIManager UI)
        {
            // Setup UI
            if (UI != null) DialogueUIManager.Instance = UI;
            else Debug.LogError("DialogueUIManager.UI Object jest Pusty!");
        }

        /// <summary>
        /// Pozwala na przypisanie aktualnego dialogu
        /// </summary>
        /// <param name="dialogue"></param>
        public void SetupDialogue(DialogueContainerSO dialogue, ChapterSaveData chapterData)
        {
            if (dialogue != null)
                dialogueContainer = dialogue;

            _chapterData = chapterData;
        }

       // public void StartDialogue(DialogueContainerSO dialogue) { StartDialogue(dialogue, ""); }
        //public void StartDialogue(string ID) { StartDialogue(null, ID); }
        //public void StartDialogue() { StartDialogue(null, ""); }
        public void StartDialogue(DialogueContainerSO DialogueSO, ChapterSaveData chapterData)
        {
            // Update Dialogue UI
            dialogueUIManager = DialogueUIManager.Instance;
            _visitedNodes = new Stack<BaseNodeData>();

            // Setup Dialogue
            SetupDialogue(DialogueSO, chapterData);

            // Error: No Setup Dialogue
            if (dialogueContainer == null)
                Debug.LogError("Error: Dialogue Container SO is not assigned!");

            // Check ID
            if (dialogueContainer.StartNodeDatas.Count == 0)
                Debug.LogError("Error: No Start Node in Dialogue Container!");

            if (string.IsNullOrEmpty(chapterData.CurrentGUID) || SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            {
                _dialogueStarted = true;
                TriggerDialogueStart(new ChapterSaveData());
                StartDialogueEvent.Invoke();
            }
            else
            {
                //Draw out all the conversation up until this point but it cannot be done with notifications
                _populatingHistory = true;
                _failLoadDialogShown = false;
                Debug.Log("Loading Dialogue History...");
                StartCoroutine(PopulateHistoryCoroutine(chapterData));
            }
        }

        private IEnumerator PopulateHistoryCoroutine(ChapterSaveData chapterData)
        {
            bool loadSuccess = true;
            var count = 0;
            foreach (var item in chapterData.PastCoversations)
            {
                count++;
                if (count >= REPOPULATE_YIELD_COUNT)
                {
                    count = 0;
                    yield return null;
                }

                var node = GetNodeByGuid(item.GUID);
                if (node == null)
                {
                    //We failed to find the node by the GUID that was saved to file. This implies that their current chapter save data is invalid.
                    //Eaxit to start the Player from the top of the chapter using the latest data
                    loadSuccess = false;
                    break;
                }

                if (item.GUID == chapterData.CurrentGUID)
                {
                    continue;
                }

                _visitedNodes.Push(node);
                if (node.GetType() == typeof(DialogueChoiceNodeData) || node.GetType() == typeof(TimerChoiceNodeData))
                {
                    var dNode = (DialogueChoiceNodeData)node;
                    if (item.SelectedChoice != "")
                    {
                        var found = false;
                        //We have an old save that was made before localization. Compare the texts against the localized version and attempt to display the correct translation
                        foreach (var lang in dNode.DialogueNodePorts)
                        {
                            if (lang.TextLanguage.Any(x => x.LanguageGenericType == item.SelectedChoice))
                            {
                                //We found the matching selection from the port. Update and choose that
                                item.SelectedChoiceTexts = lang.TextLanguage;
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            item.SelectedChoiceTexts = dNode.DialogueNodePorts.First().TextLanguage;
                        }
                    }
                }

                //Update these to blank as we will be using the new LanguageGenerics
                //Soon to be removed as obsolete
                item.SelectedChoice = "";
                item.Text = "";

                switch (node)
                {
                    case EventNodeData:
                        break;
                    case DialogueNodeData nd:
                        item.Texts = nd.Texts;
                        dialogueUIManager.PopulatePreviousMessage(nd.Texts, node, DialogueUIManager.MessageSource.Character);
                        break;
                    case TimerChoiceNodeData tChoiceNode when node is TimerChoiceNodeData:
                        if (tChoiceNode.RequireCharacterInput)
                        {
                            dialogueUIManager.PopulatePreviousMessage(tChoiceNode.TextType, node, DialogueUIManager.MessageSource.Character);
                        }
                        dialogueUIManager.PopulatePreviousMessage(item.SelectedChoiceTexts, tChoiceNode, DialogueUIManager.MessageSource.Player);
                        tChoiceNode.SelectedChoice = item.SelectedChoiceTexts;
                        break;
                    case DialogueChoiceNodeData dChoiceNode when node is DialogueChoiceNodeData:
                        if (dChoiceNode.RequireCharacterInput)
                        {
                            dialogueUIManager.PopulatePreviousMessage(dChoiceNode.TextType, node, DialogueUIManager.MessageSource.Character);
                        }
                        dialogueUIManager.PopulatePreviousMessage(item.SelectedChoiceTexts, dChoiceNode, DialogueUIManager.MessageSource.Player);
                        dChoiceNode.SelectedChoice = item.SelectedChoiceTexts;
                        break;
                }
            }

            _populatingHistory = false;
            _populatHistoryFailed = !loadSuccess;
        }

        private void TriggerDialogueStart(ChapterSaveData chapterData)
        {
            StartNodeData startNode = null;
            if (chapterData.StartID == string.Empty)
            {
                startNode = dialogueContainer.StartNodeDatas[Random.Range(0, dialogueContainer.StartNodeDatas.Count)];
            }
            else
            {
                // IF FInd ID assign Data
                foreach (StartNodeData data in dialogueContainer.StartNodeDatas)
                {
                    if (data.startID == chapterData.CurrentGUID)
                    {
                        startNode = data;
                        break;
                    }
                }
            }

            CheckNodeType(startNode);
        }

        public void Rollback()
        {
            OverlayCanvas.Instance.FadeToBlack(() =>
            {
                CoRollback();
                SaveAndLoadManager.Instance.AutoSave();
            }, .25f, .25f);
        }

        private void CoRollback()
        {
            bool targetFound = false;
            bool ignoreFirstChoice = true;
            bool atStart = false;
            BaseNodeData targetNode = lastDialogueNodeData;

            Dictionary<DialogueCharacterSO, int> rollbackList = new();
            var socialPostRollbackCount = 0;
            while (!targetFound)
            {
                if (_visitedNodes.TryPop(out BaseNodeData node))
                {
                    SaveAndLoadManager.Instance.CurrentSave.RemoveNode(node);
                    switch (node)
                    {
                        case DialogueNodeData nd:
                            if (!rollbackList.ContainsKey(nd.Character))
                                rollbackList.Add(nd.Character, 1);

                            if (nd.GetText() != string.Empty || nd.Image != null || nd.Video != null)
                                rollbackList[nd.Character] += 1;

                            if (nd.GetTimeLapse() != string.Empty)
                                rollbackList[nd.Character] += 1;

                            if (nd.Post != null)
                                socialPostRollbackCount++;
                            break;
                        case DialogueChoiceNodeData choiceNode:
                            if (!rollbackList.ContainsKey(choiceNode.Character))
                                rollbackList.Add(choiceNode.Character, 0);

                            //Removes the text from the character that is displayed before the choice
                            if(choiceNode.RequireCharacterInput)
                                rollbackList[choiceNode.Character] += 1;

                            if (ignoreFirstChoice)
                            {
                                ignoreFirstChoice = false;
                            }
                            else
                            {
                                targetFound = true;

                                //Removes the dialogue choice that the Player selected so that it can be rollbacked and re-selected
                                if (DialogueLocalizationHelper.GetText(choiceNode.SelectedChoice).Length > 0 && DialogueLocalizationHelper.GetText(choiceNode.SelectedChoice)[0] != '*')
                                    rollbackList[choiceNode.Character] += 1;

                                targetNode = choiceNode;
                            }
                            break;
                        case EventNodeData eventNode:
                            foreach (var item in eventNode.EventScriptableObjects)
                            {
                                if (item.DialogueEventSO != null)
                                {
                                    item.DialogueEventSO.RollbackEvent();
                                }
                            }
                            break;
                        case StartNodeData:
                            atStart = true;
                            targetFound = true;
                            break;
                    }
                }
                else
                {
                    //Stack is empty so we must have reached the start of the dialogue tree
                    //This is a caught for a failed stack that for some reason doesn't have the start node inside
                    atStart = true;
                    break;
                }
            }

            var emptyList = DialogueUIManager.Instance.Rollback(rollbackList);
            GameManager.Instance.SocialMediaCanvas.RemovePosts(socialPostRollbackCount);

            if (emptyList.Count > 0)
            {
                //Note: If empty list has any elements at least 1 will be the current character conversation panel
                foreach (var character in emptyList)
                {
                    NavigationManager.Instance.UndoLast();
                    GameManager.Instance.MessagingCanvas.RemoveConversationPanel(character);
                }
            }

            if (atStart)
            {
                //GameManager.Instance.ResetGameState();
                //GameManager.Instance.MessagingCanvas.ConversationClosed();

                SaveAndLoadManager.Instance.CurrentSave.CurrentChapterData.CurrentGUID = string.Empty;
                GameManager.Instance.TriggerDialogueChapter(dialogueContainer);
            }
            else
            {
                //Open the corresponding character conversation panel if there is one to open
                //This should only be relevant if we have triggered a close via the "emptyList" above
                switch (targetNode)
                {
                    case DialogueNodeData nd:
                        GameManager.Instance.MessagingCanvas.Open(nd.Character);
                        break;
                    case TimerChoiceNodeData nd:
                        GameManager.Instance.MessagingCanvas.Open(nd.Character);
                        break;
                    case DialogueChoiceNodeData nd:
                        GameManager.Instance.MessagingCanvas.Open(nd.Character);
                        break;

                }

                CheckNodeType(targetNode);
            }
        }

        public void CheckNodeType(BaseNodeData _baseNodeData)
        {
            if (GameManager.Instance.ResettingSave)
                return;

            SaveAndLoadManager.Instance.CurrentSave.AddNode(_baseNodeData);
            SaveAndLoadManager.Instance.AutoSave();

            _visitedNodes.Push(_baseNodeData);
            switch (_baseNodeData)
            {
                case StartNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case TimerChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case EventNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case EndNodeData nodeData:
                    DialogueChapterManager.Instance.CompleteCurrentChapter();

                    RunNode(nodeData);
                    break;
                case RandomNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case IfNodeData nodeData:
                    RunNode(nodeData);
                    break;
                default:
                    break;
            }
        }

        private void RunNode(StartNodeData _nodeData)
        {
            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            // Reset Audio
            AudioSource.Stop();

            CheckNodeType(GetNextNode(_nodeData));
        }

        private void RunNode(RandomNodeData _nodeData)
        {
            string GUID = _nodeData.DialogueNodePorts[Random.Range(0, _nodeData.DialogueNodePorts.Count)].InputGuid;
            CheckNodeType(GetNodeByGuid(GUID));
        }
        private void RunNode(IfNodeData _nodeData)
        {
            string ValueName = _nodeData.ValueName;
            GlobalValueIFOperations Operations = _nodeData.Operations;
            string OperationValue = _nodeData.OperationValue;

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            //Debug.Log("XXXX" + _nodeData.TrueGUID + "XXXX");
            CheckNodeType(GetNodeByGuid(manager.IfTrue(ValueName, Operations, OperationValue) ? _nodeData.TrueGUID : _nodeData.FalseGUID));
        }
        private void RunNode(DialogueNodeData _nodeData)
        {
            //IEnumerator delaytmp()
            //{
            //    yield return new WaitForSeconds(_nodeData.Delay);
            //    DialogueNode_NextNode();
            //}

           // if (_nodeData.Delay != 0) StartTrackedCoroutine(delaytmp()); ;

            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();
            var characterName = DialogueLocalizationHelper.GetCharacterName(_nodeData.Character);
            // Gloval Value Multiline
            if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null && _nodeData.Character.UseGlobalValue)
            {
                dialogueUIManager.ResetText("");
                dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}</color>");
            }
            // Normal Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null)
            {
                dialogueUIManager.ResetText("");
                dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{characterName}</color>");
            }
            // No Change Character Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null)
                dialogueUIManager.ResetText("");
            // Global Value Inline
            else if (_nodeData.Character != null && _nodeData.Character.UseGlobalValue)
                dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}: </color>");
            // Normal Inline
            else if (_nodeData.Character != null)
                dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{characterName}: </color>");
            // Last Change
            else dialogueUIManager.ResetText("");

            dialogueUIManager.SetFullText(_nodeData.Texts, _nodeData, DialogueUIManager.MessageSource.Character);

            // New Character Avatar
            if (_nodeData.AvatarPos == AvatarPosition.Left)
                dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            else if (_nodeData.AvatarPos == AvatarPosition.Right)
                dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            else
                dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);

            //MakeButtons(_nodeData.Character, _nodeData, new List<DialogueNodePort>());

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) AudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);

            _nodeDialogueInvoke = _nodeData;

            StopAllTrackedCoroutines();

            /* duct tape
            if(_nodeData.Character != null)
                DialogueManager.Instance.MainUI.dialogueCanvas.Character = _nodeData.Character;
            */

            IEnumerator tmp()
            {
                //yield return new WaitForSeconds(_nodeData.Duration);
                _nodeData.Reset();

                while (_nodeData.ShouldDelay())
                {
                    yield return new WaitForEndOfFrame();
                }

                DialogueNode_NextNode();
            }

            StartTrackedCoroutine(tmp());

        }
        private void RunNode(DialogueChoiceNodeData _nodeData)
        {
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            // Gloval Value Multiline
            if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null && _nodeData.Character.UseGlobalValue) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}</color>"); }
            // Normal Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}</color>"); }
            // Global Value Inline
            else if (_nodeData.Character != null && _nodeData.Character.UseGlobalValue) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}: </color>");
            // Normal Inline
            else if (_nodeData.Character != null) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}: </color>");
            // Last Change
            else dialogueUIManager.ResetText("");

            dialogueUIManager.SetFullText(_nodeData.TextType, _nodeData, DialogueUIManager.MessageSource.Character);

            // New Character Avatar
            if (_nodeData.AvatarPos == AvatarPosition.Left) dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            else if (_nodeData.AvatarPos == AvatarPosition.Right) dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            else dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);
            //MakeButtons(_nodeData.Character, _nodeData, new List<DialogueNodePort>());

            _nodeChoiceInvoke = _nodeData;
            StopAllTrackedCoroutines();

            IEnumerator tmp()
            {
                while (_nodeData.ShouldDelay())
                {
                    yield return new WaitForEndOfFrame();
                }

                //yield return new WaitForSeconds(_nodeData.Duration);
                ChoiceNode_GenerateChoice(_nodeData.Character, _nodeData);
            }

            StartTrackedCoroutine(tmp());;

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) AudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
        }
        private void RunNode(EventNodeData _nodeData)
        {
            foreach (var item in _nodeData.EventScriptableObjects)
            {
                if (item.DialogueEventSO != null)
                {
                    item.DialogueEventSO.RunEvent();
                }
            }
            CheckNodeType(GetNextNode(_nodeData));
        }

        private void RunNode(EndNodeData _nodeData)
        {
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", "ENDED");

            switch (_nodeData.EndNodeType)
            {
                case EndNodeType.End:
                    EndDialogueEvent.Invoke();
                    break;
                case EndNodeType.Repeat:
                    CheckNodeType(GetNodeByGuid(currentDialogueNodeData.NodeGuid));
                    break;
                case EndNodeType.GoBack:
                    CheckNodeType(GetNodeByGuid(lastDialogueNodeData.NodeGuid));
                    break;
                case EndNodeType.ReturnToStart:
                    CheckNodeType(GetNextNode(dialogueContainer.StartNodeDatas[Random.Range(0,dialogueContainer.StartNodeDatas.Count)]));
                    break;
                case EndNodeType.StartDialogue:
                    StartDialogue(_nodeData.Dialogue, SaveAndLoadManager.Instance.CurrentSave.CurrentChapterData);
                    break;
                default:
                    break;
            }
        }
        private void RunNode(TimerChoiceNodeData _nodeData)
        {
            lastDialogueNodeData = currentDialogueNodeData;
            currentDialogueNodeData = _nodeData;

            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            // Gloval Value Multiline
            if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null && _nodeData.Character.UseGlobalValue) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}</color>"); }
            // Normal Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}</color>"); }
            // Global Value Inline
            else if (_nodeData.Character != null && _nodeData.Character.UseGlobalValue) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}: </color>");
            // Normal Inline
            else if (_nodeData.Character != null) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}: </color>");
            // Last Change
            else dialogueUIManager.ResetText("");

            dialogueUIManager.SetFullText(_nodeData.TextType, _nodeData, DialogueUIManager.MessageSource.Character);

            // New Character Avatar
            if (_nodeData.AvatarPos == AvatarPosition.Left) dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            else if (_nodeData.AvatarPos == AvatarPosition.Right) dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            else dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);
            //MakeButtons(_nodeData.Character, _nodeData, new List<DialogueNodePort>());

            _nodeTimerInvoke = _nodeData;

            StopAllTrackedCoroutines();

            IEnumerator tmp()
            {
                //yield return new WaitForSecondsRealtime(_nodeData.Duration);
                _nodeData.Reset();
                while(_nodeData.ShouldDelay())
                {
                    yield return new WaitForEndOfFrame();
                }

                TimerNode_GenerateChoice(_nodeData.Character, _nodeData);
            }

            StartTrackedCoroutine(tmp());;

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) AudioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);

        }

        private void MakeButtons(DialogueCharacterSO _character, BaseNodeData nodeData, List<DialogueNodePort> _nodePorts)
        {
            List<string> texts = new();
            List<UnityAction> unityActions = new List<UnityAction>();

            foreach (DialogueNodePort nodePort in _nodePorts)
            {
                //Grab text and hint and add them to the lists
                UnityAction tempAction = null;
                tempAction += () =>
                {
                    SaveAndLoadManager.Instance.CurrentSave.MakeChoice(nodeData, nodePort.TextLanguage);
                    SaveAndLoadManager.Instance.AutoSave();
                    CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                };

                unityActions.Add(tempAction);
            }

            dialogueUIManager.SetButtons(_character, nodeData, _nodePorts.Select(x => x.TextLanguage).ToList(), _nodePorts.Select(x => x.HintLanguage).ToList(), unityActions, false);
        }

        private void MakeTimerButtons(DialogueCharacterSO _character, BaseNodeData nodeData, List<DialogueNodePort> _nodePorts, float showDuration)
        {
            List<string> texts = new List<string>();
            List<UnityAction> unityActions = new List<UnityAction>();

            IEnumerator tmp()
            {
                yield return new WaitForSeconds(showDuration);
                TimerNode_NextNode();
            }

            StartTrackedCoroutine(tmp());

            foreach (DialogueNodePort nodePort in _nodePorts)
            {
                //Grab text and hint and add them to the lists
                UnityAction tempAction = null;
                tempAction += () =>
                {
                    StopAllTrackedCoroutines();
                    SaveAndLoadManager.Instance.CurrentSave.MakeChoice(nodeData, nodePort.TextLanguage);
                    SaveAndLoadManager.Instance.AutoSave();
                    CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                };

                unityActions.Add(tempAction);
            }

            //TODO: Do we need hints on the timer options, probably not
            dialogueUIManager.SetButtons(_character, nodeData, _nodePorts.Select(x => x.TextLanguage).ToList(), _nodePorts.Select(x => x.HintLanguage).ToList(), unityActions, true);
        }

        void DialogueNode_NextNode()
        {
            CheckNodeType(GetNextNode(_nodeDialogueInvoke));
        }

        void ChoiceNode_GenerateChoice(DialogueCharacterSO _character, BaseNodeData nodeData)
        {
            MakeButtons(_character, nodeData, _nodeChoiceInvoke.DialogueNodePorts);
            dialogueUIManager.SkipButton.SetActive(false);
        }

        void TimerNode_GenerateChoice(DialogueCharacterSO _character, BaseNodeData nodeData)
        {
            MakeTimerButtons(_character, nodeData, _nodeTimerInvoke.DialogueNodePorts, _nodeTimerInvoke.time);
            dialogueUIManager.SkipButton.SetActive(false);
        }

        void TimerNode_NextNode()
        {
            CheckNodeType(GetNextNode(_nodeTimerInvoke));
        }



        #region Improve Coroutine
        public void StopAllTrackedCoroutines()
        {
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }

            activeCoroutines.Clear();
        }

        private Coroutine StartTrackedCoroutine(IEnumerator coroutine)
        {
            Coroutine newCoroutine = StartCoroutine(coroutine);
            activeCoroutines.Add(newCoroutine);
            return newCoroutine;
        }
        #endregion


        public void SkipDialogue()
        {

            // Reset Audio
            AudioSource.Stop();

            StopAllTrackedCoroutines();

            switch (currentDialogueNodeData)
            {
                case DialogueNodeData:
                    DialogueNode_NextNode();
                    break;
                case TimerChoiceNodeData nodeData:
                    TimerNode_GenerateChoice(nodeData.Character, nodeData);
                    break;
                case DialogueChoiceNodeData nodeData:
                    ChoiceNode_GenerateChoice(nodeData.Character, nodeData);
                    break;
                default:
                    break;
            }
        }
        public void ForceEndDialog()
        {
            // Reset Audio
            AudioSource.Stop();

            dialogueUIManager.dialogueCanvas.Close();
            EndDialogueEvent.Invoke();

StopAllTrackedCoroutines();

            // Reset Audio
            AudioSource.Stop();
        }
    }
}
