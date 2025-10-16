using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.IO;
using System;
#endif
using UnityEngine;
using UnityEngine.UIElements;

using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using MeetAndTalk.Event;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Serialization;

namespace MeetAndTalk
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue")]
    [System.Serializable]
    public class DialogueContainerSO : ScriptableObject
    {
        // Pro Feature: Load Saved Dialogue
        public bool AllowDialogueSave = false;
        public bool BlockingReopeningDialogue = false;

        public List<NodeLinkData> NodeLinkDatas = new List<NodeLinkData>();

        public List<DialogueChoiceNodeData> DialogueChoiceNodeDatas = new List<DialogueChoiceNodeData>();
        public List<DialogueNodeData> DialogueNodeDatas = new List<DialogueNodeData>();
        public List<TimerChoiceNodeData> TimerChoiceNodeDatas = new List<TimerChoiceNodeData>();
        public List<EndNodeData> EndNodeDatas = new List<EndNodeData>();
        public List<EventNodeData> EventNodeDatas = new List<EventNodeData>();
        public List<StartNodeData> StartNodeDatas = new List<StartNodeData>();
        public List<RandomNodeData> RandomNodeDatas = new List<RandomNodeData>();
        public List<CommandNodeData> CommandNodeDatas = new List<CommandNodeData>();
        public List<IfNodeData> IfNodeDatas = new List<IfNodeData>();

        public List<BaseNodeData> AllNodes
        {
            get
            {
                List<BaseNodeData> tmp = new List<BaseNodeData>();
                tmp.AddRange(DialogueNodeDatas);
                tmp.AddRange(DialogueChoiceNodeDatas);
                tmp.AddRange(TimerChoiceNodeDatas);
                tmp.AddRange(EndNodeDatas);
                tmp.AddRange(EventNodeDatas);
                tmp.AddRange(StartNodeDatas);
                tmp.AddRange(RandomNodeDatas);
                tmp.AddRange(CommandNodeDatas);
                tmp.AddRange(IfNodeDatas);

                return tmp;
            }
        }

#if UNITY_EDITOR
        public void GenerateCSV(string filePath, DialogueContainerSO SO)
        {
            // List to store TSV content
            List<string> tsvContent = new List<string>();

            // Define file path for saving TSV file

            /* GENERATING HEADER */
            // List to store header texts
            List<string> headerTexts = new List<string>();
            headerTexts.Add("GUID ID"); // Add GUID ID as the first header
                                        // Loop through each language enum and add it to the header
            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                headerTexts.Add(language.ToString());
            }
            // Concatenate header texts with tab separators
            string finalHeader = string.Join("\t", headerTexts);

            // Write header to file
            TextWriter tw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            tw.WriteLine(finalHeader);
            tw.Close();
            /* GENERATING HEADER */

            /* GENERATING TEXT CONTENT */
            // Loop through each type of dialogue node to extract text content
            // Dialogue Node
            for (int i = 0; i < SO.DialogueNodeDatas.Count; i++)
            {
                List<string> dialogueNodeContent = new List<string>();
                dialogueNodeContent.Add(SO.DialogueNodeDatas[i].NodeGuid); // Add Node GUID
                                                                           // Loop through each text type for the dialogue node
                for (int j = 0; j < SO.DialogueNodeDatas[i].Texts.Count; j++)
                {
                    dialogueNodeContent.Add(SO.DialogueNodeDatas[i].Texts[j].LanguageGenericType);
                }
                // Concatenate dialogue node content with tab separators
                string dialogueNodeFinal = string.Join("\t", dialogueNodeContent);
                tsvContent.Add(dialogueNodeFinal); // Add dialogue node content to TSV content list
            }

            // Choice Dialogue Node
            for (int i = 0; i < SO.DialogueChoiceNodeDatas.Count; i++)
            {
                List<string> choiceNodeContent = new List<string>();
                choiceNodeContent.Add(SO.DialogueChoiceNodeDatas[i].NodeGuid); // Add Node GUID

                // Loop through each text type for the choice dialogue node
                for (int j = 0; j < SO.DialogueChoiceNodeDatas[i].TextType.Count; j++)
                {
                    choiceNodeContent.Add(SO.DialogueChoiceNodeDatas[i].TextType[j].LanguageGenericType);
                }
                // Concatenate choice dialogue node content with tab separators
                string choiceNodeFinal = string.Join("\t", choiceNodeContent);
                tsvContent.Add(choiceNodeFinal); // Add choice dialogue node content to TSV content list

                // Loop through each dialogue node port for the choice dialogue node
                for (int j = 0; j < SO.DialogueChoiceNodeDatas[i].DialogueNodePorts.Count; j++)
                {
                    SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].ConvertForTSV(ref tsvContent);
                    // List<string> choiceNodeChoiceContent = new List<string>();
                    // choiceNodeChoiceContent.Add(SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].PortGuid); // Add Port GUID

                    // // Loop through each text language for the dialogue node port
                    // for (int k = 0; k < SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage.Count; k++)
                    // {
                    //     choiceNodeChoiceContent.Add(SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage[k].LanguageGenericType);
                    // }
                    // // Concatenate choice dialogue node port content with tab separators
                    // string choiceNodeChoiceFinal = string.Join("\t", choiceNodeChoiceContent);
                    // tsvContent.Add(choiceNodeChoiceFinal); // Add choice dialogue node port content to TSV content list
                }
            }

            // Timer Choice Node
            for (int i = 0; i < SO.TimerChoiceNodeDatas.Count; i++)
            {
                List<string> choiceNodeContent = new List<string>();
                choiceNodeContent.Add(SO.TimerChoiceNodeDatas[i].NodeGuid); // Add Node GUID

                // Loop through each text type for the timer choice node
                for (int j = 0; j < SO.TimerChoiceNodeDatas[i].TextType.Count; j++)
                {
                    choiceNodeContent.Add(SO.TimerChoiceNodeDatas[i].TextType[j].LanguageGenericType);
                }
                // Concatenate timer choice node content with tab separators
                string choiceNodeFinal = string.Join("\t", choiceNodeContent);
                tsvContent.Add(choiceNodeFinal); // Add timer choice node content to TSV content list

                // Loop through each dialogue node port for the timer choice node
                for (int j = 0; j < SO.TimerChoiceNodeDatas[i].DialogueNodePorts.Count; j++)
                {
                    SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].ConvertForTSV(ref tsvContent);
                    // List<string> choiceNodeChoiceContent = new List<string>();
                    // choiceNodeChoiceContent.Add(SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].PortGuid); // Add Port GUID

                    // // Loop through each text language for the dialogue node port
                    // for (int k = 0; k < SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage.Count; k++)
                    // {
                    //     choiceNodeChoiceContent.Add(SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage[k].LanguageGenericType);
                    // }
                    // // Concatenate timer choice node port content with tab separators
                    // string choiceNodeChoiceFinal = string.Join("\t", choiceNodeChoiceContent);
                    // tsvContent.Add(choiceNodeChoiceFinal); // Add timer choice node port content to TSV content list
                }
            }
            /* GENERATING TEXT CONTENT */

            // Append content to file
            tw = new StreamWriter(filePath, true, System.Text.Encoding.UTF8);
            // Write each line of TSV content to file
            foreach (string line in tsvContent)
            {
                tw.WriteLine(line);
            }
            tw.Close();

            // Log file path
            Debug.Log("TSV file generated at: " + filePath);
        }
        public void ImportText(string filePath, DialogueContainerSO SO)
        {
            // Define the file path for the text file
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                // Log an error if the file does not exist
                Debug.LogError("File does not exist at path: " + filePath);
                return;
            }

            try
            {
                // Open the file for reading
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    bool headerSkipped = false;
                    // Read each line of the file
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Skip the header line
                        if (!headerSkipped)
                        {
                            headerSkipped = true;
                            continue;
                        }

                        // Split the line into fields
                        string[] fields = line.Split('\t');

                        // Get GUID
                        string nodeGuid = fields[0];

                        // Update Dialogue Node data
                        for (int i = 0; i < SO.DialogueNodeDatas.Count; i++)
                        {
                            if (nodeGuid == SO.DialogueNodeDatas[i].NodeGuid)
                            {
                                // Update text for each language
                                for (int j = 0; j < fields.Length - 1; j++)
                                {
                                    SO.DialogueNodeDatas[i].Texts[j].LanguageGenericType = fields[j + 1];
                                }
                            }
                        }

                        // Update Choice Node data
                        for (int i = 0; i < SO.DialogueChoiceNodeDatas.Count; i++)
                        {
                            // Update text for Choice Node
                            if (nodeGuid == SO.DialogueChoiceNodeDatas[i].NodeGuid)
                            {
                                // Update text for each language
                                for (int j = 0; j < fields.Length - 1; j++)
                                {
                                    SO.DialogueChoiceNodeDatas[i].TextType[j].LanguageGenericType = fields[j + 1];
                                }
                            }

                            // Update text for Answer Nodes
                            foreach (var port in SO.DialogueChoiceNodeDatas[i].DialogueNodePorts)
                            {
                                if (port.PortGuid == nodeGuid)
                                    port.ExtractDataFromTSV(fields, true);
                            }
                        }

                        // Update Timer Choice Node data
                        for (int i = 0; i < SO.TimerChoiceNodeDatas.Count; i++)
                        {
                            // Update text for Choice Node
                            if (nodeGuid == SO.TimerChoiceNodeDatas[i].NodeGuid)
                            {
                                // Update text for each language
                                for (int j = 0; j < fields.Length - 1; j++)
                                {
                                    SO.TimerChoiceNodeDatas[i].TextType[j].LanguageGenericType = fields[j + 1];
                                }
                            }

                            // Update text for Answer Nodes
                            foreach (var port in SO.DialogueChoiceNodeDatas[i].DialogueNodePorts)
                            {
                                if (port.PortGuid == nodeGuid)
                                    port.ExtractDataFromTSV(fields, false);
                            }
                        }
                    }
                }

                // Log success message
                Debug.Log("Text imported successfully.");
            }
            catch (Exception e)
            {
                // Log error message if an exception occurs
                Debug.LogError("Error while importing text: " + e.Message);
            }
        }
