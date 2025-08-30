using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;

namespace MeetAndTalk
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Character")]
    public class DialogueCharacterSO : ScriptableObject
    {
        [Header("Name")]
        public List<LanguageGeneric<string>> characterName;
        public GlobalValueClass CustomizedName;
        public bool UseGlobalValue = false;
        [Header("Gallery Images")]
        public List<Sprite> Images;
        [Header("Name Color")]
        public Color textColor = new Color(.8f, .8f, .8f, 1);
        [Header("Profile Images")]
        public List<CharacterSprite> Avatars;

        public string HexColor()
        {
            return $"#{ColorUtility.ToHtmlStringRGB(textColor)}";
        }

        private void OnValidate()
        {
            //Validate();

        }

        public void Validate()
        {
#if UNITY_EDITOR
            // Check if the game is not currently playing or about to change play mode
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (characterName != null)
                {
                    if (characterName.Count < System.Enum.GetNames(typeof(LocalizationEnum)).Length)
                    {
                        foreach (LocalizationEnum language in (LocalizationEnum[])System.Enum.GetValues(typeof(LocalizationEnum)))
                        {
                            characterName.Add(new LanguageGeneric<string>
                            {
                                languageEnum = language,
                                LanguageGenericType = ""
                            });
                        }
                    }
                }
                else
                {
                    characterName = new List<LanguageGeneric<string>>();
                    Debug.Log("New");
                }
                if (Avatars != null)
                {
                    if (Avatars.Count < System.Enum.GetNames(typeof(AvatarType)).Length)
                    {
                        foreach (AvatarType language in (AvatarType[])System.Enum.GetValues(typeof(AvatarType)))
                        {
                            Avatars.Add(new CharacterSprite
                            {
                                type = language,
                                LeftPosition = null,
                                RightPosition = null,
                            });
                        }
                    }
                }
                else
                {
                    Avatars = new List<CharacterSprite>();
                    Debug.Log("New");
                }
            }
#endif
        }


        public string GetName()
        {
            LocalizationManager _manager = (LocalizationManager)Resources.Load("Languages");
            if (_manager != null)
            {
                return characterName.Find(text => text.languageEnum == _manager.SelectedLang()).LanguageGenericType;
            }
            else
            {
                return "Can't find Localization Manager in scene";
            }
        }

        public Sprite GetAvatar(AvatarPosition position, AvatarType type)
        {
            CharacterSprite cs = Avatars[(int)type];

            if (cs != null)
            {
                if (position == AvatarPosition.Left) return cs.LeftPosition;
                if (position == AvatarPosition.Right) return cs.RightPosition;
            }

            return null;
        }
    }
}

[System.Serializable]
public class CharacterSprite
{
    public AvatarType type;
    public Sprite LeftPosition;
    public Sprite RightPosition;
}

[System.Serializable]
public enum AvatarPosition { None, Left, Right }

[System.Serializable]
public enum AvatarType { Normal = 0, Smile = 1, Suprized = 2, Disgust = 3, Crying = 4, Angry = 5 }

[System.Serializable]
public enum GalleryDisplay { Display = 0, Hidden = 1 }

[System.Serializable]
public enum MediaType { Sprite = 0, Video = 1 }

[System.Serializable]
public enum MediaLockState { Unknown = 0, Locked = 1, Unlocked = 2 }

[System.Serializable]
public enum ChapterType { Unknown = 0, Story = 1, Standalone = 2 }
