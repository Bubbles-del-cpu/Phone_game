using TMPro;
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

    private Sprite _fallbackThumbnail;
    public Sprite FallbackClipThumbnail
    {
        get => _fallbackThumbnail;
        set
        {
            _fallbackThumbnail = value;
            _videoPreviewSprite = _fallbackThumbnail;
        }
    }

    public override string FileName => _clip.name;


    public override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenVideo(Clip);
    }

    private void FixedUpdate()
    {
        if (_clip)
        {
            if (_fallbackThumbnail != null)
            {
                _image.sprite = _fallbackThumbnail;
                _lockedImage.sprite = _fallbackThumbnail;
            }
            else
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
}
