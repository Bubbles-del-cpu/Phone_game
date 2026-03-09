using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class GalleryButtonBase : MonoBehaviour
{
    public string AssignedGUID { get; private set; }
    public string CharacterName { get; private set; }
    public int ChapterIndex { get; private set; }
    public string HintText { get; private set; }

    [SerializeField] protected Image _image;
    [SerializeField] protected UITextureBlur _lockedImage;
    [SerializeField] protected GameObject _lockedContainer;

    protected Button _button;
    protected bool _isSocialMediaPost;
    protected bool _isFromGallery;
    public bool Unlocked;

    public virtual string FileName => "";

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
                GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.GALLERY_ITEM_LOCKED,
                    eventToTrigger: null,
                    GameConstants.UIElementKeys.CLOSE,
                    new object[] { HintText },
                    twoButtonSetup: false
                    );
            }
        });

        _image.preserveAspect = true;
    }

    /// <summary>
    /// Sets up the gallery button using a GalleryMediaItem
    /// </summary>
    /// <param name="item"></param>
    public void Setup(GalleryMediaItem item, bool isFromGallery)
    {
        // Call the main Setup method with the appropriate parameters
        if (item.IsBaseSocialMediaProfileItem)
        {
            SetupForBaseSocialProfileItem(item, isFromGallery);
            return;
        }

        Setup(item.ChapterData, item.Node, item.IsSocialMediaPost, isFromGallery);
    }

    public virtual void SetupForBaseSocialProfileItem(GalleryMediaItem item, bool isFromGallery)
    {
        gameObject.SetActive(true);
        if (isFromGallery)
        {
            gameObject.SetActive(true);
        }

        _isFromGallery = isFromGallery;
        _isSocialMediaPost = true;
        AssignedGUID = item.NodeGuid;
        ChapterIndex = -1;
        CharacterName = item.Character.name;

        HintText = "Base profile item for " + item.Character.GetName();
    }

    /// <summary>
    /// Sets up the gallery button
    /// </summary>
    /// <param name="chapterData">The chapter data associated with the gallery item</param>
    /// <param name="nodeData">The node data associated with the gallery item</param>
    /// <param name="isSocialMediaPost">Indicates whether the gallery item is a social media post</param>
    public virtual void Setup(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData, bool isSocialMediaPost, bool isFromGallery)
    {
        if (nodeData == null || chapterData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (isFromGallery)
        {
            if (isSocialMediaPost)
            {
                gameObject.SetActive(nodeData.Post.GalleryVisibility == GalleryDisplay.Display);
            }
            else
            {
                gameObject.SetActive(nodeData.GalleryVisibility == GalleryDisplay.Display);
            }
        }

        _isFromGallery = isFromGallery;
        _isSocialMediaPost = isSocialMediaPost;

        AssignedGUID = nodeData.NodeGuid;
        ChapterIndex = chapterData.ChapterIndex;
        CharacterName = isSocialMediaPost ? nodeData.Post.Character.name : nodeData.Character.name;

        HintText = chapterData.Story.name;
    }

    public abstract void GalleryButtonClicked();

    protected void Update()
    {
        _lockedContainer.SetActive(!Unlocked);
    }
}