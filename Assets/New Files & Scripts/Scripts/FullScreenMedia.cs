using System.Collections;
using System.Collections.Generic;
using MeetAndTalk;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
public class FullScreenMedia : UIPanel
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _playButton;
    [SerializeField] private RawImage _videoImage;
    [SerializeField] private MediaType _currentMediaType;
    [SerializeField] protected GallerySetBackgroundButton _backgroundSetButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _previousButton;

    private VideoClip _clipToPlay;
    private Sprite _videoThumbnail;
    private List<GalleryMediaItem> _scrubMediaItems;
    private GalleryMediaItem _currentlyDisplayedItem;
    private int _currentScrubIndex = -1;

    public override void Awake()
    {
        _playButton.onClick.AddListener(() => OnPlayClick(0));
        _nextButton.onClick.AddListener(OnNextClick);
        _previousButton.onClick.AddListener(OnPreviousClick);
    }

    private void Update()
    {
        switch (_currentMediaType)
        {
            case MediaType.Sprite:
                _image.gameObject.SetActive(true);
                _videoImage.gameObject.SetActive(false);
                _playButton.gameObject.SetActive(false);
                break;
            case MediaType.Video:
                _playButton.gameObject.SetActive(!GameManager.Instance.MainVideoPlayer.IsPlaying);
                if (_playButton.gameObject.activeInHierarchy)
                {
                    _image.gameObject.SetActive(true);
                    _videoImage.gameObject.SetActive(false);
                    _image.sprite = _videoThumbnail;
                }
                else
                {
                    _image.gameObject.SetActive(false);
                    _videoImage.gameObject.SetActive(true);

                    var transform = _videoImage.GetComponent<RectTransform>();
                    _videoImage.texture = GameManager.Instance.MainVideoPlayer.Texture;
                    transform.sizeDelta = SizeToParent(_videoImage);
                }

                break;
        }
    }

    private Vector2 SizeToParent(RawImage image, float padding = 0)
    {
        float w = 0, h = 0;
        var parent = image.transform.parent.GetComponent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();

        // check if there is something to do
        if (image.texture != null)
        {
            if (!parent)
                return imageTransform.sizeDelta; //if we don't have a parent, just return our current width;

            padding = 1 - padding;
            float ratio = image.texture.width / (float)image.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }

            //Size by height first
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding)
            { //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }
        }

        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        return imageTransform.sizeDelta;
    }

    private void SetupMediaViewer(GalleryMediaItem galleryItem)
    {
        _backgroundSetButton.gameObject.SetActive(false);
        _currentMediaType = galleryItem.MediaType;
        switch (galleryItem.MediaType)
        {
            case MediaType.Sprite:
                _image.sprite = galleryItem.Image;
                _image.preserveAspect = true;

                //Setup the background set button
                _backgroundSetButton.gameObject.SetActive(galleryItem.IsBackgroundCapable);
                if (galleryItem.IsBackgroundCapable)
                    _backgroundSetButton.Setup(galleryItem.Node, galleryItem.IsSocialMediaPost);

                break;
            case MediaType.Video:
                if (galleryItem.VideoThumbnail == null)
                {
                    var texture = GameManager.Instance.GetVideoFrame(galleryItem.Video);
                    _videoThumbnail = texture.Item2;
                }
                else
                {
                    _videoThumbnail = galleryItem.VideoThumbnail;
                }

                _image.sprite = _videoThumbnail;
                _image.preserveAspect = true;

                GameManager.Instance.MainVideoPlayer.Stop();
                _clipToPlay = galleryItem.Video;

                break;
        }

        _currentlyDisplayedItem = galleryItem;
    }

    /// <summary>
    /// Setups the media viewer with scrub history
    /// </summary>
    /// <param name="galleryItem">Target gallery item to display</param>
    /// <param name="scrubItems">List of media items for scrubbing. Optional</param>
    public void SetupWithScrubHistory(GalleryMediaItem galleryItem, List<GalleryMediaItem> scrubItems = null)
    {
        if (scrubItems != null)
        {
            _scrubMediaItems = scrubItems;
            _currentScrubIndex = scrubItems.IndexOf(galleryItem);
        }

        _nextButton.gameObject.SetActive(scrubItems != null && scrubItems.Count > 1);
        _previousButton.gameObject.SetActive(scrubItems != null && scrubItems.Count > 1);

        SetupMediaViewer(galleryItem);
    }

    public void OnNextClick()
    {
        if (_scrubMediaItems == null || _scrubMediaItems.Count == 0)
            return;

        _currentScrubIndex = (_currentScrubIndex + 1) % _scrubMediaItems.Count;
        SetupMediaViewer(_scrubMediaItems[_currentScrubIndex]);
    }

    public void OnPreviousClick()
    {
        if (_scrubMediaItems == null || _scrubMediaItems.Count == 0)
            return;

        _currentScrubIndex = (_currentScrubIndex - 1 + _scrubMediaItems.Count) % _scrubMediaItems.Count;
        SetupMediaViewer(_scrubMediaItems[_currentScrubIndex]);
    }

    public void OnPlayClick(float delay = 0)
    {
        if (_clipToPlay == null)
            return;
        StartCoroutine(CoPlayVideoWithDelay(delay));
    }

    private IEnumerator CoPlayVideoWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.MainVideoPlayer.PlayVideo(_clipToPlay);
    }
}