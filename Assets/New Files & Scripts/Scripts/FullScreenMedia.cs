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

    private Sprite _videoThumbnail;

    public void OnPlayClick()
    {
        GameManager.Instance.MainVideoPlayer.Play();
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
                _playButton.gameObject.SetActive(!GameManager.Instance.MainVideoPlayer.isPlaying);
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
                    _videoImage.texture = GameManager.Instance.MainVideoPlayer.texture;
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

    public void Setup(DialogueNodeData nodeData, bool isSocialMediaPost)
    {
        _currentMediaType = nodeData.PostMediaType;
        (MediaType type, Sprite image, VideoClip video, Sprite videoThumbnail, bool backgroundCapable) mediaData =
            nodeData.GetNodeMediaData(isSocialMediaPost);

        _backgroundSetButton.gameObject.SetActive(false);
        switch (mediaData.type)
        {
            case MediaType.Sprite:
                _image.sprite = mediaData.image;
                _image.preserveAspect = true;

                //Setup the background set button
                _backgroundSetButton.gameObject.SetActive(mediaData.backgroundCapable);
                if (mediaData.backgroundCapable)
                    _backgroundSetButton.Setup(nodeData, isSocialMediaPost);

                break;
            case MediaType.Video:
                if (mediaData.videoThumbnail == null)
                {
                    var texture = GameManager.Instance.GetVideoFrame(mediaData.video);
                    _videoThumbnail = texture.Item2;
                }
                else
                {
                    _videoThumbnail = mediaData.videoThumbnail;
                }

                _image.sprite = _videoThumbnail;
                _image.preserveAspect = true;

                var player = GameManager.Instance.MainVideoPlayer;

                player.audioOutputMode = VideoAudioOutputMode.AudioSource;
                player.SetTargetAudioSource(0, GameManager.Instance.audioSource);
                player.source = VideoSource.VideoClip;
                player.clip = mediaData.video;
                player.frame = 1;
                player.Pause();
                break;
        }

    }
}