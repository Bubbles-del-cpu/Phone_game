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
using UnityEngine.Video;
using UnityEditor;

namespace MeetAndTalk.Nodes
{
    public class DialogueNode : BaseNode
    {
        private List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        private List<LanguageGeneric<string>> timelapses = new List<LanguageGeneric<string>>();
        private string oldTimelapse;
        private List<LanguageGeneric<AudioClip>> audioClips = new List<LanguageGeneric<AudioClip>>();
        private DialogueCharacterSO character = ScriptableObject.CreateInstance<DialogueCharacterSO>();
        private float durationShow = DialogueManager.BASE_NODE_DISPLAY_TIME;
        private MediaType mediaType;
        public GalleryDisplay visibilityType;
        private Sprite image, videoThumbnail;
        private bool notBackgroundCapable;
        private VideoClip video;
        private SocialMediaPostSO post;


        public List<DialogueNodePort> dialogueNodePorts = new List<DialogueNodePort>();

        public List<LanguageGeneric<string>> Texts { get => texts; set => texts = value; }
        public List<LanguageGeneric<string>> TimeLapses { get => timelapses; set => timelapses = value; }
        public string TimeLapse { get => oldTimelapse; set => oldTimelapse = value; }
        public List<LanguageGeneric<AudioClip>> AudioClips { get => audioClips; set => audioClips = value; }
        public DialogueCharacterSO Character { get => character; set => character = value; }
        public float DurationShow { get => durationShow; set => durationShow = value; }
        public MediaType PostMediaType { get => mediaType; set => mediaType = value; }
        public Sprite Image { get => image; set => image = value; }
        public VideoClip Video { get => video; set => video = value; }
        public bool NotBackgroundCapable { get => notBackgroundCapable; set => notBackgroundCapable = value; }
        public Sprite VideoThumbnail { get => videoThumbnail; set => videoThumbnail = value; }
        public SocialMediaPostSO Post { get => post; set => post = value; }
        //public string Timelapse { get => timelapse; set => timelapse = value; }

        private TextField texts_Field;
        private TextField timelapse_Field;
        private EnumField mediaType_Field;
        private ObjectField image_Field;
        private ObjectField videoThumbnail_Field;
        private ObjectField video_Field;
        private Toggle notBackgroundCapable_Field;
        private ObjectField audioClips_Field;
        private TextField name_Field;
        private ObjectField character_Field;
        private FloatField duration_Field;
        private ObjectField socialMedia_Field;

        private List<VisualElement> nodeFields;

        public AvatarPosition avatarPosition;
        public AvatarType avatarType;
        private EnumField AvatarPositionField;
        private EnumField AvatarTypeField;
        private EnumField visibilityTypeField;

        public DialogueNode()
        {

        }

        public DialogueNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Dialogue";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();
            AddInputPort("Input", Port.Capacity.Multi);
            AddOutputPort("Output", Port.Capacity.Single);

            AddValidationContainer();

            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                texts.Add(new LanguageGeneric<string>
                {
                    languageEnum = language,
                    LanguageGenericType = ""
                });

                timelapses.Add(new LanguageGeneric<string>
                {
                    languageEnum = language,
                    LanguageGenericType = ""
                });

                AudioClips.Add(new LanguageGeneric<AudioClip>
                {
                    languageEnum = language,
                    LanguageGenericType = null
                });
            }


