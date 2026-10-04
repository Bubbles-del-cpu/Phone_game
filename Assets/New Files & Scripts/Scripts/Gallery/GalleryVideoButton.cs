using System.Collections;
using System.IO;
using MeetAndTalk;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class GalleryVideoButton : GalleryButtonBase
{
    private VideoClip _clip;
    public override string FileName => _clip.name;

    public override void Setup(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData, bool isSocialMediaPost, bool isFromGallery)
    {
        base.Setup(chapterData, nodeData, isSocialMediaPost, isFromGallery);

        (VideoClip clip, Sprite clipThumbnial) mediaData = nodeData.GetNodeVideoData(isSocialMediaPost);

        _clip = mediaData.clip;
        SetThumbnail(mediaData.clipThumbnial);
    }

    public override void SetupForBaseSocialProfileItem(GalleryMediaItem item, bool isFromGallery)
    {
        base.SetupForBaseSocialProfileItem(item, isFromGallery);

        _clip = item.Video;
        SetThumbnail(item.VideoThumbnail);
    }


    private void SetThumbnail(Sprite thumbnail)
    {
        // Buttons are reused between pages; clear both previews for missing thumbnails.
        _image.sprite = thumbnail;
        if (_lockedImage.SourceImage != null)
            _lockedImage.SourceImage.sprite = thumbnail;

        if (thumbnail == null)
            _lockedImage.GetComponent<UnityEngine.UI.RawImage>().texture = Texture2D.blackTexture;
        else
            _lockedImage.ApplyBlur();
    }

    public override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenVideo(AssignedGUID, FileName, _isFromGallery, isSocialMediaPost: _isSocialMediaPost, includeScrubHistory: true);
    }
}
