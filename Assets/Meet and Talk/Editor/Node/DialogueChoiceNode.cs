using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using MeetAndTalk.Editor;
using MeetAndTalk.Localization;
using MeetAndTalk.Event;

namespace MeetAndTalk.Nodes
{
    public class DialogueChoiceNode : BaseNode
    {
        protected List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        protected List<LanguageGeneric<AudioClip>> audioClip = new List<LanguageGeneric<AudioClip>>();
        protected DialogueCharacterSO character = ScriptableObject.CreateInstance<DialogueCharacterSO>();
        protected float durationShow = DialogueManager.BASE_NODE_DISPLAY_TIME;
        protected bool requireCharacterInput;


        public List<DialogueNodePort> dialogueNodePorts = new List<DialogueNodePort>();

        public List<LanguageGeneric<string>> Texts { get => texts; set => texts = value; }
        public List<LanguageGeneric<AudioClip>> AudioClip { get => audioClip; set => audioClip = value; }
        public DialogueCharacterSO Character { get => character; set => character = value; }
        public float DurationShow { get => durationShow; set => durationShow = value; }
        public bool RequireInput { get => requireCharacterInput; set => requireCharacterInput = value; }

        protected TextField texts_Field;
        protected ObjectField audioClips_Field;
        protected FloatField duration_Field;
        protected ObjectField character_Field;
        protected Toggle requireCharacterInput_Field;

        public AvatarPosition avatarPosition;
        public AvatarType avatarType;
        protected EnumField AvatarPositionField;
        protected EnumField AvatarTypeField;


        public DialogueChoiceNode()
        {

        }

        public DialogueChoiceNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Choice";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            AddInputPort("Input ", Port.Capacity.Multi);

            AddValidationContainer();

            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                texts.Add(new LanguageGeneric<string>
                {
                    languageEnum = language,
                    LanguageGenericType = ""
                });
                AudioClip.Add(new LanguageGeneric<AudioClip>
                {
                    languageEnum = language,
                    LanguageGenericType = null
                });
            }

