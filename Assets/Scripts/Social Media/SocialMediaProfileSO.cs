using System.Collections.Generic;
using MeetAndTalk;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Social Media Profile", menuName = "MeetAndTalk/Social Media/Profile")]
public class SocialMediaProfileSO : ScriptableObject
{
    [System.Serializable]
    public class BaseGalleryMediaItem
    {
        public string FileName => MediaObject != null ? MediaObject.name : string.Empty;
        public MediaType MediaType => MediaObject switch
        {
            Sprite => MediaType.Sprite,
            VideoClip => MediaType.Video,
            _ => MediaType.Sprite, // Default to sprite if the media type can't be determined
        };
        public Object MediaObject;

        /// <summary>
        /// Used to convert the media object into a sprite. Because the BaseGalleryMediaItem accepts all objects, when a "sprite" it assigned to the field in the editor it can be in either format (sprite or texture2D)
        /// As such this function is used to convert the assigned media object into a sprite regardless of the original format.
        /// If the media object is a video clip or some other type, this function will return null.
        /// </summary>
        /// <returns> The sprite representation of the media object, or null if it cannot be converted. </returns>
        public Sprite GetMediaObjectAsSprite()
        {
            if (MediaObject is Sprite sprite)
            {
                return sprite;
            }
            else if (MediaObject is Texture2D texture)
            {
                // Create a new sprite from the texture
                var newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                newSprite.name = MediaObject.name; // Set the name of the sprite to match the original media object name for identification purposes
                return newSprite;
            }
            else
            {
                Debug.LogWarning($"MediaObject {MediaObject.name} is not a Sprite or Texture2D and cannot be converted to a Sprite.");
                return null;
            }
        }

        /// <summary>
        /// Used to convert the media object into a video clip. If the media object is not a video clip, this function will return null.
        /// </summary>
        /// <returns> The video clip representation of the media object, or null if it cannot be converted. </returns>
        public VideoClip GetMediaObjectAsVideoClip()
        {
            return MediaObject as VideoClip;
        }

        public Sprite CustomThumbnail; // Optional custom thumbnail for videos, if not set the video will be used as its own thumbnail
    }

    [SerializeField] private Sprite _profileImage;
    [SerializeField] private Sprite _miniProfileIcon;
    [SerializeField] private LocalizedString _profileName;
    [SerializeField] private LocalizedString _profileDescription;
    [SerializeField] private int _followerCount;
    [SerializeField] private int _followingCount;
    [SerializeField] private Color _profileColor = Color.magenta;

    public List<BaseGalleryMediaItem> BaseGalleryMediaItems = new List<BaseGalleryMediaItem>();

    /// <summary>
    /// Setup the profile page with the data from this social media profile
    /// </summary>
    /// <param name="profilePage">The profile page to setup</param>
    public void SetupProfilePage(SocialMediaProfilePage profilePage)
    {
        profilePage.ProfileIcon.sprite = _miniProfileIcon;
        profilePage.ProfileImage.sprite = _profileImage;
        profilePage.ProfileName.text = _profileName.GetLocalizedString();
        profilePage.ProfileDescription.text = _profileDescription.GetLocalizedString();
        profilePage.FollowerCountText.text = _followerCount.ToString();
        profilePage.FollowingCountText.text = _followingCount.ToString();
        profilePage.ProfileBackground.color = _profileColor;
    }

    /// <summary>
    /// Setup the profile button with the data from this social media profile
    /// </summary>
    /// <param name="profileButton">The profile button to setup</param>
    public void SetupProfileButton(SocialMediaProfileButton profileButton)
    {
        profileButton.Icon.sprite = _miniProfileIcon != null ? _miniProfileIcon : _profileImage;
    }

    /// <summary>
    /// Converts the base gallery items assigned to this profile item media data objects that will be used for save data and unlocking media.
    /// </summary>
    /// <param name="character">The character associated with the media data</param>
    /// <param name="targetPlatform">The target platform for the media data</param>
    /// <returns>A list of media data objects</returns>
    public List<SocialBaseItemMediaData> GetBaseGalleryMediaData(DialogueCharacterSO character, MediaTargetPlatform targetPlatform)
    {
        var mediaDataList = new List<SocialBaseItemMediaData>();
        foreach (var item in BaseGalleryMediaItems)
        {
            var mediaData = ConvertBaseGalleryItemToMediaData(character, item, targetPlatform);
            mediaDataList.Add(mediaData);
        }

        return mediaDataList;
    }

    public SocialBaseItemMediaData ConvertBaseGalleryItemToMediaData(DialogueCharacterSO character, BaseGalleryMediaItem item, MediaTargetPlatform targetPlatform)
    {
        var mediaData = new SocialBaseItemMediaData()
        {
            ChapterIndex = -1,
            ChapterType = ChapterType.Story,
            LockedState = MediaLockState.Locked,
            NotBackgroundCapable = true,
            IsLinearPathUnlock = false,
            IsSocialMediaPost = true,
            Node = null,
            NodeGUID = "SocialMediaProfileBaseItem_" + item.MediaObject.name, // Create a unique GUID for this media item based on the profile and the item name,
            CharacterName = character.name,
            FileName = item.FileName,
            MediaType = item.MediaType,
            TargetPlatform = targetPlatform
        };

        return mediaData;
    }
}