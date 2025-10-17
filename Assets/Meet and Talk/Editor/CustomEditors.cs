using UnityEngine;
using UnityEditor;
using MeetAndTalk;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Event;
using MeetAndTalk.Localization;
using MeetAndTalk.Nodes;
using Codice.Client.BaseCommands.Differences;
using MeetAndTalk.Settings;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine.Video;
using System.Linq;

#if UNITY_EDITOR

[CustomEditor(typeof(DialogueContainerSO))]
public class DialogueContainerSOEditor : Editor
{
    bool Settings = true;
    bool NodeLink = false;
    bool StartNode = false;
    bool EndNode = false;
    bool DialogueNode = false;
    bool DialogueChoiceNode = false;
    bool DialogueTimerChoiceNode = false;
    bool DialogueEventNode = false;
    bool RandomNode = false;
    bool SleepNode = false;
    bool CommandNode = false;
    bool IFNode = false;

    //This function needs to run for any dialogue tree that haven't been opened for the new translation timelapse system
    //Once it has completed ideally it will be removed and never need to run again but if it does run again nothing should happen
    //as the timelapse list should have been populated with the correct number of elements to match the Texts list
    private void CheckTimeLapseText(DialogueContainerSO target)
    {
        foreach(var node in target.DialogueNodeDatas)
        {
            for (var index = 0; index < node.Texts.Count; index++)
            {
                if (index >= node.Timelapses.Count)
                    node.Timelapses.Add(new LanguageGeneric<string>()
                    {
                        languageEnum = node.Texts[index].languageEnum,
                        LanguageGenericType = node.Timelapse
                    });

                node.Timelapse = "";
            }
        }
    }

    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        DialogueContainerSO _target = (DialogueContainerSO)target;
        CheckTimeLapseText(_target);
        // Base Info
        EditorGUI.indentLevel = 0;

        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight * 2 + 2));
        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 200, EditorGUIUtility.singleLineHeight), _target.name, EditorStyles.boldLabel);

        if (GUI.Button(new Rect(rect.x + rect.width - 200, rect.y, 100, EditorGUIUtility.singleLineHeight), "Import Text", EditorStyles.miniButtonLeft))
        {
            string path = EditorUtility.OpenFilePanel("Import Dialogue Localization File", Application.dataPath, "tsv");
            if (path.Length != 0)
            {
                _target.ImportText(path, _target);
                serializedObject.ApplyModifiedProperties();
            }
        }
        if (GUI.Button(new Rect(rect.x + rect.width - 100, rect.y, 100, EditorGUIUtility.singleLineHeight), "Export Text", EditorStyles.miniButtonRight))
        {
            string path = EditorUtility.SaveFilePanel("Export Dialogue Localization File", Application.dataPath, _target.name, "tsv");
            if (path.Length != 0)
            {
                _target.GenerateCSV(path, _target);
                serializedObject.ApplyModifiedProperties();
            }
        }
        if (GUI.Button(new Rect(rect.x + rect.width - 200, rect.y + EditorGUIUtility.singleLineHeight + 2, 200, EditorGUIUtility.singleLineHeight), "Translate Dialog", EditorStyles.miniButton))
        {
            AutoTranslate(_target);
        }

        if (GUI.Button(new Rect(rect.x + rect.width - 200, rect.y + ((EditorGUIUtility.singleLineHeight + 2) * 2), 200, EditorGUIUtility.singleLineHeight), "Regenerate GUIDs", EditorStyles.miniButton))
        {
            if (EditorUtility.DisplayDialog("Regenerate GUIDSs", "Are you sure you want to regenerate the GUIDS for the dialogue nodes, doing so many cause the input/output connections to be lost", "Yes", "Cancel"))
            {
                foreach (var item in _target.AllNodes)
                {
                    item.NodeGuid = Guid.NewGuid().ToString();
                }
            }
        }

        if (GUI.Button(new Rect(rect.x + rect.width - 200, rect.y + ((EditorGUIUtility.singleLineHeight + 2) * 3), 200, EditorGUIUtility.singleLineHeight), "Reset Hints", EditorStyles.miniButton))
        {
            if (EditorUtility.DisplayDialog("Clear Hints", "Are you sure you want to clear all the hint texts on dialogue nodes? This action cannot be undone", "Continue", "Cancel"))
            {
                foreach (var item in _target.AllNodes)
                {
                    switch (item)
                    {
                        case TimerChoiceNodeData tData:
                            foreach (var port in tData.DialogueNodePorts)
                            {
                                foreach (var hint in port.HintLanguage)
                                    hint.LanguageGenericType = DialogueNodePort.BLANK_HINT;
                            }
                            break;
                        case DialogueChoiceNodeData nData:
                            foreach (var port in nData.DialogueNodePorts)
                            {
                                foreach (var hint in port.HintLanguage)
                                    hint.LanguageGenericType = DialogueNodePort.BLANK_HINT;
                            }
                            break;
                    }
                }
            }
        }

        EditorGUILayout.Space(50);

        MAT_Editor.FoldoutGroup($"Dialogue Settings", "Settings Related to Dialogue Behavior", MAT_Editor.GetTinyIcon("Settings_Gizmo"), ref Settings);
        if (Settings)
        {
            EditorGUILayout.BeginVertical("HelpBox");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowDialogueSave"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockingReopeningDialogue"));

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connection Between Nodes", EditorStyles.boldLabel);

        #region Node Link
        int count = _target.NodeLinkDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"Node Links [{count}]", "List of All Links in Dialogue [Debug Only]", MAT_Editor.GetTinyIcon("Node_Gizmo"), ref NodeLink);

        if (NodeLink)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();

                MAT_Editor.BeginBoxGroup("Node Link Connection ID", index + 1);
                EditorGUILayout.TextField("Base Node    [From]", _target.NodeLinkDatas[i].BaseNodeGuid);
                EditorGUILayout.TextField("Target Node  [To]", _target.NodeLinkDatas[i].TargetNodeGuid);
                MAT_Editor.EndBoxGroup();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Base Node", EditorStyles.boldLabel);

        #region Start Node
        count = _target.StartNodeDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"Start Node [{count}]", "Dialogue Starter Nodes", MAT_Editor.GetTinyIcon("Node_Start_Gizmo"), ref StartNode);

        if (StartNode)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();

                MAT_Editor.BeginBoxGroup(_target.StartNodeDatas[i].NodeGuid, index + 1);
                _target.StartNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.StartNodeDatas[i].Position);
                _target.StartNodeDatas[i].startID = EditorGUILayout.TextField("Start ID", _target.StartNodeDatas[i].startID);
                MAT_Editor.EndBoxGroup();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region End Node
        count = _target.EndNodeDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"End Node [{count}]", "Ending Dialogue Nodes", MAT_Editor.GetTinyIcon("Node_End_Gizmo"), ref EndNode);

        if (EndNode)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();

                MAT_Editor.BeginBoxGroup(_target.EndNodeDatas[i].NodeGuid, index + 1);
                _target.EndNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.EndNodeDatas[i].Position);
                _target.EndNodeDatas[i].EndNodeType = (EndNodeType)EditorGUILayout.EnumPopup("End Enum", _target.EndNodeDatas[i].EndNodeType);
                _target.EndNodeDatas[i].Dialogue = EditorGUILayout.ObjectField("Next Dialogue SO", _target.EndNodeDatas[i].Dialogue, typeof(DialogueContainerSO), allowSceneObjects: false) as DialogueContainerSO;
                MAT_Editor.EndBoxGroup();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Dialogue Node
        count = _target.DialogueNodeDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"Dialogue Node [{count}]", "Text Spoken by the Characters", MAT_Editor.GetTinyIcon("Node_Dialogue_Gizmo"), ref DialogueNode);

        if (DialogueNode)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();

                MAT_Editor.BeginBoxGroup(_target.DialogueNodeDatas[i].NodeGuid, index + 1);

                _target.DialogueNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.DialogueNodeDatas[i].Position);
                _target.DialogueNodeDatas[i].Character = (DialogueCharacterSO)EditorGUILayout.ObjectField("Character", _target.DialogueNodeDatas[i].Character, typeof(DialogueCharacterSO), false);
                _target.DialogueNodeDatas[i].AvatarPos = (AvatarPosition)EditorGUILayout.EnumPopup("Avatar Display", _target.DialogueNodeDatas[i].AvatarPos);
                _target.DialogueNodeDatas[i].AvatarType = (AvatarType)EditorGUILayout.EnumPopup("Avatar Emotion", _target.DialogueNodeDatas[i].AvatarType);

                _target.DialogueNodeDatas[i].Duration = EditorGUILayout.FloatField("Display Time", _target.DialogueNodeDatas[i].Duration);

                EditorGUILayout.LabelField("Media Settings");
                _target.DialogueNodeDatas[i].GalleryVisibility = (GalleryDisplay)EditorGUILayout.EnumPopup("Gallery Visiblity", _target.DialogueNodeDatas[i].GalleryVisibility);
                switch(_target.DialogueNodeDatas[i].GalleryVisibility)
                {
                    case GalleryDisplay.Display:
                    case GalleryDisplay.Hidden:
                        _target.DialogueNodeDatas[i].MediaType = (MediaType)EditorGUILayout.EnumPopup("Media Type", _target.DialogueNodeDatas[i].MediaType);
                        switch(_target.DialogueNodeDatas[i].MediaType)
                        {
                            case MediaType.Sprite:
                                _target.DialogueNodeDatas[i].NotBackgroundCapable = EditorGUILayout.Toggle("Not Background Capabe", _target.DialogueNodeDatas[i].NotBackgroundCapable);
                                _target.DialogueNodeDatas[i].Image = (Sprite)EditorGUILayout.ObjectField("Image", _target.DialogueNodeDatas[i].Image, typeof(Sprite), false);
                                break;
                            case MediaType.Video:
                                _target.DialogueNodeDatas[i].Video = (VideoClip)EditorGUILayout.ObjectField("Video", _target.DialogueNodeDatas[i].Video, typeof(VideoClip), false);
                                _target.DialogueNodeDatas[i].VideoThumbnail = (Sprite)EditorGUILayout.ObjectField("Fixed Thumbnail", _target.DialogueNodeDatas[i].VideoThumbnail, typeof(Sprite), false);
                                break;
                        }
                        break;
                }

                for (int j = 0; j < _target.DialogueNodeDatas[i].Texts.Count; j++)
                {
                    MAT_Editor.BeginBoxGroup(_target.DialogueNodeDatas[i].Texts[j].languageEnum.ToString(), 00);
                    _target.DialogueNodeDatas[i].AudioClips[j].LanguageGenericType = (AudioClip)EditorGUILayout.ObjectField("Audio Clips", _target.DialogueNodeDatas[i].AudioClips[j].LanguageGenericType, typeof(AudioClip), false);
                    _target.DialogueNodeDatas[i].Texts[j].LanguageGenericType = EditorGUILayout.TextField("Displayed String", _target.DialogueNodeDatas[i].Texts[j].LanguageGenericType);
                    _target.DialogueNodeDatas[i].Timelapses[j].LanguageGenericType = EditorGUILayout.TextField("Timelapse", _target.DialogueNodeDatas[i].Timelapses[j].LanguageGenericType);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
                // Display Node
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Choice Nodes

        // Foldout

        //MAT_Editor.FoldoutGroup(, MAT_Editor.GetTinyIcon("Node_Choice_Gizmo"), ref DialogueChoiceNode);
        CustomEditorHelper.DrawDialogueChoiceNodes(_target.DialogueChoiceNodeDatas, $"Choice Node [{_target.DialogueChoiceNodeDatas.Count}]", "List of Player's Choice", "Node_Choice_Gizmo", ref DialogueChoiceNode);
        CustomEditorHelper.DrawDialogueChoiceNodes(_target.TimerChoiceNodeDatas, $"Timer Choice Node [{count}]", "List of Time-Limited Player Choices", "Node_Timer_Gizmo", ref DialogueTimerChoiceNode);
        // if (DialogueChoiceNode || DialogueTimerChoiceNode)
        // {
        //     count = DialogueChoiceNode ? _target.DialogueChoiceNodeDatas.Count : _target.TimerChoiceNodeDatas.Count;
        //     var nodeDatas = DialogueChoiceNode ? _target.DialogueChoiceNodeDatas : _target.TimerChoiceNodeDatas.Cast<DialogueChoiceNodeData>().ToList();
        //     EditorGUILayout.BeginVertical("HelpBox");
        //     // List
        //     for (int i = 0; i < count; i++)
        //     {
        //         EditorGUILayout.BeginHorizontal();
        //         // Display Node
        //         MAT_Editor.BeginBoxGroup(nodeDatas[i].NodeGuid, i + 1);

        //         nodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", nodeDatas[i].Position);
        //         nodeDatas[i].Character = (DialogueCharacterSO)EditorGUILayout.ObjectField("Character", nodeDatas[i].Character, typeof(DialogueCharacterSO), false);
        //         nodeDatas[i].AvatarPos = (AvatarPosition)EditorGUILayout.EnumPopup("Avatar Display", nodeDatas[i].AvatarPos);
        //         nodeDatas[i].AvatarType = (AvatarType)EditorGUILayout.EnumPopup("Avatar Emotion", nodeDatas[i].AvatarType);
        //         nodeDatas[i].Duration = EditorGUILayout.FloatField("Display Time", nodeDatas[i].Duration);
        //         nodeDatas[i].RequireCharacterInput = EditorGUILayout.Toggle("Require Input", nodeDatas[i].RequireCharacterInput);

        //         if (DialogueTimerChoiceNode)
        //             _target.TimerChoiceNodeDatas[i].time = EditorGUILayout.FloatField("Time to make decision", _target.TimerChoiceNodeDatas[i].time);

        //         for (int j = 0; j < GameManager.LOCALIZATION_MANAGER.lang.Count + 1; j++)
        //         {
        //             MAT_Editor.BeginBoxGroup(nodeDatas[i].TextType[j].languageEnum.ToString(), 00);
        //             nodeDatas[i].AudioClips[j].LanguageGenericType = (AudioClip)EditorGUILayout.ObjectField("Audio Clips", nodeDatas[i].AudioClips[j].LanguageGenericType, typeof(AudioClip), false);
        //             nodeDatas[i].TextType[j].LanguageGenericType = EditorGUILayout.TextField("Displayed String", nodeDatas[i].TextType[j].LanguageGenericType);
        //             var portIndex = 1;
        //             foreach (var port in nodeDatas[i].DialogueNodePorts)
        //             {
        //                 MAT_Editor.BeginBoxGroup($"Option_{j + 1}", portIndex);
        //                 port.TextLanguage[j].LanguageGenericType = EditorGUILayout.TextField(port.TextLanguage[j].LanguageGenericType);
        //                 port.HintLanguage[j].LanguageGenericType = EditorGUILayout.TextField(port.HintLanguage[j].LanguageGenericType);
        //                 EditorGUILayout.EndVertical();
        //                 portIndex++;
        //             }

        //             //EditorGUILayout.LabelField("Options: ", EditorStyles.boldLabel);

        //             // EditorGUI.indentLevel++;

        //             // EditorGUI.indentLevel--;
        //             EditorGUILayout.EndVertical();
        //         }
        //         EditorGUILayout.EndVertical();
        //         // Display Node
        //         EditorGUILayout.EndHorizontal();
        //     }
        //     EditorGUILayout.EndVertical();
        // }
        #endregion

        // #region Timer Choice Node
        // count = _target.TimerChoiceNodeDatas.Count;

        // // Foldout
        // MAT_Editor.FoldoutGroup($"Timer Choice Node [{count}]", "List of Time-Limited Player Choices", MAT_Editor.GetTinyIcon("Node_Timer_Gizmo"), ref DialogueTimerChoiceNode);

        // if (DialogueTimerChoiceNode)
        // {
        //     EditorGUILayout.BeginVertical("HelpBox");
        //     // List
        //     for (int i = 0; i < count; i++)
        //     {
        //         EditorGUILayout.BeginHorizontal();
        //         // Display Node
        //         MAT_Editor.BeginBoxGroup(_target.TimerChoiceNodeDatas[i].NodeGuid, i + 1);

        //         _target.TimerChoiceNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.TimerChoiceNodeDatas[i].Position);
        //         _target.TimerChoiceNodeDatas[i].Character = (DialogueCharacterSO)EditorGUILayout.ObjectField("Character", _target.TimerChoiceNodeDatas[i].Character, typeof(DialogueCharacterSO), false);
        //         _target.TimerChoiceNodeDatas[i].AvatarPos = (AvatarPosition)EditorGUILayout.EnumPopup("Avatar Display", _target.TimerChoiceNodeDatas[i].AvatarPos);
        //         _target.TimerChoiceNodeDatas[i].AvatarType = (AvatarType)EditorGUILayout.EnumPopup("Avatar Emotion", _target.TimerChoiceNodeDatas[i].AvatarType);
        //         _target.TimerChoiceNodeDatas[i].Duration = EditorGUILayout.FloatField("Display Time", _target.TimerChoiceNodeDatas[i].Duration);
        //         _target.TimerChoiceNodeDatas[i].time = EditorGUILayout.FloatField("Time to make decision", _target.TimerChoiceNodeDatas[i].time);

        //         for (int j = 0; j < _target.TimerChoiceNodeDatas[0].TextType.Count; j++)
        //         {
        //             MAT_Editor.BeginBoxGroup(_target.DialogueChoiceNodeDatas[i].TextType[j].languageEnum.ToString(), 00);
        //             _target.TimerChoiceNodeDatas[i].AudioClips[j].LanguageGenericType = (AudioClip)EditorGUILayout.ObjectField("Audio Clips", _target.TimerChoiceNodeDatas[i].AudioClips[j].LanguageGenericType, typeof(AudioClip), false);
        //             _target.TimerChoiceNodeDatas[i].TextType[j].LanguageGenericType = EditorGUILayout.TextField("Displayed String", _target.TimerChoiceNodeDatas[i].TextType[j].LanguageGenericType);
        //             EditorGUILayout.LabelField("Options: ", EditorStyles.boldLabel);
        //             EditorGUI.indentLevel++;

        //             foreach (var port in _target.TimerChoiceNodeDatas[i].DialogueNodePorts)
        //             {
        //                 for (var index = 0; index < port.TextLanguage.Count; index++)
        //                 {
        //                     port.TextLanguage[index].LanguageGenericType = EditorGUILayout.TextField($"Option ID: {index}", port.TextLanguage[index].LanguageGenericType);
        //                 }

        //                 for (var index = 0; index < port.HintLanguage.Count; index++)
        //                 {
        //                     port.HintLanguage[index].LanguageGenericType = EditorGUILayout.TextField($"Hint ID: {index}", port.HintLanguage[index].LanguageGenericType);
        //                 }
        //             }
        //             EditorGUI.indentLevel--;
        //             EditorGUILayout.EndVertical();
        //         }
        //         EditorGUILayout.EndVertical();
        //         // Display Node
        //         EditorGUILayout.EndHorizontal();
        //     }
        //     EditorGUILayout.EndVertical();
        // }
        // #endregion

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Functional Nodes", EditorStyles.boldLabel);

        #region Event Node
        count = _target.EventNodeDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"Event Node [{count}]", "All Activities and Events in DIalog", MAT_Editor.GetTinyIcon("Node_Event_Gizmo"), ref DialogueEventNode);

        if (DialogueEventNode)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;


                MAT_Editor.BeginBoxGroup(_target.EventNodeDatas[i].NodeGuid, index + 1);
                _target.EventNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.EventNodeDatas[i].Position);

                MAT_Editor.BeginBoxGroup("Event List", _target.EventNodeDatas[i].EventScriptableObjects.Count);
                for (int x = 0; x < _target.EventNodeDatas[i].EventScriptableObjects.Count; x++)
                {
                    _target.EventNodeDatas[i].EventScriptableObjects[x].DialogueEventSO = (DialogueEventSO)EditorGUILayout.ObjectField($"Event ID: {x + 1}", _target.EventNodeDatas[i].EventScriptableObjects[x].DialogueEventSO, typeof(DialogueEventSO), false);
                }
                MAT_Editor.EndBoxGroup();

                MAT_Editor.EndBoxGroup();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Random Node
        count = _target.RandomNodeDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"Random Node [{count}]", "All Dialogue Draws", MAT_Editor.GetTinyIcon("Node_Random_Gizmo"), ref RandomNode);

        if (RandomNode)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;

                MAT_Editor.BeginBoxGroup(_target.RandomNodeDatas[i].NodeGuid, index + 1);
                _target.RandomNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.RandomNodeDatas[i].Position);

                MAT_Editor.BeginBoxGroup("Port List", _target.RandomNodeDatas[i].DialogueNodePorts.Count);
                for (int x = 0; x < _target.RandomNodeDatas[i].DialogueNodePorts.Count; x++)
                {
                    _target.RandomNodeDatas[i].DialogueNodePorts[x].InputGuid = EditorGUILayout.TextField($"Port GUID ID: {x + 1}", _target.RandomNodeDatas[i].DialogueNodePorts[x].InputGuid);
                }
                MAT_Editor.EndBoxGroup();

                MAT_Editor.EndBoxGroup();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        //Coming Soon
        MAT_Editor.FoldoutGroup($"Sleep Node [--]", "Coming Soon [Most probably Version 1.8.0a]", MAT_Editor.GetTinyIcon("Node_Sleep_Gizmo"), ref SleepNode);

        #region IF Node
        count = _target.IfNodeDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"IF Node [{count}]", "List of Dialogue Conditions", MAT_Editor.GetTinyIcon("Node_IF_Gizmo"), ref IFNode);

        if (IFNode)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();
                // Display Node
                MAT_Editor.BeginBoxGroup(_target.IfNodeDatas[i].NodeGuid, index + 1);

                _target.IfNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.IfNodeDatas[i].Position);
                _target.IfNodeDatas[i].ValueName = EditorGUILayout.TextField("Global Value Name", _target.IfNodeDatas[i].ValueName);
                _target.IfNodeDatas[i].Operations = (GlobalValueIFOperations)EditorGUILayout.EnumPopup("Operation", _target.IfNodeDatas[i].Operations);
                _target.IfNodeDatas[i].OperationValue = EditorGUILayout.TextField("Operation Value", _target.IfNodeDatas[i].OperationValue);

                EditorGUILayout.EndVertical();
                // Display Node
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Decoration Nodes", EditorStyles.boldLabel);

        #region Comment Node
        count = _target.CommandNodeDatas.Count;

        // Foldout
        MAT_Editor.FoldoutGroup($"Comment Node [{count}]", "Notes in DIalog [Editor Only]", MAT_Editor.GetTinyIcon("Node_Comment_Gizmo"), ref CommandNode);

        if (CommandNode)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();
                // Display Node

                MAT_Editor.BeginBoxGroup(_target.CommandNodeDatas[i].NodeGuid, index + 1);
                _target.CommandNodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", _target.CommandNodeDatas[i].Position);
                _target.CommandNodeDatas[i].commmand = EditorGUILayout.TextField("Comment", _target.CommandNodeDatas[i].commmand, EditorStyles.textArea);
                EditorGUILayout.EndVertical();
                // Display Node
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        serializedObject.ApplyModifiedProperties();
    }

    #region Translate

    public void AutoTranslate(DialogueContainerSO _target)
    {
        bool isGoogle = false;
        string apiKey = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").DeeplApiKey;
        if(apiKey == "") { apiKey = Resources.Load<MeetAndTalkSettings>("MeetAndTalkSettings").DeeplApiKey; isGoogle = true;  }

        if (string.IsNullOrEmpty(apiKey))
        {
            EditorUtility.DisplayDialog("API Key Missing", "DeepL API ot Google Translate Key is missing. Please set it in the settings.", "OK");
            return;
        }

        foreach (var dialogueData in _target.DialogueNodeDatas)
        {
            TranslateTextList(dialogueData.Texts, apiKey, isGoogle);
            TranslateTextList(dialogueData.Timelapses, apiKey, isGoogle);
        }
        foreach (var dialogueChoiceData in _target.DialogueChoiceNodeDatas)
        {
            TranslateTextList(dialogueChoiceData.TextType, apiKey, isGoogle);
            TranslateNodePorts(dialogueChoiceData.DialogueNodePorts, apiKey, isGoogle);
        }
        foreach (var timerChoiceData in _target.TimerChoiceNodeDatas)
        {
            TranslateTextList(timerChoiceData.TextType, apiKey, isGoogle);
            TranslateNodePorts(timerChoiceData.DialogueNodePorts, apiKey, isGoogle);
        }
    }

    private void TranslateTextList(List<LanguageGeneric<string>> textList, string apiKey, bool isGoogle)
    {
        // Znajd� tekst angielski
        var englishText = textList.Find(t => t.languageEnum == LocalizationEnum.English)?.LanguageGenericType;
        if (string.IsNullOrEmpty(englishText))
        {
            Debug.LogWarning("No English text found for translation.");
            return;
        }

        foreach (var text in textList)
        {
            // Je�li pole w innym j�zyku jest ju� wype�nione, pomi� t�umaczenie
            if (text.languageEnum != LocalizationEnum.English && string.IsNullOrEmpty(text.LanguageGenericType))
            {
                if(isGoogle) text.LanguageGenericType = TranslateTextWithGoogle(englishText, text.languageEnum.ToString(), apiKey);
                else text.LanguageGenericType = TranslateTextWithDeepL(englishText, text.languageEnum.ToString(), apiKey);
            }
        }
    }
    private void TranslateNodePorts(List<DialogueNodePort> nodePorts, string apiKey, bool isGoogle)
    {
        foreach (var port in nodePorts)
        {
            TranslateTextList(port.TextLanguage, apiKey, isGoogle);
            TranslateTextList(port.HintLanguage, apiKey, isGoogle);
        }
    }

    private string TranslateTextWithDeepL(string text, string targetLanguage, string apiKey)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null; // Zwraca null, je�li tekst jest pusty
        }

        string url = "https://api-free.deepl.com/v2/translate";

        WWWForm form = new WWWForm();
        form.AddField("auth_key", apiKey);
        form.AddField("text", text);
        form.AddField("target_lang", LocalizationManager.GetIsoLanguageCode(targetLanguage));

        using (var www = UnityWebRequest.Post(url, form))
        {
            www.SendWebRequest();

            while (!www.isDone) { }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                return null;
            }
            else
            {
                var jsonResponse = JsonUtility.FromJson<DeepLResponse>(www.downloadHandler.text);
                return jsonResponse.translations[0].text;
            }
        }
    }

    private string TranslateTextWithGoogle(string text, string targetLanguage, string apiKey)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null; // Zwraca null, je�li tekst jest pusty
        }

        string url = $"https://translation.googleapis.com/language/translate/v2?key={apiKey}";

        // Tworzymy cia�o ��dania JSON
        var requestData = new
        {
            q = text,
            target = targetLanguage
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (var www = UnityWebRequest.PostWwwForm(url, UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            www.SendWebRequest();

            while (!www.isDone) { }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                return null;
            }
            else
            {
                var jsonResponse = JsonUtility.FromJson<GoogleTranslateResponse>(www.downloadHandler.text);
                return jsonResponse.data.translations[0].translatedText;
            }
        }
    }

    [Serializable]
    private class DeepLResponse
    {
        public List<Translation> translations;

        [Serializable]
        public class Translation
        {
            public string text;
        }
    }

    [System.Serializable]
    private class GoogleTranslateResponse
    {
        public TranslationData data;

        [System.Serializable]
        public class TranslationData
        {
            public List<Translation> translations;
        }

        [System.Serializable]
        public class Translation
        {
            public string translatedText;
        }
    }

    #endregion

}

