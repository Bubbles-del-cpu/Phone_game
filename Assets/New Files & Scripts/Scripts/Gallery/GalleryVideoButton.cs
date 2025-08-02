using UnityEngine;
using UnityEngine.Video;

public class GalleryVideoButton : GalleryButtonBase
{
    [SerializeField] private Texture _previewTexture;
    private Sprite _videoPreviewSprite;

    private VideoClip _clip;
    public VideoClip Clip
    {
        get
        {
            return _clip;
        }
        set
        {
            _clip = value;
            _previewTexture = GameManager.Instance.GetVideoFrame(_clip);
        }
    }


    protected override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenVideo(Clip);
    }

    private void FixedUpdate()
    {
        if (_clip)
        {
            _previewTexture = GameManager.Instance.GetVideoFrame(_clip);
            if (_previewTexture != null && _videoPreviewSprite == null)
            {
                _videoPreviewSprite = Sprite.Create((Texture2D)_previewTexture, new Rect(0, 0, _previewTexture.width, _previewTexture.height), new Vector2(0.5f, 0.5f));
                _image.sprite = _videoPreviewSprite;
                _lockedImage.sprite = _videoPreviewSprite;
            }
        }
    }
}
