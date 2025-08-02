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
        GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.EARLY_REPLAY_EXIT, () =>
        {
            DialogueChapterManager.Instance.CompleteChapterReplayEarly();
        });
    }
}
