using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EarlyReplayExitButton : MonoBehaviour
{
    private Button _button;
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.EARLY_REPLAY_EXIT, () =>
        {
            // Returning to linear path mode so both flags should be false
            SaveAndLoadManager.Instance.ReplayingCompletedChapter = false;
            SaveAndLoadManager.Instance.PlayingStandaloneChapter = false;

            DialogueChapterManager.Instance.CompleteChapterReplayEarly();
        });
    }
}
