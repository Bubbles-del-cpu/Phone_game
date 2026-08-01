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

    public override void PopulateHistory(List<string> visiblePostGuids)
    {
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

    /// <summary>
    /// Adds a profile button for the specified post data if one doesn't already exist, and unlocks any associated gallery media items.
    /// </summary>
    /// <param name="postData">The social media post data for which to add a profile button.</param>
    public override void AddProfileButton(SocialMediaPostSO postData)
    {
        if (_profileButtons.ContainsKey(postData.Character))
            return;

        var buttonObj = Instantiate(_profilePageButtonPrefab, _profilePageButtonContainer);
        var buttonComp = buttonObj.GetComponent<SocialMediaProfileButton>();
        _profileButtons[postData.Character] = buttonComp;
        buttonComp.Initialize(postData.Character, postData.Character.SpicySocialMediaProfile);

        var galleryItems = postData.Character.SpicySocialMediaProfile.GetBaseGalleryMediaData(postData.Character, MediaTargetPlatform.SpicySocialMediaPost);

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
    protected override void RemoveProfileButton(DialogueCharacterSO character)
    {
        if (!_profileButtons.ContainsKey(character))
            return;

        var buttonComp = _profileButtons[character];
        Destroy(buttonComp.gameObject);

        _profileButtons.Remove(character);

        // Remove any associated gallery media items from the save data
        var galleryItems = character.SpicySocialMediaProfile.GetBaseGalleryMediaData(character, MediaTargetPlatform.SpicySocialMediaPost);
        foreach (var mediaData in galleryItems)
        {
            SaveAndLoadManager.Instance.CurrentSave.RollbackUnlockMedia(mediaData.NodeGUID, mediaData.FileName);
        }

        // Refresh the gallery page to reflect the changes and ensure any rolled back media are shown as locked again
        GameManager.Instance.GalleryCanvas.RefreshGalleryPage();
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