using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.Serialization;
using MeetAndTalk;

public class MessagingBubble : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("label")] TMP_Text _label;
    [SerializeField, FormerlySerializedAs("image")] Image _image;
    [SerializeField] GameObject _imageContainer;
    [SerializeField] GameObject _videoContainer;
    [SerializeField] Image _backgroundImage;
    [SerializeField] FullScreenMediaMessageViewer _mediaViewer;

    CanvasGroup cg;
    RectTransform rect;
    RectTransform labelRect;

    [Header("Video Clip Components")]
    [SerializeField] RawImage _videoImage;
    [SerializeField] private Texture2D _videoPreviewTexture;

    private void SetContainerSize(float width, float height, RectTransform container)
    {
        var isLandscape = width > height;
        var size = DialogueUIManager.Instance.MaxMessageSize;
        if (isLandscape)
        {
            var aspectRatio = height / width;
            container.GetComponent<RectTransform>().sizeDelta = new(size, size * aspectRatio);
        }
        else
        {
            var aspectRatio = width / height;
            container.GetComponent<RectTransform>().sizeDelta = new(size, size / aspectRatio);
        }
    }

    public void Init(bool hide, string text)
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        labelRect = _label.GetComponent<RectTransform>();

        transform.parent.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            transform.parent.GetComponent<RectTransform>().rect.size.y + 50f
            );

        //GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f;
        Message = text;


        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());

        StartCoroutine(COEnable(hide));
    }

    public void SetupMediaViewer(DialogueNodeData nodeData)
    {
        if (nodeData == null)
            return;

        _videoContainer.SetActive(false);
        _imageContainer.SetActive(false);
        Sprite postImage = null;
        VideoClip video = null;
        Sprite thumbnail = null;

        switch (nodeData.MediaType)
        {
            case MediaType.Sprite:
                postImage = nodeData.Image;
                thumbnail = postImage;
                break;
            case MediaType.Video:
                video = nodeData.Video;
                thumbnail = nodeData.VideoThumbnail;
                break;
        }

        if (postImage == null && video == null)
            return;

        if (postImage != null)
        {
            _image.preserveAspect = true;
            _image.sprite = postImage;
            _imageContainer.SetActive(true);
            SetContainerSize(_image.sprite.texture.width, _image.sprite.texture.height, _imageContainer.GetComponent<RectTransform>());
        }
        else
        {
            _videoImage.texture = thumbnail == null ? GameManager.Instance.GetVideoFrame(video).Item1 : thumbnail.texture;
            _videoContainer.SetActive(true);
            SetContainerSize(_videoImage.texture.width, _videoImage.texture.height, _videoContainer.GetComponent<RectTransform>());
        }

        _mediaViewer.Setup(nodeData, false);
    }

    private IEnumerator COEnable(bool hide)
    {
        cg.alpha = 0;
        yield return new WaitForEndOfFrame();
        if (!hide)
        {
            cg.alpha = 1;
        }
    }

    public string Message
    {
        set
        {
            _label.text = value;
            _label.gameObject.SetActive(value != string.Empty);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            if (_label.GetPreferredValues().x > DialogueUIManager.Instance.MaxMessageSize - GameManager.Instance.MessagingCanvas.BubbleMarginRight)
            {
                //labelRect.GetComponent<ContentSizeFitter>().enabled = false;
                labelRect.GetComponent<LayoutElement>().enabled = true;
                labelRect.GetComponent<LayoutElement>().preferredWidth = DialogueUIManager.Instance.MaxMessageSize;
            }
            else
            {
                //labelRect.GetComponent<ContentSizeFitter>().enabled = true;
                labelRect.GetComponent<LayoutElement>().enabled = false;
            }

            //if (labelRect.sizeDelta.x > GetComponentInParent<CanvasScaler>().referenceResolution.x - GameManager.Instance.MessagingCanvas.BubbleMarginRight)

        }
    }

    public Sprite Image
    {
        get
        {
            return _image.sprite;
        }
        set
        {
            _image.sprite = value;
        }
    }
    public MediaType MediaType;
    public VideoClip VideoClip;

    public bool IsTimelapse
    {
        set
        {
            if (!value) return;
            _label.fontSize = 15;
            _label.fontStyle = FontStyles.Italic;
            //GetComponent<Image>().sprite = GameManager.Instance.MessagingCanvas.TimelapsePanelBackground;
            _backgroundImage.color = Color.grey;
        }
    }

    private void Update()
    {
        switch (MediaType)
        {
            case MediaType.Video:
                if (VideoClip)
                {
                    if (_videoPreviewTexture != null)
                        return;

                    _videoPreviewTexture = GameManager.Instance.GetVideoFrame(VideoClip).Item1;
                    _videoImage.texture = _videoPreviewTexture;
                }
                break;
        }
    }
}
