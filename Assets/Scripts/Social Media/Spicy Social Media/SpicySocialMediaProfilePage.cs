using System;
using MeetAndTalk;

public class SpicySocialMediaProfilePage : SocialMediaProfilePage
{
    protected override Func<GalleryMediaItem, bool> _galleryItemFilter => p => p.IsLinearPathUnlock == true && p.TargetPlatform == MediaTargetPlatform.SpicySocialMediaPost;

    public override void OpenProfile(DialogueCharacterSO character)
    {
        ClearProfilePage();
        character.SpicySocialMediaProfile.SetupProfilePage(this);
        PopulateGallery(character);

        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
}