using System.Collections;
using System.IO;
using MeetAndTalk;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class GalleryVideoButton : GalleryButtonBase
{
    private Sprite _videoPreviewSprite;
    private Sprite _fallbackThumbnail;
    private VideoClip _clip;
    public override string FileName => _clip.name;

    public override void Setup(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData, bool isSocialMediaPost, bool isFromGallery)
    {
        base.Setup(chapterData, nodeData, isSocialMediaPost, isFromGallery);

        (VideoClip clip, Sprite clipThumbnial) mediaData = nodeData.GetNodeVideoData(isSocialMediaPost);

        _clip = mediaData.clip;
        _fallbackThumbnail = mediaData.clipThumbnial;
        if (_fallbackThumbnail != null)
        {
            _videoPreviewSprite = _fallbackThumbnail;
        }

        _image.sprite = _videoPreviewSprite;
        _lockedImage.ApplyBlur();
    }

    public override void SetupForBaseSocialProfileItem(GalleryMediaItem item, bool isFromGallery)
    {
        base.SetupForBaseSocialProfileItem(item, isFromGallery);

        _clip = item.Video;
        _fallbackThumbnail = item.VideoThumbnail;
        if (_fallbackThumbnail != null)
        {
            _videoPreviewSprite = _fallbackThumbnail;
        }

        _image.sprite = _videoPreviewSprite;
        _lockedImage.ApplyBlur();
    }


    public override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenVideo(AssignedGUID, FileName, _isFromGallery, isSocialMediaPost: _isSocialMediaPost, includeScrubHistory: true);
    }
}
