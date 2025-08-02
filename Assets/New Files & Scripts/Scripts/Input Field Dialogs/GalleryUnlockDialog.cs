using UnityEngine;

public class GalleryUnlockDialog : InputFieldDialogBox
{
    public string TestString = "TestUnlock";
    public override void Submit()
    {
        //Unlock the current gallery
        if (IsCodeValid(_inputField.text))
        {
            GameManager.Instance.GalleryConfig.Unlock();
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.GALLERY_CODE_SUCCESS, () => base.Submit(), "Continue", false);
        }
        else
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.GALLERY_CODE_FAIL, null, "Try again", false);
        }
    }

    private bool IsCodeValid(string code)
    {
        return GameManager.Instance.GalleryConfig.IsValidCode(code);
    }
}
