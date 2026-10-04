using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;

public class FullScreenMediaMessageViewer : MonoBehaviour, IPointerClickHandler
{
    private MediaType _postType;
    [SerializeField] private GameObject _playButton;
    private bool _isSocialMediaPost;
    private string _fileName;
    private string _nodeGuid;
    private DialogueNodeData _sourceNode;

    public void Setup(DialogueNodeData sourceNode, string fileName, MediaType postType, bool isSocialMediaPost)
    {
        _sourceNode = sourceNode;
        _nodeGuid = sourceNode.NodeGuid;
        _fileName = fileName;
        _isSocialMediaPost = isSocialMediaPost;
        _postType = postType;
        gameObject.SetActive(true);

        if (_playButton != null)
            _playButton.SetActive(_postType == MediaType.Video);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var galleryCanvas = GameManager.Instance.GalleryCanvas;
        switch (_postType)
        {
            case MediaType.Sprite:
                galleryCanvas.OpenImage(_nodeGuid, _fileName, openedFromGallery: false, _isSocialMediaPost, includeScrubHistory: false, sourceNode: _sourceNode);
                break;
            case MediaType.Video:
                galleryCanvas.OpenVideo(_nodeGuid, _fileName, openedFromGallery: false, _isSocialMediaPost, includeScrubHistory: false, sourceNode: _sourceNode);
                break;
        }
    }
}