public static class MAT_Editor
{
    public static bool FoldoutGroup(string Name, string Description, Texture Icon, ref bool State)
    {
        EditorGUILayout.BeginVertical("helpbox");

        // Base
        float height = EditorGUIUtility.singleLineHeight;
        float margin = 0;
        float marginTop = 0;
        float foldoutWidth = 4;

        if (!string.IsNullOrEmpty(Description) || Icon != null) height += 14;
        if (string.IsNullOrEmpty(Description) && Icon != null) marginTop = 7;

        if (Description != "" || Icon != null) foldoutWidth = 0;

        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(height));
        EditorGUI.HelpBox(new Rect(rect.x - 4, rect.y - 3, rect.width + 8, rect.height + 6), "", MessageType.None);

        State = EditorGUI.Foldout(new Rect(rect.x - 4, rect.y - 3, rect.width + 8, rect.height + 6), State, "", true, EditorStyles.label);

        EditorGUI.HelpBox(new Rect(rect.x + rect.width - rect.height - 4, rect.y - 3, rect.height + 8, rect.height + 6), "", MessageType.None);

        Rect foldoutRect = new Rect(rect.x + rect.width - ((rect.height + 8) / 4) + foldoutWidth, rect.y, 0, rect.height);
        State = EditorGUI.Foldout(foldoutRect, State, "");



