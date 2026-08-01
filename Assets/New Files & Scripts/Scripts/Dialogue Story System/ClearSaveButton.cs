using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClearSaveButton : MonoBehaviour
{
    [SerializeField]
    private bool _displayDialog = true;
    [SerializeField] private GameObject _dialogPrefab;

    [SerializeField] private bool _startNewGameAfterClear = true;
    public void Click()
    {
        if (_displayDialog)
        {
            if (_dialogPrefab != null)
            {
                // If we have a custom prefab assigned use that for the dialog instead of the default one
                var dialog = Instantiate(_dialogPrefab);
                if (dialog.TryGetComponent(out ClearSaveDialog clearSaveDialog))
                {
                    clearSaveDialog.Setup(_startNewGameAfterClear);
                }
                GameManager.Instance.DisplayPopup(dialog);
            }
            else
            {
                // If no custom prefab is assigned, use the default dialog
                GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.CLEAR_SAVE, () =>
                {
                    FadeAndRestart();
                });
            }
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
