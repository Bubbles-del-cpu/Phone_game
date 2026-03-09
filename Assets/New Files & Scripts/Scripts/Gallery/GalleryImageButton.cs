using MeetAndTalk;
using UnityEngine;

public class GalleryImageButton : GalleryButtonBase
{
    public Sprite Sprite
    {
        get
        {
            return _image.sprite;
        }
        set
        {
            _image.sprite = value;
        }
    }

    public override string FileName => _image.sprite.name;

    public override void Setup(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData, bool isSocialMediaPost, bool isFromGallery)
    {
        base.Setup(chapterData, nodeData, isSocialMediaPost, isFromGallery);

        (Sprite image, bool backgroundCapable) mediaData = nodeData.GetNodeImageData(isSocialMediaPost);
        _image.sprite = mediaData.image;
        _lockedImage.ApplyBlur();
    }

    public override void SetupForBaseSocialProfileItem(GalleryMediaItem item, bool isFromGallery)
    {
        base.SetupForBaseSocialProfileItem(item, isFromGallery);

        _image.sprite = item.Image;
        _lockedImage.ApplyBlur();
    }

    public override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenImage(AssignedGUID, FileName, _isFromGallery, isSocialMediaPost: _isSocialMediaPost, includeScrubHistory: true);
    }
}
