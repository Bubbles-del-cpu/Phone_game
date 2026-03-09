using UnityEngine;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System;

public class GalleryCanvas : UICanvas
{
    [Space(10f)]
    [Header("Gallery")]
    [SerializeField] UIPanel foldersPanel;
    [SerializeField] GalleryFolderButton folderButtonPrefab;
    [SerializeField] RectTransform folderButtonsContainer;
    //[SerializeField] UIPanel imagesPanel;
    [SerializeField] GalleryImageButton imageButtonPrefab;
    [SerializeField] GalleryVideoButton videoButtonPrefab;
    [SerializeField] RectTransform imageButtonsContainer;
    [SerializeField] RectTransform videoButtonsContainer;
    [SerializeField] FullScreenMedia fullScreenMedia;
    [SerializeField] TMP_Text _unlockedCount;
    // [SerializeField] FullScreenMedia fullImage;
    // [SerializeField] FullVideoPlayer videoPanel;

    [SerializeField] GameObject _galleryImageContainer;
    [SerializeField] GameObject _galleryVideoContainer;
    [SerializeField] private int _imagePageNumber = 0;
    [SerializeField] private int _videoPageNumber = 0;
    [SerializeField] private int _buttonsPerPage = 20;
    [SerializeField] private bool _autoplayVideosOnOpen = true;
    [SerializeField] private float _mediaPlayDelay = 0.15f;
    [SerializeField] private List<GalleryImageButton> _imageButtons;
    [SerializeField] private List<GalleryVideoButton> _videoButtons;
    [SerializeField] private MediaType _currentMediaType = MediaType.Sprite;
    [SerializeField] private TMP_Text _pageNumberText;
    [SerializeField] private int _totalImagePages => Mathf.CeilToInt((float)_galleryImageItems.Count / _buttonsPerPage);
    [SerializeField] private int _totalVideoPages => Mathf.CeilToInt((float)_galleryVideoItems.Count / _buttonsPerPage);

    [SerializeField] private List<GalleryMediaItem> _galleryImageItems;
    [SerializeField] private List<GalleryMediaItem> _galleryVideoItems;
    [NonSerialized] public GalleryUnlockData UnlockData;

