using UnityEngine;

public class ChapterSelectButton : MonoBehaviour
{
    [SerializeField]
    private bool _displayDialog = true;

    public void Click()
    {
        if (_displayDialog)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.REPLAY_EXIT_WARNING, () =>
            {
                DialogueChapterManager.Instance.ReturnToChapterSelection();
            });
        }
        else
        {
            DialogueChapterManager.Instance.ReturnToChapterSelection();
        }
    }
}
