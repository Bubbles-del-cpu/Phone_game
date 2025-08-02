using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class FullScreenMediaMessageViewer : MonoBehaviour, IPointerClickHandler
{
    private MediaType _type;
    private Sprite _image;
    private VideoClip _clip;

    public void Setup(MediaType type, Sprite image, VideoClip clip)
    {
        _type = type;
        _image = image;
        _clip = clip;

        switch (_type)
        {
            case MediaType.Sprite:
                if (_image == null)
                    gameObject.SetActive(false);
                break;
            case MediaType.Video:
                if (_clip == null)
                    gameObject.SetActive(false);
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var galleryCanvas = FindFirstObjectByType<GalleryCanvas>();
        switch (_type)
        {
            case MediaType.Sprite:
                galleryCanvas.OpenImage(_image, true);
                break;
            case MediaType.Video:
                galleryCanvas.OpenVideo(_clip, true);
                break;
        }
    }
}
