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
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.GALLERY_CODE_SUCCESS, () => base.Submit(), GameConstants.UIElementKeys.CONTINUE, args: null, twoButtonSetup: false);
        }
        else
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.GALLERY_CODE_FAIL, eventToTrigger: null, GameConstants.UIElementKeys.TRY_AGAIN, args: null, twoButtonSetup: false);
        }
    }
}
