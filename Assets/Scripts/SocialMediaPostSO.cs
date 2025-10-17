using UnityEngine;
using MeetAndTalk;
using System.Collections.Generic;
using UnityEngine.Video;

[CreateAssetMenu()]
public class SocialMediaPostSO : ScriptableObject
{
    [System.Serializable]
    public class SocialMediaComment
    {
        public string Name;

        [TextArea]
        public string Content;
    }

    public DialogueCharacterSO Character;

    public List<LanguageGeneric<string>> MessageTexts;
    [HideInInspector] public string Message;
    public MediaType MediaType;
    public GalleryDisplay GalleryVisibility;
    public Sprite Image;
    public VideoClip Video;
    public Sprite VideoThumbnail;
    public bool NotBackgroundCapable;

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

    /// <summary>
    /// List of social media comments that are attached to this post
    /// </summary>
    public List<SocialMediaComment> Comments;

}
