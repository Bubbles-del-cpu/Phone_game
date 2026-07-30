using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.Serialization;
using MeetAndTalk;
using static MeetAndTalk.DialogueUIManager;

public class MessagingBubble : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("label")] TMP_Text _label;
    [SerializeField, FormerlySerializedAs("image")] Image _image;
    [SerializeField] GameObject _imageContainer;
    [SerializeField] GameObject _videoContainer;
    [SerializeField] Image _backgroundImage;
    [SerializeField] FullScreenMediaMessageViewer _mediaViewer;

    CanvasGroup _cg;
    RectTransform _rect;
    RectTransform _labelRect;
    private MessageSource _source;
    private string _guid;
    public string NodeGUID => _guid;
    public MessageSource Source => _source;

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

    public void Init(bool hide, string text, bool timelapse, string guid, MessageSource containerSource)
    {
        _cg = GetComponent<CanvasGroup>();
        _rect = GetComponent<RectTransform>();
        _labelRect = _label.GetComponent<RectTransform>();

        transform.parent.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            transform.parent.GetComponent<RectTransform>().rect.size.y + 50f
            );


        gameObject.SetActive(true);

        _guid = guid;
        _source = containerSource;
        _cg.alpha = 0;
        Message = text;
        IsTimelapse = timelapse;

        StartCoroutine(COEnable(hide));
    }

    public void Clear()
    {
        Message = string.Empty;
        VideoClip = null;
        Image = null;

        _videoImage.texture = null;
        _videoPreviewTexture = null;

        _videoContainer.SetActive(false);
        _imageContainer.SetActive(false);

        if (_mediaViewer != null)
            _mediaViewer.gameObject.SetActive(false);
    }

    public void SetupMediaViewer(DialogueNodeData nodeData)
    {
        if (_mediaViewer != null)
            _mediaViewer.gameObject.SetActive(false);

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

        _mediaViewer.Setup(nodeData.NodeGuid, nodeData.MediaType == MediaType.Video ? video.name : postImage.name, nodeData.MediaType, isSocialMediaPost: false);
    }

    private IEnumerator COEnable(bool hide)
    {
        _cg.alpha = 0;
        yield return null;
        if (!hide)
        {
            while (_cg.alpha < 1)
            {
                _cg.alpha += Time.deltaTime * DialogueUIManager.Instance.MessagingBubbleFadeInSpeed;
                yield return null;
            }

            _cg.alpha = 1;
        }
    }

    public string Message
    {
        set
        {
            _label.text = value;
            _label.gameObject.SetActive(value != string.Empty);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);

            if (_label.GetPreferredValues().x > DialogueUIManager.Instance.MaxMessageSize - GameManager.Instance.MessagingCanvas.BubbleMarginRight)
            {
                //labelRect.GetComponent<ContentSizeFitter>().enabled = false;
                _labelRect.GetComponent<LayoutElement>().enabled = true;
                _labelRect.GetComponent<LayoutElement>().preferredWidth = DialogueUIManager.Instance.MaxMessageSize;
            }
            else
            {
                //labelRect.GetComponent<ContentSizeFitter>().enabled = true;
                _labelRect.GetComponent<LayoutElement>().enabled = false;
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
            if (value)
            {
                _label.fontSize = 15;
                _label.fontStyle = FontStyles.Italic;
                _backgroundImage.color = Color.grey;
            }
            else
            {
                _label.fontSize = 18;
                _label.fontStyle = FontStyles.Normal;
                switch (_source)
                {
                    case MessageSource.Character:
                        _backgroundImage.color = new Color(0, 0.08235294f, 0.2470588f);
                        break;
                    case MessageSource.Player:
                        _backgroundImage.color = new Color(0.3490566f, 0.3490566f, 0.3490566f);
                        break;
                }
            }
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
