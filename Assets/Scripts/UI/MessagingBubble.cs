using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.Collections;

public class MessagingBubble : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Image image;
    [SerializeField] GameObject _imageContainer;
    [SerializeField] GameObject _videoContainer;
    [SerializeField] FullScreenMediaMessageViewer _mediaViewer;


    CanvasGroup cg;
    RectTransform rect;
    RectTransform labelRect;

    [SerializeField]
    private Vector2 _padding;
    [SerializeField] private Vector2 _imageAllowance;

    [Header("Video Clip Components")]
    [SerializeField] RawImage _videoImage;
    [SerializeField] private Texture2D _videoPreviewTexture;


    public void Init(bool hide, string text, Sprite image)
    {
        MediaType = MediaType.Sprite;
        VideoClip = null;
        Image = image;
        Init(hide, text);

        _videoContainer.SetActive(false);
        _imageContainer.SetActive(Image != null);

        _mediaViewer.Setup(MediaType, Image, null);
    }

    public void Init(bool hide, string text, VideoClip clip)
    {
        MediaType = MediaType.Video;
        VideoClip = clip;
        Image = null;
        Init(hide, text);

        _videoPreviewTexture = GameManager.Instance.GetVideoFrame(clip);
        _videoImage.texture = _videoPreviewTexture;

        _videoContainer.SetActive(VideoClip != null);
        _imageContainer.SetActive(false);

    }

    public void Init(bool hide, string text)
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        labelRect = label.GetComponent<RectTransform>();

        transform.parent.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            transform.parent.GetComponent<RectTransform>().rect.size.y + 50f
            );

        GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f;
        Message = text;

        _videoContainer.SetActive(false);
        _imageContainer.SetActive(false);

        _mediaViewer.Setup(MediaType, Image, VideoClip);


        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());

        StartCoroutine(COEnable(hide));
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
            label.text = value;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            if (labelRect.sizeDelta.x > GetComponentInParent<CanvasScaler>().referenceResolution.x - GameManager.Instance.MessagingCanvas.BubbleMarginRight)
                rect.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
    }

    public Sprite Image
    {
        get
        {
            return image.sprite;
        }
        set
        {
            image.sprite = value;
        }
    }
    public MediaType MediaType;
    public VideoClip VideoClip;

    public bool IsTimelapse
    {
        set
        {
            if (!value) return;
            label.fontSize = 15;
            label.fontStyle = FontStyles.Italic;
            //GetComponent<Image>().sprite = GameManager.Instance.MessagingCanvas.TimelapsePanelBackground;
            GetComponent<Image>().color = Color.grey;
        }
    }

    private void Update()
    {
        rect.sizeDelta = label.rectTransform.sizeDelta + _padding + (image.gameObject.activeInHierarchy ? _imageAllowance : Vector2.zero);
        switch (MediaType)
        {
            case MediaType.Video:
                if (VideoClip)
                {
                    _videoPreviewTexture = GameManager.Instance.GetVideoFrame(VideoClip);
                    _videoImage.texture = _videoPreviewTexture;
                }
                break;
        }
    }
}