        // Add Margin
        if (Icon != null)
        {
            margin += rect.height + 6;
            //EditorGUI.HelpBox(new Rect(rect.x - 4, rect.y - 3, rect.height + 6, rect.height + 6), "", MessageType.None);
            EditorGUI.LabelField(new Rect(rect.x - 4, rect.y - 3, rect.height + 6, rect.height + 6), new GUIContent(Icon));

            //EditorGUI.LabelField(new Rect(rect.x - 4, rect.y - 3, rect.height + 6, rect.height + 6), new GUIContent(Icon));
        }

        // Title
        EditorGUI.LabelField(new Rect(rect.x + margin, rect.y + marginTop, rect.width - foldoutWidth, EditorGUIUtility.singleLineHeight), Name, EditorStyles.boldLabel);
        if (Description != "") EditorGUI.LabelField(new Rect(rect.x + margin, rect.y + 14, rect.width - foldoutWidth, EditorGUIUtility.singleLineHeight), Description, EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.EndVertical();

        return State;
    }

    public static void BeginBoxGroup(string Title, int ID)
    {
        EditorGUILayout.BeginVertical("helpbox");

        // Base
        float height = EditorGUIUtility.singleLineHeight;
        float margin = 0;
        float marginTop = 0;
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(height));
        EditorGUI.HelpBox(new Rect(rect.x - 4, rect.y - 3, rect.width + 8, rect.height + 6), "", MessageType.None);
        EditorGUI.HelpBox(new Rect(rect.x - 4, rect.y - 3, rect.height + 6, rect.height + 6), "", MessageType.None);

        // Title
        EditorGUI.LabelField(new Rect(rect.x + margin + rect.height + 6, rect.y + marginTop, rect.width, EditorGUIUtility.singleLineHeight), Title, EditorStyles.boldLabel);
        EditorGUI.LabelField(new Rect(rect.x - 4, rect.y - 3, rect.height + 6, rect.height + 6), ID.ToString("d2"), EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.Space(2);
    }
    public static void EndBoxGroup() { EditorGUILayout.EndVertical(); }

    public static Texture GetTinyIcon(string IconName)
    {
        Texture tmp = Resources.Load($"Icon/MT_{IconName}") as Texture;

        if (tmp == null)
        {
            tmp = Resources.Load(IconName) as Texture;
            if (tmp == null)
            {
                tmp = EditorGUIUtility.IconContent(IconName).image;
            }
        }

        return tmp;
    }
}

#endif

