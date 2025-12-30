using System;
using MeetAndTalk;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class GalleryMediaItem
{
    public DialogueNodeData Node;
    public DialogueCharacterSO Character
    {
        get
        {
            if (Node.Post != null && Node.Post.Character != null)
                return Node.Post.Character;

            return Node.Character;
        }
    }

    public string FileName => MediaType switch
    {
        MediaType.Sprite => Image != null ? Image.name : string.Empty,
        MediaType.Video => Video != null ? Video.name : string.Empty,
        _ => string.Empty,
    };

    public bool IsSocialMediaPost;
    public MediaType MediaType;
    public Sprite Image;
    public VideoClip Video;
    public Sprite VideoThumbnail;
    public MediaLockState LockState;
    public bool IsLinearPathUnlock;
    public DialogueChapterManager.ChapterData ChapterData;
}