using System.Collections.Generic;
using MeetAndTalk;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocialMediaProfilePage : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _profileIcon;
    [SerializeField] private Image _profileImage;
    [SerializeField] private TMP_Text _profileName;
    [SerializeField] private TMP_Text _profileDescription;
    [SerializeField] private RectTransform _galleryContainer;

    #region Properties
    public Image ProfileIcon => _profileIcon;
    public Image ProfileImage => _profileImage;
    public TMP_Text ProfileName => _profileName;
    public TMP_Text ProfileDescription => _profileDescription;
    public RectTransform GalleryContainer => _galleryContainer;
    #endregion

    [Header("Gallery Item Prefabs")]
    [SerializeField] private GameObject _galleryImageButtonPrefab;
    [SerializeField] private GameObject _galleryVideoButtonPrefab;
    [SerializeField] private RectTransform _galleryImagePoolParent;
    [SerializeField] private RectTransform _galleryVideoPoolParent;

    private ObjectPool<GalleryImageButton> _galleryImagePool;
    private ObjectPool<GalleryVideoButton> _galleryVideoPool;
    private List<GalleryButtonBase> _galleryItems;

    private int _galleryItemPool = 50;
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
    public void OpenProfile(DialogueCharacterSO character)
    {
        character.SocialMediaProfile.SetupProfilePage(this);

        // Populate the gallery with the items relating to this profile
        var galleryCanvas = GameManager.Instance.GalleryCanvas;
        var (imageItems, videoItems) = galleryCanvas.GetGalleryItems(character, p => p.IsLinearPathUnlock == true && p.IsSocialMediaPost == true);

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