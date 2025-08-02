using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;
using UnityEngine.Events;
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
    public string Message;
    public MediaType MediaType;
    public GalleryDisplay GalleryVisibility;
    public Sprite Image;
    public VideoClip Video;

    /// <summary>
    /// List of social media comments that are attached to this post
    /// </summary>
    public List<SocialMediaComment> Comments;

}
