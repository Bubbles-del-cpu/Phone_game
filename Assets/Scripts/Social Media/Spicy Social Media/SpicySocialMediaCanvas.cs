using System;
using System.Collections.Generic;
using MeetAndTalk;
using UnityEngine;

public class SpicySocialMediaCanvas : SocialMediaCanvas
{
    public new static void PostToFeed(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification = true)
    {
        GameManager.Instance.SpicySocialMediaCanvas.PostToFeedCanvas(_data, nodeData, showNotification);
    }

    protected override void PostToFeedCanvas(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification = true)
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
        MainMenuCanvas.Instance.SetSpicySocialMediaAppNotification(nodeData.Character, postSeen: false);

        // Check and add a profile button to the profile page button container if it doesn't already exist
        AddProfileButton(_data);
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
        MainMenuCanvas.Instance.SetSpicySocialMediaAppNotification(null, postSeen: true);
    }
}