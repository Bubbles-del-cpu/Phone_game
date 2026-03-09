using System;
using System.Linq;
using MeetAndTalk;
using UnityEngine;
using UnityEngine.Video;

public enum MediaTargetPlatform
{
    Gallery,
    SocialMediaPost,
    SpicySocialMediaPost,
}

[Serializable]
public class GalleryMediaItem
{
    private DialogueNodeData _node; // Backing field for the Node property, which will attempt to find the node if it's not already set
    public DialogueNodeData Node
    {
        get
        {
            if (_node != null)
                return _node;

            return null;
        }
        set
        {
            _node = value;
        }

    }
    public string NodeGuid;
    public DialogueCharacterSO Character;

    public string FileName => MediaType switch
    {
        MediaType.Sprite => Image != null ? Image.name : string.Empty,
        MediaType.Video => Video != null ? Video.name : string.Empty,
        _ => string.Empty,
    };

    public bool IsSocialMediaPost => TargetPlatform == MediaTargetPlatform.SocialMediaPost || TargetPlatform == MediaTargetPlatform.SpicySocialMediaPost;
    public MediaTargetPlatform TargetPlatform;
    public MediaType MediaType;
    public Sprite Image;
    public VideoClip Video;
    public Sprite VideoThumbnail;
    public MediaLockState LockState;
    public bool IsLinearPathUnlock;
    public bool IsBackgroundCapable;
    public bool IsBaseSocialMediaProfileItem => Node == null; // If the node is null, this is a base social media profile item that isn't associated with a specific node
    public DialogueChapterManager.ChapterData ChapterData;

    public GalleryMediaItem()
    {
        // Default constructor
    }

    public GalleryMediaItem(DialogueNodeData nodeData, MediaData mediaData, DialogueChapterManager.ChapterData chapterData)
    {
        Node = nodeData;
        NodeGuid = mediaData.NodeGUID;
        Character = mediaData.IsSocialMediaPost ? nodeData.Post.Character : nodeData.Character;
        MediaType = mediaData.IsSocialMediaPost ? nodeData.Post.MediaType : nodeData.MediaType;
        Image = mediaData.IsSocialMediaPost ? nodeData.Post.Image : nodeData.Image;
        Video = mediaData.IsSocialMediaPost ? nodeData.Post.Video : nodeData.Video;
        VideoThumbnail = mediaData.IsSocialMediaPost ? nodeData.Post.VideoThumbnail : nodeData.VideoThumbnail;
        ChapterData = chapterData;
        LockState = mediaData.LockedState;
        IsLinearPathUnlock = mediaData.IsLinearPathUnlock;
        IsBackgroundCapable = mediaData.IsSocialMediaPost ? !Node.Post.NotBackgroundCapable : !Node.NotBackgroundCapable;
        TargetPlatform = mediaData.TargetPlatform;
    }

    /// <summary>
    /// Factory method to create a GalleryMediaItem from SocialBaseItemMediaData, which represents media associated with a character's social media profile rather than a specific dialogue node
    /// </summary>
    /// <param name="socialBaseItemMediaData">The SocialBaseItemMediaData object containing the media data</param>
    /// <returns>A GalleryMediaItem representing the social media profile media</returns>
    public static GalleryMediaItem GetGalleryMediaItem(SocialBaseItemMediaData socialBaseItemMediaData)
    {
        var character = DialogueChapterManager.Instance.AllDialogueCharacters.FirstOrDefault(c => c.name == socialBaseItemMediaData.CharacterName);
        if (character == null)
        {
            return null;
        }

        Sprite image = null;
        Sprite videoThumbnail = null;
        VideoClip video = null;
        switch (socialBaseItemMediaData.TargetPlatform)
        {
            case MediaTargetPlatform.SocialMediaPost:
                {
                    var resource = character.SocialMediaProfile.BaseGalleryMediaItems.FirstOrDefault(i => i.FileName == socialBaseItemMediaData.FileName);
                    if (resource != null)
                    {
                        image = resource.MediaType == MediaType.Sprite ? resource.GetMediaObjectAsSprite() : null;
                        video = resource.MediaType == MediaType.Video ? resource.GetMediaObjectAsVideoClip() : null;
                        videoThumbnail = resource.CustomThumbnail;
                    }
                }
                break;
            case MediaTargetPlatform.SpicySocialMediaPost:
                {
                    var resource = character.SpicySocialMediaProfile.BaseGalleryMediaItems.FirstOrDefault(i => i.FileName == socialBaseItemMediaData.FileName);
                    if (resource != null)
                    {
                        image = resource.MediaType == MediaType.Sprite ? resource.GetMediaObjectAsSprite() : null;
                        video = resource.MediaType == MediaType.Video ? resource.GetMediaObjectAsVideoClip() : null;
                        videoThumbnail = resource.CustomThumbnail;
                    }
                }
                break;
        }

        var item = new GalleryMediaItem()
        {
            Node = null,
            NodeGuid = socialBaseItemMediaData.NodeGUID,
            Character = character,
            TargetPlatform = socialBaseItemMediaData.TargetPlatform,
            MediaType = socialBaseItemMediaData.MediaType, // Use the MediaType from the SocialBaseItemMediaData
            Image = image, // Use the image loaded from the character's social media profile
            Video = video, // Use the video loaded from the character's social media profile
            VideoThumbnail = videoThumbnail, // Use the custom video thumbnail loaded from the character's social media profile
            LockState = socialBaseItemMediaData.LockedState,
            IsLinearPathUnlock = socialBaseItemMediaData.IsLinearPathUnlock,
            IsBackgroundCapable = false,
        };

        return item;
    }
}