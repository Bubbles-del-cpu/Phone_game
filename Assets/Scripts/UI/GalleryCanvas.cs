using UnityEngine;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;
using UnityEngine.Video;
using System.Collections;
using System.Linq;
using Unity.Collections;
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
    [SerializeField] private List<GalleryImageButton> _imageButtons;
    [SerializeField] private List<GalleryVideoButton> _videoButtons;
    [SerializeField] private MediaType _currentMediaType = MediaType.Sprite;
    [SerializeField] private TMP_Text _pageNumberText;
    [SerializeField] private int _totalImagePages => Mathf.CeilToInt((float)_galleryImageItems.Count / _buttonsPerPage);
    [SerializeField] private int _totalVideoPages => Mathf.CeilToInt((float)_galleryVideoItems.Count / _buttonsPerPage);

    [SerializeField] private List<GalleryMediaItem> _galleryImageItems;
    [SerializeField] private List<GalleryMediaItem> _galleryVideoItems;

    private bool _imageOpenFromMessage = false;

    [Serializable]
    private class GalleryMediaItem
    {
        public DialogueNodeData Node;
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
        public DialogueChapterManager.ChapterData ChapterData;
    }

    [NonSerialized] public GalleryUnlockData UnlockData;
    public class GalleryUnlockData
    {
        public string Salt, Hash;
        public int Length;
        public bool UnlockTriggered;
        public string UsedPass;

        public GalleryUnlockData()
        {
            Salt = GameManager.Instance.GalleryConfig.Salt;
            Hash = GameManager.Instance.GalleryConfig.Hash;
            Length = GameManager.Instance.GalleryConfig.Length;
        }
    }

    protected override void Awake()
    {
        base.Awake();

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
        CreateMediaButtons(SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia);
    }

    private void CreateButtons()
    {
        for(var index = 0; index < _buttonsPerPage; index++)
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

    public void RefreshGalleryPage()
    {
        DisplayGalleryPage(_currentMediaType, _currentMediaType == MediaType.Sprite ? _imagePageNumber : _videoPageNumber);
    }

    public void UnlockMedia(string guid, bool reloadedGallery)
    {
        if (string.IsNullOrEmpty(guid))
            return;

        UnlockedGalleryMediaButton(guid, reloadedGallery);
    }


    public void UnlockMediaButton(DialogueNodeData nodeData, bool reloadedGallery)
    {
        if (nodeData == null)
            return;

        UnlockedGalleryMediaButton(nodeData.MediaFileName, reloadedGallery);
        if (nodeData.Post != null)
        {
            UnlockedGalleryMediaButton(nodeData.Post.MediaFileName, reloadedGallery);
        }
    }

    private void UnlockedGalleryMediaButton(string fileName, bool reloadedGallery)
    {
        //Find and unlocked the button on the gallery canvas
        var content = _galleryImageItems.FirstOrDefault(x => x.FileName == fileName);
        if (content != null)
        {
            content.LockState = MediaLockState.Unlocked;
        }

        var videoContent = _galleryVideoItems.FirstOrDefault(x => x.FileName == fileName);
        if (videoContent != null)
        {
            videoContent.LockState = MediaLockState.Unlocked;
        }

        if (reloadedGallery)
        {
            var mediaBefore = _currentMediaType;
            ResetGalleryButtons();
            _currentMediaType = mediaBefore;
            DisplayGalleryPage(_currentMediaType, _currentMediaType == MediaType.Sprite ? _imagePageNumber : _videoPageNumber);
        }
    }

    public void Close(bool imageOpenFromMessage)
    {
        if (imageOpenFromMessage)
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
                        button.Setup(data.ChapterData, data.Node, data.IsSocialMediaPost);
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
                        button.Setup(data.ChapterData, data.Node, data.IsSocialMediaPost);
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

    private void CreateMediaButtons(IEnumerable<SaveFileData.MediaData> saveFileData)
    {
        _galleryImageItems = new List<GalleryMediaItem>();
        _galleryVideoItems = new List<GalleryMediaItem>();
        foreach (var mediaData in saveFileData)
        {
            switch (mediaData.ChapterType)
            {
                case ChapterType.Story:
                    {
                        var chapter = DialogueChapterManager.Instance.StoryList[mediaData.ChapterIndex];
                        var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
                        var nd = (DialogueNodeData)node;

                        var type = mediaData.IsSocialMediaPost ? nd.Post.MediaType : nd.MediaType;
                        var imageData = new GalleryMediaItem()
                        {
                            Node = nd,
                            IsSocialMediaPost = mediaData.IsSocialMediaPost,
                            MediaType = mediaData.IsSocialMediaPost ? nd.Post.MediaType : nd.MediaType,
                            Image = mediaData.IsSocialMediaPost ? nd.Post.Image : nd.Image,
                            Video = mediaData.IsSocialMediaPost ? nd.Post.Video : nd.Video,
                            VideoThumbnail = mediaData.IsSocialMediaPost ? nd.Post.VideoThumbnail : nd.VideoThumbnail,
                            ChapterData = chapter,
                            LockState = mediaData.LockedState
                        };

                        switch (type)
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

                        var chapter = DialogueChapterManager.Instance.StandaloneChapters[mediaData.ChapterIndex];
                        var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
                        var nd = (DialogueNodeData)node;

                        var imageData = new GalleryMediaItem()
                        {
                            Node = nd,
                            IsSocialMediaPost = mediaData.IsSocialMediaPost,
                            MediaType = mediaData.IsSocialMediaPost ? nd.Post.MediaType : nd.MediaType,
                            Image = mediaData.IsSocialMediaPost ? nd.Post.Image : nd.Image,
                            Video = mediaData.IsSocialMediaPost ? nd.Post.Video : nd.Video,
                            VideoThumbnail = mediaData.IsSocialMediaPost ? nd.Post.VideoThumbnail : nd.VideoThumbnail,
                            ChapterData = chapter,
                            LockState = mediaData.LockedState
                        };

                        var type = mediaData.IsSocialMediaPost ? nd.Post.MediaType : nd.MediaType;
                        switch (type)
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

    // private IEnumerator CoCreateMediaButtons(IEnumerable<SaveFileData.MediaData> saveFileData)
    // {
    //     //Unlock the gallery content, initially everything will start out as locked
    //     var count = 0;
    //     foreach (var mediaData in saveFileData)
    //     {
    //         switch (mediaData.ChapterType)
    //         {
    //             case ChapterType.Story:
    //                 {
    //                     var chapter = DialogueChapterManager.Instance.StoryList[mediaData.ChapterIndex];
    //                     var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
    //                     AddMediaButton(chapter, (DialogueNodeData)node);
    //                     switch (mediaData.LockedState)
    //                     {
    //                         case MediaLockState.Unlocked:
    //                             UnlockMediaButton((DialogueNodeData)node);
    //                             break;
    //                     }
    //                 }
    //                 break;
    //             case ChapterType.Standalone:
    //                 {
    //                     var chapter = DialogueChapterManager.Instance.StandaloneChapters[mediaData.ChapterIndex];
    //                     var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
    //                     AddMediaButton(chapter, (DialogueNodeData)node);
    //                     switch (mediaData.LockedState)
    //                     {
    //                         case MediaLockState.Unlocked:
    //                             UnlockMediaButton((DialogueNodeData)node);
    //                             break;
    //                     }
    //                 }
    //                 break;
    //         }
    //         count++;
    //         if (count % 5 == 0)
    //             yield return new WaitForEndOfFrame();
    //     }
    // }

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

    // private void SpawnMediaButton(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData)
    // {
    //     if (nodeData == null)
    //         return;


    //     switch (nodeData.MediaType)
    //     {
    //         case MediaType.Sprite:
    //             if (nodeData.Image != null)
    //                 SpawnButton(chapterData, nodeData, nodeData.Image, isSocialMediaPost: false);
    //             break;
    //         case MediaType.Video:
    //             if (nodeData.Video != null)
    //                 SpawnButton(chapterData, nodeData, nodeData.Video, isSocialMediaPost: false);
    //             break;
    //     }

    //     if (nodeData.Post != null)
    //     {
    //         if (!_unlockedSocialMediaImages.Contains(nodeData.Post.GetHashCode()))
    //         {
    //             _unlockedSocialMediaImages.Add(nodeData.Post.GetHashCode());
    //             switch (nodeData.Post.MediaType)
    //             {
    //                 case MediaType.Sprite:
    //                     SpawnButton(chapterData, nodeData, nodeData.Post.Image, isSocialMediaPost: true);
    //                     break;
    //                 case MediaType.Video:
    //                     SpawnButton(chapterData, nodeData, nodeData.Post.Video, isSocialMediaPost: true);
    //                     break;
    //             }
    //         }
    //     }

    //     _buttonGuids.Add(nodeData.NodeGuid);
    // }

    // private void SpawnButton(DialogueChapterManager.ChapterData chapterData, DialogueNodeData node, UnityEngine.Object data, bool isSocialMediaPost)
    // {
    //     switch (data)
    //     {
    //         case VideoClip videoClip:
    //             {
    //                 if (!_galleryButtons.Select(x => x.FileName).Contains(videoClip.name))
    //                 {
    //                     var videoButton = Instantiate(videoButtonPrefab, videoButtonsContainer);
    //                     videoButton.Setup(chapterData, node, isSocialMediaPost);

    //                     _galleryButtons.Add(videoButton);
    //                 }
    //             }
    //             break;
    //         case Sprite image:
    //             {
    //                 if (!_galleryButtons.Select(x => x.FileName).Contains(image.name))
    //                 {
    //                     var imageButton = Instantiate(imageButtonPrefab, imageButtonsContainer);
    //                     imageButton.Setup(chapterData, node, isSocialMediaPost);

    //                     _galleryButtons.Add(imageButton);
    //                 }
    //             }
    //             break;
    //     }
    // }

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

    public void OpenImage(DialogueNodeData nodeData, bool openedFromMessage = false, bool isSocialMediaPost = false)
    {
        if (nodeData == null)
            return;

        ShowGalleryTable(MediaType.Sprite);

        fullScreenMedia.Setup(nodeData, isSocialMediaPost);

        StartCoroutine(CoOpenMediaPanel(fullScreenMedia, openedFromMessage));

        _imageOpenFromMessage = openedFromMessage;
    }

    public void OpenVideo(DialogueNodeData nodeData, bool openedFromMessage = false, bool isSocialMediaPost = false)
    {
        if (nodeData == null)
            return;

        ShowGalleryTable(MediaType.Video);

        fullScreenMedia.Setup(nodeData, isSocialMediaPost);

        StartCoroutine(CoOpenMediaPanel(fullScreenMedia, openedFromMessage));

        _imageOpenFromMessage = openedFromMessage;
    }

    private IEnumerator CoOpenMediaPanel(UIPanel panel, bool openedFromMessage)
    {
        yield return new WaitForSeconds(openedFromMessage ? 0 : 0.05f);
        var command = new MediaOpenCommand(this, openState: true, panel, openedFromMessage);
        NavigationManager.Instance.InvokeCommand(command);
    }
}
