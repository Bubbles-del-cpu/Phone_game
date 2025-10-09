using System.Collections;
using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class FullScreenMediaMessageViewer : MonoBehaviour, IPointerClickHandler
{
    private MediaType _type => _assignedNode.PostMediaType;
    private Sprite _image => _assignedNode.Image;
    private VideoClip _clip => _assignedNode.Video;

    private DialogueNodeData _assignedNode;
    private bool _isSocialMediaPost;

    public void Setup(DialogueNodeData nodeData, bool isSocialMediaPost)
    {
        _assignedNode = nodeData;
        _isSocialMediaPost = isSocialMediaPost;

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
                galleryCanvas.OpenImage(_assignedNode, openedFromMessage: true, _isSocialMediaPost);
                break;
            case MediaType.Video:
                galleryCanvas.OpenVideo(_assignedNode, openedFromMessage: true, _isSocialMediaPost);
                break;
        }
    }
}
