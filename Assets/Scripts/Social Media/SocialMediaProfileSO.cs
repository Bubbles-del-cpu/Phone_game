using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Social Media Profile", menuName = "MeetAndTalk/Social Media/Profile")]
public class SocialMediaProfileSO : ScriptableObject
{
    [SerializeField] private Sprite _profileImage;
    [SerializeField] private Sprite _miniProfileIcon;
    [SerializeField] private LocalizedString _profileName;
    [SerializeField] private LocalizedString _profileDescription;
    [SerializeField] private int _followerCount;
    [SerializeField] private int _followingCount;

    /// <summary>
    /// Setup the profile page with the data from this social media profile
    /// </summary>
    /// <param name="profilePage">The profile page to setup</param>
    public void SetupProfilePage(SocialMediaProfilePage profilePage)
    {
        profilePage.ProfileIcon.sprite = _miniProfileIcon;
        profilePage.ProfileImage.sprite = _profileImage;
        profilePage.ProfileName.text = _profileName.GetLocalizedString();
        profilePage.ProfileDescription.text = _profileDescription.GetLocalizedString();
        profilePage.FollowerCountText.text = _followerCount.ToString();
        profilePage.FollowingCountText.text = _followingCount.ToString();
    }

    /// <summary>
    /// Setup the profile button with the data from this social media profile
    /// </summary>
    /// <param name="profileButton">The profile button to setup</param>
    public void SetupProfileButton(SocialMediaProfileButton profileButton)
    {
        profileButton.Icon.sprite = _miniProfileIcon != null ? _miniProfileIcon : _profileImage;
    }
}