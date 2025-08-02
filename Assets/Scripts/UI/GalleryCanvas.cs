using UnityEngine;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;
using UnityEngine.Video;
using System.Collections;
using System.Linq;

public class GalleryCanvas : UICanvas
{
    [Space(10f)]
    [Header("Gallery")]
    [SerializeField] UIPanel foldersPanel;
    [SerializeField] GalleryFolderButton folderButtonPrefab;
    [SerializeField] RectTransform folderButtonsContainer;
    [SerializeField] UIPanel imagesPanel;
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

    protected override void Awake()
    {
        base.Awake();

        _buttonGuids = new List<string>();
        _unlockedSocialMediaImages = new List<int>();
        _galleryButtons = new List<GalleryButtonBase>();

        ShowGalleryTable(MediaType.Sprite);
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
        if (nodeData == null || !_buttonGuids.Contains(nodeData.NodeGuid))
            return;

        UnlockedGalleryMediaButton(nodeData.NodeGuid);
        if (nodeData.Post != null)
            UnlockedGalleryMediaButton(nodeData.NodeGuid);
    }

    private void UnlockedGalleryMediaButton(string guid)
    {
        //Find and unlocked the button on the gallery canvas
        var content = _galleryButtons.Where(x => x.AssignedGUID == guid);
        foreach (var button in content)
        {
            if (button)
            {
                button.Unlocked = true;
            }
        }
    }

    public override void Close()
    {
        if (_imageOpenFromMessage)
        {
            _imageOpenFromMessage = false;
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

        var mediaCopy = new List<SaveFileData.MediaData>(SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia);
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
            if (mediaData.IsStoryChapter)
            {
                var chapter = DialogueChapterManager.Instance.StoryList[mediaData.ChapterIndex];
                var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, mediaData.NodeGUID);
                AddMediaButton(chapter, (DialogueNodeData)node);
            }
            else
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
        }

        // //Take the new media buttons and unlock them based on our save file data
        // foreach (var item in saveFileData)
        // {
        //     try
        //     {
        //         var chapter = chapterData[item.ChapterIndex];
        //         var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, item.NodeGUID);
        //         if (node != null && item.LockedState == MediaLockState.Unlocked)
        //         {
        //             var dialogueNodeData = (DialogueNodeData)node;
        //             UnlockMediaButton(dialogueNodeData);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError($"Valied to collect media from chapter. {ex.Message}");
        //     }
        // }
    }

    public override void Open()
    {
        imagesPanel.Open();

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

        switch (nodeData.PostMediaType)
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
                    var videoButton = Instantiate(videoButtonPrefab, videoButtonsContainer);
                    videoButton.Setup(chapterData, node, isSocialMediaPost);
                    videoButton.Clip = videoClip;

                    _galleryButtons.Add(videoButton);
                }
                break;
            case Sprite image:
                {
                    var imageButton = Instantiate(imageButtonPrefab, imageButtonsContainer);
                    imageButton.Setup(chapterData, node, isSocialMediaPost);
                    imageButton.Sprite = image;
                    _galleryButtons.Add(imageButton);
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
        ShowGalleryTable((MediaType)type);
    }

    public void OpenImage(Sprite sprite, bool openedFromMessage = false)
    {
        ShowGalleryTable(MediaType.Sprite);

        fullScreenMedia.Setup(MediaType.Sprite, sprite, null);
        StartCoroutine(CoOpenVideoPanel(fullScreenMedia, openedFromMessage ? 0 : 0.05f));

        _imageOpenFromMessage = openedFromMessage;
    }

    public void OpenVideo(VideoClip video, bool openedFromMessage = false)
    {
        ShowGalleryTable(MediaType.Video);

        fullScreenMedia.Setup(MediaType.Video, null, video);
        StartCoroutine(CoOpenVideoPanel(fullScreenMedia, openedFromMessage ? 0 : 0.05f));

        _imageOpenFromMessage = openedFromMessage;
    }

    private IEnumerator CoOpenVideoPanel(UIPanel panel, float delay)
    {
        panel.Open();
        yield return new WaitForSeconds(delay);
        Open();
    }
}
