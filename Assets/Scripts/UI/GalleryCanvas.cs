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

    private List<string> _buttonGuids;
    private List<int> _unlockedSocialMediaImages;
    private List<GalleryButtonBase> _galleryButtons;

    private bool _imageOpenFromMessage = false;

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

        _buttonGuids = new List<string>();
        _unlockedSocialMediaImages = new List<int>();
        _galleryButtons = new List<GalleryButtonBase>();

        ShowGalleryTable(MediaType.Sprite);
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

    public bool AddMediaButton(DialogueChapterManager.ChapterData chapter, DialogueNodeData nodeData)
    {
        if (nodeData == null || _buttonGuids.Contains(nodeData.NodeGuid))
            return false;

        if (nodeData.Image != null || nodeData.Video != null || nodeData.Post != null)
        {
            SpawnMediaButton(chapter, nodeData);
            return true;
        }

        return false;
    }

    public void UnlockMedia(string guid)
    {
        UnlockedGalleryMediaButton(guid);
    }

    public void UnlockMediaButton(DialogueNodeData nodeData)
    {
        if (nodeData == null)
            return;

        UnlockedGalleryMediaButton(nodeData.MediaFileName);
        if (nodeData.Post != null)
        {
            UnlockedGalleryMediaButton(nodeData.Post.MediaFileName);
        }
    }

    private void UnlockedGalleryMediaButton(string fileName)
    {
        //Find and unlocked the button on the gallery canvas
        var content = _galleryButtons.FirstOrDefault(x => x.FileName == fileName);
        if (content)
        {
            content.Unlocked = true;
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

        GameManager.Instance.MainVideoPlayer.time = 0;
        GameManager.Instance.MainVideoPlayer.Stop();
    }

    public void Load()
    {
        foreach (var item in _galleryButtons)
            Destroy(item.gameObject);

        _galleryButtons.Clear();
        _buttonGuids.Clear();
        _unlockedSocialMediaImages.Clear();

        CreateMediaButtons(SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia);
        //Unlock the gallery content, initially everything will start out as locked
        // CollectMediaFromChapters(DialogueChapterManager.Instance.StoryList, mediaCopy.Where(x => x.IsStoryChapter));
        // CollectMediaFromChapters(DialogueChapterManager.Instance.StandaloneChapters, mediaCopy.Where(x => !x.IsStoryChapter));
    }

    private void CreateMediaButtons(IEnumerable<SaveFileData.MediaData> saveFileData)
    {
        //Unlock the gallery content, initially everything will start out as locked
        foreach (var mediaData in saveFileData)
        {
            switch (mediaData.ChapterType)
            {
                case ChapterType.Story:
                    {
                        var chapter = DialogueChapterManager.Instance.StoryList[mediaData.ChapterIndex];
                        var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
                        AddMediaButton(chapter, (DialogueNodeData)node);
                        switch (mediaData.LockedState)
                        {
                            case MediaLockState.Unlocked:
                                UnlockMediaButton((DialogueNodeData)node);
                                break;
                        }
                    }
                    break;
                case ChapterType.Standalone:
                    {
                        var chapter = DialogueChapterManager.Instance.StandaloneChapters[mediaData.ChapterIndex];
                        var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
                        AddMediaButton(chapter, (DialogueNodeData)node);
                        switch (mediaData.LockedState)
                        {
                            case MediaLockState.Unlocked:
                                UnlockMediaButton((DialogueNodeData)node);
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
        base.Open();
    }

    public void OpenDefault()
    {
        ShowGalleryTable(MediaType.Sprite);
        Open();
    }

    private void SpawnMediaButton(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData)
    {
        if (nodeData == null)
            return;


        switch (nodeData.MediaType)
        {
            case MediaType.Sprite:
                if (nodeData.Image != null)
                    SpawnButton(chapterData, nodeData, nodeData.Image, isSocialMediaPost: false);
                break;
            case MediaType.Video:
                if (nodeData.Video != null)
                    SpawnButton(chapterData, nodeData, nodeData.Video, isSocialMediaPost: false);
                break;
        }

        if (nodeData.Post != null)
        {
            if (!_unlockedSocialMediaImages.Contains(nodeData.Post.GetHashCode()))
            {
                _unlockedSocialMediaImages.Add(nodeData.Post.GetHashCode());
                switch (nodeData.Post.MediaType)
                {
                    case MediaType.Sprite:
                        SpawnButton(chapterData, nodeData, nodeData.Post.Image, isSocialMediaPost: true);
                        break;
                    case MediaType.Video:
                        SpawnButton(chapterData, nodeData, nodeData.Post.Video, isSocialMediaPost: true);
                        break;
                }
            }
        }

        _buttonGuids.Add(nodeData.NodeGuid);
    }

    private void SpawnButton(DialogueChapterManager.ChapterData chapterData, DialogueNodeData node, UnityEngine.Object data, bool isSocialMediaPost)
    {
        switch (data)
        {
            case VideoClip videoClip:
                {
                    if (!_galleryButtons.Select(x => x.FileName).Contains(videoClip.name))
                    {
                        var videoButton = Instantiate(videoButtonPrefab, videoButtonsContainer);
                        videoButton.Setup(chapterData, node, isSocialMediaPost);

                        _galleryButtons.Add(videoButton);
                    }
                }
                break;
            case Sprite image:
                {
                    if (!_galleryButtons.Select(x => x.FileName).Contains(image.name))
                    {
                        var imageButton = Instantiate(imageButtonPrefab, imageButtonsContainer);
                        imageButton.Setup(chapterData, node, isSocialMediaPost);

                        _galleryButtons.Add(imageButton);
                    }
                }
                break;
        }
    }

    public void ShowGalleryTable(MediaType type)
    {
        _galleryImageContainer.SetActive(type == MediaType.Sprite);
        _galleryVideoContainer.SetActive(type == MediaType.Video);

        IEnumerable<GalleryButtonBase> maxCount = _galleryButtons;
        switch (type)
        {
            case MediaType.Sprite:
                maxCount = _galleryButtons.Where(x => x.GetType() == typeof(GalleryImageButton));
                break;
            case MediaType.Video:
                maxCount = _galleryButtons.Where(x => x.GetType() == typeof(GalleryVideoButton));
                break;
        }

        _unlockedCount.text = $"{maxCount.Count(x => x.Unlocked)} / {maxCount.Count()}";
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
