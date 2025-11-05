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
        // --- NEW FIX #1: Check if the node is null ---
        // This stops the crash if Setup() was never called (e.g., on save load).
        if (_assignedNode == null)
        {
            Debug.LogError("FullScreenMediaMessageViewer was clicked, but _assignedNode is null. Setup() was likely never called on this bubble.");
            return; // Stop the crash
        }
        // --- END FIX #1 ---

        var galleryCanvas = FindFirstObjectByType<GalleryCanvas>();

        // --- NEW FIX #2: Check if the canvas was found ---
        if (galleryCanvas == null)
        {
            Debug.LogError("FullScreenMediaMessageViewer could not find an active GalleryCanvas in the scene.");
            return; // Stop a potential crash here too
        }
        // --- END FIX #2 ---

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