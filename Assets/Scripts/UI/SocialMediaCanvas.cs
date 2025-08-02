using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class SocialMediaCanvas : UICanvas
{
    [SerializeField] SocialMediaPost socialMediaPostPrefab;
    [SerializeField] RectTransform socialMediaPostsContainer;

    public void PostToSocialMedia(SocialMediaPostSO _data, DialogueNodeData nodeData, bool showNotification = true)
    {
        SocialMediaPost _post = Instantiate(socialMediaPostPrefab, socialMediaPostsContainer);
        _post.SetData(_data, nodeData, showNotification);
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
