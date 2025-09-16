using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Linq;
using System.Collections;

public class SocialMediaPost : MonoBehaviour
{
    [SerializeField] ProfileIcon icon;
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text postLabel;
    [SerializeField] Image postImage;
    [SerializeField] FullScreenMediaMessageViewer _mediaViewer;

    [Header("Post Comment Components")]
    [SerializeField] RectTransform _commentsSection;
    [SerializeField] SocialMediaPostComment _commentPrefab;
    [SerializeField] Button _commentDisplayButton;
    [SerializeField] Button _likedButton;
    [SerializeField] Sprite _likeSprite;
    [SerializeField] Sprite _unlikeSprite;

    bool _isDispalyed = false;
    bool _isLiked;
    DialogueNodeData _tiedNode;

    public bool IsLiked
    {
        get { return _isLiked; }
        set
        {
            _isLiked = value;
            _likedButton.image.sprite = _isLiked ? _likeSprite : _unlikeSprite;
        }
    }

    public bool DisplayComments
    {
        get { return _isDispalyed; }
        set
        {
            _isDispalyed = value;
            _commentsSection.gameObject.SetActive(value);
        }
    }

    IEnumerator CoSpawnNotification(SocialMediaPostSO value)
    {
        yield return new WaitForSeconds(1f);
        DialogueUIManager.Instance.SpawnNotification(Notification.NotificationType.SocialMedia, value.Character, value.Message);
    }

    public void SetData(SocialMediaPostSO data, DialogueNodeData nodeData, bool showNotification)
    {
        icon.Character = data.Character;
        nameLabel.text = data.Character.name;
        postLabel.text = data.Message;
        switch (data.MediaType)
        {
            case MediaType.Sprite:
                postImage.sprite = data.Image;
                break;
            case MediaType.Video:
                var videoFrame = GameManager.Instance.GetVideoFrame(data.Video);
                postImage.sprite = data.VideoThumbnail;

                if (data.VideoThumbnail == null)
                    postImage.sprite = Sprite.Create(videoFrame, new Rect(0, 0, videoFrame.width, videoFrame.height), new Vector2(0.5f, 0.5f));

                break;
        }

        postImage.preserveAspect = true;
        _mediaViewer.Setup(data.MediaType, data.Image, data.Video);

        //Populate the comments
        PopulateComments(data);

        //Check against the current save to see if the post has already been liked
        if (!SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            if (SaveAndLoadManager.Instance.CurrentSave.CurrentState.LikedPosts.Select(x=>x.NodeGUID).Contains(nodeData.NodeGuid))
            {
                IsLiked = true;
            }
        }

        _tiedNode = nodeData;
        // if (data.ImageTarget == ImageTarget.Gallery && nodeData.Post != null)
        //     GameManager.Instance.AddMediaToGallery(nodeData);


        if (showNotification)
            StartCoroutine(CoSpawnNotification(data));
    }

    public void ToggleCommentDisplay()
    {
        DisplayComments = !DisplayComments;
    }

    public void TogglePostLike()
    {
        IsLiked = !IsLiked;
        SaveAndLoadManager.Instance.CurrentSave.LikePost(_tiedNode, IsLiked);
    }

    /// <summary>
    /// Loops over the comments attached to the social media posts and creates individual comment prefabs to display the data
    /// </summary>
    /// <param name="post"></param>
    private void PopulateComments(SocialMediaPostSO post)
    {
        for(var index = 0; index < post.Comments.Count; index++)
        {
            if (post.Comments[index] != null)
            {
                var newComment = Instantiate(_commentPrefab, _commentsSection);
                newComment.SetComment(post.Comments[index], index);
            }
        }
    }
}
