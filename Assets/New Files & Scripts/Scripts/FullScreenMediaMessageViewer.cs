using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;

public class FullScreenMediaMessageViewer : MonoBehaviour, IPointerClickHandler
{
    private MediaType _type => _isSocialMediaPost ? _assignedNode.Post.MediaType : _assignedNode.MediaType;

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
        //try
        //{
            var galleryCanvas = GameManager.Instance.GalleryCanvas;
            switch (_type)
            {
                case MediaType.Sprite:
                    galleryCanvas.OpenImage(_assignedNode, openedFromMessage: true, _isSocialMediaPost);
                    break;
                case MediaType.Video:
                    galleryCanvas.OpenVideo(_assignedNode, openedFromMessage: true, _isSocialMediaPost);
                    break;
            }
        //}
        // catch (System.Exception e)
        // {
        //     Debug.LogError("Gallery Canvas is not assigned in Game Manager.");
        //     Debug.LogError(e.Message);
        //     return;
        // }
    }
}
