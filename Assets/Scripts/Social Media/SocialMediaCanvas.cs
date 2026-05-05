using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class SocialMediaCanvas : UICanvas
{
    [SerializeField, FormerlySerializedAs("socialMediaPostPrefab")] protected SocialMediaPost _socialMediaPostPrefab;
    [SerializeField, FormerlySerializedAs("socialMediaPostsContainer")] protected RectTransform _socialMediaPostsContainer;
    [SerializeField] protected SocialMediaProfilePage _socialMediaPage;
    [SerializeField] protected RectTransform _profilePageButtonContainer;
    [SerializeField] protected GameObject _profilePageButtonPrefab;
    [SerializeField] protected GameObject _noPostsWarning;
    [SerializeField] protected int _currentPostCount => _socialMediaPostsContainer.childCount;

    /// <summary>
    /// Dictionary to keep track of profile buttons for each character
    /// </summary>
    protected Dictionary<DialogueCharacterSO, SocialMediaProfileButton> _profileButtons = new Dictionary<DialogueCharacterSO, SocialMediaProfileButton>();
    /// <summary>
    /// Dictionary to keep track of posts for each character
    /// </summary>
    protected Dictionary<DialogueCharacterSO, List<SocialMediaPost>> _characterPosts = new Dictionary<DialogueCharacterSO, List<SocialMediaPost>>();

    protected virtual void Update()
    {
        _noPostsWarning.SetActive(_socialMediaPostsContainer.childCount == 0);
    }

    public virtual void PopulateHistory(List<string> visiblePostGuids)
    {
        // Clear the feed before populating with posts from save data
        ClearSocialFeed();

        if (visiblePostGuids == null || visiblePostGuids.Count == 0)
            return;

        foreach (var postGuid in visiblePostGuids)
        {
            var galleryItem = GameManager.Instance.GalleryCanvas.GetGalleryItem(postGuid, true);
            if (galleryItem != null && galleryItem.Node != null && galleryItem.Node.Post != null)
            {
                PostToFeed(galleryItem.Node.Post, galleryItem.Node, showNotification: false, adjustSaveData: false);
            }
        }
    }

    public void RemovePosts(int count)
    {
        if (count == 0 || _socialMediaPostsContainer.transform.childCount <= 0)
            return;

        var destroyList = new List<GameObject>();
        for (var index = 1; index <= count; index++)
        {
            try
            {
                var item = _socialMediaPostsContainer.transform.GetChild(_socialMediaPostsContainer.transform.childCount - index);
                var post = item.GetComponent<SocialMediaPost>();
                if (post == null)
                    continue;

                item.gameObject.SetActive(false);
                _characterPosts[post.Character].Remove(post);

                // Remove from save data
                SaveAndLoadManager.Instance.CurrentSave.RemoveSocialMediaPost(post.AssignedNodeData);

                destroyList.Add(item.gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to remove social media post. Error: {ex.Message}");
                break;
            }
        }

        foreach (var item in destroyList)
        {
            Destroy(item);
        }

        // Remove any profile buttons for characters that no longer have posts
        foreach (var kvp in _characterPosts)
        {
            if (kvp.Value.Count == 0)
                RemoveProfileButton(kvp.Key);
        }
    }

    public static void PostToFeed(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification, bool adjustSaveData)
    {
        GameManager.Instance.SocialMediaCanvas.PostToFeedCanvas(_data, nodeData, showNotification, adjustSaveData);
    }

    protected virtual void PostToFeedCanvas(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification, bool adjustSaveData)
    {
        Debug.Log($"Attempting to post to social media. Prefab valid: {_socialMediaPostPrefab != null}, Container valid: {_socialMediaPostsContainer != null}");
        if (_socialMediaPostPrefab == null || _socialMediaPostsContainer == null)
        {
            Debug.LogError("Cannot post! Missing prefab or container reference on SocialMediaCanvas!");
            return;
        }

        CreateNewSocialMediaPost(_data, nodeData, showNotification, adjustSaveData);

        // Note: The social media canvas is different than messages as all posts are tied to the assigned character rather than a specific dialogue character
        // Pass the assigned story character's character data for notification purposes
        if (showNotification)
            MainMenuCanvas.Instance.SetSocialMediaAppNotification(nodeData.Character, postSeen: false);

        // Check and add a profile button to the profile page button container if it doesn't already exist
        AddProfileButton(_data);
    }

    /// <summary>
    /// Creates a new social media post and adds it to the feed. If the maximum post count is reached, removes the oldest post.
    /// </summary>
    /// <param name="postData">Social media post data</param>
    /// <param name="nodeData">Node data associated with the post</param>
    /// <param name="showNotification">Whether to show a notification for the new post</param>
    protected virtual void CreateNewSocialMediaPost(SocialMediaPostSO postData, DialogueNodeData nodeData, bool showNotification, bool adjustSaveData)
    {
        // Check for maximum post count and remove oldest posts if necessary
        if (_currentPostCount >= DialogueManager.Instance.MaximumNumberOfSocialPosts)
        {
            // Delete the oldest post
            var item = _socialMediaPostsContainer.transform.GetChild(0);
            var oldPost = item.GetComponent<SocialMediaPost>();
            if (oldPost != null)
            {
                item.gameObject.SetActive(false);
                _characterPosts[oldPost.Character].Remove(oldPost);

                // Remove from save data
                if (adjustSaveData)
                    SaveAndLoadManager.Instance.CurrentSave.RemoveSocialMediaPost(oldPost.AssignedNodeData);

                Destroy(item.gameObject);
            }
        }

        var post = Instantiate(_socialMediaPostPrefab, _socialMediaPostsContainer);
        post.name = $"Post_{postData.Character.GetName()}_{nodeData.NodeGuid}";
        post.SetData(postData, nodeData, showNotification);

        _characterPosts.TryAdd(postData.Character, new List<SocialMediaPost>());
        _characterPosts[postData.Character].Add(post);

        Debug.Log($"Instantiated post: {post.name}", post.gameObject); // Log the instance
        if (adjustSaveData)
            SaveAndLoadManager.Instance.CurrentSave.AddSocialMediaPost(nodeData);
    }

    /// <summary>
    /// Adds a profile button for the specified post data if one doesn't already exist, and unlocks any associated gallery media items.
    /// </summary>
    /// <param name="postData">The social media post data for which to add a profile button.</param>
    public virtual void AddProfileButton(SocialMediaPostSO postData)
    {
        if (_profileButtons.ContainsKey(postData.Character))
            return;

        var buttonObj = Instantiate(_profilePageButtonPrefab, _profilePageButtonContainer);
        var buttonComp = buttonObj.GetComponent<SocialMediaProfileButton>();
        _profileButtons[postData.Character] = buttonComp;
        buttonComp.Initialize(postData.Character, postData.Character.SocialMediaProfile);

        var galleryItems = postData.Character.SocialMediaProfile.GetBaseGalleryMediaData(postData.Character, MediaTargetPlatform.SocialMediaPost);

        // Unlock the gallery media items
        foreach (var mediaData in galleryItems)
        {
            if (!SaveAndLoadManager.Instance.ReplayingCompletedChapter)
                SaveAndLoadManager.Instance.CurrentSave.UnlockMedia(mediaData.NodeGUID, mediaData.FileName, linearPath: true);

            GameManager.Instance.GalleryCanvas.UnlockMedia(mediaData.NodeGUID, mediaData.FileName, reloadedGallery: false);
        }

        // Refresh the gallery page to reflect the changes and ensure any unlocked media are shown as unlocked
        GameManager.Instance.GalleryCanvas.RefreshGalleryPage();
    }

    /// <summary>
    /// Removes the profile button for the specified character and rolls back any associated gallery media item unlocks in the save data.
    /// </summary>
    /// <param name="character">The character for which to remove the profile button.</param>
    protected virtual void RemoveProfileButton(DialogueCharacterSO character)
    {
        if (!_profileButtons.ContainsKey(character))
            return;

        var buttonComp = _profileButtons[character];
        Destroy(buttonComp.gameObject);

        _profileButtons.Remove(character);

        // Remove any associated gallery media items from the save data
        var galleryItems = character.SocialMediaProfile.GetBaseGalleryMediaData(character, MediaTargetPlatform.SocialMediaPost);
        foreach (var mediaData in galleryItems)
        {
            SaveAndLoadManager.Instance.CurrentSave.RollbackUnlockMedia(mediaData.NodeGUID, mediaData.FileName);
        }

        // Refresh the gallery page to reflect the changes and ensure any rolled back media are shown as locked again
        GameManager.Instance.GalleryCanvas.RefreshGalleryPage();
    }

    public void OpenProfilePage(DialogueCharacterSO character)
    {
        if (_socialMediaPage == null)
        {
            Debug.LogError("Cannot open profile page! Missing SocialMediaProfilePage reference on SocialMediaCanvas!");
            return;
        }

        _socialMediaPage.OpenProfile(character);
    }

    public void CloseProfilePage()
    {
        if (_socialMediaPage == null)
        {
            Debug.LogError("Cannot close profile page! Missing SocialMediaProfilePage reference on SocialMediaCanvas!");
            return;
        }

        _socialMediaPage.CloseProfile();
    }

    public void ClearProfileButtons()
    {
        foreach (var item in _profileButtons)
        {
            Destroy(item.Value.gameObject);
        }

        _profileButtons.Clear();
    }

    public void ClearSocialFeed()
    {
        for (var index = 0; index < _socialMediaPostsContainer.childCount; index++)
        {
            Destroy(_socialMediaPostsContainer.GetChild(index).gameObject, .5f);
        }
    }

    public override void Open()
    {
        if (_canvas != null)
        {
            var command = new PanelOpenCommand(this, openState: true);
            NavigationManager.Instance.InvokeCommand(command, allowUndo: true);
        }

        // Note: The social media canvas is different than messages as all posts are tied to the assigned character rather than a specific dialogue character
        // Pass the assigned story character's character data for notification purposes
        MainMenuCanvas.Instance.SetSocialMediaAppNotification(null, postSeen: true);
    }
}
