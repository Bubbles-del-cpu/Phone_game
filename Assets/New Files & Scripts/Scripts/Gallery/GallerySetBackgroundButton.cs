using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GallerySetBackgroundButton : MonoBehaviour
{
    private Button _button;
    private DialogueNodeData _assignedNode;
    private bool _isSocialMediaPost;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button)
        {
            _button.onClick.AddListener(() =>
            {
                if (_assignedNode != null)
                {
                    GameManager.Instance.SetBackgroundImage(_assignedNode, _isSocialMediaPost);
                }
            });
        }
    }

    public void Setup(DialogueNodeData nodeData, bool socialMediaPostItem)
    {
        _isSocialMediaPost = socialMediaPostItem;
        _assignedNode = nodeData;
    }
}