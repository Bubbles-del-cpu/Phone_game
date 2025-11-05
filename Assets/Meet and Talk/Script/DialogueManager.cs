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
        public void SetupDialogue(DialogueContainerSO dialogue)
        {
            if (dialogue != null) dialogueContainer = dialogue;
            else Debug.LogError("DialogueContainerSO.dialogue Object jest Pusty!");
        }

       // public void StartDialogue(DialogueContainerSO dialogue) { StartDialogue(dialogue, ""); }
        //public void StartDialogue(string ID) { StartDialogue(null, ID); }
        //public void StartDialogue() { StartDialogue(null, ""); }
        public void StartDialogue(DialogueContainerSO DialogueSO, ChapterSaveData chapterData)
        {
            Debug.Log("NEW BUILD v1.3: StartDialogue called."); // <-- NEW LOG
            // Update Dialogue UI
            dialogueUIManager = DialogueUIManager.Instance;
            _visitedNodes = new Stack<BaseNodeData>();

            // Setup Dialogue
            SetupDialogue(DialogueSO);

            // Error: No Setup Dialogue
            if (dialogueContainer == null) { Debug.LogError("Error: Dialogue Container SO is not assigned!"); }

            // Check ID
            if (dialogueContainer.StartNodeDatas.Count == 0) { Debug.LogError("Error: No Start Node in Dialogue Container!"); }

            if (chapterData.CurrentGUID == string.Empty || SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            {
                Debug.Log("NEW BUILD v1.3: Starting new chapter from scratch."); // <-- NEW LOG
                TriggerDialogueStart(chapterData);
            }
            else
            {
                Debug.Log("NEW BUILD v1.3: Attempting to load from save file."); // <-- NEW LOG
                //Draw out all the conversation up until this point but it cannot be done with notifications
                bool populateSuccess = PopulateVisitedNodes(chapterData); // <-- Modified
                if (populateSuccess)
                {
                    // --- MODIFIED CODE BLOCK ---
                    Debug.Log("NEW BUILD v1.3: PopulateVisitedNodes succeeded. Checking CurrentGUID.");
                    var currentNode = GetNodeByGuid(chapterData.CurrentGUID);

                    if (currentNode == null)
                    {
                        Debug.LogError($"FATAL ERROR: CurrentGUID '{chapterData.CurrentGUID}' is NULL or missing. This save file is corrupted. Resetting chapter.");
                        // This manually triggers your existing fallback logic
                        TriggerInvalidSaveReset(chapterData);
                    }
                    else
                    {
                        Debug.Log($"NEW BUILD v1.3: CurrentGUID is valid ({currentNode.NodeGuid}). Calling CheckNodeType."); // <-- NEW LOG
                        CheckNodeType(currentNode);
                    }
                    // --- END MODIFIED CODE BLOCK ---
                }
                else
                {
                    Debug.LogWarning("NEW BUILD v1.3: PopulateVisitedNodes returned false. Save data is invalid. Resetting chapter."); // <-- NEW LOG
                    //The save file could not be loaded for this chapter, this implies that it is out of date with the latest verison.
                    //Reset the game state and start the Player from the top of the chapter
                    TriggerInvalidSaveReset(chapterData); // <-- Refactored to use new helper method
                }
            }

            StartDialogueEvent.Invoke();
        }
        
        // --- NEW HELPER FUNCTION ---
        private void TriggerInvalidSaveReset(ChapterSaveData chapterData)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.INVALID_SAVE_DATA, () =>
            {
                //Reset the chapter data in the save file
                SaveAndLoadManager.Instance.ClearChapterData(chapterData.FileIndex);

                OverlayCanvas.Instance.FadeToBlack(() =>
                {
                    SaveAndLoadManager.Instance.LoadSave();
                    GameManager.Instance.ResetGameState();
                });

            }, GameConstants.UIElementKeys.CONTINUE, args: null, twoButtonSetup: false);
        }
        // --- END NEW HELPER FUNCTION ---

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

        public bool PopulateVisitedNodes(ChapterSaveData savaData)
        {
            bool loadSuccess = true;

            // --- FIX 1: Limit message history to prevent GPU crash ---
            int maxMessagesToRepopulate = 30; // Only load the last 30 messages
            int startIndex = Mathf.Max(0, savaData.PastCoversations.Count - maxMessagesToRepopulate);
            // --- END FIX 1 ---

            // --- MODIFIED LOOP: Start from our new startIndex ---
            for (int i = startIndex; i < savaData.PastCoversations.Count; i++)
            {
                var item = savaData.PastCoversations[i];
                // --- END MODIFIED LOOP ---

                // --- FIX 2: Stop duplication bug ---
                if (item.GUID == savaData.CurrentGUID)
                {
                    continue;
                }
                // --- END FIX 2 ---
            
                var node = GetNodeByGuid(item.GUID);
                if (node == null)
                {
                    //We failed to find the node by the GUID that was saved to file. This implies that their current chapter save data is invalid.
                    //Eaxit to start the Player from the top of the chapter using the latest data
                    loadSuccess = false;
                    break;
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
                        dialogueUIManager.SetFullText(nd.Texts, node, DialogueUIManager.MessageSource.Character, false);
                        break;
                    case TimerChoiceNodeData tChoiceNode when node is TimerChoiceNodeData:
                        dialogueUIManager.SetFullText(item.SelectedChoiceTexts, tChoiceNode, DialogueUIManager.MessageSource.Player, false);
                        tChoiceNode.SelectedChoice = item.SelectedChoiceTexts;
                        break;
                    case DialogueChoiceNodeData dChoiceNode when node is DialogueChoiceNodeData:
                        dialogueUIManager.SetFullText(item.SelectedChoiceTexts, dChoiceNode, DialogueUIManager.MessageSource.Player, false);
                        dChoiceNode.SelectedChoice = item.SelectedChoiceTexts;
                        break;
                }
            }

            return loadSuccess;
        }

        public void CheckNodeType(BaseNodeData _baseNodeData)
        {
            // --- NEW CODE STARTS HERE ---
            Debug.Log($"NEW BUILD v1.3: CheckNodeType called with node: {(_baseNodeData != null ? _baseNodeData.NodeGuid : "NULL")}");

            if (_baseNodeData == null)
            {
                Debug.LogError("FATAL ERROR: CheckNodeType was called on a NULL node. This is likely a corrupted save file. Cannot continue.");
                // We can't proceed, but we also don't want to crash.
                return; 
            }
            // --- NEW CODE ENDS HERE ---

            if (GameManager.Instance.ResettingSave)
            {
                Debug.Log("NEW BUILD v1.3: GameManager is resetting save, skipping CheckNodeType."); // <-- NEW LOG
                return;
            }
            
            Debug.Log($"NEW BUILD v1.3: Adding node {_baseNodeData.NodeGuid} to save file."); // <-- NEW LOG
            SaveAndLoadManager.Instance.CurrentSave.AddNode(_baseNodeData);
            Debug.Log("NEW BUILD v1.3: Calling AutoSave."); // <-- NEW LOG
            SaveAndLoadManager.Instance.AutoSave();
            Debug.Log("NEW BUILD v1.3: AutoSave complete. Pushing node to visited stack."); // <-- NEW LOG

            _visitedNodes.Push(_baseNodeData);
            
            Debug.Log($"NEW BUILD v1.3: Running switch on node type: {_baseNodeData.GetType()}"); // <-- NEW LOG
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
            Debug.Log($"NEW BUILD v1.3: RunNode(StartNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
            string GUID = _nodeData.NodeGuid;
            PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", GUID);

            // Reset Audio
            AudioSource.Stop();

            CheckNodeType(GetNextNode(_nodeData));
        }

        private void RunNode(RandomNodeData _nodeData)
        {
            Debug.Log($"NEW BUILD v1.3: RunNode(RandomNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
            string GUID = _nodeData.DialogueNodePorts[Random.Range(0, _nodeData.DialogueNodePorts.Count)].InputGuid;
            CheckNodeType(GetNodeByGuid(GUID));
        }
        private void RunNode(IfNodeData _nodeData)
        {
            Debug.Log($"NEW BUILD v1.3: RunNode(IfNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
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
            Debug.Log($"NEW BUILD v1.3: RunNode(DialogueNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
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
            Debug.Log($"NEW BUILD v1.3: RunNode(DialogueChoiceNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
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
            Debug.Log($"NEW BUILD v1.3: RunNode(EventNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
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
            Debug.Log($"NEW BUILD v1.3: RunNode(EndNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
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
            Debug.Log($"NEW BUILD v1.3: RunNode(TimerChoiceNodeData) called for {_nodeData.NodeGuid}"); // <-- NEW LOG
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