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

    public override string FileName => _image.sprite.name;

    protected override void GalleryButtonClicked()
    {
        GameManager.Instance.GalleryCanvas.OpenImage(Sprite);
    }
}
