using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClearSaveButton : MonoBehaviour
{
    [SerializeField]
    private bool _displayDialog = true;

    [SerializeField] private bool _startNewGameAfterClear = true;
    public void Click()
    {
        if (_displayDialog)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.CLEAR_SAVE, () =>
            {
                FadeAndRestart();
            });
        }
        else
        {
            //Clears the current auto save and starts a new game
            FadeAndRestart();
        }
    }

    private void FadeAndRestart()
    {
        GameManager.Instance.ResettingSave = true;
        DialogueManager.Instance.StopAllTrackedCoroutines();

        OverlayCanvas.Instance.FadeToBlack(() =>
        {
            SaveAndLoadManager.Instance.StartNewSave(_startNewGameAfterClear);
            GameManager.Instance.ResettingSave = false;
        });
    }
}
