using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using Unity.VisualScripting;

namespace MeetAndTalk
{
    public class DialogueManager : DialogueGetData
    {
        public const float BASE_NODE_DISPLAY_TIME = 2;

        [HideInInspector] public static DialogueManager Instance;
        public LocalizationManager localizationManager;

        [HideInInspector] public DialogueUIManager dialogueUIManager;
        public AudioSource audioSource;
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

        private void Awake()
        {
            Instance = this;

            // Setup UI
            DialogueUIManager[] all = FindObjectsByType<DialogueUIManager>(FindObjectsSortMode.None);
            foreach(DialogueUIManager ui in all) { if(ui.dialogueCanvas != null) ui.dialogueCanvas.Close(); }

            DialogueUIManager.Instance = MainUI;
            dialogueUIManager = DialogueUIManager.Instance;

            audioSource = GetComponent<AudioSource>();
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
            // Update Dialogue UI
            dialogueUIManager = DialogueUIManager.Instance;

            // Setup Dialogue
            SetupDialogue(DialogueSO);

            // Error: No Setup Dialogue
            if (dialogueContainer == null) { Debug.LogError("Error: Dialogue Container SO is not assigned!"); }

            // Check ID
            if (dialogueContainer.StartNodeDatas.Count == 0) { Debug.LogError("Error: No Start Node in Dialogue Container!"); }

            BaseNodeData _start = null;

            if (chapterData.CurrentGUID == string.Empty || SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            {
                if (chapterData.StartID == string.Empty)
                {
                    _start = dialogueContainer.StartNodeDatas[Random.Range(0, dialogueContainer.StartNodeDatas.Count)];
                }
                else
                {
                    // IF FInd ID assign Data
                    foreach (StartNodeData data in dialogueContainer.StartNodeDatas)
                    {
                        if (data.startID == chapterData.CurrentGUID)
                        {
                            _start = data;
                            break;
                        }
                    }
                }

                CheckNodeType(GetNextNode(_start));
            }
            else
            {
                //Draw out all the conversation up until this point but it cannot be done with notifications
                if (PopulateVisitedNodes(chapterData))
                {
                    CheckNodeType(GetNodeByGuid(chapterData.CurrentGUID));
                }
                else
                {
                    //The save file could not be loaded for this chapter, this implies that it is out of date with the latest verison.
                    //Reset the game state and start the Player from the top of the chapter
                    GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.INVALID_SAVE_DATA, () =>
                    {
                        //Reset the chapter data in the save file
                        SaveAndLoadManager.Instance.ClearChapterData(chapterData.FileIndex);

                        OverlayCanvas.Instance.FadeToBlack(() =>
                        {
                            SaveAndLoadManager.Instance.LoadSave();
                        });

                    }, "Continue", false);
                }
            }

            StartDialogueEvent.Invoke();
        }

        public bool PopulateVisitedNodes(ChapterSaveData savaData)
        {
            bool loadSuccess = true;
            foreach (var item in savaData.PastCoversations)
            {
                var node = GetNodeByGuid(item.GUID);
                if (node == null)
                {
                    //We failed to find the node by the GUID that was saved to file. This implies that their current chapter save data is invalid.
                    //Eaxit to start the Player from the top of the chapter using the latest data
                    loadSuccess = false;
                    break;
                }

                if (item.GUID == savaData.CurrentGUID)
                {
                    continue;
                }

                dialogueUIManager.SetFullText(item.Text, node, DialogueUIManager.MessageSource.Character, false);

                if (item.IsChoice)
                    dialogueUIManager.SetFullText(item.SelectedChoice, node, DialogueUIManager.MessageSource.Player, false);
            }

            return loadSuccess;
        }


        public void CheckNodeType(BaseNodeData _baseNodeData)
        {
            // PlayerPrefs.SetString($"{dialogueContainer.name}_Progress", _baseNodeData.NodeGuid);

            if (_baseNodeData.GetType() != typeof(EventNodeData))
            {
                SaveAndLoadManager.Instance.CurrentSave.AddNode(_baseNodeData);
                SaveAndLoadManager.Instance.AutoSave();
            }


            switch (_baseNodeData)
            {
                case StartNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueChoiceNodeData nodeData:
                    RunNode(nodeData);
                    break;
                case TimerChoiceNodeData nodeData:
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
            audioSource.Stop();

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

            // Gloval Value Multiline
            if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null && _nodeData.Character.UseGlobalValue) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}</color>"); }
            // Normal Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null) { dialogueUIManager.ResetText(""); dialogueUIManager.SetSeparateName($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}</color>"); }
            // No Change Character Multiline
            else if (dialogueUIManager.showSeparateName && dialogueUIManager.nameLabel != null && _nodeData.Character != null) { dialogueUIManager.ResetText(""); }
            // Global Value Inline
            else if (_nodeData.Character != null && _nodeData.Character.UseGlobalValue) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{manager.Get<string>(GlobalValueType.String, _nodeData.Character.CustomizedName.ValueName)}: </color>");
            // Normal Inline
            else if (_nodeData.Character != null) dialogueUIManager.ResetText($"<color={_nodeData.Character.HexColor()}>{_nodeData.Character.characterName.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}: </color>");
            // Last Change
            else dialogueUIManager.ResetText("");

            dialogueUIManager.SetFullText(
                $"{_nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}",
                _nodeData,
                DialogueUIManager.MessageSource.Character
                );

