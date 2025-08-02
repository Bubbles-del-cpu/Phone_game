using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ChapterReplayButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Update()
    {
        _button.image.color = SaveAndLoadManager.Instance.ReplayingCompletedChapter ? Color.grey : Color.white;
    }
}