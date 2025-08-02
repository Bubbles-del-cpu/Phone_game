using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClearSaveButton : MonoBehaviour
{
    [SerializeField]
    private bool _displayDialog = true;

    public void Click()
    {
        if (_displayDialog)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.CLEAR_SAVE, () =>
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
        OverlayCanvas.Instance.FadeToBlack(() =>
        {
            SaveAndLoadManager.Instance.StartNewSave();
        });
    }
}
