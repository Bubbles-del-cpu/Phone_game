using System;

public class SpicySocialMediaProfilePage : SocialMediaProfilePage
{
    protected override Func<GalleryMediaItem, bool> _galleryItemFilter => p => p.IsLinearPathUnlock == true && p.TargetPlatform == MediaTargetPlatform.SpicySocialMediaPost;
}