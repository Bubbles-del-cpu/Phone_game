using UnityEngine;

public class GalleryUnlockDialog : InputFieldDialogBox
{
    public string TestString = "TestUnlock";
    private GalleryHelper _helper;
    public override void Submit()
    {
        //Unlock the current gallery
        var helper = new GalleryHelper(GameManager.Instance.GalleryCanvas.UnlockData, _inputField.text);
        if (helper.CheckContent(_inputField.text))
        {
            GameManager.Instance.GalleryCanvas.UnlockData.UnlockTriggered = true;
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.GALLERY_CODE_SUCCESS, () => base.Submit(), "Continue", false);
        }
        else
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.GALLERY_CODE_FAIL, null, "Try again", false);
        }
    }
}
