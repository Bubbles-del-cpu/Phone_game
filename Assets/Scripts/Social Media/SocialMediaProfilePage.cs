using System;
using System.Collections.Generic;
using MeetAndTalk;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocialMediaProfilePage : MonoBehaviour
{
    [SerializeField] protected CanvasGroup _canvasGroup;
    [SerializeField] protected RectTransform _galleryContainer;
    [SerializeField] protected Image _profileIcon;
    [SerializeField] protected Image _profileImage;
    [SerializeField] protected Image _profileBackground;
    [SerializeField] protected TMP_Text _profileName;
    [SerializeField] protected TMP_Text _profileDescription;
    [SerializeField] protected TMP_Text _followerCountText;
    [SerializeField] protected TMP_Text _followingCountText;

    #region Properties
    public RectTransform GalleryContainer => _galleryContainer;
    public Image ProfileIcon => _profileIcon;
    public Image ProfileImage => _profileImage;
    public Image ProfileBackground => _profileBackground;
    public TMP_Text ProfileName => _profileName;
    public TMP_Text ProfileDescription => _profileDescription;
    public TMP_Text FollowerCountText => _followerCountText;
    public TMP_Text FollowingCountText => _followingCountText;
    #endregion

    [Header("Gallery Item Prefabs")]
    [SerializeField] protected GameObject _galleryImageButtonPrefab;
    [SerializeField] protected GameObject _galleryVideoButtonPrefab;
    [SerializeField] protected RectTransform _galleryImagePoolParent;
    [SerializeField] protected RectTransform _galleryVideoPoolParent;

    protected ObjectPool<GalleryImageButton> _galleryImagePool;
    protected ObjectPool<GalleryVideoButton> _galleryVideoPool;
    protected List<GalleryButtonBase> _galleryItems;
    protected virtual Func<GalleryMediaItem, bool> _galleryItemFilter => p => p.IsLinearPathUnlock == true && p.TargetPlatform == MediaTargetPlatform.SocialMediaPost;

    protected int _galleryItemPool = 50;
    private void Awake()
    {
        _galleryItems = new List<GalleryButtonBase>();
        _galleryImagePool = new ObjectPool<GalleryImageButton>(_galleryItemPool, 10, _galleryImageButtonPrefab, _galleryImagePoolParent);
        _galleryVideoPool = new ObjectPool<GalleryVideoButton>(_galleryItemPool, 10, _galleryVideoButtonPrefab, _galleryVideoPoolParent);
    }

    /// <summary>
    /// Initializes and opens the profile page with the given character's social media profile data
    /// </summary>
    /// <param name="character">The character whose social media profile data will be used to initialize the page</param>
    public virtual void OpenProfile(DialogueCharacterSO character)
    {
        ClearProfilePage();
        character.SocialMediaProfile.SetupProfilePage(this);

        // Populate the gallery with the items relating to this profile
        var galleryCanvas = GameManager.Instance.GalleryCanvas;
        var (imageItems, videoItems) = galleryCanvas.GetGalleryItems(character, _galleryItemFilter);

        foreach (var item in imageItems)
        {
            var imageButton = _galleryImagePool.GetObject();
            imageButton.Setup(item, isFromGallery: false);
            imageButton.Unlocked = true; // Social media posts are always unlocked if they appear on a profile
            imageButton.transform.SetParent(_galleryContainer, false);
            _galleryItems.Add(imageButton);
        }

        foreach (var item in videoItems)
        {
            var videoButton = _galleryVideoPool.GetObject();
            videoButton.Setup(item, isFromGallery: false);
            videoButton.Unlocked = true; // Social media posts are always unlocked if they appear on a profile
            videoButton.transform.SetParent(_galleryContainer, false);
            _galleryItems.Add(videoButton);
        }

        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Closes the profile page and returns all gallery items to their respective pools
    /// </summary>
    public void CloseProfile()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        ClearProfilePage();
    }

    /// <summary>
    /// Clears the profile page by returning all gallery items to their respective pools
    /// </summary>
    protected void ClearProfilePage()
    {
        // Return all gallery items to their respective pools
        foreach (var item in _galleryItems)
        {
            if (item is GalleryImageButton imageButton)
            {
                _galleryImagePool.ReturnObject(imageButton);
            }
            else if (item is GalleryVideoButton videoButton)
            {
                _galleryVideoPool.ReturnObject(videoButton);
            }
        }

        _galleryItems.Clear();
    }
}