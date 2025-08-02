using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class GalleryButtonBase : MonoBehaviour
{
    public string AssignedGUID { get; private set; }
    public int ChapterIndex { get; private set; }
    public string HintText { get; private set; }

    [SerializeField] protected Image _image;
    [SerializeField] protected Image _lockedImage;
    [SerializeField] protected GameObject _lockedContainer;

    protected Button _button;
    public bool Unlocked;

    protected virtual void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            if (Unlocked)
            {
                GalleryButtonClicked();
            }
            else
            {
                GameManager.Instance.DisplayDialog($"{GameConstants.DialogTexts.GALLERY_ITEM_LOCKED} {HintText}", null, "Close", false);
            }
        });

        _image.preserveAspect = true;
        _lockedImage.preserveAspect = true;
    }

    public void Setup(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData, bool isSocialMediaPost)
    {
        if (nodeData == null || chapterData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (isSocialMediaPost)
        {
            gameObject.SetActive(nodeData.Post.GalleryVisibility == GalleryDisplay.Display);
        }
        else
        {
            gameObject.SetActive(nodeData.GalleryVisibility == GalleryDisplay.Display);
        }

        AssignedGUID = nodeData.NodeGuid;
        ChapterIndex = chapterData.ChapterIndex;

        HintText = $"Unlocked in {chapterData.Story.name}";
    }

    protected abstract void GalleryButtonClicked();

    protected void Update()
    {
        _lockedContainer.SetActive(!Unlocked);
    }
}