    private bool _buttonsCreated = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        UnlockData = new GalleryUnlockData();
        CreateButtons();
    }

    public void ResetGalleryButtons()
    {
        foreach (var button in _imageButtons)
        {
            Destroy(button.gameObject);
        }

        foreach (var button in _videoButtons)
        {
            Destroy(button.gameObject);
        }

        _videoButtons.Clear();
        _imageButtons.Clear();
        _currentMediaType = MediaType.Sprite;
        _imagePageNumber = 0;
        _videoPageNumber = 0;

        CreateButtons();
        CreateMediaButtons(SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia, SaveAndLoadManager.Instance.CurrentSave.UnlockedBaseSocialMediaProfileItems);
        DisplayGalleryPage(_currentMediaType, 0);
    }

    /// <summary>
    /// Refreshes the gallery content based on a list of rolled back nodes, locking any media associated with those nodes
    /// </summary>
    /// <param name="rolledBackNodes">List of DialogueNodeData representing the nodes that have been rolled back</param>
    public void RefreshGalleryContentForRollback(List<DialogueNodeData> rolledBackNodes)
    {
        var mediaToLock = new List<GalleryMediaItem>();
        foreach (var node in rolledBackNodes)
        {
            foreach (var item in _galleryImageItems.Where(x => x.Node == node))
            {
                item.LockState = MediaLockState.Locked;
                item.IsLinearPathUnlock = false;
            }

            foreach (var item in _galleryVideoItems.Where(x => x.Node == node))
            {
                item.LockState = MediaLockState.Locked;
                item.IsLinearPathUnlock = false;
            }
        }

        RefreshGalleryPage();
    }

    private void CreateButtons()
    {
        _buttonsCreated = true;
        for (var index = 0; index < _buttonsPerPage; index++)
        {
            var imageButton = Instantiate(imageButtonPrefab, imageButtonsContainer);
            imageButton.gameObject.SetActive(false);
            _imageButtons.Add(imageButton);

            var videoButton = Instantiate(videoButtonPrefab, videoButtonsContainer);
            videoButton.gameObject.SetActive(false);
            _videoButtons.Add(videoButton);
        }
    }

    private void Update()
    {
        if (UnlockData.UnlockTriggered)
        {
            var helper = new GalleryHelper(GameManager.Instance.GalleryCanvas.UnlockData, GalleryHelper.USED_PASS);
            if (helper.CheckLength() && helper.CheckHash())
                helper.Unlock();
        }
    }

    /// <summary>
    /// Gets all gallery items (images and videos) associated with a specific character
    /// </summary>
    /// <param name="character">The character to filter gallery items by</param>
    /// <returns>A tuple containing lists of image and video gallery items for the specified character</returns>
    public (List<GalleryMediaItem>, List<GalleryMediaItem>) GetGalleryItems(DialogueCharacterSO character, Func<GalleryMediaItem, bool> filter = null)
    {
        var imageItems = _galleryImageItems.Where(x => x.Character == character && (filter == null || filter(x))).ToList();
        var videoItems = _galleryVideoItems.Where(x => x.Character == character && (filter == null || filter(x))).ToList();
        return (imageItems, videoItems);
    }

    /// <summary>
    /// Gets a specific gallery item by its GUID
    /// </summary>
    /// <param name="guid">The GUID of the gallery item</param>
    /// <param name="isSocialMediaPost">Indicates if the item is a social media post</param>
    /// <returns>The gallery media item if found; otherwise, null</returns>
    public GalleryMediaItem GetGalleryItem(string guid, bool isSocialMediaPost)
    {
        var item = _galleryImageItems.FirstOrDefault(x => x.NodeGuid == guid && x.IsSocialMediaPost == isSocialMediaPost);
        if (item != null)
            return item;

        item = _galleryVideoItems.FirstOrDefault(x => x.NodeGuid == guid && x.IsSocialMediaPost == isSocialMediaPost);
        return item;
    }

    public void RefreshGalleryPage()
    {
        if (!_buttonsCreated)
            return;

        DisplayGalleryPage(_currentMediaType, _currentMediaType == MediaType.Sprite ? _imagePageNumber : _videoPageNumber);
    }

    public void UnlockMedia(string nodeGUID, string fileName, bool reloadedGallery)
    {
        if (string.IsNullOrEmpty(nodeGUID) || string.IsNullOrEmpty(fileName))
            return;

        UnlockedGalleryMediaButton(nodeGUID, fileName, reloadedGallery);
    }

    public void UnlockMediaButton(DialogueNodeData nodeData, bool reloadedGallery)
    {
        if (nodeData == null)
            return;

        UnlockedGalleryMediaButton(nodeData.NodeGuid, nodeData.MediaFileName, reloadedGallery);
        if (nodeData.Post != null)
        {
            UnlockedGalleryMediaButton(nodeData.NodeGuid, nodeData.Post.MediaFileName, reloadedGallery);
        }
    }

    private void UnlockedGalleryMediaButton(string nodeGUID, string fileName, bool reloadedGallery)
    {
        //Find and unlocked the button on the gallery canvas
        var content = _galleryImageItems.FirstOrDefault(x => x.NodeGuid == nodeGUID && x.FileName == fileName);
        if (content != null)
        {
            content.LockState = MediaLockState.Unlocked;
            content.IsLinearPathUnlock = SaveAndLoadManager.Instance.ReplayingCompletedChapter == false;
        }

        var videoContent = _galleryVideoItems.FirstOrDefault(x => x.NodeGuid == nodeGUID && x.FileName == fileName);
        if (videoContent != null)
        {
            videoContent.LockState = MediaLockState.Unlocked;
            videoContent.IsLinearPathUnlock = SaveAndLoadManager.Instance.ReplayingCompletedChapter == false;
        }

        if (reloadedGallery)
        {
            var mediaBefore = _currentMediaType;
            ResetGalleryButtons();
            _currentMediaType = mediaBefore;
            DisplayGalleryPage(_currentMediaType, _currentMediaType == MediaType.Sprite ? _imagePageNumber : _videoPageNumber);
        }
    }

    public void Close(bool imageOpenFromOutsideGallery)
    {
        if (imageOpenFromOutsideGallery)
        {
            fullScreenMedia.Close();
            ShowGalleryTable(MediaType.Sprite);
            base.Close();
        }
        else
        {
            if (fullScreenMedia.IsOpen)
                fullScreenMedia.Close();
            else
            {
                ShowGalleryTable(MediaType.Sprite);
                base.Close();
            }
        }

        GameManager.Instance.MainVideoPlayer.Stop();
    }

    public void Load()
    {
        ResetGalleryButtons();
        DisplayGalleryPage(MediaType.Sprite, 0);
        //Unlock the gallery content, initially everything will start out as locked
        // CollectMediaFromChapters(DialogueChapterManager.Instance.StoryList, mediaCopy.Where(x => x.IsStoryChapter));
        // CollectMediaFromChapters(DialogueChapterManager.Instance.StandaloneChapters, mediaCopy.Where(x => !x.IsStoryChapter));
    }

    public void NextPage()
    {
        switch (_currentMediaType)
        {
            case MediaType.Sprite:
                {
                    var maxPages = Mathf.CeilToInt((float)_galleryImageItems.Count / _buttonsPerPage);
                    if (_imagePageNumber + 1 < maxPages)
                    {
                        _imagePageNumber++;
                        DisplayGalleryPage(MediaType.Sprite, _imagePageNumber);
                    }
                    _pageNumberText.text = $"Page {_imagePageNumber + 1} / {_totalImagePages}";
                }
                break;
            case MediaType.Video:
                {
                    var maxPages = Mathf.CeilToInt((float)_galleryVideoItems.Count / _buttonsPerPage);
                    if (_videoPageNumber + 1 < maxPages)
                    {
                        _videoPageNumber++;
                        DisplayGalleryPage(MediaType.Video, _videoPageNumber);
                    }
                    _pageNumberText.text = $"Page {_videoPageNumber + 1} / {_totalVideoPages}";
                }
                break;
        }
    }

    public void PreviousPage()
    {
        switch (_currentMediaType)
        {
            case MediaType.Sprite:
                {
                    if (_imagePageNumber - 1 >= 0)
                    {
                        _imagePageNumber--;
                        DisplayGalleryPage(MediaType.Sprite, _imagePageNumber);
                    }
                    _pageNumberText.text = $"Page {_imagePageNumber + 1} / {_totalImagePages}";
                }
                break;
            case MediaType.Video:
                {
                    if (_videoPageNumber - 1 >= 0)
                    {
                        _videoPageNumber--;
                        DisplayGalleryPage(MediaType.Video, _videoPageNumber);
                    }

                    _pageNumberText.text = $"Page {_videoPageNumber + 1} / {_totalVideoPages}";
                }
                break;
        }
    }

    private void DisplayGalleryPage(MediaType type, int pageNumber)
    {
        switch (type)
        {
            case MediaType.Sprite:
                {
                    var items = _galleryImageItems.GetRange(pageNumber * _buttonsPerPage, Math.Min(_buttonsPerPage, _galleryImageItems.Count - (pageNumber * _buttonsPerPage))).ToList();
                    for (var i = 0; i < _buttonsPerPage; i++)
                    {
                        var button = _imageButtons[i];
                        if (i >= items.Count)
                        {
                            button.gameObject.SetActive(false);
                            continue;
                        }

                        var data = items[i];
                        button.Setup(data, isFromGallery: true);
                        if (data.LockState == MediaLockState.Unlocked)
                            button.Unlocked = true;
                        else
                            button.Unlocked = false;

                        button.gameObject.SetActive(true);
                    }
                }
                break;
            case MediaType.Video:
                {
                    var items = _galleryVideoItems.GetRange(pageNumber * _buttonsPerPage, Math.Min(_buttonsPerPage, _galleryVideoItems.Count - (pageNumber * _buttonsPerPage))).ToList();
                    for (var i = 0; i < _buttonsPerPage; i++)
                    {
                        var button = _videoButtons[i];
                        if (i >= items.Count)
                        {
                            button.gameObject.SetActive(false);
                            continue;
                        }

                        var data = items[i];
                        button.Setup(data, isFromGallery: true);
                        if (data.LockState == MediaLockState.Unlocked)
                            button.Unlocked = true;
                        else
                            button.Unlocked = false;

                        button.gameObject.SetActive(true);
                    }
                }
                break;
        }

        UpdateUnlockedCount();
    }

    private void CreateMediaButtons(IEnumerable<MediaData> saveFileData, IEnumerable<SocialBaseItemMediaData> baseSocialMediaProfileItems)
    {
        _galleryImageItems = new List<GalleryMediaItem>();
        _galleryVideoItems = new List<GalleryMediaItem>();

        foreach (var mediaData in baseSocialMediaProfileItems)
        {
            switch (mediaData)
            {
                case SocialBaseItemMediaData sMd:
                    switch (sMd.MediaType)
                    {
                        case MediaType.Sprite:
                            _galleryImageItems.Add(GalleryMediaItem.GetGalleryMediaItem(sMd));
                            break;
                        case MediaType.Video:
                            _galleryVideoItems.Add(GalleryMediaItem.GetGalleryMediaItem(sMd));
                            break;
                    }
                    continue;
            }
        }

        foreach (var mediaData in saveFileData)
        {
            switch (mediaData.ChapterType)
            {
                case ChapterType.Story:
                    {
                        var chapter = DialogueChapterManager.Instance.StoryList[mediaData.ChapterIndex];
                        var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
                        var nd = (DialogueNodeData)node;

                        var imageData = new GalleryMediaItem(nd, mediaData, chapter);
                        switch (imageData.MediaType)
                        {
                            case MediaType.Sprite:
                                _galleryImageItems.Add(imageData);
                                break;
                            case MediaType.Video:
                                _galleryVideoItems.Add(imageData);
                                break;
                        }
                    }
                    break;
                case ChapterType.Standalone:
                    {
                        if (mediaData.ChapterIndex >= DialogueChapterManager.Instance.StandaloneChapters.Count)
                            continue;

                        var chapter = DialogueChapterManager.Instance.StandaloneChapters[mediaData.ChapterIndex];
                        var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
                        var nd = (DialogueNodeData)node;

                        var imageData = new GalleryMediaItem(nd, mediaData, chapter);
                        switch (imageData.MediaType)
                        {
                            case MediaType.Sprite:
                                _galleryImageItems.Add(imageData);
                                break;
                            case MediaType.Video:
                                _galleryVideoItems.Add(imageData);
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public override void Open()
    {
        _galleryImageContainer.GetComponent<ScrollRect>().verticalScrollbar.value = 1;
        _galleryVideoContainer.GetComponent<ScrollRect>().verticalScrollbar.value = 1;
        _pageNumberText.text = $"Page {_imagePageNumber + 1} / {_totalImagePages}";
        UpdateUnlockedCount();
        base.Open();
    }

    public void OpenDefault()
    {
        ShowGalleryTable(MediaType.Sprite);
        Open();
    }

    public void ShowGalleryTable(MediaType type)
    {
        _galleryImageContainer.SetActive(type == MediaType.Sprite);
        _galleryVideoContainer.SetActive(type == MediaType.Video);

        UpdateUnlockedCount();

        _currentMediaType = type;
        DisplayGalleryPage(type, _currentMediaType == MediaType.Sprite ? _imagePageNumber : _videoPageNumber);
        switch (_currentMediaType)
        {
            case MediaType.Sprite:
                _pageNumberText.text = $"Page {_imagePageNumber + 1} / {_totalImagePages}";
                break;
            case MediaType.Video:
                _pageNumberText.text = $"Page {_videoPageNumber + 1} / {_totalVideoPages}";
                break;
        }
    }

    private void UpdateUnlockedCount()
    {
        List<GalleryMediaItem> maxCount = _galleryImageItems;
        switch (_currentMediaType)
        {
            case MediaType.Video:
                maxCount = _galleryVideoItems;
                break;
        }

        _unlockedCount.text = $"{maxCount.Count(x => x.LockState == MediaLockState.Unlocked)} / {maxCount.Count()}";
    }

    public void OnShowGalleryTable(int type)
    {
        var previousType = _galleryImageContainer.activeInHierarchy ? MediaType.Sprite : MediaType.Video;
        var targetType = (MediaType)type;
        if (targetType != previousType)
        {
            var command = new GalleryTabSelectCommand((MediaType)type, previousType);
            NavigationManager.Instance.InvokeCommand(command);
        }
    }

    public void OpenImage(string nodeGuid, string fileName, bool openedFromGallery = false, bool isSocialMediaPost = false, bool includeScrubHistory = false)
    {
        if (string.IsNullOrEmpty(nodeGuid))
            return;

        ShowGalleryTable(MediaType.Sprite);

        var galleryMedia = _galleryImageItems.FirstOrDefault(x => x.NodeGuid == nodeGuid && x.FileName == fileName && x.IsSocialMediaPost == isSocialMediaPost);
        if (galleryMedia == null || galleryMedia.LockState == MediaLockState.Locked)
            return;

        fullScreenMedia.SetupWithScrubHistory(galleryMedia, includeScrubHistory ? GetScrubHistory(galleryMedia, openedFromGallery) : null);
        StartCoroutine(CoOpenMediaPanel(fullScreenMedia, openedFromGallery));
    }

    public void OpenVideo(string nodeGuid, string fileName, bool openedFromGallery = false, bool isSocialMediaPost = false, bool includeScrubHistory = false)
    {
        if (string.IsNullOrEmpty(nodeGuid))
            return;

        ShowGalleryTable(MediaType.Video);

        var galleryMedia = _galleryVideoItems.FirstOrDefault(x => x.NodeGuid == nodeGuid && x.FileName == fileName && x.IsSocialMediaPost == isSocialMediaPost);
        if (galleryMedia == null || galleryMedia.LockState == MediaLockState.Locked)
            return;

        fullScreenMedia.SetupWithScrubHistory(galleryMedia, includeScrubHistory ? GetScrubHistory(galleryMedia, openedFromGallery) : null);
        if (_autoplayVideosOnOpen && galleryMedia.MediaType == MediaType.Video)
        {
            fullScreenMedia.OnPlayClick(_mediaPlayDelay);
        }
        StartCoroutine(CoOpenMediaPanel(fullScreenMedia, openedFromGallery));
    }

    private List<GalleryMediaItem> GetScrubHistory(GalleryMediaItem item, bool openedFromGallery)
    {
        if (openedFromGallery)
            return _currentMediaType == MediaType.Sprite ?
                _galleryImageItems.Where(x => x.LockState == MediaLockState.Unlocked).ToList() :
                _galleryVideoItems.Where(x => x.LockState == MediaLockState.Unlocked).ToList();

        switch (item.TargetPlatform)
        {
            case MediaTargetPlatform.SocialMediaPost:
            case MediaTargetPlatform.SpicySocialMediaPost:
                return _currentMediaType == MediaType.Sprite ?
                    _galleryImageItems.Where(x => x.LockState == MediaLockState.Unlocked && x.IsSocialMediaPost && x.Character == item.Character && x.TargetPlatform == item.TargetPlatform).ToList() :
                    _galleryVideoItems.Where(x => x.LockState == MediaLockState.Unlocked && x.IsSocialMediaPost && x.Character == item.Character && x.TargetPlatform == item.TargetPlatform).ToList();
        }

        return null;
    }

    private IEnumerator CoOpenMediaPanel(UIPanel panel, bool openedFromGallery)
    {
        yield return new WaitForSeconds(openedFromGallery ? 0 : 0.05f);
        var command = new MediaOpenCommand(this, openState: true, panel, openedFromGallery);
        NavigationManager.Instance.InvokeCommand(command);
    }
}