            CreateFields();
            PopulateContainer();
        }

        protected virtual void CreateFields()
        {
            /* AUDIO CLIPS */
            audioClips_Field = new ObjectField()
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType,
            };
            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            character_Field = new ObjectField()
            {
                objectType = typeof(DialogueCharacterSO),
                allowSceneObjects = false,
            };

            character_Field.RegisterValueChangedCallback(value =>
            {
                character = value.newValue as DialogueCharacterSO;
            });

            character_Field.SetValueWithoutNotify(character);

            AvatarPositionField = new EnumField("Avatar Position", avatarPosition);
            AvatarPositionField.RegisterValueChangedCallback(value =>
            {
                avatarPosition = (AvatarPosition)value.newValue;
            });
            AvatarPositionField.SetValueWithoutNotify(avatarPosition);

            AvatarTypeField = new EnumField("Avatar Emotion", avatarType);
            AvatarTypeField.RegisterValueChangedCallback(value =>
            {
                avatarType = (AvatarType)value.newValue;
            });
            AvatarTypeField.SetValueWithoutNotify(avatarType);

            //Require Character Field
            requireCharacterInput_Field = new Toggle("Has Character Input");
            requireCharacterInput_Field.RegisterValueChangedCallback(value =>
            {
                requireCharacterInput = value.newValue;
                AddTextFields(value.newValue);
            });

            requireCharacterInput_Field.SetValueWithoutNotify(requireCharacterInput);


            /* TEXT BOX */
            // displayText_Label = new Label("Displayed Text");
            // displayText_Label.AddToClassList("label_texts");
            // displayText_Label.AddToClassList("Label");

            texts_Field = new TextField("");
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            texts_Field.multiline = true;
            texts_Field.AddToClassList("TextBox");

            /* DIALOGUE DURATION */
            duration_Field = new FloatField("Delay to Display");
            duration_Field.RegisterValueChangedCallback(value =>
            {
                durationShow = value.newValue;
            });

            duration_Field.SetValueWithoutNotify(durationShow);
            duration_Field.AddToClassList("TextDuration");


            Button button = new Button()
            {
                text = "+ Add Choice Option"
            };
            button.clicked += () =>
            {
                AddChoicePort(this);
            };

            titleButtonContainer.Add(button);
        }

        protected virtual void PopulateContainer()
        {
            /* Character CLIPS */
            Label label_character = new Label("Character SO");
            label_character.AddToClassList("label_name");
            label_character.AddToClassList("Label");

            mainContainer.Add(label_character);
            mainContainer.Add(character_Field);
            mainContainer.Add(AvatarPositionField);
            mainContainer.Add(AvatarTypeField);
            mainContainer.Add(requireCharacterInput_Field);

            Label label_audio = new Label("Audio Clip");
            label_audio.AddToClassList("label_name");
            label_audio.AddToClassList("Label");
            mainContainer.Add(label_audio);
            mainContainer.Add(audioClips_Field);
            
            if (RequireInput)
            {
                Label displayText_Label = new Label("Displayed Text");
                displayText_Label.AddToClassList("label_texts");
                displayText_Label.AddToClassList("Label");
                mainContainer.Add(displayText_Label);
                mainContainer.Add(texts_Field);
            }

            mainContainer.Add(duration_Field);

        }

        protected virtual void AddTextFields(bool add)
        {
            // mainContainer.Add(displayText_Label);
            // mainContainer.Add(texts_Field);
                
            // if (add)
            // {
            //     mainContainer.Add(displayText_Label);
            //     mainContainer.Add(texts_Field);
            // }
            // else
            // {
            //     if (mainContainer.Contains(displayText_Label))
            //         mainContainer.Remove(displayText_Label);

            //     if (mainContainer.Contains(texts_Field))
            //         mainContainer.Remove(texts_Field);
            //     //mainContainer.Remove(duration_Label);
            //     //mainContainer.Remove(duration_Field);
            // }
        }

        public virtual void ReloadLanguage()
        {
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClip.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            foreach (DialogueNodePort nodePort in dialogueNodePorts)
            {
                TextFieldRegisterLangCallback(nodePort.TextField, nodePort.TextLanguage);
                TextFieldRegisterLangCallback(nodePort.HintField, nodePort.HintLanguage);
            }
        }

        public override void LoadValueInToField()
        {
            texts_Field.SetValueWithoutNotify(texts.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            character_Field.SetValueWithoutNotify(character);
            AvatarPositionField.SetValueWithoutNotify(avatarPosition);
            AvatarTypeField.SetValueWithoutNotify(avatarType);
            duration_Field.SetValueWithoutNotify(durationShow);

            audioClips_Field.SetValueWithoutNotify(audioClip.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            requireCharacterInput_Field.SetValueWithoutNotify(RequireInput);

            PopulateContainer();
        }

        public virtual Port AddChoicePort(BaseNode basenote, DialogueNodePort inDialogueNodePort = null)
        {
            Port port = this.GetPort();
            DialogueNodePort dialogueNodePort = this.GeneratePort(basenote, inDialogueNodePort);

            var mainContainer = this.GeneratePortVisualElement(() => DeleteButton(basenote, port), out dialogueNodePort.TextField, out dialogueNodePort.HintField);
            port.contentContainer.Add(mainContainer);

            // dialogueNodePort.TextField = new TextField();
            TextFieldRegisterLangCallback(dialogueNodePort.TextField, dialogueNodePort.TextLanguage);
            TextFieldRegisterLangCallback(dialogueNodePort.HintField, dialogueNodePort.HintLanguage);

#if UNITY_EDITOR
            dialogueNodePort.MyPort = port;
#endif
            port.portName = "";

            dialogueNodePorts.Add(dialogueNodePort);

            basenote.outputContainer.Add(port);

            basenote.RefreshPorts();
            basenote.RefreshExpandedState();

            return port;
        }

        protected virtual void DeleteButton(BaseNode _node, Port _port)
        {
#if UNITY_EDITOR
            DialogueNodePort tmp = dialogueNodePorts.Find(port => port.MyPort == _port);
            dialogueNodePorts.Remove(tmp);
#endif

            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(edge => edge.output == _port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            _node.outputContainer.Remove(_port);

            _node.RefreshPorts();
            _node.RefreshExpandedState();
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");
            if (dialogueNodePorts.Count < 1) error.Add("You need to add more Choice");
            else
            {
                for (int i = 0; i < dialogueNodePorts.Count; i++)
                {
                    if (!dialogueNodePorts[i].MyPort.connected)
                        error.Add($"Choice ID:{i} does not lead to any node");
                }
            }
            for (int i = 0; i < Texts.Count; i++)
            {
                if (RequireInput && Texts[i].LanguageGenericType == "")
                    warning.Add($"No Text for {Texts[i].languageEnum} Language");
            }

            ErrorList = error;
            WarningList = warning;

            // Update List
            if (character != null)
            {
                AvatarPositionField.style.display = DisplayStyle.Flex;
                AvatarTypeField.style.display = DisplayStyle.Flex;
            }
            else
            {
                AvatarPositionField.style.display = DisplayStyle.None;
                AvatarTypeField.style.display = DisplayStyle.None;
            }
        }
    }
}