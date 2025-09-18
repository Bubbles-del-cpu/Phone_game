using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System;
using System.Collections.Generic;

public class SocialMediaCanvas : UICanvas
{
    [SerializeField] SocialMediaPost socialMediaPostPrefab;
    [SerializeField] RectTransform socialMediaPostsContainer;


    public void RemovePosts(int count)
    {
        if (count == 0 || socialMediaPostsContainer.transform.childCount <= 0)
            return;

        var destroyList = new List<GameObject>();
        for (var index = 1; index <= count; index++)
        {
            try
            {
                var item = socialMediaPostsContainer.transform.GetChild(socialMediaPostsContainer.transform.childCount - index);
                item.gameObject.SetActive(false);
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
    }

    public void PostToSocialMedia(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification = true)
    {
        SocialMediaPost post = Instantiate(socialMediaPostPrefab, socialMediaPostsContainer);
        post.SetData(_data, nodeData, showNotification);
    }


    public void Clear()
    {
        for(var index = 0; index < socialMediaPostsContainer.childCount; index++)
        {
            Destroy(socialMediaPostsContainer.GetChild(index).gameObject, .5f);
        }
    }

    public override void Open()
    {
        base.Open();
        //GameManager.Instance.MessagingCanvas.Close();
    }
}
