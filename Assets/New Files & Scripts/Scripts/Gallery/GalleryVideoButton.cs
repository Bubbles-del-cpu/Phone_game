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

    public override void Setup(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData, bool isSocialMediaPost)
    {
        base.Setup(chapterData, nodeData, isSocialMediaPost);

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
        GameManager.Instance.GalleryCanvas.OpenVideo(_assignedNode, openedFromMessage: false, isSocialMediaPost: _isSocialMediaPost);
    }

    private void FixedUpdate()
    {
        // if (_clip)
        // {
        //     if (_fallbackThumbnail == null)
        //     {
        //         _image.sprite = _fallbackThumbnail;
        //         _lockedImage.ApplyBlur();
        //     }
        //     else
        //     {
        //         var frame = GameManager.Instance.GetVideoFrame(_clip);
        //         _videoPreviewSprite = frame.Item2;
        //         if ( _videoPreviewSprite == null)
        //         {
        //             _image.sprite = _videoPreviewSprite;
        //             if (_videoPreviewSprite != null)
        //                 _lockedImage.ApplyBlur();
        //         }
        //     }
        // }
    }
}