            nodeFields = new List<VisualElement>();
            PopulateFields();
        }

        private void ClearAndPopulateFields()
        {
            foreach (var item in nodeFields)
            {
                mainContainer.Remove(item);
            }

            nodeFields.Clear();
            PopulateFields();
        }

        private void PopulateFields()
        {
            /* AUDIO CLIPS */
            audioClips_Field = new ObjectField()
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = audioClips.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType,
            };
            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClips.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClips.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            nodeFields.Add(audioClips_Field);

            /* Character CLIPS */
            Label label_character = new Label("Character SO");
            label_character.AddToClassList("label_name");
            label_character.AddToClassList("Label");
            nodeFields.Add(label_character);
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
            nodeFields.Add(character_Field);

            Label label_characterSM = new Label("Social Media SO");
            label_characterSM.AddToClassList("label_name");
            label_characterSM.AddToClassList("Label");
            nodeFields.Add(label_characterSM);
            socialMedia_Field = new ObjectField()
            {
                objectType = typeof(SocialMediaPostSO),
                allowSceneObjects = false,
            };
            socialMedia_Field.RegisterValueChangedCallback(value =>
            {
                post = value.newValue as SocialMediaPostSO;
            });
            socialMedia_Field.SetValueWithoutNotify(post);
            nodeFields.Add(socialMedia_Field);

            AvatarPositionField = new EnumField("Avatar Position", avatarPosition);
            AvatarPositionField.RegisterValueChangedCallback(value =>
            {
                avatarPosition = (AvatarPosition)value.newValue;
            });
            AvatarPositionField.SetValueWithoutNotify(avatarPosition);
            nodeFields.Add(AvatarPositionField);


            AvatarTypeField = new EnumField("Avatar Emotion", avatarType);
            AvatarTypeField.RegisterValueChangedCallback(value =>
            {
                avatarType = (AvatarType)value.newValue;
            });
            AvatarTypeField.SetValueWithoutNotify(avatarType);
            nodeFields.Add(AvatarTypeField);

            /* TEXT BOX */
            Label label_texts = new Label("Displayed Text");
            label_texts.AddToClassList("label_texts");
            label_texts.AddToClassList("Label");
            nodeFields.Add(label_texts);

            texts_Field = new TextField("");
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            texts_Field.multiline = true;

            texts_Field.AddToClassList("TextBox");
            nodeFields.Add(texts_Field);

            /* TIMELAPSE */
            Label label_timelapse = new Label("Timelapse Text");
            label_timelapse.AddToClassList("label_timelapse");
            label_timelapse.AddToClassList("Label");
            nodeFields.Add(label_timelapse);

            timelapse_Field = new TextField("");
            timelapse_Field.RegisterValueChangedCallback(value =>
            {
                timelapses.Find(tl => tl.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });

            timelapse_Field.multiline = true; // Enable multi-line entry
            timelapse_Field.SetValueWithoutNotify(timelapses.Find(tl => tl.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            timelapse_Field.multiline = true;

            timelapse_Field.AddToClassList("TextTimelapse");
            nodeFields.Add(timelapse_Field);


            /* TEXT NAME */
            Label label_duration = new Label("Display Time");
            label_duration.AddToClassList("label_duration");
            label_duration.AddToClassList("Label");
            nodeFields.Add(label_duration);

            duration_Field = new FloatField("");
            duration_Field.RegisterValueChangedCallback(value =>
            {
                durationShow = value.newValue;
            });

            duration_Field.SetValueWithoutNotify(durationShow);

            duration_Field.AddToClassList("TextDuration");
            nodeFields.Add(duration_Field);

            Label media_label = new Label("Post Media Settings");
            media_label.AddToClassList("label_duration");
            media_label.AddToClassList("Label");
            nodeFields.Add(media_label);

            mediaType_Field = new EnumField("Media Type", mediaType);
            mediaType_Field.RegisterValueChangedCallback(value =>
            {
                mediaType = (MediaType)value.newValue;
                ClearAndPopulateFields();
            });

            mediaType_Field.SetValueWithoutNotify(mediaType);
            nodeFields.Add(mediaType_Field);

            visibilityTypeField = new EnumField("Gallery Visibility", visibilityType);
            visibilityTypeField.RegisterValueChangedCallback(value =>
            {
                visibilityType = (GalleryDisplay)value.newValue;
                ClearAndPopulateFields();
            });

            visibilityTypeField.SetValueWithoutNotify(visibilityType);
            nodeFields.Add(visibilityTypeField);


            image_Field = new ObjectField()
            {
                objectType = typeof(Sprite),
                allowSceneObjects = false,
                value = image
            };

            image_Field.RegisterValueChangedCallback(value =>
            {
                image = value.newValue as Sprite;
            });

            notBackgroundCapable_Field = new Toggle("Background Capable")
            {
                value = notBackgroundCapable
            };

            notBackgroundCapable_Field.RegisterValueChangedCallback(value =>
            {
                notBackgroundCapable = value.newValue;
            });

            videoThumbnail_Field = new ObjectField()
            {
                objectType = typeof(Sprite),
                allowSceneObjects = false,
                value = videoThumbnail
            };

            videoThumbnail_Field.RegisterValueChangedCallback(value =>
            {
                videoThumbnail = value.newValue as Sprite;
            });

            video_Field = new ObjectField()
            {
                objectType = typeof(VideoClip),
                allowSceneObjects = false,
                value = video
            };

            video_Field.RegisterValueChangedCallback(value =>
            {
                video = value.newValue as VideoClip;
            });

            switch (mediaType)
            {
                case MediaType.Sprite:
                    var spriteLabel = new Label("Sprite");
                    spriteLabel.AddToClassList("label_duration");
                    spriteLabel.AddToClassList("Label");

                    if (visibilityType == GalleryDisplay.Display)
                        nodeFields.Add(notBackgroundCapable_Field);

                    nodeFields.Add(spriteLabel);
                    nodeFields.Add(image_Field);
                    break;
                case MediaType.Video:
                    var videoLabel = new Label("Video");
                    videoLabel.AddToClassList("label_duration");
                    videoLabel.AddToClassList("Label");
                    nodeFields.Add(videoLabel);
                    nodeFields.Add(video_Field);

                    var videoThumbnail_label = new Label("Video Fallback Thumbnail");
                    videoThumbnail_label.AddToClassList("label_duration");
                    videoThumbnail_label.AddToClassList("Label");
                    nodeFields.Add(videoThumbnail_label);
                    nodeFields.Add(videoThumbnail_Field);
                    break;
            }

            foreach (var item in nodeFields)
            {
                mainContainer.Add(item);
            }
        }

        public void ReloadLanguage()
        {
            texts_Field.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            texts_Field.SetValueWithoutNotify(texts.Find(text => text.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            timelapse_Field.RegisterValueChangedCallback(value =>
            {
                timelapses.Find(tl => tl.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue;
            });
            timelapse_Field.SetValueWithoutNotify(timelapses.Find(tl => tl.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);

            audioClips_Field.RegisterValueChangedCallback(value =>
            {
                audioClips.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType = value.newValue as AudioClip;
            });
            audioClips_Field.SetValueWithoutNotify(audioClips.Find(audioClips => audioClips.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
        }

        public override void LoadValueInToField()
        {
            texts_Field.SetValueWithoutNotify(texts.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            timelapse_Field.SetValueWithoutNotify(timelapses.Find(tl => tl.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            audioClips_Field.SetValueWithoutNotify(audioClips.Find(language => language.languageEnum == editorWindow.LanguageEnum).LanguageGenericType);
            character_Field.SetValueWithoutNotify(character);
            AvatarPositionField.SetValueWithoutNotify(avatarPosition);
            AvatarTypeField.SetValueWithoutNotify(avatarType);
            duration_Field.SetValueWithoutNotify(durationShow);
            mediaType_Field.SetValueWithoutNotify(mediaType);
            image_Field.SetValueWithoutNotify(image);
            video_Field.SetValueWithoutNotify(video);
            notBackgroundCapable_Field.SetValueWithoutNotify(notBackgroundCapable);
            videoThumbnail_Field.SetValueWithoutNotify(videoThumbnail);
            visibilityTypeField.SetValueWithoutNotify(visibilityType);
            socialMedia_Field.SetValueWithoutNotify(post);

            ClearAndPopulateFields();
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");

            Port output = outputContainer.Query<Port>().First();
            if (!output.connected) error.Add("Output does not lead to any node");

            if (durationShow < 1 && durationShow != 0)
                warning.Add("Short time for Make Decision");

            for (int i = 0; i < Texts.Count; i++)
            {
                if (Texts[i].LanguageGenericType == "")
                    warning.Add($"No Text for {Texts[i].languageEnum} Language");
            }

            for (int i = 0; i < TimeLapses.Count; i++)
            {
                if (TimeLapses[i].LanguageGenericType == "")
                    warning.Add($"No TimeLapses for {TimeLapses[i].languageEnum} Language");
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