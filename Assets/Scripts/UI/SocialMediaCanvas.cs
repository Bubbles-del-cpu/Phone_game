using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class SocialMediaCanvas : UICanvas
{
    [SerializeField] SocialMediaPost socialMediaPostPrefab;
    [SerializeField] RectTransform socialMediaPostsContainer;


    public void RemovePosts(int count)
    {
        if (count == 0)
            return;

        for (var index = 1; index < count; index++)
        {
            var item = socialMediaPostsContainer.transform.GetChild(socialMediaPostsContainer.transform.childCount - index);
            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
        }
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
        GameManager.Instance.MessagingCanvas.Close();
    }
}
