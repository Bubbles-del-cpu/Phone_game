using System.Collections;
using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class FullScreenMediaMessageViewer : MonoBehaviour, IPointerClickHandler
{
    private MediaType _type => _isSocialMediaPost ? _assignedNode.Post.MediaType : _assignedNode.MediaType;
    private Sprite _image => _isSocialMediaPost ? _assignedNode.Post.Image : _assignedNode.Image;
    private VideoClip _clip => _isSocialMediaPost ? _assignedNode.Post.Video : _assignedNode.Video;

    private DialogueNodeData _assignedNode;
    private bool _isSocialMediaPost;

    public void Setup(DialogueNodeData nodeData, bool isSocialMediaPost)
    {
        _assignedNode = nodeData;
        _isSocialMediaPost = isSocialMediaPost;
        gameObject.SetActive(true);
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
