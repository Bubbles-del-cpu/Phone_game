using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

public class ClearSaveDialog : MonoBehaviour
{
    [SerializeField] private DialogWarningMessage _dialog;
    [SerializeField] private Toggle _clearGalleryToggle;

    private bool _startDialogue;
    public void Setup(bool startDialogueAfterClear)
    {
        _startDialogue = startDialogueAfterClear;
    }

    private void Awake()
    {
        if (_dialog)
        {
            _dialog.SetConfirmEvent(() =>
            {
                GameManager.Instance.ResettingSave = true;
                DialogueManager.Instance.StopAllTrackedCoroutines();

                OverlayCanvas.Instance.FadeToBlack(() =>
                {
                    SaveAndLoadManager.Instance.StartNewSave(_startDialogue, clearGallery: _clearGalleryToggle.isOn);
                    GameManager.Instance.ResettingSave = false;
                });
            });
        }
    }
}
