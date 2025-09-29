using MeetAndTalk;
using UnityEngine;
using UnityEngine.Video;

public class GalleryImageButton : GalleryButtonBase
{
    public override string FileName => _image.sprite.name;

    public override void Setup(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData, bool isSocialMediaPost)
    {
        base.Setup(chapterData, nodeData, isSocialMediaPost);

        (Sprite image, bool backgroundCapable) mediaData = nodeData.GetNodeImageData(isSocialMediaPost);
        _image.sprite = mediaData.image;
        _lockedImage.sprite = mediaData.image;
    }

    public override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenImage(_assignedNode, openedFromMessage: false, isSocialMediaPost: _isSocialMediaPost);
    }
}
