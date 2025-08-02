using UnityEngine;

public class CloseGameButton : MonoBehaviour
{
    [SerializeField]
    private bool _displayDialog = true;

    public void Click()
    {
        if (_displayDialog)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.QUIT, () =>
            {
                ConfirmClose();
            });
        }
        else
        {
            ConfirmClose();
        }
    }

    private void ConfirmClose()
    {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
