using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;

public class FullScreenMediaMessageViewer : MonoBehaviour, IPointerClickHandler
{
    private MediaType _type => _isSocialMediaPost ? _assignedNode.Post.MediaType : _assignedNode.MediaType;
    [SerializeField] private bool _openedFromGallery;
    [SerializeField] private GameObject _playButton;

    private DialogueNodeData _assignedNode;
    private bool _isSocialMediaPost;

    public void Setup(DialogueNodeData nodeData, bool isSocialMediaPost)
    {
        _assignedNode = nodeData;
        _isSocialMediaPost = isSocialMediaPost;
        gameObject.SetActive(true);
        _playButton.SetActive(_type == MediaType.Video);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var galleryCanvas = GameManager.Instance.GalleryCanvas;
        switch (_type)
        {
            case MediaType.Sprite:
                galleryCanvas.OpenImage(_assignedNode, _openedFromGallery, _isSocialMediaPost, includeScrubHistory: false);
                break;
            case MediaType.Video:
                galleryCanvas.OpenVideo(_assignedNode, _openedFromGallery, _isSocialMediaPost, includeScrubHistory: false);
                break;
        }
    }
}