#endif
    }
    [System.Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGuid;
        public string TargetNodeGuid;
    }

    [System.Serializable]
    public class BaseNodeData
    {
        public string NodeGuid;
        public Vector2 Position;
    }

    [System.Serializable]
    public class DialogueChoiceNodeData : BaseNodeData
    {
        public List<DialogueNodePort> DialogueNodePorts;
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public DialogueCharacterSO Character;
        public AvatarPosition AvatarPos;
        public AvatarType AvatarType;
        public List<LanguageGeneric<string>> TextType;
        public float Duration;
        public float Delay;
        public string Timelapse;
        public bool RequireCharacterInput;
        public List<LanguageGeneric<string>> SelectedChoice;
        public string GetText(LocalizationManager localizationManager) => TextType.Find(text => text.languageEnum == localizationManager.SelectedLang()).LanguageGenericType;

        protected float _delayTimer = 0;

        public virtual void Reset()
        {
            _delayTimer = 0;
        }

        public virtual bool ShouldDelay()
        {
            _delayTimer += Time.deltaTime * (float)DialogueManager.Instance.DisplaySpeedMultipler;
            if (_delayTimer >= Duration)
                return true;

            return false;
        }
    }

    [System.Serializable]
    public class TimerChoiceNodeData : DialogueChoiceNodeData
    {
        public float time;
    }

    [System.Serializable]
    public class RandomNodeData : BaseNodeData
    {
        public List<DialogueNodePort> DialogueNodePorts;
    }

    [System.Serializable]
    public class DialogueNodeData : BaseNodeData
    {
        public List<DialogueNodePort> DialogueNodePorts;
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public DialogueCharacterSO Character;
        public AvatarPosition AvatarPos;
        public AvatarType AvatarType;
        [FormerlySerializedAs("TextType")] public List<LanguageGeneric<string>> Texts;
        public List<LanguageGeneric<string>> Timelapses;
        [HideInInspector] public string Timelapse;
        public float Duration;
        public float Delay;
        [FormerlySerializedAs("PostMediaType")] public MediaType MediaType;
        public Sprite Image;
        public VideoClip Video;
        public Sprite VideoThumbnail;
        public bool NotBackgroundCapable;
        public GalleryDisplay GalleryVisibility;
        public SocialMediaPostSO Post;
        public string GetText() => Texts.Find(text => text.languageEnum == GameManager.LOCALIZATION_MANAGER.SelectedLang()).LanguageGenericType;
        public string GetTimeLapse() => Timelapses.Find(x => x.languageEnum == GameManager.LOCALIZATION_MANAGER.SelectedLang()).LanguageGenericType;

        // public DialogueNodeData()
        // {
        //     // if (Timelapse != null && Timelapse != "")
        //     // {
        //     //     Timelapses = new List<LanguageGeneric<string>>();
        //     //     for (var index = 0; index < Texts.Count; index++)
        //     //         Timelapses.Add(new LanguageGeneric<string>() { languageEnum = Texts[index].languageEnum, LanguageGenericType = Timelapse });
        //     // }
        // }

        public string MediaFileName
        {
            get
            {
                switch (MediaType)
                {
                    case MediaType.Sprite:
                        return Image != null ? Image.name : string.Empty;
                    case MediaType.Video:
                        return Video != null ? Video.name : string.Empty;
                }

                return string.Empty;
            }
        }

        public float DelayTimer = 0;
        public void Reset()
        {
            DelayTimer = 0;
        }

        public bool ShouldDelay()
        {
            DelayTimer += (Time.deltaTime * (float)DialogueManager.Instance.DisplaySpeedMultipler);
            if (DelayTimer <= Duration)
                return true;

            return false;
        }

        public (MediaType, Sprite, VideoClip, Sprite, bool) GetNodeMediaData(bool fromSocialMediaPost)
        {
            if (fromSocialMediaPost && Post != null)
            {
                return (Post.MediaType, Post.Image, Post.Video, Post.VideoThumbnail, !Post.NotBackgroundCapable);
            }
            else
            {
                return (MediaType, Image, Video, VideoThumbnail, !NotBackgroundCapable);
            }
        }

        public (Sprite, bool) GetNodeImageData(bool fromSocialMediaPost)
        {
            if (fromSocialMediaPost)
            {
                return (Post.Image, !Post.NotBackgroundCapable);
            }
            else
            {
                return (Image, !NotBackgroundCapable);
            }
        }

        public (VideoClip, Sprite) GetNodeVideoData(bool fromSocialMediaPost)
        {
            if (fromSocialMediaPost)
                return (Post.Video, Post.VideoThumbnail);
            else
                return (Video, VideoThumbnail);
        }
    }

    [System.Serializable]
    public class EndNodeData : BaseNodeData
    {
        public EndNodeType EndNodeType;
        public DialogueContainerSO Dialogue;
    }

    [System.Serializable]
    public class StartNodeData : BaseNodeData
    {
        public string startID;
    }


    [System.Serializable]
    public class EventNodeData : BaseNodeData
    {
        public List<EventScriptableObjectData> EventScriptableObjects;
    }
    [System.Serializable]
    public class EventScriptableObjectData
    {
        public DialogueEventSO DialogueEventSO;
    }

    [System.Serializable]
    public class CommandNodeData : BaseNodeData
    {
        public string commmand;
    }

    [System.Serializable]
    public class IfNodeData : BaseNodeData
    {
        public string ValueName;
        public GlobalValueIFOperations Operations;
        public string OperationValue;

        public string TrueGUID;
        public string FalseGUID;
    }


    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public LocalizationEnum languageEnum;
        public T LanguageGenericType;
    }

    [System.Serializable]
    public class DialogueNodePort
    {
        public static string BLANK_HINT = "";
        public string PortGuid; //NOWE
        public string InputGuid;
        public string OutputGuid;
#if UNITY_EDITOR
        [HideInInspector] public Port MyPort;
#endif
        public TextField TextField;
        public TextField HintField;
        public List<LanguageGeneric<string>> TextLanguage = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<string>> HintLanguage = new List<LanguageGeneric<string>>();

        public void ConvertForTSV(ref List<string> tsv)
        {
            // Loop through each dialogue node port for the choice dialogue node
            List<string> choiceNodeChoiceContent = new()
            {
                PortGuid // Add Port GUID
            };

            // Loop through each text and hintlanguage for the dialogue node port
            for (var index = 0; index < TextLanguage.Count; index++)
            {
                choiceNodeChoiceContent.Add(TextLanguage[index].LanguageGenericType);
                choiceNodeChoiceContent.Add(HintLanguage[index].LanguageGenericType == "" ? BLANK_HINT : HintLanguage[index].LanguageGenericType);
            }
            // foreach (var lang in TextLanguage)

            // // Loop through each hint language for the dialogue node port
            // foreach (var lang in HintLanguage)
            //     choiceNodeChoiceContent.Add(lang.LanguageGenericType);

            // Concatenate choice dialogue node port content with tab separators
            string choiceNodeChoiceFinal = string.Join("\t", choiceNodeChoiceContent);
            tsv.Add(choiceNodeChoiceFinal); // Add choice dialogue node port content to TSV content list
        }

        public void ExtractDataFromTSV(string[] fields, bool includeHint)
        {
            var choiceCount = TextLanguage.Count;
            for (int index = 0; index < choiceCount; index++)
            {
                var fieldPosition = (index * (includeHint ? 2 : 1)) + 1;
                TextLanguage[index].LanguageGenericType = fields[fieldPosition];

                if (includeHint)
                {
                    var text = fields[fieldPosition + 1];
                    HintLanguage[index].LanguageGenericType = text == BLANK_HINT ? "" : text;
                }
            }
        }

        public void GetConvertedText(LocalizationEnum lang, out string text, out string hint)
        {
            text = DialogueLocalizationHelper.GetText(TextLanguage);
            var hintLang = DialogueLocalizationHelper.GetText(HintLanguage);
            hint = "";
            if (hintLang != null)
            {
                hint = hintLang;
            }
        }
    }

    [System.Serializable]
    public enum EndNodeType
    {
        End,
        Repeat,
        GoBack,
        ReturnToStart,
        StartDialogue
    }
}