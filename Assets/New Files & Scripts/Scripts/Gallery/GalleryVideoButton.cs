using System.Collections;
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


    public override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenVideo(_assignedNode, _isFromGallery, isSocialMediaPost: _isSocialMediaPost);
    }
}
