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

    // --- TEMPORARY DEBUG ---
    [SerializeField] private Sprite _debugSprite; // Assign a sprite here in the Inspector!
    // -----------------------

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

    // --- TEMPORARY DEBUG FUNCTION ---
    [ContextMenu("DEBUG Assign Sprite")] // Allows right-clicking the component in Inspector
    private void DebugAssignSprite()
    {
        if (postImage == null)
        {
            Debug.LogError("postImage reference is MISSING!");
            return;
        }
        if (_debugSprite == null)
        {
            Debug.LogError("Assign a sprite to _debugSprite in the Inspector first!");
            return;
        }

        Debug.Log($"Manually assigning sprite: {_debugSprite.name} to {postImage.gameObject.name}");
        postImage.sprite = _debugSprite;
        postImage.color = Color.white; // Ensure it's not transparent
        postImage.enabled = true;      // Ensure it's enabled
        Debug.Log($"Manual assignment complete. postImage.sprite is now: {(postImage.sprite != null ? postImage.sprite.name : "null")}");
    }
    // ------------------------------

    IEnumerator CoSpawnNotification(SocialMediaPostSO value)
    {
        yield return new WaitForSeconds(1f);
        // Ensure DialogueUIManager.Instance is not null before accessing it
        if (DialogueUIManager.Instance != null)
        {
            DialogueUIManager.Instance.SpawnNotification(Notification.NotificationType.SocialMedia, value.Character, DialogueLocalizationHelper.GetText(value.MessageTexts));
        }
        else
        {
            Debug.LogError("DialogueUIManager.Instance is null in CoSpawnNotification!", this.gameObject);
        }
    }

    public void SetData(SocialMediaPostSO data, DialogueNodeData nodeData, bool showNotification)
    {
        // --- Start Debug Logs ---
        if (data == null)
        {
            Debug.LogError("SetData received NULL SocialMediaPostSO!", this.gameObject);
            return; // Stop if data is null
        }
         // Ensure component references are valid before proceeding
        if (icon == null || nameLabel == null || postLabel == null || postImage == null || _mediaViewer == null)
        {
            Debug.LogError($"SetData has missing component references! Icon: {icon}, NameLabel: {nameLabel}, PostLabel: {postLabel}, PostImage: {postImage}, MediaViewer: {_mediaViewer}", this.gameObject);
            return;
        }
        Debug.Log($"SetData called for SO: '{data.name}' on GameObject: '{this.gameObject.name}'", this.gameObject);
        Debug.Log($"   - SO Character: {(data.Character != null ? data.Character.name : "null")}", this.gameObject);
        Debug.Log($"   - SO MediaType: {data.MediaType}", this.gameObject);
        // --- End Debug Logs ---

        // Added null check for safety
        if (data.Character != null)
        {
            icon.Character = data.Character;
            nameLabel.text = data.Character.name;
        }
        else
        {
             Debug.LogWarning($"   - SocialMediaPostSO '{data.name}' is missing a Character!", this.gameObject);
             nameLabel.text = "Unknown User"; // Provide a fallback name
        }

        postLabel.text = DialogueLocalizationHelper.GetText(data.MessageTexts);

        // --- Add Logs Around Image Assignment ---
        Sprite spriteToAssign = null; // Temporary variable to hold the sprite
        switch (data.MediaType)
        {
            case MediaType.Sprite:
                Debug.Log($"   - Attempting to assign Sprite. Is SO.Image null? {data.Image == null}", this.gameObject);
                if(data.Image != null) Debug.Log($"   - SO.Image name: {data.Image.name}", this.gameObject);
                spriteToAssign = data.Image;
                break;
            case MediaType.Video:
                Debug.Log($"   - Attempting to assign Video Thumbnail. Is SO.VideoThumbnail null? {data.VideoThumbnail == null}", this.gameObject);
                if(data.VideoThumbnail != null) Debug.Log($"   - SO.VideoThumbnail name: {data.VideoThumbnail.name}", this.gameObject);
                spriteToAssign = data.VideoThumbnail;

                if (spriteToAssign == null && data.Video != null) // Check if Video exists before getting frame
                {
                    Debug.Log($"   - VideoThumbnail is null. Attempting fallback frame for video: {data.Video.name}", this.gameObject);
                    // Ensure GameManager.Instance is not null
                    if (GameManager.Instance != null)
                    {
                        var videoFrame = GameManager.Instance.GetVideoFrame(data.Video);
                        Debug.Log($"   - Fallback frame result. Is Item2 (Sprite) null? {(videoFrame.Item2 == null)}", this.gameObject);
                        if(videoFrame.Item2 != null) Debug.Log($"   - Fallback frame sprite name: {videoFrame.Item2.name}", this.gameObject);
                        spriteToAssign = videoFrame.Item2;
                    }
                    else
                    {
                        Debug.LogError("   - GameManager.Instance is null, cannot get fallback video frame!", this.gameObject);
                    }
                }
                else if (data.Video == null)
                {
                     Debug.LogWarning($"   - MediaType is Video, but SO.Video is null!", this.gameObject);
                }
                break;
            default:
                 Debug.LogWarning($"   - Unknown MediaType: {data.MediaType}", this.gameObject);
                 break;
        }

        Debug.Log($"   - Assigning sprite named: {(spriteToAssign != null ? spriteToAssign.name : "null")} to postImage.", this.gameObject);
        postImage.sprite = spriteToAssign; // Assign the determined sprite

        // --- NEW DEBUG LINES ---
        // Check IMMEDIATELY after assignment
        Debug.Log($"   - IMMEDIATELY AFTER ASSIGNMENT, postImage.sprite is: {(postImage.sprite != null ? postImage.sprite.name : "null")}", this.gameObject);
        // Check component enabled state
        Debug.Log($"   - Is postImage component enabled? {postImage.enabled}", this.gameObject);
        // Check GameObject active state
        Debug.Log($"   - Is postImage GameObject active in hierarchy? {postImage.gameObject.activeInHierarchy}", this.gameObject);
        // Check color alpha
        Debug.Log($"   - postImage color alpha: {postImage.color.a}", this.gameObject);
        // Check RectTransform size
        RectTransform rt = postImage.GetComponent<RectTransform>();
        if (rt != null) {
            Debug.Log($"   - postImage RectTransform size (rect): ({rt.rect.width}, {rt.rect.height})", this.gameObject);
             Debug.Log($"   - postImage RectTransform size (sizeDelta): ({rt.sizeDelta.x}, {rt.sizeDelta.y})", this.gameObject); // Also check sizeDelta
        } else {
             Debug.LogError("   - postImage is missing RectTransform?!?", this.gameObject);
        }
        // Force Canvas Update (Optional test)
        // Canvas.ForceUpdateCanvases();
        // Debug.Log("   - Called Canvas.ForceUpdateCanvases()", this.gameObject);
        // --- END NEW DEBUG LINES ---


        if (postImage.sprite == null)
        {
            Debug.LogError($"   - FAILED TO ASSIGN SPRITE! postImage.sprite is still null after assignment!", this.gameObject);
        }
        else
        {
             Debug.Log($"   - SUCCESS! postImage.sprite is now: {postImage.sprite.name}", this.gameObject); // You already had this
        }
        // --- End Logs Around Image Assignment ---


        postImage.preserveAspect = true;

        // Added null check for safety
        if (nodeData != null)
        {
            _mediaViewer.Setup(nodeData, isSocialMediaPost: true);
        }
        else
        {
             Debug.LogError("SetData received NULL DialogueNodeData!", this.gameObject);
        }


        PopulateComments(data); // Assumes data is not null based on earlier check

        // Added null check for safety
        if (SaveAndLoadManager.Instance != null && SaveAndLoadManager.Instance.CurrentSave != null && !SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            // Added null check for nodeData
            if (nodeData != null && SaveAndLoadManager.Instance.CurrentSave.CurrentState.LikedPosts.Select(x => x.NodeGUID).Contains(nodeData.NodeGuid))
            {
                IsLiked = true;
            }
        }
        else if (SaveAndLoadManager.Instance == null)
        {
             Debug.LogError("SaveAndLoadManager.Instance is null, cannot check liked status!", this.gameObject);
        }
        else if (SaveAndLoadManager.Instance.CurrentSave == null)
        {
             Debug.LogError("SaveAndLoadManager.Instance.CurrentSave is null, cannot check liked status!", this.gameObject);
        }


        _tiedNode = nodeData; // nodeData might be null if previous check failed, handle accordingly if needed later

        if (showNotification)
            StartCoroutine(CoSpawnNotification(data)); // Assumes data is not null

         Debug.Log($"SetData finished for SO: '{data.name}'", this.gameObject); // Log finish
    }

    public void ToggleCommentDisplay()
    {
        DisplayComments = !DisplayComments;
    }

    public void TogglePostLike()
    {
        IsLiked = !IsLiked;
         // Added null checks for safety
        if (SaveAndLoadManager.Instance != null && SaveAndLoadManager.Instance.CurrentSave != null && _tiedNode != null)
        {
             SaveAndLoadManager.Instance.CurrentSave.LikePost(_tiedNode, IsLiked);
        }
         else
         {
              Debug.LogError("Cannot toggle like! SaveAndLoadManager, CurrentSave, or _tiedNode is null!", this.gameObject);
         }
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
            Debug.LogError($"Cannot populate comments! Post: {post}, CommentsList: {post?.Comments}, Prefab: {_commentPrefab}, Section: {_commentsSection}", this.gameObject);
            return;
        }

         // Clear existing comments first? Optional, but often needed.
         // foreach (Transform child in _commentsSection) { Destroy(child.gameObject); }

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
