using UnityEngine;

public class SelectNewSaveSlotButton : MonoBehaviour
{
    [SerializeField]
    private bool _displayDialog = true;

    public void Click()
    {
        if (_displayDialog)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.LOGOUT_WARNING, () =>
            {
                ConfirmReset();
            });
        }
        else
        {
            ConfirmReset();
        }
    }

    private void ConfirmReset()
    {
        PlayerPrefs.DeleteAll();
        DialogueChapterManager.Instance.ReturnToChapterSelection();
    }
}
