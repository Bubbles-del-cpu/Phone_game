using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class SocialMediaCanvas : UICanvas
{
    private static SocialMediaCanvas _instance;
    public static SocialMediaCanvas Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SocialMediaCanvas>();
            }

            return _instance;
        }
    }

    [SerializeField, FormerlySerializedAs("socialMediaPostPrefab")] private SocialMediaPost _socialMediaPostPrefab;
    [SerializeField, FormerlySerializedAs("socialMediaPostsContainer")] private RectTransform _socialMediaPostsContainer;
    [SerializeField] private SocialMediaProfilePage _socialMediaPage;
    [SerializeField] private RectTransform _profilePageButtonContainer;
    [SerializeField] private GameObject _profilePageButtonPrefab;

    /// <summary>
    /// Dictionary to keep track of profile buttons for each character
    /// </summary>
    private Dictionary<DialogueCharacterSO, SocialMediaProfileButton> _profileButtons = new Dictionary<DialogueCharacterSO, SocialMediaProfileButton>();
    /// <summary>
    /// Dictionary to keep track of posts for each character
    /// </summary>
    private Dictionary<DialogueCharacterSO, List<SocialMediaPost>> _characterPosts = new Dictionary<DialogueCharacterSO, List<SocialMediaPost>>();


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

                destroyList.Add(item.gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to remove social media post. Error: {ex.Message}");
                break;
            }
        }

        foreach (var item in destroyList)
            Destroy(item);

        // Remove any profile buttons for characters that no longer have posts
        foreach (var kvp in _characterPosts)
        {
            if (kvp.Value.Count == 0)
                RemoveProfileButton(kvp.Key);
        }
    }

    public static void PostToSoicalMediaApp(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification = true)
    {
        GameManager.Instance.SocialMediaCanvas.PostToSocialMedia(_data, nodeData, showNotification);
    }

    public void PostToSocialMedia(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification = true)
    {
        Debug.Log($"Attempting to post to social media. Prefab valid: {_socialMediaPostPrefab != null}, Container valid: {_socialMediaPostsContainer != null}");
        if (_socialMediaPostPrefab == null || _socialMediaPostsContainer == null)
        {
            Debug.LogError("Cannot post! Missing prefab or container reference on SocialMediaCanvas!");
            return;
        }

        SocialMediaPost post = Instantiate(_socialMediaPostPrefab, _socialMediaPostsContainer);
        Debug.Log($"Instantiated post: {post.name}", post.gameObject); // Log the instance

        try // Add temporary error catching for SetData
        {
            post.SetData(_data, nodeData, showNotification);

            _characterPosts.TryAdd(_data.Character, new List<SocialMediaPost>());
            _characterPosts[_data.Character].Add(post);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error calling SetData on new post: {ex.Message}\n{ex.StackTrace}", post.gameObject);
        }

        // Note: The social media canvas is different than messages as all posts are tied to the assigned character rather than a specific dialogue character
        // Pass the assigned story character's character data for notification purposes
        MainMenuCanvas.Instance.SetSocialMediaAppNotification(nodeData.Character, postSeen: false);

        // Check and add a profile button to the profile page button container if it doesn't already exist
        AddProfileButton(_data);
    }

    /// <summary>
    /// Adds a profile button for the character if it doesn't already exist
    /// </summary>
    /// <param name="postData">Social media post data</param>
    private void AddProfileButton(SocialMediaPostSO postData)
    {
        if (_profileButtons.ContainsKey(postData.Character))
            return;

        var buttonObj = Instantiate(_profilePageButtonPrefab, _profilePageButtonContainer);
        var buttonComp = buttonObj.GetComponent<SocialMediaProfileButton>();
        _profileButtons[postData.Character] = buttonComp;
        buttonComp.Initialize(postData.Character);
    }

    /// <summary>
    /// Removes the profile button for the specified character
    /// </summary>
    /// <param name="character">Character whose profile button should be removed</param>
    private void RemoveProfileButton(DialogueCharacterSO character)
    {
        if (!_profileButtons.ContainsKey(character))
            return;

        var buttonComp = _profileButtons[character];
        Destroy(buttonComp.gameObject);

        _profileButtons.Remove(character);
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

    public void Clear()
    {
        for (var index = 0; index < _socialMediaPostsContainer.childCount; index++)
        {
            Destroy(_socialMediaPostsContainer.GetChild(index).gameObject, .5f);
        }
    }

    public override void Open()
    {
        base.Open();

        // Note: The social media canvas is different than messages as all posts are tied to the assigned character rather than a specific dialogue character
        // Pass the assigned story character's character data for notification purposes
        MainMenuCanvas.Instance.SetSocialMediaAppNotification(null, postSeen: true);
    }
}
