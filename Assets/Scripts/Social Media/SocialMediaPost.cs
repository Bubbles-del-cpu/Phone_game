using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Linq;
using System.Collections;

public class SocialMediaPost : MonoBehaviour
{
    [SerializeField] SocialMediaProfileButton profileButton;
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

    /// <summary>
    /// The character associated with this social media post
    /// </summary>
    public DialogueCharacterSO Character
    {
        get
        {
            if (_tiedNode.Post != null && _tiedNode.Post.Character != null)
                return _tiedNode.Post.Character;

            return _tiedNode.Character;
        }
    }

    /// <summary>
    /// The DialogueNodeData associated with this social media post
    /// </summary>
    public DialogueNodeData AssignedNodeData => _tiedNode;

    private void LogError(string message, Object context = null)
    {
        Debug.LogError($"[SocialMediaPost] {message}", context ?? this.gameObject);
    }

    public void SetData(SocialMediaPostSO data, DialogueNodeData nodeData, bool showNotification)
    {
        // --- Start Debug Logs ---
        if (data == null || nodeData == null)
        {
            LogError("SetData received NULL parameters! Data: " + (data == null ? "NULL" : "Valid") + ", NodeData: " + (nodeData == null ? "NULL" : "Valid"));
        }

        // Ensure component references are valid before proceeding
        if (profileButton == null || nameLabel == null || postLabel == null || postImage == null || _mediaViewer == null)
        {
            LogError($"SetData has missing component references! Icon: {profileButton}, NameLabel: {nameLabel}, PostLabel: {postLabel}, PostImage: {postImage}, MediaViewer: {_mediaViewer}");
            return;
        }

        // Added null check for safety
        if (data.Character != null)
        {
            profileButton.Initialize(data.Character,
                data.TargetPlatform == MediaTargetPlatform.SpicySocialMediaPost ? data.Character.SpicySocialMediaProfile : data.Character.SocialMediaProfile
            );
            nameLabel.text = data.Character.GetName();
        }
        else
        {
            nameLabel.text = "Unknown User"; // Provide a fallback name
        }

        postLabel.text = DialogueLocalizationHelper.GetText(data.MessageTexts);

        // --- Add Logs Around Image Assignment ---
        Sprite spriteToAssign = null; // Temporary variable to hold the sprite
        switch (data.MediaType)
        {
            case MediaType.Sprite:
                spriteToAssign = data.Image;
                break;
            case MediaType.Video:
                spriteToAssign = data.VideoThumbnail;
                if (spriteToAssign == null && data.Video != null) // Check if Video exists before getting frame
                {
                    var videoFrame = GameManager.Instance.GetVideoFrame(data.Video);
                    spriteToAssign = videoFrame.Item2;
                }
                break;
        }

        postImage.sprite = spriteToAssign; // Assign the determined sprite
        postImage.preserveAspect = true;
        _mediaViewer.Setup(nodeData.NodeGuid, data.MediaType == MediaType.Video ? data.Video.name : data.Image.name, data.MediaType, isSocialMediaPost: true);

        PopulateComments(data); // Assumes data is not null based on earlier check

        // Added null check for safety
        if (!SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            // Added null check for nodeData
            if (SaveAndLoadManager.Instance.CurrentSave.CurrentState.LikedPosts.Select(x => x.NodeGUID).Contains(nodeData.NodeGuid))
            {
                IsLiked = true;
            }
        }

        _tiedNode = nodeData; // nodeData might be null if previous check failed, handle accordingly if needed later
        if (showNotification)
            data.SpawnNotification();
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
        // Added null checks
        if (post == null || post.Comments == null || _commentPrefab == null || _commentsSection == null)
        {
            LogError($"Cannot populate comments! Post: {post}, CommentsList: {post?.Comments}, Prefab: {_commentPrefab}, Section: {_commentsSection}");
            return;
        }

        // Clear existing comments first? Optional, but often needed.
        // foreach (Transform child in _commentsSection) { Destroy(child.gameObject); }

        for (var index = 0; index < post.Comments.Count; index++)
        {
            if (post.Comments[index] != null)
            {
                var newComment = Instantiate(_commentPrefab, _commentsSection);
                newComment.SetComment(post.Comments[index], index);
            }
        }
    }
}
