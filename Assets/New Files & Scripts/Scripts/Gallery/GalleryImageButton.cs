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
            _lockedImage.sprite = value;
        }
    }

    protected override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenImage(Sprite);
    }
}