            // New Character Avatar
            if (_nodeData.AvatarPos == AvatarPosition.Left) dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            else if (_nodeData.AvatarPos == AvatarPosition.Right) dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            else dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);
            MakeButtons(_nodeData.Character, _nodeData, new List<DialogueNodePort>());

            if(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) audioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);

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

            dialogueUIManager.SetFullText($"{_nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}", _nodeData, DialogueUIManager.MessageSource.Character);

            // New Character Avatar
            if (_nodeData.AvatarPos == AvatarPosition.Left) dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            else if (_nodeData.AvatarPos == AvatarPosition.Right) dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            else dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);
            MakeButtons(_nodeData.Character, _nodeData, new List<DialogueNodePort>());

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

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) audioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);
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

            dialogueUIManager.SetFullText(
                $"{_nodeData.TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType}",
                _nodeData,
                DialogueUIManager.MessageSource.Character
                );

            // New Character Avatar
            if (_nodeData.AvatarPos == AvatarPosition.Left) dialogueUIManager.UpdateAvatars(_nodeData.Character, null, _nodeData.AvatarType);
            else if (_nodeData.AvatarPos == AvatarPosition.Right) dialogueUIManager.UpdateAvatars(null, _nodeData.Character, _nodeData.AvatarType);
            else dialogueUIManager.UpdateAvatars(null, null, _nodeData.AvatarType);

            dialogueUIManager.SkipButton.SetActive(true);
            MakeButtons(_nodeData.Character, _nodeData, new List<DialogueNodePort>());

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

            if (_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType != null) audioSource.PlayOneShot(_nodeData.AudioClips.Find(clip => clip.languageEnum == localizationManager.SelectedLang()).LanguageGenericType);

        }

        private void MakeButtons(DialogueCharacterSO _character, BaseNodeData nodeData, List<DialogueNodePort> _nodePorts)
        {
            List<string> texts = new();
            List<string> hints = new();
            List<UnityAction> unityActions = new List<UnityAction>();

            foreach (DialogueNodePort nodePort in _nodePorts)
            {
                //Grab text and hint and add them to the lists
                nodePort.GetConvertedText(localizationManager.SelectedLang(), out string text, out string hint);
                texts.Add(text);
                hints.Add(hint);

                UnityAction tempAction = null;
                tempAction += () =>
                {
                    SaveAndLoadManager.Instance.CurrentSave.MakeChoiceNoce(nodeData, text);
                    CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                };

                unityActions.Add(tempAction);
            }

            dialogueUIManager.SetButtons(_character, nodeData, texts, hints, unityActions, false);
        }
        private void MakeTimerButtons(DialogueCharacterSO _character, BaseNodeData nodeData, List<DialogueNodePort> _nodePorts, float ShowDuration, float timer)
        {
            List<string> texts = new List<string>();
            List<string> hints = new();
            List<UnityAction> unityActions = new List<UnityAction>();

            IEnumerator tmp() { yield return new WaitForSeconds(timer); TimerNode_NextNode(); }
            StartTrackedCoroutine(tmp());;

            foreach (DialogueNodePort nodePort in _nodePorts)
            {
                if (nodePort != _nodePorts[0])
                {
                    nodePort.GetConvertedText(localizationManager.SelectedLang(), out string text, out string hint);
                    texts.Add(text);
                    hints.Add(hint);

                    UnityAction tempAction = null;
                    tempAction += () =>
                    {
                        StopAllTrackedCoroutines();

                        SaveAndLoadManager.Instance.CurrentSave.MakeChoiceNoce(nodeData, text);
                        CheckNodeType(GetNodeByGuid(nodePort.InputGuid));
                    };

                    unityActions.Add(tempAction);
                }
            }

            //TODO: Do we need hints on the timer options, probably not
            dialogueUIManager.SetButtons(_character, nodeData, texts, hints, unityActions, true);
        }

        void DialogueNode_NextNode() { CheckNodeType(GetNextNode(_nodeDialogueInvoke)); }
        void ChoiceNode_GenerateChoice(DialogueCharacterSO _character, BaseNodeData nodeData) { MakeButtons(_character, nodeData, _nodeChoiceInvoke.DialogueNodePorts);
            dialogueUIManager.SkipButton.SetActive(false);
        }
        void TimerNode_GenerateChoice(DialogueCharacterSO _character, BaseNodeData nodeData) { MakeTimerButtons(_character, nodeData, _nodeTimerInvoke.DialogueNodePorts, _nodeTimerInvoke.Duration, _nodeTimerInvoke.time);
            dialogueUIManager.SkipButton.SetActive(false);
        }
        void TimerNode_NextNode() { CheckNodeType(GetNextNode(_nodeTimerInvoke)); }



        #region Improve Coroutine
        private void StopAllTrackedCoroutines()
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
            audioSource.Stop();

            StopAllTrackedCoroutines();

            switch (currentDialogueNodeData)
            {
                case DialogueNodeData nodeData:
                    DialogueNode_NextNode();
                    break;
                case DialogueChoiceNodeData nodeData:
                    ChoiceNode_GenerateChoice(nodeData.Character, nodeData);
                    break;
                case TimerChoiceNodeData nodeData:
                    TimerNode_GenerateChoice(nodeData.Character, nodeData);
                    break;
                default:
                    break;
            }
        }
        public void ForceEndDialog()
        {
            // Reset Audio
            audioSource.Stop();

            dialogueUIManager.dialogueCanvas.Close();
            EndDialogueEvent.Invoke();

StopAllTrackedCoroutines();

            // Reset Audio
            audioSource.Stop();
        }
    }
}
