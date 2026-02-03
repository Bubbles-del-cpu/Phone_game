using System;
using System.Collections.Generic;
using MeetAndTalk;
using UnityEngine;

public class SpicySocialMediaCanvas : SocialMediaCanvas
{
    public new static void PostToFeed(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification, bool adjustSaveData)
    {
        GameManager.Instance.SpicySocialMediaCanvas.PostToFeedCanvas(_data, nodeData, showNotification, adjustSaveData);
    }

    protected override void PostToFeedCanvas(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification, bool adjustSaveData)
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